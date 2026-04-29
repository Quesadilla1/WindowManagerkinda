using System;
using System.Text.RegularExpressions;

namespace VDManager.Models
{
    /// <summary>
    /// Represents a rule for automatically positioning windows
    /// </summary>
    public class WindowRule
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Process name to match (e.g., "chrome", "notepad")
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// Window title pattern to match (optional, supports wildcards and regex if UseRegex is true)
        /// </summary>
        public string? WindowTitlePattern { get; set; }

        /// <summary>
        /// Use regex pattern matching for window title
        /// </summary>
        public bool UseRegex { get; set; } = false;

        /// <summary>
        /// Target specific instance number (0 = any, 1 = first, 2 = second, etc.)
        /// </summary>
        public int InstanceNumber { get; set; } = 0;

        /// <summary>
        /// Priority for rule evaluation (higher priority rules are checked first)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Target desktop index (0-based)
        /// </summary>
        public int DesktopIndex { get; set; }

        /// <summary>
        /// Target quadrant for positioning
        /// </summary>
        public Quadrant Quadrant { get; set; } = Quadrant.None;

        /// <summary>
        /// Monitor index for multi-monitor setups (0-based)
        /// </summary>
        public int MonitorIndex { get; set; } = 0;

        /// <summary>
        /// Whether this rule is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// When true and position enforcement is globally enabled, the window monitor
        /// will snap this window back to its rule-defined desktop/position if moved.
        /// </summary>
        public bool EnforcePosition { get; set; } = false;

        /// <summary>
        /// Description of the rule
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Check if a window matches this rule (basic matching, no instance counting)
        /// </summary>
        public bool Matches(WindowInfo window)
        {
            if (!Enabled)
                return false;

            // Check process name (case-insensitive)
            if (!window.ProcessName.Equals(ProcessName, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check window title pattern if specified
            if (!string.IsNullOrEmpty(WindowTitlePattern))
            {
                // Wildcard matching
                if (WindowTitlePattern == "*")
                    return true;

                // Regex pattern matching
                if (UseRegex)
                {
                    try
                    {
                        // Add timeout to prevent ReDoS attacks
                        var regex = new Regex(WindowTitlePattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
                        if (!regex.IsMatch(window.Title))
                            return false;
                    }
                    catch (ArgumentException)
                    {
                        // Invalid regex, fall back to contains
                        if (!window.Title.Contains(WindowTitlePattern, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        // Regex timed out (likely pathological pattern), fall back to contains
                        if (!window.Title.Contains(WindowTitlePattern, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }
                else
                {
                    // Simple contains matching (case-insensitive)
                    if (!window.Title.Contains(WindowTitlePattern, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            string quadrantText = Quadrant == Quadrant.None ? "No positioning" : $"{Quadrant}";
            string titlePart = !string.IsNullOrEmpty(WindowTitlePattern) && WindowTitlePattern != "*"
                ? $" [{WindowTitlePattern}]"
                : string.Empty;
            return $"{ProcessName}{titlePart} → Desktop {DesktopIndex + 1}, {quadrantText}";
        }
    }
}
