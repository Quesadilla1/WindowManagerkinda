using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VDManager.Models;

namespace VDManager.Services
{
    /// <summary>
    /// Handles launching applications from launch profiles,
    /// including startup auto-launch with per-entry delays and global hotkey registration.
    ///
    /// Instance tracking is now delegated entirely to <see cref="WindowInstanceTracker"/>
    /// via the rule-claim mechanism: each <see cref="AppLaunchEntry"/> carries a
    /// <see cref="AppLaunchEntry.LinkedRuleId"/> that points to a <see cref="Models.WindowRule"/>.
    /// When the window for that rule is open, <see cref="WindowInstanceTracker"/> holds its
    /// HWND as a rule claim.  <see cref="IsEntryRunning"/> simply reads that claim —
    /// no separate window scan is needed.
    /// </summary>
    public class LauncherService : ILauncherService
    {
        private readonly IWindowManager _windowManager;
        private readonly IWindowInstanceTracker _instanceTracker;
        private readonly List<System.Windows.Forms.Timer> _startupTimers = new();
        private readonly IRulesManager? _rulesManager;

        public LauncherService(IWindowManager windowManager, IWindowInstanceTracker instanceTracker, IRulesManager? rulesManager = null)
        {
            _windowManager = windowManager;
            _instanceTracker = instanceTracker;
            _rulesManager = rulesManager;
        }

        // ─── Single entry ──────────────────────────────────────────────────────

        /// <summary>
        /// Launch a single AppLaunchEntry immediately.
        /// If the entry has a <see cref="AppLaunchEntry.LinkedRuleId"/> and the rule's
        /// window is still alive in <see cref="WindowInstanceTracker"/>, the entry is skipped.
        /// If the window is minimized, it will be restored to its rule-defined position.
        /// </summary>
        public bool LaunchEntry(AppLaunchEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.ExecutablePath))
                return false;

            // ── Check if the linked rule's window is already open ──────────────
            var existingHwnd = ResolveExistingEntryWindow(entry);
            if (existingHwnd != IntPtr.Zero)
            {
                var hwnd = existingHwnd;

                // Window exists - check if it's minimized
                if (Win32API.IsIconic(hwnd))
                {
                    Debug.WriteLine($"[LauncherService] Linked rule window minimized: {entry.Name} - attempting to restore position");
                    
                    // Try to restore the minimized window to its rule-defined position
                    if (_rulesManager != null)
                    {
                        var rule = _rulesManager.GetAllRules().FirstOrDefault(r => r.Id == entry.LinkedRuleId);
                        if (rule != null)
                        {
                            // Get window info and restore position
                            var windows = _windowManager.GetAllWindows();
                            var windowInfo = windows.FirstOrDefault(w => w.Handle == hwnd);
                            if (windowInfo != null)
                            {
                                bool restored = _windowManager.MoveAndPositionWindow(
                                    windowInfo, 
                                    rule.DesktopIndex, 
                                    rule.Quadrant, 
                                    rule.MonitorIndex);
                                Debug.WriteLine($"[LauncherService] Restored minimized window: {entry.Name} - success={restored}");
                                return true; // Window was restored, not re-launched
                            }
                        }
                    }

                    // If we couldn't restore, at least bring it to the foreground
                    Win32API.ShowWindow(hwnd, 9); // SW_RESTORE
                    Debug.WriteLine($"[LauncherService] Restored minimized window (fallback): {entry.Name}");
                    return true;
                }

                // Window is open but not minimized.
                // Show without activation first, and only attempt foreground when
                // the window is already on the current desktop.
                //Thread.Sleep(100);
                // Re-validate: window could have closed during the sleep.
                if (!Win32API.IsWindow(hwnd))
                    return true; // was open, no relaunch needed

                Win32API.ShowWindow(hwnd, Win32API.SW_SHOWNA);

                if (VirtualDesktopAPI.IsWindowOnCurrentVirtualDesktop(hwnd) != 0)
                {
                    bool foregrounded = Win32API.SetForegroundWindow(hwnd);
                    Debug.WriteLine($"[LauncherService] Foreground attempt (same desktop) for existing window '{entry.Name}': success={foregrounded}");
                }
                else
                {
                    Debug.WriteLine($"[LauncherService] Skipped foreground (different desktop) for existing window '{entry.Name}'");
                }
                return true;
            }

