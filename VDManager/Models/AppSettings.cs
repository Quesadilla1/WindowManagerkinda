using System;
using System.Collections.Generic;

namespace VDManager.Models
{
    /// <summary>
    /// Application settings that persist across sessions
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Enable auto-apply rules on application startup
        /// </summary>
        public bool AutoApplyOnLaunch { get; set; } = true;

        /// <summary>
        /// Auto-apply monitoring interval in milliseconds
        /// </summary>
        public int MonitoringInterval { get; set; } = 2000;

        /// <summary>
        /// Minimize to system tray instead of taskbar
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// Close to system tray instead of exiting (default: on)
        /// </summary>
        public bool CloseToTray { get; set; } = true;

        /// <summary>
        /// Show balloon tips for notifications
        /// </summary>
        public bool ShowBalloonTips { get; set; } = true;

        /// <summary>
        /// Theme preference: 0=Light, 1=Dark, 2=System
        /// </summary>
        public int ThemePreference { get; set; } = 2; // Default to System

        /// <summary>
        /// Start with Windows on login
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// Application font name (default: Segoe UI)
        /// </summary>
        public string FontName { get; set; } = "Segoe UI";

        /// <summary>
        /// How long (in milliseconds) to wait for a virtual desktop switch to complete
        /// before giving up and proceeding.  Increase on slow or heavily loaded machines.
        /// Range: 200–5000 ms. Default: 800 ms.
        /// </summary>
        public int DesktopSwitchTimeoutMs { get; set; } = 800;

        /// <summary>
        /// Launch profiles — groups of apps that can be triggered together.
        /// Persisted as part of settings.json.
        /// </summary>
        public List<LaunchProfile> LaunchProfiles { get; set; } = new List<LaunchProfile>();

        // ── Position Enforcement ─────────────────────────────────────────────

        /// <summary>
        /// When true, windows matched by rules that have EnforcePosition=true will be
        /// snapped back to their rule-defined desktop/position if they are moved.
        /// </summary>
        public bool PositionEnforcementEnabled { get; set; } = true;

        /// <summary>
        /// How long (ms) to wait after initial placement before starting enforcement.
        /// Allows apps to finish repositioning themselves during startup.
        /// Range: 500–30000 ms. Default: 3000 ms.
        /// </summary>
        public int EnforcementGracePeriodMs { get; set; } = 3000;

        /// <summary>
        /// Minimum time (ms) between consecutive snap-backs for the same window.
        /// Prevents thrashing while the user is actively dragging a window.
        /// Range: 200–10000 ms. Default: 1000 ms.
        /// </summary>
        public int EnforcementCooldownMs { get; set; } = 1000;

        /// <summary>
        /// When true, skip position enforcement for minimized windows.
        /// The window will be enforced once it is restored.
        /// </summary>
        public bool SkipEnforcementWhenMinimized { get; set; } = true;

        // ── Window Detection Timing ──────────────────────────────────────────

        /// <summary>
        /// How long (ms) to wait after a new window is detected before applying rules.
        /// Increase for slow-launching apps (Electron, browsers, UWP, etc.) that take
        /// longer to finish initialising their window title/class.
        /// Range: 50–5000 ms. Default: 300 ms.
        /// </summary>
    public int NewWindowRuleDelayMs { get; set; } = 500;
    }
}
