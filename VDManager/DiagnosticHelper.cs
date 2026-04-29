using System;
using System.Text;

namespace VDManager
{
    /// <summary>
    /// Helper class for diagnostics and debugging
    /// </summary>
    public static class DiagnosticHelper
    {
        /// <summary>
        /// Get diagnostic information about a window
        /// </summary>
        public static string GetWindowDiagnostics(WindowInfo window)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Process: {window.ProcessName}");
            sb.AppendLine($"Title: {window.Title}");
            sb.AppendLine($"Handle: {window.Handle} (0x{window.Handle:X})");
            sb.AppendLine($"Process ID: {window.ProcessId}");
            sb.AppendLine($"Desktop: {window.DesktopNumber}");
            sb.AppendLine($"Visible: {window.IsVisible}");
            sb.AppendLine($"Minimized: {window.IsMinimized}");

            // Check if window still exists
            bool stillVisible = Win32API.IsWindowVisible(window.Handle);
            sb.AppendLine($"Still Visible: {stillVisible}");

            // Get window rect
            if (Win32API.GetWindowRect(window.Handle, out Win32API.RECT rect))
            {
                sb.AppendLine($"Position: ({rect.Left}, {rect.Top})");
                sb.AppendLine($"Size: {rect.Width} x {rect.Height}");
            }
            else
            {
                sb.AppendLine("Could not get window position");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get diagnostic information about the VirtualDesktop API
        /// </summary>
        public static string GetVirtualDesktopDiagnostics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("VirtualDesktop API Diagnostics:");
            sb.AppendLine("--------------------------------");

            if (!VirtualDesktopAPI.IsAvailable())
            {
                sb.AppendLine("❌ API NOT AVAILABLE");
                sb.AppendLine(VirtualDesktopAPI.GetLoadError());
                return sb.ToString();
            }

            sb.AppendLine("✓ API Available");

            try
            {
                int desktopCount = VirtualDesktopAPI.GetDesktopCount();
                sb.AppendLine($"✓ Desktop Count: {desktopCount}");

                int currentDesktop = VirtualDesktopAPI.GetCurrentDesktopNumber();
                sb.AppendLine($"✓ Current Desktop: {currentDesktop} (Desktop {currentDesktop + 1})");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error calling API: {ex.Message}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Test moving a window
        /// </summary>
        public static string TestMoveWindow(IntPtr hwnd, int targetDesktop)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Testing move of window {hwnd} to desktop {targetDesktop}");

            // Check window is valid
            if (hwnd == IntPtr.Zero)
            {
                sb.AppendLine("❌ Invalid window handle (Zero)");
                return sb.ToString();
            }

            if (!Win32API.IsWindowVisible(hwnd))
            {
                sb.AppendLine("⚠ Window is not visible");
            }

            // Get current desktop
            try
            {
                int currentDesktop = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd);
                sb.AppendLine($"Window is currently on desktop: {currentDesktop}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Could not get window desktop: {ex.Message}");
            }

            // Try to move
            try
            {
                int result = VirtualDesktopAPI.MoveWindowToDesktopNumber(hwnd, targetDesktop);
                sb.AppendLine($"Move result: {result}");

                // Verify new desktop
                int newDesktop = VirtualDesktopAPI.GetWindowDesktopNumber(hwnd);
                sb.AppendLine($"Window is now on desktop: {newDesktop}");

                if (newDesktop == targetDesktop)
                {
                    sb.AppendLine("✓ Move succeeded");
                }
                else
                {
                    sb.AppendLine($"⚠ Move may have failed (expected {targetDesktop}, got {newDesktop})");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Move failed: {ex.Message}");
            }

            return sb.ToString();
        }
    }
}
