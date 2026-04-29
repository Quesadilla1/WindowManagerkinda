using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using VDManager.Models;

namespace VDManager.Services
{
    /// <summary>
    /// Service for persisting rules and profiles to disk
    /// </summary>
    public class PersistenceService : IPersistenceService
    {
        private readonly string appDataPath;
        private readonly string rulesFilePath;
        private readonly string profilesDirectory;
        private readonly string settingsFilePath;
        private readonly string customPositionsFilePath;
        private readonly string hotkeysFilePath;

        public PersistenceService()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DeskBulldozer"))
        {
        }

        /// <summary>
        /// Constructor for testing - allows specifying custom base path
        /// </summary>
        public PersistenceService(string basePath)
        {
            appDataPath = basePath;
            rulesFilePath = Path.Combine(appDataPath, "rules.json");
            profilesDirectory = Path.Combine(appDataPath, "Profiles");
            settingsFilePath = Path.Combine(appDataPath, "settings.json");
            customPositionsFilePath = Path.Combine(appDataPath, "custom_positions.json");
            hotkeysFilePath = Path.Combine(appDataPath, "hotkeys.json");

            // Ensure directories exist
            Directory.CreateDirectory(appDataPath);
            Directory.CreateDirectory(profilesDirectory);
        }

        #region Rules Persistence

        /// <summary>
        /// Save rules to disk
        /// </summary>
        public bool SaveRules(List<WindowRule> rules)
        {
            try
            {
                string json = JsonConvert.SerializeObject(rules, Formatting.Indented);
                File.WriteAllText(rulesFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                // Fix H-5: log instead of silently swallowing
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] SaveRules failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Load rules from disk
        /// </summary>
        public List<WindowRule> LoadRules()
        {
            try
            {
                if (!File.Exists(rulesFilePath))
                    return new List<WindowRule>();

                string json = File.ReadAllText(rulesFilePath);
                var rules = JsonConvert.DeserializeObject<List<WindowRule>>(json);
                return rules ?? new List<WindowRule>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] LoadRules failed: {ex}");
                return new List<WindowRule>();
            }
        }

        #endregion

        #region Profile Persistence

        /// <summary>
        /// Save a profile to disk
        /// </summary>
        public bool SaveProfile(WorkspaceProfile profile)
        {
            try
            {
                profile.ModifiedAt = DateTime.Now;

                string fileName = GetSafeFileName(profile.Name) + ".json";
                string filePath = Path.Combine(profilesDirectory, fileName);

                string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] SaveProfile failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Load a specific profile by name
        /// </summary>
        public WorkspaceProfile? LoadProfile(string profileName)
        {
            try
            {
                string fileName = GetSafeFileName(profileName) + ".json";
                string filePath = Path.Combine(profilesDirectory, fileName);

                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<WorkspaceProfile>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] LoadProfile '{profileName}' failed: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Load all profiles
        /// </summary>
        public List<WorkspaceProfile> LoadAllProfiles()
        {
            var profiles = new List<WorkspaceProfile>();

            try
            {
                var files = Directory.GetFiles(profilesDirectory, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var profile = JsonConvert.DeserializeObject<WorkspaceProfile>(json);
                        if (profile != null)
                            profiles.Add(profile);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[PersistenceService] Skipping corrupt profile '{file}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] LoadAllProfiles failed: {ex}");
            }

            return profiles;
        }

        /// <summary>
        /// Delete a profile
        /// </summary>
        public bool DeleteProfile(string profileName)
        {
            try
            {
                string fileName = GetSafeFileName(profileName) + ".json";
                string filePath = Path.Combine(profilesDirectory, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] DeleteProfile '{profileName}' failed: {ex}");
                return false;
            }
        }

        #endregion

        #region Settings Persistence

        /// <summary>
        /// Save application settings to disk
        /// </summary>
        public bool SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] SaveSettings failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Load application settings from disk
        /// </summary>
        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                    return new AppSettings();

                string json = File.ReadAllText(settingsFilePath);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] LoadSettings failed: {ex}");
                return new AppSettings();
            }
        }

        #endregion

        #region Custom Positions Persistence

        /// <summary>
        /// Save custom positions to disk
        /// </summary>
        public bool SaveCustomPositions(List<CustomPosition> positions)
        {
            try
            {
                string json = JsonConvert.SerializeObject(positions, Formatting.Indented);
                File.WriteAllText(customPositionsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] SaveCustomPositions failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Load custom positions from disk
        /// </summary>
        public List<CustomPosition> LoadCustomPositions()
        {
            try
            {
                if (!File.Exists(customPositionsFilePath))
                    return new List<CustomPosition>();

                string json = File.ReadAllText(customPositionsFilePath);
                var positions = JsonConvert.DeserializeObject<List<CustomPosition>>(json);
                return positions ?? new List<CustomPosition>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] LoadCustomPositions failed: {ex}");
                return new List<CustomPosition>();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Convert a profile name to a safe file name
        /// </summary>
        private string GetSafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            // Path.GetFileName collapses any surviving ".." or path-separator sequences
            // (e.g. "../../evil" → "evil"), preventing directory traversal attacks.
            name = Path.GetFileName(name);

            // Guard edge case: name was entirely dots (e.g. ".."), which Path.GetFileName
            // does not strip — those are valid file-name characters individually.
            if (string.IsNullOrWhiteSpace(name) || name.TrimStart('.').Length == 0)
                name = "_";

            return name;
        }

        /// <summary>
        /// Get the app data directory path
        /// </summary>
        public string GetAppDataPath() => appDataPath;

        /// <summary>
        /// Get the profiles directory path
        /// </summary>
        public string GetProfilesDirectory() => profilesDirectory;

        #endregion

        #region Hotkeys Persistence

        /// <summary>
        /// Save hotkey configurations to disk
        /// </summary>
        public bool SaveHotkeys(List<HotkeyConfig> hotkeys)
        {
            try
            {
                string json = JsonConvert.SerializeObject(hotkeys, Formatting.Indented);
                File.WriteAllText(hotkeysFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] SaveHotkeys failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Load hotkey configurations from disk
        /// </summary>
        public List<HotkeyConfig> LoadHotkeys()
        {
            try
            {
                if (File.Exists(hotkeysFilePath))
                {
                    string json = File.ReadAllText(hotkeysFilePath);
                    var hotkeys = JsonConvert.DeserializeObject<List<HotkeyConfig>>(json);
                    return hotkeys ?? new List<HotkeyConfig>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PersistenceService] LoadHotkeys failed: {ex}");
            }

            return new List<HotkeyConfig>();
        }

        #endregion
    }
}
