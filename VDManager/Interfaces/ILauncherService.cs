using System;
using System.Collections.Generic;
using VDManager.Models;

namespace VDManager.Services
{
    public interface ILauncherService : IDisposable
    {
        bool LaunchEntry(AppLaunchEntry entry);
        bool IsEntryRunning(AppLaunchEntry entry);
        int LaunchProfile(LaunchProfile profile);
        void AutoLaunchStartupProfiles(IEnumerable<LaunchProfile> profiles);
        void RegisterProfileHotkeys(IEnumerable<LaunchProfile> profiles, HotkeyManager hotkeyManager);
        void UnregisterProfileHotkeys(IEnumerable<LaunchProfile> profiles, HotkeyManager hotkeyManager);
    }
}
