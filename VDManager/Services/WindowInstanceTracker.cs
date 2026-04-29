using System;
using System.Collections.Generic;
using System.Linq;

namespace VDManager.Services
{
    /// <summary>
    /// Tracks window instances to maintain stable instance numbering throughout the application session.
    /// Assigns instance numbers when windows are detected and maintains them until windows close.
    /// </summary>
    public class WindowInstanceTracker : IWindowInstanceTracker
    {
        private readonly Dictionary<IntPtr, WindowInstance> instanceMap;
        private readonly Dictionary<string, HashSet<int>> processInstances;

        // Rule claims: a title-filter rule can be "held" by at most one window at a time.
        // Key = ruleId, Value = handle of the window currently holding that rule.
        private readonly Dictionary<string, IntPtr> ruleClaims;

        private readonly object lockObject = new object();

        private class WindowInstance
        {
            public int InstanceNumber { get; set; }
            public string ProcessName { get; set; } = string.Empty;
            public DateTime AssignedAt { get; set; }
        }

        public WindowInstanceTracker()
        {
            instanceMap = new Dictionary<IntPtr, WindowInstance>();
            processInstances = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
            ruleClaims = new Dictionary<string, IntPtr>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Assign an instance number to a window.
        /// </summary>
        /// <param name="window">The window to assign an instance number to</param>
        /// <returns>The assigned instance number (1-based)</returns>
        public int AssignInstance(WindowInfo window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            lock (lockObject)
            {
                // If already assigned, return existing
                if (instanceMap.TryGetValue(window.Handle, out var existing))
                {
                    return existing.InstanceNumber;
                }

                // Get or create the set of instances for this process
                if (!processInstances.TryGetValue(window.ProcessName, out var instances))
                {
                    instances = new HashSet<int>();
                    processInstances[window.ProcessName] = instances;
                }

                // Find the lowest available instance number (fill gaps)
                int instanceNumber = 1;
                while (instances.Contains(instanceNumber))
                {
                    instanceNumber++;
                }

                // Assign and track
                var windowInstance = new WindowInstance
                {
                    InstanceNumber = instanceNumber,
                    ProcessName = window.ProcessName,
                    AssignedAt = DateTime.Now
                };

                instanceMap[window.Handle] = windowInstance;
                instances.Add(instanceNumber);

                return instanceNumber;
            }
        }

        /// <summary>
        /// Get the instance number for a window handle.
        /// </summary>
        /// <param name="handle">The window handle</param>
        /// <returns>The instance number if found, otherwise null</returns>
        public int? GetInstance(IntPtr handle)
        {
            lock (lockObject)
            {
                if (instanceMap.TryGetValue(handle, out var instance))
                {
                    return instance.InstanceNumber;
                }
                return null;
            }
        }

        /// <summary>
        /// Remove a window from tracking when it closes.
        /// Also releases any rule claims this window was holding so those rules
        /// become available for the next matching window.
        /// </summary>
        /// <param name="handle">The window handle to remove</param>
        public void RemoveWindow(IntPtr handle)
        {
            lock (lockObject)
            {
                if (instanceMap.TryGetValue(handle, out var instance))
                {
                    // Remove from instance map
                    instanceMap.Remove(handle);

                    // Remove from process instances
                    if (processInstances.TryGetValue(instance.ProcessName, out var instances))
                    {
                        instances.Remove(instance.InstanceNumber);

                        // Clean up empty process entries
                        if (instances.Count == 0)
                        {
                            processInstances.Remove(instance.ProcessName);
                        }
                    }
                }

                // Release any rule claims held by this window
                var claimedRules = ruleClaims
                    .Where(kvp => kvp.Value == handle)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var ruleId in claimedRules)
                    ruleClaims.Remove(ruleId);
            }
        }

        // ── Rule Claims ───────────────────────────────────────────────────────────
        // Title-filter rules act as one-at-a-time reservations. A rule can only be
        // held by one window; when that window closes the seat is freed automatically.

        /// <summary>
        /// Attempt to claim a title-filter rule for a window.
        /// Returns true if the claim succeeded (rule was free or already held by this window).
        /// Returns false if the rule is currently held by a different window.
        /// </summary>
        public bool TryClaimRule(string ruleId, IntPtr handle)
        {
            lock (lockObject)
            {
                if (ruleClaims.TryGetValue(ruleId, out var currentHolder))
                {
                    // Already claimed by this window (re-apply scenario) — OK
                    return currentHolder == handle;
                }

                // Rule is free — claim it
                ruleClaims[ruleId] = handle;
                return true;
            }
        }

        /// <summary>
        /// Returns true if the given rule is currently claimed by the given window handle.
        /// </summary>
        public bool IsRuleClaimedBy(string ruleId, IntPtr handle)
        {
            lock (lockObject)
            {
                return ruleClaims.TryGetValue(ruleId, out var holder) && holder == handle;
            }
        }

        /// <summary>
        /// Returns true if the given rule is currently unclaimed (available).
        /// </summary>
        public bool IsRuleAvailable(string ruleId)
        {
            lock (lockObject)
            {
                return !ruleClaims.ContainsKey(ruleId);
            }
        }

        /// <summary>
        /// Returns the HWND currently holding the given rule claim,
        /// or <see cref="IntPtr.Zero"/> if the rule is not currently claimed.
        /// Used by <see cref="LauncherService"/> to check whether the window
        /// for a linked rule is still alive without running its own scan.
        /// </summary>
        public IntPtr GetClaimedHandle(string ruleId)
        {
            lock (lockObject)
            {
                return ruleClaims.TryGetValue(ruleId, out var hwnd) ? hwnd : IntPtr.Zero;
            }
        }

        /// <summary>
        /// Seed the tracker from a snapshot of currently open windows.
        /// Safe to call at any time — already-tracked handles are skipped (idempotent).
        /// Use this on startup or before bulk rule application to ensure every window
        /// has a stable instance number before matching begins.
        /// </summary>
        public void SeedFromWindows(IEnumerable<WindowInfo> windows)
        {
            if (windows == null)
                throw new ArgumentNullException(nameof(windows));

            foreach (var window in windows)
                AssignInstance(window);
        }

        /// <summary>
        /// Clear all tracked instances and rule claims (useful for testing or reset).
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                instanceMap.Clear();
                processInstances.Clear();
                ruleClaims.Clear();
            }
        }

        /// <summary>
        /// Get the count of tracked windows for a specific process.
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <returns>The number of tracked windows for this process</returns>
        public int GetProcessWindowCount(string processName)
        {
            lock (lockObject)
            {
                if (processInstances.TryGetValue(processName, out var instances))
                {
                    return instances.Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// Get all tracked windows (for debugging/diagnostics).
        /// </summary>
        /// <returns>Dictionary of handle to instance number</returns>
        public Dictionary<IntPtr, int> GetAllTrackedWindows()
        {
            lock (lockObject)
            {
                return instanceMap.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.InstanceNumber
                );
            }
        }
    }
}
