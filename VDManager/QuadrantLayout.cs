using System;
using System.Drawing;
using System.Windows.Forms;

namespace VDManager
{
    /// <summary>
    /// Defines quadrant positions for window arrangement
    /// </summary>
    public enum Quadrant
    {
        None = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4,
        LeftHalf = 5,         // Left 50% (full height)
        RightHalf = 6,        // Right 50% (full height)
        TopHalf = 7,          // Top 50% (full width)
        BottomHalf = 8,       // Bottom 50% (full width)
        Maximized = 9,        // Maximized window
        LeftThird = 10,       // Left 33% (full height)
        CenterThird = 11,     // Center 33% (full height)
        RightThird = 12,      // Right 33% (full height)
        LeftTwoThirds = 13,   // Left 66% (full height)
        RightTwoThirds = 14,  // Right 66% (full height)
        CenterHalf = 15,      // Center 50% (full height)
        // Quarter-width layouts (25% wide, full height)
        LeftQuarter = 16,         // Left 25%
        CenterLeftQuarter = 17,   // Center-left 25% (25–50%)
        CenterRightQuarter = 18,  // Center-right 25% (50–75%)
        RightQuarter = 19,        // Right 25%
        LeftThreeQuarters = 20,   // Left 75%
        RightThreeQuarters = 21,  // Right 75% (starting at 25%)
        Custom = 99           // Custom saved position
    }

    /// <summary>
    /// Manages quadrant layout calculations for window positioning
    /// </summary>
    public class QuadrantLayout
    {
        // Cache for Screen.AllScreens - expensive Win32 EnumDisplayMonitors call
        private static Screen[]? _cachedScreens;
        private static DateTime _cacheTimestamp;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
        private static readonly object _cacheLock = new object();

        private readonly Rectangle workingArea;

        /// <summary>
        /// Get all screens with caching to avoid expensive Win32 calls.
        /// Cache expires after 5 seconds to handle display changes.
        /// </summary>
        private static Screen[] GetCachedScreens()
        {
            lock (_cacheLock)
            {
                if (_cachedScreens == null || DateTime.Now - _cacheTimestamp > CacheDuration)
                {
                    _cachedScreens = Screen.AllScreens;
                    _cacheTimestamp = DateTime.Now;
                }
                return _cachedScreens;
            }
        }

        /// <summary>
        /// Invalidate the screen cache (call if display settings change).
        /// </summary>
        public static void InvalidateScreenCache()
        {
            lock (_cacheLock)
            {
                _cachedScreens = null;
            }
        }

        public QuadrantLayout(Screen screen)
        {
            // Use WorkingArea to exclude taskbar
            workingArea = screen.WorkingArea;
        }

        public QuadrantLayout(Rectangle bounds)
        {
            workingArea = bounds;
        }

        /// <summary>
        /// Get the default primary monitor layout.
        /// Falls back to first available screen if PrimaryScreen is null (e.g., server environments).
        /// Throws if no screens are available.
        /// </summary>
        public static QuadrantLayout Primary
        {
            get
            {
                // Try primary screen first
                var primary = Screen.PrimaryScreen;
                if (primary != null)
                    return new QuadrantLayout(primary);

                // Fall back to first available screen
                var screens = Screen.AllScreens;
                if (screens.Length > 0)
                    return new QuadrantLayout(screens[0]);

                // No screens available - this should not happen on a normal desktop
                throw new InvalidOperationException("No displays available. Cannot create primary quadrant layout.");
            }
        }

