using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDManager.Services;

namespace VDManager
{
    /// <summary>
    /// Service for enumerating and managing windows
    /// </summary>
    public class WindowManager : IWindowManager
    {
        /// <summary>
        /// How long (in ms) to wait for a virtual desktop switch to be confirmed
        /// before giving up.  Sourced from AppSettings.DesktopSwitchTimeoutMs.
        /// </summary>
        public int DesktopSwitchTimeoutMs { get; set; } = 800;

        /// <summary>
        /// Get all application windows (visible, non-tool windows)
        /// </summary>
        public List<WindowInfo> GetAllWindows()
        {
            var windows = new List<WindowInfo>();

            Win32API.EnumWindows((hwnd, lParam) =>
            {
                // Only include valid application windows
                if (!Win32API.IsApplicationWindow(hwnd))
                    return true;

                try
                {
                    // Get window title
                    string title = Win32API.GetWindowTitle(hwnd);

                    // Get process information
                    Win32API.GetWindowThreadProcessId(hwnd, out uint processId);
                    string processName = "Unknown";

                    try
                    {
                        using var process = Process.GetProcessById((int)processId);
                        processName = process.ProcessName;
                    }
                    catch
                    {
                        // Process may have closed
                    }

                    // Get desktop number
                    int desktopNumber = -1;
                    if (VirtualDesktopAPI.IsAvailable())
                    {
                        try
                        {
                            desktopNumber = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd);
                        }
                        catch
                        {
                            // May fail for some windows
                        }
                    }

                    var windowInfo = new WindowInfo
                    {
                        Handle = hwnd,
                        Title = title,
                        ProcessName = processName,
                        ProcessId = (int)processId,
                        DesktopNumber = desktopNumber,
                        IsVisible = Win32API.IsWindowVisible(hwnd),
                        IsMinimized = Win32API.IsIconic(hwnd)
                    };

                    windows.Add(windowInfo);
                }
                catch
                {
                    // Skip windows that cause errors
                }

                return true; // Continue enumeration
            }, IntPtr.Zero);

            return windows.OrderBy(w => w.ProcessName).ThenBy(w => w.Title).ToList();
        }

        /// <summary>
        /// Get all application window handles without fetching process names or desktop numbers.
        /// Significantly cheaper than GetAllWindows() — use this when only handle identity is needed.
        /// </summary>
        public List<IntPtr> GetAllWindowHandles()
        {
            var handles = new List<IntPtr>();
            Win32API.EnumWindows((hwnd, _) =>
            {
                if (Win32API.IsApplicationWindow(hwnd))
                    handles.Add(hwnd);
                return true;
            }, IntPtr.Zero);
            return handles;
        }