            try
            {
                string resolvedExecutable = ResolveExecutableForLaunch(entry.ExecutablePath);

                var psi = new ProcessStartInfo
                {
                    FileName = resolvedExecutable,
                    UseShellExecute = true
                };

                if (!string.IsNullOrWhiteSpace(entry.Arguments))
                    psi.Arguments = entry.Arguments;

                if (!string.IsNullOrWhiteSpace(entry.WorkingDirectory))
                    psi.WorkingDirectory = entry.WorkingDirectory;

                Process.Start(psi);

                // Optionally switch to target desktop after a short pause
                if (entry.TargetDesktopIndex >= 0)
                {
                    Debug.WriteLine($"[LauncherService] Desktop switch scheduled after launch: {entry.Name} -> desktop {entry.TargetDesktopIndex}");
                    var timer = new System.Windows.Forms.Timer { Interval = 1500 };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        try
                        {
                            _windowManager.SwitchToDesktop(entry.TargetDesktopIndex);
                            Debug.WriteLine($"[LauncherService] Desktop switch success: {entry.Name} switched to desktop {entry.TargetDesktopIndex}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[LauncherService] Desktop switch failed: {ex.Message}");
                        }
                        timer.Dispose();
                    };
                    _startupTimers.Add(timer);
                    timer.Start();
                }

