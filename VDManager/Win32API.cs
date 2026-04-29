using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VDManager
{
    /// <summary>
    /// P/Invoke wrappers for Win32 window management APIs
    /// </summary>
    public static class Win32API
    {
        #region Window Management

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool AllowSetForegroundWindow(uint dwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Returns true if the specified handle identifies an existing window.
        /// Unlike checking a PID, this remains reliable even when a broker-launched app
        /// (e.g. Chrome, Edge, Spotify) exits its initial process and spawns a new one,
        /// because the HWND persists as long as the visible window exists.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        #endregion

        #region DPI APIs

        /// <summary>
        /// Gets the DPI of the specified monitor.
        /// Available on Windows 8.1+.
        /// </summary>
        [DllImport("shcore.dll", SetLastError = false)]
        public static extern int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType,
            out uint dpiX, out uint dpiY);

        /// <summary>
        /// Gets the DPI for the monitor that contains the given window.
        /// Available on Windows 10 1607+.
        /// </summary>
        [DllImport("user32.dll", SetLastError = false)]
        public static extern uint GetDpiForWindow(IntPtr hWnd);

        /// <summary>
        /// Returns the handle of the monitor nearest to a window.
        /// </summary>
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        /// <summary>
        /// Returns the handle of the monitor nearest to a point.
        /// </summary>
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        // MonitorFromWindow / MonitorFromPoint flags
        public const uint MONITOR_DEFAULTTONULL    = 0x00000000;
        public const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        /// <summary>
        /// Type of DPI to request from GetDpiForMonitor.
        /// </summary>
        public enum MonitorDpiType
        {
            EffectiveDpi = 0,   // DPI that affects layout (respects accessibility scaling)
            AngularDpi  = 1,    // Raw angular DPI of the monitor
            RawDpi       = 2    // Raw physical DPI
        }

        /// <summary>
        /// Helper: return the effective DPI for a given monitor handle.
        /// Falls back to 96 if the call fails.
        /// </summary>
        public static uint GetEffectiveDpiForMonitor(IntPtr hMonitor)
        {
            if (GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out uint dpiX, out _) == 0)
                return dpiX;
            return 96; // S_OK = 0; non-zero means failure
        }

        /// <summary>
        /// Helper: return the effective DPI scaling factor (1.0 = 100%, 1.5 = 150%, …)
        /// for the monitor that currently contains the given window handle.
        /// Falls back to 1.0 if DPI cannot be determined.
        /// </summary>
        public static float GetScalingFactorForWindow(IntPtr hWnd)
        {
            try
            {
                uint dpi = GetDpiForWindow(hWnd);
                if (dpi == 0) dpi = 96;
                return dpi / 96f;
            }
            catch
            {
                return 1.0f;
            }
        }

        /// <summary>
        /// Helper: return the DPI scaling factor for a Screen (by matching monitor handle).
        /// </summary>
        public static float GetScalingFactorForScreen(System.Windows.Forms.Screen screen)
        {
            try
            {
                // Use the centre point of the screen to locate the monitor
                var centre = new POINT
                {
                    X = screen.Bounds.Left + screen.Bounds.Width / 2,
                    Y = screen.Bounds.Top  + screen.Bounds.Height / 2
                };
                IntPtr hMonitor = MonitorFromPoint(centre, MONITOR_DEFAULTTONEAREST);
                uint dpi = GetEffectiveDpiForMonitor(hMonitor);
                return dpi / 96f;
            }
            catch
            {
                return 1.0f;
            }
        }

        #endregion

        #region Constants

        // SetWindowPos flags
        public const uint SWP_NOSIZE      = 0x0001;
        public const uint SWP_NOMOVE      = 0x0002;
        public const uint SWP_NOZORDER    = 0x0004;
        public const uint SWP_NOACTIVATE  = 0x0010;
        public const uint SWP_SHOWWINDOW  = 0x0040;
        public const uint SWP_FRAMECHANGED = 0x0020;

        // ShowWindow commands
        public const int SW_HIDE          = 0;
        public const int SW_SHOWNORMAL    = 1;
        public const int SW_NORMAL        = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE      = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW          = 5;
        public const int SW_MINIMIZE      = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA        = 8;
        public const int SW_RESTORE       = 9;

        // GetWindow constants
        public const uint GW_OWNER = 4;

        // AllowSetForegroundWindow — pass to grant any process foreground rights
        public const uint ASFW_ANY = 0xFFFFFFFF;

        // GetWindowLong constants
        public const int GWL_STYLE   = -16;
        public const int GWL_EXSTYLE = -20;

        // Window styles
        public const long WS_VISIBLE       = 0x10000000L;
        public const long WS_BORDER        = 0x00800000L;
        public const long WS_CAPTION       = 0x00C00000L;
        public const long WS_EX_TOOLWINDOW = 0x00000080L;
        public const long WS_EX_APPWINDOW  = 0x00040000L;

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width  => Right  - Left;
            public int Height => Bottom - Top;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        #region DWM (Desktop Window Manager) APIs

        /// <summary>
        /// Retrieves the value of a Desktop Window Manager (DWM) attribute for a window.
        /// Used to detect the invisible extended frame / shadow padding that modern Windows
        /// apps carry around their visible border.
        /// </summary>
        [DllImport("dwmapi.dll", SetLastError = false)]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, uint dwAttribute,
            out RECT pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttribute,
            ref int pvAttribute, int cbAttribute);

        // DWMWINDOWATTRIBUTE values
        private const uint DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        private const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        /// <summary>
        /// Sets the title bar to dark or light mode (Windows 10 1809+ / Windows 11).
        /// Silently does nothing on older OS versions.
        /// </summary>
        public static void SetTitleBarDarkMode(IntPtr hwnd, bool dark)
        {
            int value = dark ? 1 : 0;
            // Return value intentionally ignored — non-zero (failure) is expected on
            // Windows versions that don't support immersive dark mode (pre-1809).
            _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        }

        /// <summary>
        /// Returns the invisible frame (shadow) padding for each side of <paramref name="hwnd"/>
        /// as a <see cref="RECT"/> whose fields represent the inset in pixels:
        /// <list type="bullet">
        ///   <item><description>Left   – pixels the visible left edge is inset from the window RECT left</description></item>
        ///   <item><description>Top    – pixels the visible top edge is inset from the window RECT top</description></item>
        ///   <item><description>Right  – pixels the visible right edge is inset from the window RECT right</description></item>
        ///   <item><description>Bottom – pixels the visible bottom edge is inset from the window RECT bottom</description></item>
        /// </list>
        /// Returns a zeroed RECT if DwmGetWindowAttribute fails (e.g. for non-DWM windows).
        /// </summary>
        public static RECT GetWindowFramePadding(IntPtr hwnd)
        {
            // GetWindowRect returns the full rect including invisible shadow/frame
            if (!GetWindowRect(hwnd, out RECT windowRect))
                return default;

            // DWMWA_EXTENDED_FRAME_BOUNDS returns the rect of the *visible* window border
            int hr = DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS,
                out RECT visibleRect, Marshal.SizeOf<RECT>());

            if (hr != 0) // S_OK == 0
                return default;

            // Compute how far each side of the window rect extends beyond the visible frame.
            // These values are typically 8px left/right/bottom and 0–1px top on Windows 11.
            return new RECT
            {
                Left   = visibleRect.Left   - windowRect.Left,
                Top    = visibleRect.Top    - windowRect.Top,
                Right  = windowRect.Right   - visibleRect.Right,
                Bottom = windowRect.Bottom  - visibleRect.Bottom
            };
        }

        #endregion

        #region Power Throttling

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessInformation(
            IntPtr hProcess,
            int ProcessInformationClass,
            ref PROCESS_POWER_THROTTLING_STATE ProcessInformation,
            uint ProcessInformationSize);

        // ProcessInformationClass value for power throttling
        private const int ProcessPowerThrottling = 4;

        // Control flag: execution speed throttling
        private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_POWER_THROTTLING_STATE
        {
            public uint Version;        // must be 1
            public uint ControlMask;    // which fields are being changed
            public uint StateMask;      // desired state for those fields (0 = off)
        }

        /// <summary>
        /// Opts the current process out of Windows Power Throttling (Efficiency Mode).
        /// When a WinForms app is hidden in the system tray, Windows may throttle its
        /// message pump, causing RegisterHotKey WM_HOTKEY messages to stop arriving.
        /// Calling this at startup prevents that behaviour.
        /// </summary>
        public static void DisablePowerThrottling()
        {
            try
            {
                var state = new PROCESS_POWER_THROTTLING_STATE
                {
                    Version     = 1,
                    ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                    StateMask   = 0   // 0 = disable throttling for the controlled bits
                };

                SetProcessInformation(
                    GetCurrentProcess(),
                    ProcessPowerThrottling,
                    ref state,
                    (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>());
            }
            catch
            {
                // Not critical — older Windows versions don't support this API.
                // Silently ignore so startup is never blocked.
            }
        }

        #endregion

        #region Delegates

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the title text of a window
        /// </summary>
        public static string GetWindowTitle(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Check if a window is a valid application window
        /// </summary>
        public static bool IsApplicationWindow(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd))
                return false;

            // Check if window has an owner
            if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
                return false;

            // Check extended style for tool windows
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            if ((exStyle & WS_EX_TOOLWINDOW) != 0)
                return false;

            return true;
        }

        #endregion
    }
}
