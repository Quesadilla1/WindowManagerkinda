using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VDManager.Services;

namespace VDManager.Models
{
    /// <summary>
    /// A named collection of app launch entries that can be triggered together.
    /// </summary>
    public class LaunchProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name for the profile, e.g. "Work Setup"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The app entries that belong to this profile
        /// </summary>
        public List<AppLaunchEntry> Entries { get; set; } = new List<AppLaunchEntry>();

        /// <summary>
        /// Whether to auto-launch this profile when VDManager starts.
        /// The individual entries' DelaySeconds values control their timing.
        /// </summary>
        public bool LaunchOnStartup { get; set; } = false;

        /// <summary>
        /// Hotkey modifier keys (combination of HotkeyManager.MOD_* constants).
        /// 0 = no hotkey.
        /// </summary>
        public uint HotkeyModifiers { get; set; } = 0;

        /// <summary>
        /// Hotkey key code. Keys.None = no hotkey.
        /// </summary>
        public Keys HotkeyKey { get; set; } = Keys.None;

        /// <summary>
        /// Runtime-only: the registered hotkey ID returned by HotkeyManager.RegisterHotkey().
        /// Not serialized.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public int RegisteredHotkeyId { get; set; } = -1;

        /// <summary>
        /// Gets a friendly display string for the assigned hotkey.
        /// </summary>
        public string GetHotkeyDisplayString()
        {
            if (HotkeyKey == Keys.None)
                return "None";

            string modName = HotkeyManager.GetModifierName(HotkeyModifiers);
            return string.IsNullOrEmpty(modName) ? HotkeyKey.ToString() : $"{modName}+{HotkeyKey}";
        }

        public override string ToString() => Name;
    }
}
