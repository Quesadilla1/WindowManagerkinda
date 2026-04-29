using System;

namespace VDManager
{
    /// <summary>
    /// Represents information about a window
    /// </summary>
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public int DesktopNumber { get; set; }
        public bool IsVisible { get; set; }
        public bool IsMinimized { get; set; }

        public override string ToString()
        {
            string displayTitle = string.IsNullOrWhiteSpace(Title) ? "[No Title]" : Title;
            return $"{ProcessName} - {displayTitle} (Desktop {DesktopNumber + 1})";
        }

        /// <summary>
        /// Get a short display name for UI lists
        /// </summary>
        public string GetDisplayName()
        {
            string displayTitle = string.IsNullOrWhiteSpace(Title) ? "[No Title]" : Title;
            return $"{ProcessName} - {displayTitle}";
        }
    }
}
