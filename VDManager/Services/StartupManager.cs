using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Windows.ApplicationModel;

namespace VDManager.Services
{
    /// <summary>
    /// Manages "start with Windows" behaviour for DeskBulldozer.
    ///
    /// When running as a packaged MSIX app (Microsoft Store build):
    ///   • Uses the StartupTask API (Windows.ApplicationModel.StartupTask).
    ///   • The task ID "DeskBulldozerStartup" must match the TaskId declared
    ///     in Package.appxmanifest's windows.startupTask extension.
    ///   • The user can also toggle this in Windows Settings → Apps → Startup.
    ///
    /// When running as an unpackaged .exe (development / direct install):
    ///   • Falls back to the classic HKCU registry Run key.
    /// </summary>
    public class StartupManager
    {
        private const string AppName        = "DeskBulldozer";
        private const string StartupTaskId  = "DeskBulldozerStartup";
        private const string RunKeyPath     = @"Software\Microsoft\Windows\CurrentVersion\Run";

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Enable or disable startup with Windows.
        /// Automatically selects MSIX StartupTask or registry based on packaging.
        /// </summary>
        public static bool SetStartupEnabled(bool enabled, out string errorMessage)
        {
            return LicenseManager.IsPackaged
                ? SetStartupEnabledMsix(enabled, out errorMessage)
                : SetStartupEnabledRegistry(enabled, out errorMessage);
        }

        /// <summary>
        /// Check if app is set to start with Windows.
        /// </summary>
        public static bool IsStartupEnabled()
        {
            return LicenseManager.IsPackaged
                ? IsStartupEnabledMsix()
                : IsStartupEnabledRegistry();
        }

        // ─────────────────────────────────────────────────────────────────────
        // MSIX path — StartupTask API
        // ─────────────────────────────────────────────────────────────────────

        private static bool SetStartupEnabledMsix(bool enabled, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                var task = StartupTask.GetAsync(StartupTaskId).AsTask().GetAwaiter().GetResult();

                if (enabled)
                {
                    var state = task.RequestEnableAsync().AsTask().GetAwaiter().GetResult();
                    if (state == StartupTaskState.Enabled || state == StartupTaskState.EnabledByPolicy)
                        return true;

                    // DisabledByUser means the user turned it off in Settings — respect that.
                    if (state == StartupTaskState.DisabledByUser)
                    {
                        errorMessage = "Startup was disabled by the user in Windows Settings. " +
                                       "Please re-enable it there (Settings → Apps → Startup).";
                        return false;
                    }

                    errorMessage = $"Could not enable startup task (state: {state}).";
                    return false;
                }
                else
                {
                    task.Disable();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Startup task error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[StartupManager] MSIX error: {ex}");
                return false;
            }
        }

        private static bool IsStartupEnabledMsix()
        {
            try
            {
                var task  = StartupTask.GetAsync(StartupTaskId).AsTask().GetAwaiter().GetResult();
                return task.State == StartupTaskState.Enabled ||
                       task.State == StartupTaskState.EnabledByPolicy;
            }
            catch
            {
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Unpackaged path — classic registry Run key
        // ─────────────────────────────────────────────────────────────────────

        private static bool SetStartupEnabledRegistry(bool enabled, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null)
                    {
                        errorMessage = "Failed to open registry key.";
                        return false;
                    }

                    if (enabled)
                    {
                        // Fix M-4: use Environment.ProcessPath (.NET 6+) which correctly returns
                        // the .exe path even for single-file published apps, where
                        // Assembly.GetExecutingAssembly().Location returns "" or the DLL path.
                        string? exePath = Environment.ProcessPath;

                        // Fallback to MainModule for edge cases (e.g., hosted test runner)
                        if (string.IsNullOrEmpty(exePath))
                            exePath = Process.GetCurrentProcess().MainModule?.FileName;

                        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                        {
                            errorMessage = "Executable not found at expected path.";
                            return false;
                        }

                        key.SetValue(AppName, $"\"{exePath}\"");
                        return true;
                    }
                    else
                    {
                        if (key.GetValue(AppName) != null)
                            key.DeleteValue(AppName);
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                errorMessage = "Administrator rights required to modify startup settings.";
                System.Diagnostics.Debug.WriteLine($"[StartupManager] Unauthorized: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[StartupManager] Error: {ex}");
                return false;
            }
        }

        private static bool IsStartupEnabledRegistry()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    if (key == null)
                        return false;

                    return key.GetValue(AppName) != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
