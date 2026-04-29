using System;
using System.Windows.Forms;
using VDManager.Services;

namespace VDManager.Models
{
    /// <summary>
    /// Represents a hotkey configuration
    /// </summary>
    public class HotkeyConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Friendly name for the hotkey action
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Modifier keys (combination of MOD_ALT, MOD_CONTROL, MOD_SHIFT, MOD_WIN)
        /// </summary>
        public uint Modifiers { get; set; }

        /// <summary>
        /// The key code
        /// </summary>
        public Keys Key { get; set; }

        /// <summary>
        /// Action type (e.g., "SwitchDesktop", "MoveActiveWindow", "ApplyRule")
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Additional parameters for the action (JSON serialized)
        /// </summary>
        public string? ActionParameters { get; set; }

        /// <summary>
        /// Whether this hotkey is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Get a friendly display string for the hotkey
        /// </summary>
        public string GetDisplayString()
        {
            var modifierName = HotkeyManager.GetModifierName(Modifiers);
            return string.IsNullOrEmpty(modifierName)
                ? Key.ToString()
                : $"{modifierName}+{Key}";
        }

        public override string ToString()
        {
            return $"{Name}: {GetDisplayString()}";
        }
    }

    /// <summary>
    /// Hotkey action types
    /// </summary>
    public static class HotkeyActionTypes
    {
        public const string SwitchDesktop = "SwitchDesktop";
        public const string MoveActiveWindow = "MoveActiveWindow";
        public const string SwitchToPreviousDesktop = "SwitchToPreviousDesktop";
        public const string SwitchToNextDesktop = "SwitchToNextDesktop";
        public const string ApplyRuleToActiveWindow = "ApplyRuleToActiveWindow";
        public const string ApplyAllRules = "ApplyAllRules";
        public const string ShowManager = "ShowManager";
        public const string PinActiveWindow = "PinActiveWindow";
        public const string UnpinActiveWindow = "UnpinActiveWindow";
        public const string CreateNewDesktop = "CreateNewDesktop";
        public const string RemoveCurrentDesktop = "RemoveCurrentDesktop";
        public const string LaunchProfile = "LaunchProfile";
    }
}