                Debug.WriteLine($"[LauncherService] Launched: {resolvedExecutable} {entry.Arguments}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LauncherService] Failed to launch '{entry.ExecutablePath}': {ex.Message}");
                return false;
            }
        }

        // ─── Tracking helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the given entry has a linked rule whose window is
        /// currently alive according to <see cref="WindowInstanceTracker"/>.
        /// </summary>
        public bool IsEntryRunning(AppLaunchEntry entry)
        {
            return ResolveExistingEntryWindow(entry) != IntPtr.Zero;
        }

        /// <summary>
        /// Resolve an existing window for a launch entry.
        ///
        /// Primary path: use the rule-claim handle tracked by <see cref="IWindowInstanceTracker"/>.
        /// Fallback path: if claims are still warming up at startup, resolve the linked rule
        /// and scan current windows for a match, then opportunistically seed the rule claim.
        /// </summary>
        private IntPtr ResolveExistingEntryWindow(AppLaunchEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.LinkedRuleId))
                return IntPtr.Zero;

            // Fast path: existing claim.
            var claimed = _instanceTracker.GetClaimedHandle(entry.LinkedRuleId);
            if (claimed != IntPtr.Zero && Win32API.IsWindow(claimed))
                return claimed;

            // Fallback path: claim map not ready yet (startup warm-up) or not populated.
            if (_rulesManager == null)
                return IntPtr.Zero;

            var rule = _rulesManager.GetAllRules().FirstOrDefault(r => r.Id == entry.LinkedRuleId);
            if (rule == null)
                return IntPtr.Zero;

            var matchedWindow = _windowManager.GetAllWindows()
                .FirstOrDefault(w => RuleMatchesForLauncher(rule, w));

            if (matchedWindow == null)
                return IntPtr.Zero;

            // Seed claim if possible so future checks are cheap and deterministic.
            _instanceTracker.TryClaimRule(rule.Id, matchedWindow.Handle);
            Debug.WriteLine($"[LauncherService] Fallback running-window match for '{entry.Name}' (ruleId={rule.Id}, hwnd={matchedWindow.Handle}).");
            return matchedWindow.Handle;
        }

        /// <summary>
        /// WindowRule.Matches includes rule.Enabled gating, but launcher running-window detection
        /// should still work for linked rules even if the rule is temporarily disabled.
        /// </summary>
        private static bool RuleMatchesForLauncher(WindowRule rule, WindowInfo window)
        {
            if (!window.ProcessName.Equals(rule.ProcessName, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(rule.WindowTitlePattern))
            {
                if (rule.WindowTitlePattern == "*")
                    return true;

                if (rule.UseRegex)
                {
                    try
                    {
                        var regex = new Regex(rule.WindowTitlePattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
                        if (!regex.IsMatch(window.Title))
                            return false;
                    }
                    catch (ArgumentException)
                    {
                        if (!window.Title.Contains(rule.WindowTitlePattern, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        if (!window.Title.Contains(rule.WindowTitlePattern, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }
                else
                {
                    if (!window.Title.Contains(rule.WindowTitlePattern, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            return true;
        }

        // ─── Profile ───────────────────────────────────────────────────────────

        /// <summary>
        /// Launch all entries in a profile, respecting each entry's DelaySeconds.
        /// Returns the number of entries queued for launch.
        /// </summary>
        public int LaunchProfile(LaunchProfile profile)
        {
            if (profile == null || profile.Entries.Count == 0)
                return 0;

            var ordered = profile.Entries.OrderBy(e => e.SortOrder).ThenBy(e => e.DelaySeconds).ToList();
            int count = 0;

            // Grant foreground permission to any process for this call chain.
            // Required because DeskBulldozer is a tray app (hidden window) and
            // SetForegroundWindow on a hidden handle silently fails. ASFW_ANY
            // is automatically revoked by Windows after the next user input event.
            Win32API.AllowSetForegroundWindow(Win32API.ASFW_ANY);

            foreach (var entry in ordered)
            {
                if (string.IsNullOrWhiteSpace(entry.ExecutablePath))
                    continue;

                count++;

                if (entry.DelaySeconds <= 0)
                {
                    LaunchEntry(entry);
                }
                else
                {
                    // Schedule launch after DelaySeconds
                    int delayMs = entry.DelaySeconds * 1000;
                    var capturedEntry = entry;
                    var timer = new System.Windows.Forms.Timer { Interval = delayMs };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        LaunchEntry(capturedEntry);
                        timer.Dispose();
                    };
                    _startupTimers.Add(timer);
                    timer.Start();
                }
            }

            return count;
        }

        // ─── Startup auto-launch ───────────────────────────────────────────────

        /// <summary>
        /// Called once at startup. Scans all profiles for entries that have LaunchOnStartup=true
        /// and schedules them using their DelaySeconds values.
        /// </summary>
        public void AutoLaunchStartupProfiles(IEnumerable<LaunchProfile> profiles)
        {
            foreach (var profile in profiles.Where(p => p.LaunchOnStartup))
            {
                LaunchProfile(profile);
                Debug.WriteLine($"[LauncherService] Auto-launch queued for profile: {profile.Name}");
            }
        }

        // ─── Hotkey registration ───────────────────────────────────────────────

        /// <summary>
        /// Register a global hotkey for each profile that has one configured.
        /// Stores the registered ID back on the profile object so it can be unregistered later.
        /// </summary>
        public void RegisterProfileHotkeys(IEnumerable<LaunchProfile> profiles, HotkeyManager hotkeyManager)
        {
            foreach (var profile in profiles)
            {
                // Unregister previous hotkey if any
                if (profile.RegisteredHotkeyId >= 0)
                {
                    hotkeyManager.UnregisterHotkey(profile.RegisteredHotkeyId);
                    profile.RegisteredHotkeyId = -1;
                }

                if (profile.HotkeyKey == Keys.None)
                    continue;

                var capturedProfile = profile;
                int id = hotkeyManager.RegisterHotkey(
                    profile.HotkeyModifiers,
                    profile.HotkeyKey,
                    () => LaunchProfile(capturedProfile)
                );

                profile.RegisteredHotkeyId = id;

                if (id >= 0)
                    Debug.WriteLine($"[LauncherService] Hotkey registered for profile '{profile.Name}': {profile.GetHotkeyDisplayString()} (id={id})");
                else
                    Debug.WriteLine($"[LauncherService] Hotkey CONFLICT for profile '{profile.Name}': {profile.GetHotkeyDisplayString()}");
            }
        }

        /// <summary>
        /// Unregister all profile hotkeys.
        /// </summary>
        public void UnregisterProfileHotkeys(IEnumerable<LaunchProfile> profiles, HotkeyManager hotkeyManager)
        {
            foreach (var profile in profiles)
            {
                if (profile.RegisteredHotkeyId >= 0)
                {
                    hotkeyManager.UnregisterHotkey(profile.RegisteredHotkeyId);
                    profile.RegisteredHotkeyId = -1;
                }
            }
        }

        // ─── Preset helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Resolve launch targets that are known to route through console wrappers
        /// (for example VS Code's "code.cmd") so startup launches don't flash a
        /// transient console/conhost window.
        /// </summary>
        private static string ResolveExecutableForLaunch(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
                return executablePath;

            string expanded = Environment.ExpandEnvironmentVariables(executablePath.Trim());
            string fileName = Path.GetFileName(expanded);

            bool looksLikeVsCodeCli =
                expanded.Equals("code", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("code", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("code.cmd", StringComparison.OrdinalIgnoreCase);

            if (looksLikeVsCodeCli)
            {
                string codeExe = FindVSCode();
                if (!codeExe.Equals("code", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[LauncherService] Resolved '{executablePath}' to '{codeExe}' to avoid cmd wrapper startup.");
                    return codeExe;
                }
            }

            return expanded;
        }

        /// <summary>
        /// Try to find the VS Code GUI executable (Code.exe).
        /// Returns the path if found, otherwise returns "code".
        /// </summary>
        public static string FindVSCode()
        {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "Code.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft VS Code", "Code.exe")
            };

            foreach (var c in candidates)
                if (System.IO.File.Exists(c))
                    return c;

            return "code"; // fallback to PATH/CLI wrapper if needed
        }

        /// <summary>
        /// Try to find the Visual Studio 2022 devenv.exe via the registry / common paths.
        /// Returns the path if found, otherwise returns "devenv.exe".
        /// </summary>
        public static string FindVisualStudio2022()
        {
            // Try registry (VS setup)
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\devenv.exe");
                if (key?.GetValue(null) is string path && System.IO.File.Exists(path))
                    return path;
            }
            catch { /* ignore */ }

            // Common install paths
            var candidates = new[]
            {
                @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
            };

            foreach (var c in candidates)
                if (System.IO.File.Exists(c))
                    return c;

            return "devenv.exe"; // fallback to PATH
        }

        public void Dispose()
        {
            foreach (var t in _startupTimers)
            {
                try { t.Stop(); t.Dispose(); } catch { /* ignore */ }
            }
            _startupTimers.Clear();
        }
    }
}