        /// <summary>
        /// Get full WindowInfo for a single window handle. Returns null if the window is
        /// no longer valid or is not an application window.
        /// </summary>
        public WindowInfo? GetWindowInfo(IntPtr hwnd)
        {
            if (!Win32API.IsApplicationWindow(hwnd))
                return null;

            try
            {
                string title = Win32API.GetWindowTitle(hwnd);

                Win32API.GetWindowThreadProcessId(hwnd, out uint processId);
                string processName = "Unknown";
                try
                {
                    using var process = Process.GetProcessById((int)processId);
                    processName = process.ProcessName;
                }
                catch { }

                int desktopNumber = -1;
                if (VirtualDesktopAPI.IsAvailable())
                {
                    try { desktopNumber = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd); }
                    catch { }
                }

                return new WindowInfo
                {
                    Handle = hwnd,
                    Title = title,
                    ProcessName = processName,
                    ProcessId = (int)processId,
                    DesktopNumber = desktopNumber,
                    IsVisible = Win32API.IsWindowVisible(hwnd),
                    IsMinimized = Win32API.IsIconic(hwnd)
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all windows for a specific process
        /// </summary>
        public List<WindowInfo> GetWindowsForProcess(string processName)
        {
            return GetAllWindows()
                .Where(w => w.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Get all windows on a specific desktop
        /// </summary>
        public List<WindowInfo> GetWindowsOnDesktop(int desktopNumber)
        {
            return GetAllWindows()
                .Where(w => w.DesktopNumber == desktopNumber)
                .ToList();
        }

        /// <summary>
        /// Move a window to a specific desktop.
        /// The VDA move is async in the Windows shell, so we poll until the desktop number
        /// matches or we hit the timeout, instead of reading the value immediately which
        /// can return a stale result.
        /// </summary>
        public bool MoveWindowToDesktop(IntPtr hwnd, int desktopNumber)
        {
            if (!VirtualDesktopAPI.IsAvailable())
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager] MoveWindowToDesktop: VirtualDesktopAPI not available.");
                return false;
            }

            // Validate window handle
            if (hwnd == IntPtr.Zero || !Win32API.IsWindowVisible(hwnd))
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager] MoveWindowToDesktop: invalid/invisible handle {hwnd}.");
                return false;
            }

            try
            {
                int sourceDesktop = -1;
                try { sourceDesktop = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd); } catch { }
                System.Diagnostics.Debug.WriteLine(
                    $"[WindowManager] Moving hwnd={hwnd} from desktop {sourceDesktop} → desktop {desktopNumber} (timeout={DesktopSwitchTimeoutMs}ms).");

                VirtualDesktopAPI.MoveWindowToDesktopNumber(hwnd, desktopNumber);

                // Poll until the shell confirms the move or we time out.
                // Reading the desktop number immediately can return the old value because
                // the move is processed asynchronously by the Windows shell.
                // Use half of DesktopSwitchTimeoutMs for the move-confirmation poll.
                const int pollIntervalMs = 30;
                int maxAttempts = Math.Max(1, DesktopSwitchTimeoutMs / 2 / pollIntervalMs);
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    int currentDesktop = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd);
                    if (currentDesktop == desktopNumber)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[WindowManager] hwnd={hwnd} confirmed on desktop {desktopNumber} after {attempt + 1} poll(s).");
                        return true;
                    }
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowManager] Poll {attempt + 1}/{maxAttempts}: hwnd={hwnd} still on desktop {currentDesktop}, waiting...");
                    System.Threading.Thread.Sleep(pollIntervalMs);
                }

                int finalDesktop = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd);
                bool success = finalDesktop == desktopNumber;
                System.Diagnostics.Debug.WriteLine(
                    success
                        ? $"[WindowManager] hwnd={hwnd} on desktop {desktopNumber} after full poll cycle."
                        : $"[WindowManager] FAILED to move hwnd={hwnd} to desktop {desktopNumber} — still on desktop {finalDesktop} after polling.");
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[WindowManager] EXCEPTION moving hwnd={hwnd} to desktop {desktopNumber}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Move a window to a specific desktop by WindowInfo
        /// </summary>
        public bool MoveWindowToDesktop(WindowInfo window, int desktopNumber)
        {
            return MoveWindowToDesktop(window.Handle, desktopNumber);
        }

        /// <summary>
        /// Switch to a specific desktop
        /// </summary>
        public bool SwitchToDesktop(int desktopNumber)
        {
            if (!VirtualDesktopAPI.IsAvailable())
                return false;

            try
            {
                int desktopCount = VirtualDesktopAPI.GetDesktopCount();
                if (desktopNumber < 0 || desktopNumber >= desktopCount)
                    return false;

                VirtualDesktopAPI.GoToDesktopNumber(desktopNumber);

                // Verify the switch succeeded
                int currentDesktop = VirtualDesktopAPI.GetCurrentDesktopNumber();
                bool success = (currentDesktop == desktopNumber);

                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[HOTKEY] Failed to switch to desktop {desktopNumber}. " +
                        $"Current desktop is {currentDesktop}"
                    );
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[HOTKEY] Exception switching to desktop {desktopNumber}: {ex.Message}"
                );
                return false;
            }
        }

        /// <summary>
        /// Get the current desktop number
        /// </summary>
        public int GetCurrentDesktop()
        {
            if (!VirtualDesktopAPI.IsAvailable())
                return -1;

            try
            {
                return VirtualDesktopAPI.GetCurrentDesktopNumber();
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Get the total number of desktops
        /// </summary>
        public int GetDesktopCount()
        {
            if (!VirtualDesktopAPI.IsAvailable())
                return 0;

            try
            {
                return VirtualDesktopAPI.GetDesktopCount();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Position a window in a specific quadrant
        /// </summary>
        public bool PositionWindowInQuadrant(IntPtr hwnd, Quadrant quadrant, int monitorIndex = 0)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[WindowManager] PositionWindowInQuadrant: hwnd={hwnd}, quadrant={quadrant}, monitor={monitorIndex}.");
            try
            {
                var layout = QuadrantLayout.ForMonitor(monitorIndex);
                bool result = layout.PositionWindow(hwnd, quadrant);
                System.Diagnostics.Debug.WriteLine(
                    $"[WindowManager] PositionWindowInQuadrant result={result} for hwnd={hwnd}.");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[WindowManager] EXCEPTION in PositionWindowInQuadrant hwnd={hwnd}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Position a window in a specific quadrant by WindowInfo
        /// </summary>
        public bool PositionWindowInQuadrant(WindowInfo window, Quadrant quadrant, int monitorIndex = 0)
        {
            return PositionWindowInQuadrant(window.Handle, quadrant, monitorIndex);
        }

        /// <summary>
        /// Move a window to a desktop and position it in a quadrant.
        /// </summary>
        /// <remarks>
        /// We do NOT switch the active desktop before calling SetWindowPos.
        /// Virtual desktops are a Windows Shell concept; the underlying Win32
        /// SetWindowPos call works on any window regardless of which virtual
        /// desktop it currently lives on.  Switching desktops back and forth
        /// for every window was the cause of the screen flashing visible when
        /// repositioning many windows at once.
        /// </remarks>
        public bool MoveAndPositionWindow(IntPtr hwnd, int desktopNumber, Quadrant quadrant, int monitorIndex = 0)
        {
            if (hwnd == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager] MoveAndPositionWindow: hwnd is Zero, aborting.");
                return false;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[WindowManager] MoveAndPositionWindow: hwnd={hwnd}, desktop={desktopNumber}, quadrant={quadrant}, monitor={monitorIndex}.");

            try
            {
                // Move the window to the target virtual desktop.
                if (!MoveWindowToDesktop(hwnd, desktopNumber))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowManager] MoveAndPositionWindow: MoveWindowToDesktop failed for hwnd={hwnd} → aborting position step.");
                    return false;
                }

                if (quadrant == Quadrant.None)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[WindowManager] MoveAndPositionWindow: quadrant=None, skipping position step.");
                    return true;
                }

                // Position the window directly — SetWindowPos works cross-desktop at the
                // Win32 level so there is no need to switch the active desktop first.
                return PositionWindowInQuadrant(hwnd, quadrant, monitorIndex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[WindowManager] EXCEPTION in MoveAndPositionWindow hwnd={hwnd}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Move and position a window by WindowInfo
        /// </summary>
        public bool MoveAndPositionWindow(WindowInfo window, int desktopNumber, Quadrant quadrant, int monitorIndex = 0)
        {
            return MoveAndPositionWindow(window.Handle, desktopNumber, quadrant, monitorIndex);
        }

        /// <summary>
        /// Poll until the current desktop matches <paramref name="expectedDesktop"/> or
        /// <see cref="DesktopSwitchTimeoutMs"/> elapses.  Replaces fixed Thread.Sleep calls
        /// that were both racy (too short on slow machines) and wasteful (too long on fast ones).
        /// </summary>
        private void WaitForDesktopSwitch(int expectedDesktop)
        {
            const int pollIntervalMs = 30;
            int elapsed = 0;
            while (elapsed < DesktopSwitchTimeoutMs)
            {
                try
                {
                    if (VirtualDesktopAPI.GetCurrentDesktopNumber() == expectedDesktop)
                        return;
                }
                catch
                {
                    // Ignore transient API errors during polling
                }
                System.Threading.Thread.Sleep(pollIntervalMs);
                elapsed += pollIntervalMs;
            }
        }

        /// <summary>
        /// Pin a window to all virtual desktops.
        /// </summary>
        /// <param name="hwnd">Handle to the window to pin</param>
        /// <returns>True if the operation succeeded, false otherwise</returns>
        public bool PinWindow(IntPtr hwnd)
        {
            if (!VirtualDesktopAPI.IsAvailable())
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager] PinWindow: VirtualDesktopAPI not available.");
                return false;
            }

            if (hwnd == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager] PinWindow: hwnd is Zero.");
                return false;
            }

            try
            {
                int result = VirtualDesktopAPI.PinWindow(hwnd);
                if (result == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[WindowManager] Pinned hwnd={hwnd} to all desktops.");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[WindowManager] PinWindow failed for hwnd={hwnd}, result={result}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager] EXCEPTION in PinWindow hwnd={hwnd}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a window is pinned to all virtual desktops.
        /// </summary>
        /// <param name="hwnd">Handle to the window to check</param>
        /// <returns>True if the window is pinned, false otherwise</returns>
        public bool IsWindowPinned(IntPtr hwnd)
        {
            if (!VirtualDesktopAPI.IsAvailable())
            {
                return false;
            }

            if (hwnd == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                int result = VirtualDesktopAPI.IsPinnedWindow(hwnd);
                return result != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
