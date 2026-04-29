using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VDManager.Services
{
    /// <summary>
    /// Service for monitoring new windows and auto-applying rules.
    /// </summary>
    public class WindowMonitor : IWindowMonitor
    {
        private readonly IWindowManager windowManager;
        private readonly IRulesManager rulesManager;
        private readonly IWindowInstanceTracker instanceTracker;

        // Fix C-1: replaced System.Threading.Timer + async void with a CancellationToken-driven Task loop
        private CancellationTokenSource? _cts;
        private HashSet<IntPtr> knownWindows;
        private bool isMonitoring;

        // Fix C-2: _lock now also guards RefreshKnownWindowsCore
        private readonly object _lock = new object();

        // ── Enforcement state ──────────────────────────────────────────────────
        private sealed class EnforcedWindowState
        {
            public Models.WindowRule Rule { get; set; } = null!;
            public int ExpectedDesktop { get; set; }
            public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
            public DateTime LastEnforcedAt { get; set; } = DateTime.MinValue;
        }
        private readonly Dictionary<IntPtr, EnforcedWindowState> _enforcedWindows = new();
        private readonly object _enforceLock = new object();
        // volatile provides the memory barrier needed so the background enforcement thread
        // always sees the latest value written by the UI thread. The access pattern is
        // single-write (UI thread sets true/false) + single-read (background thread checks
        // once per cycle), so no compound atomicity beyond volatile is required. Missing one
        // enforcement tick while a display change is pending is harmless.
        private volatile bool _displayChangePending = false;
        // Tracks which windows have already had a skip-warning surfaced to the UI this cycle,
        // to avoid spamming UpdateStatus every enforcement tick (~2s).
        private readonly HashSet<IntPtr> _skipWarnedWindows = new();

        /// <summary>Whether position enforcement (snap-back) is active.</summary>
        public bool EnforcementEnabled { get; set; } = false;
        /// <summary>Milliseconds to wait after initial placement before enforcing.</summary>
        public int GracePeriodMs { get; set; } = 3000;
        /// <summary>Minimum milliseconds between consecutive snap-backs for the same window.</summary>
        public int CooldownMs { get; set; } = 1000;
        /// <summary>Milliseconds to wait after a new window is detected before applying rules.</summary>
        public int NewWindowRuleDelayMs { get; set; } = 500;
        /// <summary>When true, skip position enforcement for minimized windows.</summary>
        public bool SkipEnforcementWhenMinimized { get; set; } = false;

        public event EventHandler<WindowDetectedEventArgs>? WindowDetected;
        public event EventHandler<RuleAppliedEventArgs>? RuleApplied;
        /// <summary>
        /// Fired (once per window) when enforcement is skipped because the target
        /// monitor or virtual desktop no longer exists. The string argument is a
        /// human-readable message suitable for the status bar.
        /// </summary>
        public event EventHandler<string>? EnforcementSkipped;

        public bool IsMonitoring => isMonitoring;

        public WindowMonitor(IWindowManager windowManager, IRulesManager rulesManager, IWindowInstanceTracker instanceTracker)
        {
            this.windowManager = windowManager;
            this.rulesManager = rulesManager;
            this.instanceTracker = instanceTracker;
            this.knownWindows = new HashSet<IntPtr>();
            rulesManager.RuleAppliedSuccessfully += OnRuleAppliedSuccessfully;
        }

        /// <summary>
        /// Start monitoring for new windows.
        /// </summary>
        public void StartMonitoring(int intervalMs = 2000)
        {
            if (isMonitoring)
                return;

            System.Diagnostics.Debug.WriteLine(
                $"[WindowMonitor] StartMonitoring: intervalMs={intervalMs}, NewWindowRuleDelayMs={NewWindowRuleDelayMs}.");

            // Fix C-2: always initialise knownWindows under the lock so the timer thread can never race with this
            lock (_lock)
            {
                RefreshKnownWindowsCore();
            }

            _cts = new CancellationTokenSource();
            isMonitoring = true;

            // Fix C-1: fire-and-forget a Task (not async void) so unhandled exceptions don't crash the process
            _ = MonitorLoopAsync(intervalMs, _cts.Token);
        }

        /// <summary>
        /// Stop monitoring.
        /// </summary>
        public void StopMonitoring()
        {
            if (!isMonitoring)
                return;

            System.Diagnostics.Debug.WriteLine("[WindowMonitor] StopMonitoring called.");
            _cts?.Cancel();
            _cts = null;
            isMonitoring = false;
        }

        /// <summary>
        /// Snapshot current open windows into the known set and seed the instance tracker
        /// so that windows already open when monitoring starts get stable instance numbers.
        /// Caller MUST hold _lock.
        /// </summary>
        private void RefreshKnownWindowsCore()
        {
            knownWindows.Clear();
            var windows = windowManager.GetAllWindows();
            foreach (var window in windows)
                knownWindows.Add(window.Handle);

            // Seed outside the snapshot loop so AssignInstance (which acquires its own lock)
            // is not called while we are still iterating — safe because AssignInstance is idempotent.
            instanceTracker.SeedFromWindows(windows);
        }

        /// <summary>
        /// Fix C-1: main monitor loop – runs as a proper Task, never as async void.
        /// Exceptions are caught so the loop survives transient failures.
        /// </summary>
        private async Task MonitorLoopAsync(int intervalMs, CancellationToken ct)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowMonitor] MonitorLoop started (intervalMs={intervalMs}).");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(intervalMs, ct).ConfigureAwait(false);
                    await ProcessNewWindowsAsync(ct).ConfigureAwait(false);
                    if (EnforcementEnabled)
                        EnforceWindowPositions();
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("[WindowMonitor] MonitorLoop cancelled — shutting down.");
                    break; // normal shutdown
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WindowMonitor] Error in monitor loop: {ex}");
                    // Continue – transient errors should not stop monitoring
                }
            }
            System.Diagnostics.Debug.WriteLine("[WindowMonitor] MonitorLoop exited.");
        }

        /// <summary>
        /// Detect new/closed windows and apply matching rules.
        ///
        /// Lock ordering discipline:
        ///   _lock (WindowMonitor) must NEVER be acquired while instanceTracker's
        ///   internal lock is held, and vice-versa.  To prevent an AB/BA deadlock we
        ///   collect the handles we need to process under _lock, then call ALL
        ///   instanceTracker methods (AssignInstance, RemoveWindow) OUTSIDE _lock.
        /// </summary>
        private async Task ProcessNewWindowsAsync(CancellationToken ct)
        {
            // Cheap handle-only scan — no Process.GetProcessById() or VDA lookup.
            var currentHandles = windowManager.GetAllWindowHandles();
            List<IntPtr> newHandles;
            List<IntPtr> closedHandles;

            // Step 1: determine which handles are new / closed using only _lock.
            // Do NOT call instanceTracker here — it acquires its own internal lock.
            lock (_lock)
            {
                var currentHandleSet = new HashSet<IntPtr>(currentHandles);

                newHandles = currentHandles
                    .Where(h => !knownWindows.Contains(h))
                    .ToList();

                foreach (var h in newHandles)
                    knownWindows.Add(h);

                closedHandles = knownWindows.Where(h => !currentHandleSet.Contains(h)).ToList();
                knownWindows.RemoveWhere(h => !currentHandleSet.Contains(h));
            }

            // Full WindowInfo only for new handles (usually 0–1 per cycle, not all 50+).
            List<WindowInfo> newWindows = newHandles
                .Select(h => windowManager.GetWindowInfo(h))
                .Where(w => w != null)
                .Select(w => w!)
                .ToList();

            // Step 2: update instanceTracker OUTSIDE _lock to avoid AB/BA deadlock.
            foreach (var window in newWindows)
                instanceTracker.AssignInstance(window);

            foreach (var handle in closedHandles)
                instanceTracker.RemoveWindow(handle);

            if (newWindows.Count == 0)
                return;

            System.Diagnostics.Debug.WriteLine(
                $"[WindowMonitor] {newWindows.Count} new window(s) detected: {string.Join(", ", newWindows.Select(w => $"'{w.ProcessName}' (hwnd={w.Handle})"))}.");
            System.Diagnostics.Debug.WriteLine(
                $"[WindowMonitor] {closedHandles.Count} window(s) closed.");

            // Configurable delay to let newly-launched windows fully initialise
            System.Diagnostics.Debug.WriteLine(
                $"[WindowMonitor] Waiting {NewWindowRuleDelayMs}ms for new windows to initialise...");
            await Task.Delay(NewWindowRuleDelayMs, ct).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("[WindowMonitor] Initialisation wait complete. Processing rules...");

            foreach (var window in newWindows)
            {
                ct.ThrowIfCancellationRequested();

                // Re-fetch window info so the title reflects the post-initialisation state.
                // The WindowInfo captured before the delay may still have the raw URL or
                // "loading…" title that browsers show before the page finishes loading.
                var freshWindow = windowManager.GetWindowInfo(window.Handle);
                if (freshWindow == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowMonitor] hwnd={window.Handle} ('{window.ProcessName}') closed during initialisation wait — skipping.");
                    continue;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[WindowMonitor] Processing '{freshWindow.ProcessName}' (hwnd={freshWindow.Handle}, title='{freshWindow.Title}', desktop={freshWindow.DesktopNumber}).");

                WindowDetected?.Invoke(this, new WindowDetectedEventArgs(freshWindow));

                var rule = rulesManager.FindMatchingRule(freshWindow);
                if (rule == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowMonitor] No matching rule for '{freshWindow.ProcessName}'.");
                }
                else if (!rule.Enabled)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowMonitor] Matched rule '{rule.Description ?? rule.ProcessName}' for '{freshWindow.ProcessName}' but rule is disabled — skipping.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowMonitor] Applying rule '{rule.Description ?? rule.ProcessName}' to '{freshWindow.ProcessName}' → desktop={rule.DesktopIndex}, quadrant={rule.Quadrant}, monitor={rule.MonitorIndex}.");
                    bool success = rulesManager.ApplyRule(rule, freshWindow);
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowMonitor] Rule apply result={success} for '{freshWindow.ProcessName}'.");
                    RuleApplied?.Invoke(this, new RuleAppliedEventArgs(freshWindow, rule, success));
                }
            }
        }

        // ── Enforcement methods ────────────────────────────────────────────────

        private void OnRuleAppliedSuccessfully(WindowInfo window, Models.WindowRule rule)
        {
            if (!rule.EnforcePosition) return;
            lock (_enforceLock)
            {
                _enforcedWindows[window.Handle] = new EnforcedWindowState
                {
                    Rule = rule,
                    ExpectedDesktop = rule.DesktopIndex,
                    PlacedAt = DateTime.UtcNow,
                    LastEnforcedAt = DateTime.MinValue
                };
            }
        }

        private void EnforceWindowPositions()
        {
            if (_displayChangePending)
            {
                System.Diagnostics.Debug.WriteLine("[Enforcement] Skipping — display change pending.");
                return;
            }

            var now = DateTime.UtcNow;
            List<(IntPtr hwnd, EnforcedWindowState state)> snapshot;
            lock (_enforceLock)
                snapshot = _enforcedWindows.Select(kv => (kv.Key, kv.Value)).ToList();

            foreach (var (hwnd, state) in snapshot)
            {
                try
                {
                    if (!Win32API.IsWindowVisible(hwnd))
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] hwnd={hwnd} (rule='{state.Rule.ProcessName}') is no longer visible — removing from enforcement.");
                        lock (_enforceLock) _enforcedWindows.Remove(hwnd);
                        continue;
                    }

                    // Skip minimized windows if the setting is enabled (don't remove from enforcement list)
                    if (SkipEnforcementWhenMinimized && Win32API.IsIconic(hwnd))
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] hwnd={hwnd} is minimized — skipping enforcement (will resume when restored).");
                        continue;
                    }

                    double ageSincePlace = (now - state.PlacedAt).TotalMilliseconds;
                    if (ageSincePlace < GracePeriodMs)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] hwnd={hwnd} still in grace period ({ageSincePlace:0}ms < {GracePeriodMs}ms) — skipping.");
                        continue;
                    }

                    if (state.LastEnforcedAt != DateTime.MinValue)
                    {
                        double ageSinceLast = (now - state.LastEnforcedAt).TotalMilliseconds;
                        if (ageSinceLast < CooldownMs)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"[Enforcement] hwnd={hwnd} in cooldown ({ageSinceLast:0}ms < {CooldownMs}ms) — skipping.");
                            continue;
                        }
                    }

                    // If the rule targets a specific monitor that is disconnected, skip the
                    // window entirely — don't attempt desktop moves or position snaps.
                    if (state.Rule.Quadrant != Quadrant.None &&
                        state.Rule.MonitorIndex >= QuadrantLayout.GetMonitorCount())
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] MonitorIndex={state.Rule.MonitorIndex} out of range " +
                            $"({QuadrantLayout.GetMonitorCount()} monitors). " +
                            $"Skipping '{state.Rule.ProcessName}' entirely until monitor reconnects.");
                        if (_skipWarnedWindows.Add(hwnd))
                            EnforcementSkipped?.Invoke(this,
                                $"'{state.Rule.ProcessName}' skipped — Monitor {state.Rule.MonitorIndex + 1} disconnected");
                        continue;
                    }

                    // If the rule targets a virtual desktop that no longer exists, skip entirely.
                    if (state.Rule.DesktopIndex >= VirtualDesktopAPI.GetDesktopCount())
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] DesktopIndex={state.Rule.DesktopIndex} out of range " +
                            $"({VirtualDesktopAPI.GetDesktopCount()} desktop(s)). " +
                            $"Skipping '{state.Rule.ProcessName}' entirely until desktop is restored.");
                        if (_skipWarnedWindows.Add(hwnd))
                            EnforcementSkipped?.Invoke(this,
                                $"'{state.Rule.ProcessName}' skipped — Desktop {state.Rule.DesktopIndex + 1} no longer exists");
                        continue;
                    }

                    bool needsEnforcement = false;

                    int currentDesktop = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd);
                    if (currentDesktop >= 0 && currentDesktop != state.ExpectedDesktop)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] hwnd={hwnd} desktop mismatch: expected={state.ExpectedDesktop}, current={currentDesktop}.");
                        needsEnforcement = true;
                    }

                    if (!needsEnforcement && state.Rule.Quadrant != Quadrant.None)
                    {
                        if (Win32API.GetWindowRect(hwnd, out var currentRect))
                        {
                            var expected = QuadrantLayout.ForMonitor(state.Rule.MonitorIndex)
                                    .GetQuadrantBounds(state.Rule.Quadrant);
                            const int tolerance = 20;
                            bool rectMismatch =
                                Math.Abs(currentRect.Left - expected.Left) > tolerance ||
                                Math.Abs(currentRect.Top - expected.Top) > tolerance ||
                                Math.Abs(currentRect.Width - expected.Width) > tolerance ||
                                Math.Abs(currentRect.Height - expected.Height) > tolerance;
                            if (rectMismatch)
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"[Enforcement] hwnd={hwnd} position mismatch: " +
                                    $"current=({currentRect.Left},{currentRect.Top} {currentRect.Width}x{currentRect.Height}), " +
                                    $"expected=({expected.Left},{expected.Top} {expected.Width}x{expected.Height}).");
                                needsEnforcement = true;
                            }
                        }
                    }

                    if (!needsEnforcement)
                    {
                        //System.Diagnostics.Debug.WriteLine(
                         //   $"[Enforcement] hwnd={hwnd} ('{state.Rule.ProcessName}') is in the correct position — no snap needed.");
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"[Enforcement] Snapping '{state.Rule.ProcessName}' (hwnd={hwnd}) back to desktop={state.Rule.DesktopIndex}, quadrant={state.Rule.Quadrant}.");

                    bool success;
                    if (state.Rule.Quadrant == Quadrant.None)
                        success = windowManager.MoveWindowToDesktop(hwnd, state.Rule.DesktopIndex);
                    else
                        success = windowManager.MoveAndPositionWindow(hwnd,
                            state.Rule.DesktopIndex, state.Rule.Quadrant, state.Rule.MonitorIndex);

                    if (success)
                    {
                        state.LastEnforcedAt = now;
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] Snapped '{state.Rule.ProcessName}' back to Desktop {state.Rule.DesktopIndex + 1} ✓");
                        // Fetch WindowInfo only for the RuleApplied event (rare path).
                        var window = windowManager.GetWindowInfo(hwnd);
                        if (window != null)
                            RuleApplied?.Invoke(this, new RuleAppliedEventArgs(window, state.Rule, true));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Enforcement] FAILED to snap '{state.Rule.ProcessName}' (hwnd={hwnd}) back.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Enforcement] Error for hwnd={hwnd}: {ex.Message}");
                }
            }
        }

        /// <summary>Clear all enforcement registrations (e.g. when enforcement is toggled off).</summary>
        public void ClearAllEnforcedWindows()
        {
            lock (_enforceLock) _enforcedWindows.Clear();
        }

        /// <summary>
        /// Temporarily suspends position enforcement during a display reconfiguration.
        /// The enforcement loop continues running but skips all snap-back actions.
        /// </summary>
        public void SuspendEnforcementForDisplayChange()
        {
            _displayChangePending = true;
            System.Diagnostics.Debug.WriteLine("[WindowMonitor] Enforcement suspended for display change.");
        }

        /// <summary>
        /// Resumes enforcement after display change processing is complete.
        /// If the monitor count changed (monitors added/removed), clears all enforcement
        /// registrations because monitor indices may have shifted and we cannot reliably
        /// know which physical monitor any stored index now refers to.
        /// If only resolution/DPI changed (same monitor count), just resets grace periods.
        /// </summary>
        public void ResumeEnforcementAfterDisplayChange(bool monitorCountChanged)
        {
            lock (_enforceLock)
            {
                if (monitorCountChanged)
                {
                    // Indices may have shifted — enforcement state is no longer trustworthy.
                    // Clear all registrations; windows will be re-registered when rules are
                    // re-applied or when they are next seen by the monitoring loop.
                    _enforcedWindows.Clear();
                    _skipWarnedWindows.Clear();
                    System.Diagnostics.Debug.WriteLine(
                        "[WindowMonitor] Monitor count changed — cleared enforcement registry. " +
                        "Re-apply rules to restore enforcement.");
                }
                else
                {
                    // Same monitors, just a resolution/DPI change. Indices are stable;
                    // reset grace periods so windows aren't immediately snapped back.
                    var now = DateTime.UtcNow;
                    foreach (var state in _enforcedWindows.Values)
                    {
                        state.PlacedAt = now;
                        state.LastEnforcedAt = DateTime.MinValue;
                    }
                    System.Diagnostics.Debug.WriteLine(
                        "[WindowMonitor] Same monitor count — reset grace periods and resumed enforcement.");
                }
            }
            _displayChangePending = false;
        }

        /// <summary>
        /// Called when virtual desktops are added or removed externally (e.g. via Task View).
        /// Clears all enforcement registrations because desktop indices may have shifted.
        /// Windows will be re-registered when rules are re-applied.
        /// </summary>
        public void OnVirtualDesktopCountChanged()
        {
            lock (_enforceLock)
            {
                _enforcedWindows.Clear();
                _skipWarnedWindows.Clear();
                System.Diagnostics.Debug.WriteLine(
                    "[WindowMonitor] VD count changed — cleared enforcement registry.");
            }
        }

        public void Dispose()
        {
            rulesManager.RuleAppliedSuccessfully -= OnRuleAppliedSuccessfully;
            StopMonitoring();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Event args for when a new window is detected.
    /// </summary>
    public class WindowDetectedEventArgs : EventArgs
    {
        public WindowInfo Window { get; }

        public WindowDetectedEventArgs(WindowInfo window)
        {
            Window = window;
        }
    }

    /// <summary>
    /// Event args for when a rule is applied to a window.
    /// </summary>
    public class RuleAppliedEventArgs : EventArgs
    {
        public WindowInfo Window { get; }
        public Models.WindowRule Rule { get; }
        public bool Success { get; }

        public RuleAppliedEventArgs(WindowInfo window, Models.WindowRule rule, bool success)
        {
            Window = window;
            Rule = rule;
            Success = success;
        }
    }
}
