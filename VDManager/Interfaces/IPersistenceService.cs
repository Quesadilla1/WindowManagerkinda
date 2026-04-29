using System.Collections.Generic;
using VDManager.Models;

namespace VDManager.Services
{
    public interface IPersistenceService
    {
        bool SaveRules(List<WindowRule> rules);
        List<WindowRule> LoadRules();

        bool SaveProfile(WorkspaceProfile profile);
        WorkspaceProfile? LoadProfile(string profileName);
        List<WorkspaceProfile> LoadAllProfiles();
        bool DeleteProfile(string profileName);

        bool SaveSettings(AppSettings settings);
        AppSettings LoadSettings();

        bool SaveCustomPositions(List<CustomPosition> positions);
        List<CustomPosition> LoadCustomPositions();

        bool SaveHotkeys(List<HotkeyConfig> hotkeys);
        List<HotkeyConfig> LoadHotkeys();

        string GetAppDataPath();
        string GetProfilesDirectory();
    }
}