        /// <summary>
        /// Calculate the rectangle for a specific quadrant
        /// </summary>
        public Rectangle GetQuadrantBounds(Quadrant quadrant)
        {
            int halfWidth = workingArea.Width / 2;
            int halfHeight = workingArea.Height / 2;
            // Use complements for the "far" half so any remainder pixel is absorbed
            // rather than leaving a 1px gap on odd-width/height monitors.
            int halfWidthFar  = workingArea.Width  - halfWidth;
            int halfHeightFar = workingArea.Height - halfHeight;
            int thirdWidth = workingArea.Width / 3;
            int twoThirdsWidth = (workingArea.Width * 2) / 3;
            int quarterWidth = workingArea.Width / 4;

            return quadrant switch
            {
                // Quarter quadrants
                Quadrant.TopLeft => new Rectangle(
                    workingArea.Left,
                    workingArea.Top,
                    halfWidth,
                    halfHeight),

                Quadrant.TopRight => new Rectangle(
                    workingArea.Left + halfWidth,
                    workingArea.Top,
                    halfWidthFar,
                    halfHeight),

                Quadrant.BottomLeft => new Rectangle(
                    workingArea.Left,
                    workingArea.Top + halfHeight,
                    halfWidth,
                    halfHeightFar),

                Quadrant.BottomRight => new Rectangle(
                    workingArea.Left + halfWidth,
                    workingArea.Top + halfHeight,
                    halfWidthFar,
                    halfHeightFar),

                // Half layouts
                Quadrant.LeftHalf => new Rectangle(
                    workingArea.Left,
                    workingArea.Top,
                    halfWidth,
                    workingArea.Height),

                Quadrant.RightHalf => new Rectangle(
                    workingArea.Left + halfWidth,
                    workingArea.Top,
                    halfWidthFar,
                    workingArea.Height),

                Quadrant.TopHalf => new Rectangle(
                    workingArea.Left,
                    workingArea.Top,
                    workingArea.Width,
                    halfHeight),

                Quadrant.BottomHalf => new Rectangle(
                    workingArea.Left,
                    workingArea.Top + halfHeight,
                    workingArea.Width,
                    halfHeightFar),

                // Third layouts
                Quadrant.LeftThird => new Rectangle(
                    workingArea.Left,
                    workingArea.Top,
                    thirdWidth,
                    workingArea.Height),

                Quadrant.CenterThird => new Rectangle(
                    workingArea.Left + thirdWidth,
                    workingArea.Top,
                    thirdWidth,
                    workingArea.Height),

                Quadrant.RightThird => new Rectangle(
                    workingArea.Left + twoThirdsWidth,
                    workingArea.Top,
                    thirdWidth,
                    workingArea.Height),

                // Two-thirds layouts
                Quadrant.LeftTwoThirds => new Rectangle(
                    workingArea.Left,
                    workingArea.Top,
                    twoThirdsWidth,
                    workingArea.Height),

                Quadrant.RightTwoThirds => new Rectangle(
                    workingArea.Left + thirdWidth,
                    workingArea.Top,
                    twoThirdsWidth,
                    workingArea.Height),

                // Center layouts
                Quadrant.CenterHalf => new Rectangle(
                    workingArea.Left + quarterWidth,
                    workingArea.Top,
                    halfWidth,
                    workingArea.Height),

                // Quarter-width layouts (25% wide, full height)
                Quadrant.LeftQuarter => new Rectangle(
                    workingArea.Left,
                    workingArea.Top,
                    quarterWidth,
                    workingArea.Height),

                Quadrant.CenterLeftQuarter => new Rectangle(
                    workingArea.Left + quarterWidth,
                    workingArea.Top,
                    quarterWidth,
                    workingArea.Height),

                Quadrant.CenterRightQuarter => new Rectangle(
                    workingArea.Left + quarterWidth * 2,
                    workingArea.Top,
                    quarterWidth,
                    workingArea.Height),

                Quadrant.RightQuarter => new Rectangle(
                    workingArea.Left + quarterWidth * 3,
                    workingArea.Top,
                    quarterWidth,
                    workingArea.Height),

                // Three-quarter-width layouts (75% wide, full height)
                Quadrant.LeftThreeQuarters => new Rectangle(
                    workingArea.Left,
                    workingArea.Top,
                    quarterWidth * 3,
                    workingArea.Height),

                Quadrant.RightThreeQuarters => new Rectangle(
                    workingArea.Left + quarterWidth,
                    workingArea.Top,
                    quarterWidth * 3,
                    workingArea.Height),

                // Maximized is handled specially by PositionWindow (via ShowWindow SW_MAXIMIZE),
                // but if GetQuadrantBounds is called directly with Maximized we return the full
                // working area, which is the correct logical extent.
                Quadrant.Maximized => workingArea,

                _ => workingArea // None, Custom, or unrecognised value
            };
        }

