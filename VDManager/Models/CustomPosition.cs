using System;
using System.Drawing;

namespace VDManager.Models
{
    /// <summary>
    /// Represents a custom saved window position
    /// </summary>
    public class CustomPosition
    {
        public string Name { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MonitorIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Get as Rectangle
        /// </summary>
        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        /// <summary>
        /// Create from Rectangle
        /// </summary>
        public static CustomPosition FromRectangle(string name, Rectangle rect, int monitorIndex)
        {
            return new CustomPosition
            {
                Name = name,
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height,
                MonitorIndex = monitorIndex,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Create from current window position
        /// </summary>
        public static CustomPosition FromWindow(string name, IntPtr hwnd)
        {
            if (Win32API.GetWindowRect(hwnd, out Win32API.RECT rect))
            {
                // Find which monitor this window is on
                var screens = System.Windows.Forms.Screen.AllScreens;
                int monitorIndex = 0;

                for (int i = 0; i < screens.Length; i++)
                {
                    var bounds = screens[i].Bounds;
                    if (rect.Left >= bounds.Left && rect.Left < bounds.Right &&
                        rect.Top >= bounds.Top && rect.Top < bounds.Bottom)
                    {
                        monitorIndex = i;
                        break;
                    }
                }

                return new CustomPosition
                {
                    Name = name,
                    X = rect.Left,
                    Y = rect.Top,
                    Width = rect.Right - rect.Left,
                    Height = rect.Bottom - rect.Top,
                    MonitorIndex = monitorIndex,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                };
            }

            throw new InvalidOperationException("Could not get window position");
        }
    }
}
