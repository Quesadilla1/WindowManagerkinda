using System;

namespace VDManager.Models
{
    /// <summary>
    /// Represents a single application launch entry within a launch profile.
    /// </summary>
    public class AppLaunchEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name for this entry, e.g. "Work Email"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Executable path or name, e.g. "msedge.exe" or "C:\Program Files\VS Code\Code.exe"
        /// </summary>
        public string ExecutablePath { get; set; } = string.Empty;

        /// <summary>
        /// Command-line arguments, e.g. "--new-window https://github.com"
        /// </summary>
        public string Arguments { get; set; } = string.Empty;

        /// <summary>
        /// Optional working directory for the process
        /// </summary>
        public string WorkingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Delay in seconds after profile launch before this entry fires.
        /// 0 = launch immediately when profile is triggered.
        /// </summary>
        public int DelaySeconds { get; set; } = 0;

        /// <summary>
        /// Optional virtual desktop index (0-based) to switch to after launching.
        /// -1 = do not switch.
        /// </summary>
        public int TargetDesktopIndex { get; set; } = -1;

        /// <summary>
        /// Sort order within the profile (lower numbers launch first).
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// The ID of the WindowRule that corresponds to this launch entry.
        /// The launcher uses the rule's window claim (tracked by WindowInstanceTracker)
        /// to determine whether the app is already running, replacing the old per-entry
        /// process scan. A non-empty value is required to save a launch entry.
        /// </summary>
        public string LinkedRuleId { get; set; } = string.Empty;

        public override string ToString() =>
            string.IsNullOrEmpty(Name) ? ExecutablePath : Name;
    }
}