        /// <summary>
        /// Get all quadrant names for UI display
        /// </summary>
        public static string[] GetQuadrantNames()
        {
            return new[]
            {
                "None (No positioning)",       // 0
                "Top Left (Quarter)",           // 1
                "Top Right (Quarter)",          // 2
                "Bottom Left (Quarter)",        // 3
                "Bottom Right (Quarter)",       // 4
                "Left Half (50%)",              // 5
                "Right Half (50%)",             // 6
                "Top Half (50%)",               // 7
                "Bottom Half (50%)",            // 8
                "Maximized",                    // 9
                "Left Third (33%)",             // 10
                "Center Third (33%)",           // 11
                "Right Third (33%)",            // 12
                "Left Two-Thirds (66%)",        // 13
                "Right Two-Thirds (66%)",       // 14
                "Center Half (50%)",            // 15
                "Left Quarter (25%)",           // 16
                "Center-Left Quarter (25%)",    // 17
                "Center-Right Quarter (25%)",   // 18
                "Right Quarter (25%)",          // 19
                "Left Three-Quarters (75%)",    // 20
                "Right Three-Quarters (75%)"    // 21
            };
        }

        /// <summary>
        /// Get quadrant enum from index (0-based)
        /// </summary>
        public static Quadrant GetQuadrantFromIndex(int index)
        {
            return index switch
            {
                0 => Quadrant.None,
                1 => Quadrant.TopLeft,
                2 => Quadrant.TopRight,
                3 => Quadrant.BottomLeft,
                4 => Quadrant.BottomRight,
                5 => Quadrant.LeftHalf,
                6 => Quadrant.RightHalf,
                7 => Quadrant.TopHalf,
                8 => Quadrant.BottomHalf,
                9 => Quadrant.Maximized,
                10 => Quadrant.LeftThird,
                11 => Quadrant.CenterThird,
                12 => Quadrant.RightThird,
                13 => Quadrant.LeftTwoThirds,
                14 => Quadrant.RightTwoThirds,
                15 => Quadrant.CenterHalf,
                16 => Quadrant.LeftQuarter,
                17 => Quadrant.CenterLeftQuarter,
                18 => Quadrant.CenterRightQuarter,
                19 => Quadrant.RightQuarter,
                20 => Quadrant.LeftThreeQuarters,
                21 => Quadrant.RightThreeQuarters,
                _ => Quadrant.None
            };
        }

        /// <summary>
        /// Get index from quadrant enum (0-based for UI)
        /// </summary>
        public static int GetIndexFromQuadrant(Quadrant quadrant)
        {
            return (int)quadrant;
        }

