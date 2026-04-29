using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VDManager.Services
{
    /// <summary>
    /// Manages global hotkeys for the application
    /// </summary>
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Modifier key constants
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

        private readonly IntPtr windowHandle;
        private readonly Dictionary<int, Action> hotkeyActions;

        // Registration metadata needed by the watchdog to re-register lost hotkeys
        private readonly Dictionary<int, (uint modifiers, uint vk)> hotkeyParams;

        private int nextHotkeyId = 1;

        // Watchdog timer — periodically verifies each registered hotkey is still alive
        // and re-registers any that were silently dropped (e.g. after system resume).
        private readonly System.Windows.Forms.Timer _watchdogTimer;
        private const int WatchdogIntervalMs = 30_000; // 30 seconds

        public HotkeyManager(IntPtr handle)
        {
            windowHandle = handle;
            hotkeyActions = new Dictionary<int, Action>();
            hotkeyParams  = new Dictionary<int, (uint, uint)>();

            _watchdogTimer = new System.Windows.Forms.Timer { Interval = WatchdogIntervalMs };
            _watchdogTimer.Tick += WatchdogTick;
            _watchdogTimer.Start();
        }

        /// <summary>
        /// Register a global hotkey
        /// </summary>
        /// <param name="modifiers">Modifier keys (MOD_ALT, MOD_CONTROL, etc.)</param>
        /// <param name="key">The key to register</param>
        /// <param name="action">Action to execute when hotkey is pressed</param>
        /// <returns>Hotkey ID if successful, -1 if failed</returns>
        public int RegisterHotkey(uint modifiers, Keys key, Action action)
        {
            int id = nextHotkeyId++;

            // Add MOD_NOREPEAT to prevent multiple triggers
            modifiers |= MOD_NOREPEAT;

            if (RegisterHotKey(windowHandle, id, modifiers, (uint)key))
            {
                hotkeyActions[id] = action;
                hotkeyParams[id]  = (modifiers, (uint)key);
                return id;
            }

            return -1;
        }

        /// <summary>
        /// Unregister a specific hotkey
        /// </summary>
        public bool UnregisterHotkey(int id)
        {
            if (hotkeyActions.ContainsKey(id))
            {
                hotkeyActions.Remove(id);
                hotkeyParams.Remove(id);
                return UnregisterHotKey(windowHandle, id);
            }
            return false;
        }

        /// <summary>
        /// Process a hotkey message from WndProc
        /// </summary>
        public void ProcessHotkey(int hotkeyId)
        {
            if (hotkeyActions.TryGetValue(hotkeyId, out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    // Log error but don't crash
                    System.Diagnostics.Debug.WriteLine($"Hotkey execution error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Unregister all hotkeys
        /// </summary>
        public void UnregisterAll()
        {
            var ids = new List<int>(hotkeyActions.Keys);
            foreach (var id in ids)
            {
                UnregisterHotkey(id);
            }
        }

        /// <summary>
        /// Watchdog: called every 30 s to detect and recover hotkeys that Windows
        /// silently dropped (e.g. after system resume from sleep, or due to
        /// Power Throttling suspending the message pump momentarily).
        /// </summary>
        private void WatchdogTick(object? sender, EventArgs e)
        {
            // Work on a snapshot so we don't modify while iterating
            var snapshot = new List<KeyValuePair<int, (uint modifiers, uint vk)>>(hotkeyParams);

            foreach (var kvp in snapshot)
            {
                int id             = kvp.Key;
                uint modifiers     = kvp.Value.modifiers;
                uint vk            = kvp.Value.vk;

                // Try to register the same id/key combo.
                // • If it fails with ERROR_HOTKEY_ALREADY_REGISTERED (1409) the hotkey
                //   is still alive — great, nothing to do.
                // • If it succeeds, the hotkey was silently dropped; immediately unregister
                //   the probe registration and re-register under the original id.
                bool probeSuccess = RegisterHotKey(windowHandle, id, modifiers, vk);
                if (probeSuccess)
                {
                    // The hotkey was lost — our probe just re-claimed it with the same id,
                    // so it is already restored.  No further action required.
                    System.Diagnostics.Debug.WriteLine(
                        $"[HOTKEY WATCHDOG] Re-registered lost hotkey id={id}");
                }
                else
                {
                    // Probe failed — check WHY. Error 1409 (ERROR_HOTKEY_ALREADY_REGISTERED)
                    // means the hotkey is alive and correctly rejected our probe. Any other
                    // error means the hotkey is gone for an unexpected reason; attempt recovery.
                    int err = Marshal.GetLastWin32Error(); // must be the very first call after RegisterHotKey
                    if (err != 1409)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[HOTKEY WATCHDOG] Probe failed with unexpected error {err} for id={id} — attempting re-registration.");
                        UnregisterHotKey(windowHandle, id); // clear any stale system state
                        bool ok = RegisterHotKey(windowHandle, id, modifiers, vk);
                        System.Diagnostics.Debug.WriteLine(
                            $"[HOTKEY WATCHDOG] Re-registration result={ok} for id={id}");
                    }
                    // else: error 1409 → hotkey is alive, probe correctly rejected
                }
            }
        }

        public void Dispose()
        {
            _watchdogTimer.Stop();
            _watchdogTimer.Dispose();
            UnregisterAll();
        }

        /// <summary>
        /// Get a friendly name for modifier keys
        /// </summary>
        public static string GetModifierName(uint modifiers)
        {
            var parts = new List<string>();

            if ((modifiers & MOD_WIN) != 0) parts.Add("Win");
            if ((modifiers & MOD_CONTROL) != 0) parts.Add("Ctrl");
            if ((modifiers & MOD_ALT) != 0) parts.Add("Alt");
            if ((modifiers & MOD_SHIFT) != 0) parts.Add("Shift");

            return string.Join("+", parts);
        }
    }
}