        /// <summary>
        /// Position a window in the specified quadrant
        /// </summary>
        public bool PositionWindow(IntPtr hwnd, Quadrant quadrant)
        {
            if (quadrant == Quadrant.None)
                return true; // No positioning needed

            System.Diagnostics.Debug.WriteLine(
                $"[QuadrantLayout] PositionWindow: hwnd={hwnd}, quadrant={quadrant}, workingArea={workingArea}.");

            try
            {
                // Special handling for maximized
                if (quadrant == Quadrant.Maximized)
                {
                    System.Diagnostics.Debug.WriteLine($"[QuadrantLayout] Restoring hwnd={hwnd} before maximize...");
                    Win32API.ShowWindow(hwnd, Win32API.SW_RESTORE);
                    System.Threading.Thread.Sleep(50);
                    System.Diagnostics.Debug.WriteLine($"[QuadrantLayout] Restore sleep done. Moving hwnd={hwnd} to monitor area before maximizing...");

                    // Move window to the target monitor by positioning it in the working area
                    // This ensures it's on the correct monitor before maximizing
                    Win32API.SetWindowPos(
                        hwnd,
                        IntPtr.Zero,
                        workingArea.X,
                        workingArea.Y,
                        workingArea.Width / 2,  // Use half size temporarily
                        workingArea.Height / 2,
                        Win32API.SWP_NOZORDER | Win32API.SWP_SHOWWINDOW
                    );

                    // Small delay to ensure window is on the correct monitor
                    System.Threading.Thread.Sleep(50);
                    System.Diagnostics.Debug.WriteLine($"[QuadrantLayout] Monitor-placement sleep done. Maximizing hwnd={hwnd}...");

                    // Now maximize the window (it will maximize on the monitor it's currently on)
                    Win32API.ShowWindow(hwnd, Win32API.SW_MAXIMIZE);
                    System.Diagnostics.Debug.WriteLine($"[QuadrantLayout] Maximize call complete for hwnd={hwnd}.");
                    return true;
                }

                Rectangle bounds = GetQuadrantBounds(quadrant);
                System.Diagnostics.Debug.WriteLine(
                    $"[QuadrantLayout] Target bounds for hwnd={hwnd}: {bounds.X},{bounds.Y} {bounds.Width}x{bounds.Height}.");

                // First, restore the window from minimized or maximized state
                // This is crucial - we must restore before positioning
                System.Diagnostics.Debug.WriteLine($"[QuadrantLayout] Calling SW_RESTORE for hwnd={hwnd}...");
                Win32API.ShowWindow(hwnd, Win32API.SW_RESTORE);

                // Small delay to ensure window has restored
                System.Threading.Thread.Sleep(50);
                System.Diagnostics.Debug.WriteLine($"[QuadrantLayout] Restore sleep done. Calling SetWindowPos for hwnd={hwnd}...");

                // Compensate for the invisible DWM shadow/frame padding.
                // Modern Windows apps have a transparent border (typically ~8px on left/right/bottom,
                // ~0px on top) that is included in the RECT returned by GetWindowRect but is NOT
                // visible on screen.  If we position without compensation two adjacent windows will
                // appear to have a visible gap between them equal to twice that padding.
                // We expand the target rect by the per-side padding so the *visible* edge of each
                // window lands exactly on the quadrant boundary.
                Win32API.RECT pad = Win32API.GetWindowFramePadding(hwnd);
                int adjX      = bounds.X      - pad.Left;
                int adjY      = bounds.Y      - pad.Top;
                int adjWidth  = bounds.Width  + pad.Left + pad.Right;
                int adjHeight = bounds.Height + pad.Top  + pad.Bottom;

                System.Diagnostics.Debug.WriteLine(
                    $"[QuadrantLayout] Frame padding for hwnd={hwnd}: L={pad.Left} T={pad.Top} R={pad.Right} B={pad.Bottom}. " +
                    $"Adjusted position: {adjX},{adjY} {adjWidth}x{adjHeight}.");

                // Now position and resize the window
                bool result = Win32API.SetWindowPos(
                    hwnd,
                    IntPtr.Zero,
                    adjX,
                    adjY,
                    adjWidth,
                    adjHeight,
                    Win32API.SWP_NOZORDER | Win32API.SWP_SHOWWINDOW | Win32API.SWP_FRAMECHANGED
                );

                System.Diagnostics.Debug.WriteLine(
                    $"[QuadrantLayout] SetWindowPos result={result} for hwnd={hwnd} → {adjX},{adjY} {adjWidth}x{adjHeight}.");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[QuadrantLayout] EXCEPTION in PositionWindow hwnd={hwnd}, quadrant={quadrant}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the working area dimensions
        /// </summary>
        public Rectangle WorkingArea => workingArea;

        /// <summary>
        /// Get layout for a specific monitor
        /// </summary>
        public static QuadrantLayout ForMonitor(int monitorIndex)
        {
            var screens = GetCachedScreens();
            if (monitorIndex >= 0 && monitorIndex < screens.Length)
            {
                return new QuadrantLayout(screens[monitorIndex]);
            }
            return Primary;
        }

        /// <summary>
        /// Get the number of available monitors
        /// </summary>
        public static int GetMonitorCount()
        {
            return GetCachedScreens().Length;
        }

        /// <summary>
        /// Get monitor names for UI display
        /// </summary>
        public static string[] GetMonitorNames()
        {
            var screens = GetCachedScreens();
            var names = new string[screens.Length];

            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                string primary = screen.Primary ? " (Primary)" : "";
                names[i] = $"Monitor {i + 1}{primary} - {screen.Bounds.Width}x{screen.Bounds.Height}";
            }

            return names;
        }
    }
}
