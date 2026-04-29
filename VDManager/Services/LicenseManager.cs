using System.Text.Json;
using Windows.Services.Store;

namespace VDManager.Services
{
    /// <summary>
    /// Manages Microsoft Store license checks for DeskBulldozer.
    /// Requires the app to be packaged as MSIX with a Store identity.
    /// </summary>
    public class LicenseManager
    {
        private StoreContext? _store;

        // ── Grace counter configuration ───────────────────────────────────────
        // When the Store API is unreachable we allow up to MaxFailures consecutive
        // failures before switching to fail-closed.  The counter is stored in an
        // innocuously-named file using XOR obfuscation so it doesn't read as a
        // plain integer.
        private const int MaxFailures = 3;
        private const uint ObfuscationMask = 0x4A3B2C1Du;   // keep private
        private const string CacheKey = "_svc";              // field name in JSON

        private static readonly string CacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DeskBulldozer");

        // Named "telemetry.json" — looks like usage-analytics cache, not a license counter.
        private static readonly string CacheFile = Path.Combine(CacheDir, "telemetry.json");

        /// <summary>
        /// Returns true if the app is running with a valid Store package identity.
        /// When unpackaged (e.g. direct .exe run), Store APIs are unavailable.
        /// </summary>
        public static bool IsPackaged
        {
            get
            {
                try { var _ = Windows.ApplicationModel.Package.Current.Id; return true; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LicenseManager] IsPackaged check failed: {ex.Message}");
                    return false;
                }
            }
        }

        private StoreContext Store => _store ??= StoreContext.GetDefault();

        /// <summary>
        /// Retrieves the current app license from the Microsoft Store.
        ///
        /// Unpackaged (dev / sideload): always returns a full active stub.
        /// Packaged: queries the Store API; on success resets the grace counter;
        ///           on failure increments it and fails open until MaxFailures is
        ///           reached, after which it fails closed.
        /// </summary>
        public async Task<LicenseInfo> GetLicenseInfoAsync()
        {
            // Not packaged → allow full access (dev / sideload scenario)
            if (!IsPackaged)
                return FullLicense();

            try
            {
                StoreAppLicense license = await Store.GetAppLicenseAsync();

                // Got a real response — reset the failure counter.
                ResetGraceCounter();

                return new LicenseInfo
                {
                    IsTrial        = license.IsTrial,
                    IsActive       = license.IsActive,
                    ExpirationDate = license.ExpirationDate,
                    IsExpired      = license.IsTrial && DateTimeOffset.Now > license.ExpirationDate
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[LicenseManager] Store API error: {ex.Message}");

                int failures = IncrementGraceCounter();

                if (failures < MaxFailures)
                {
                    // Still within grace — fail open so legitimate offline users aren't blocked.
                    System.Diagnostics.Debug.WriteLine(
                        $"[LicenseManager] Grace failure {failures}/{MaxFailures} — allowing launch");
                    return FullLicense();
                }

                // Grace period exhausted — fail closed.
                System.Diagnostics.Debug.WriteLine(
                    "[LicenseManager] Grace period exhausted — blocking launch");
                return new LicenseInfo
                {
                    IsTrial        = true,
                    IsActive       = false,
                    ExpirationDate = DateTimeOffset.MinValue,
                    IsExpired      = true
                };
            }
        }

        /// <summary>
        /// Prompts the user to purchase the app through the Store in-app purchase sheet.
        /// Only available when running as a packaged MSIX app.
        /// </summary>
        public async Task<bool> RequestPurchaseAsync(string? storeProductId = null)
        {
            if (!IsPackaged)
                return false;

            const string AppStoreId = "9N3XH2VMGZ7Q";

            try
            {
                StorePurchaseResult result = string.IsNullOrEmpty(storeProductId)
                    ? await Store.RequestPurchaseAsync(AppStoreId)
                    : await Store.RequestPurchaseAsync(storeProductId);

                return result.Status == StorePurchaseStatus.Succeeded
                    || result.Status == StorePurchaseStatus.AlreadyPurchased;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LicenseManager] Purchase failed: {ex.Message}");
                return false;
            }
        }

        // ── Grace counter helpers ─────────────────────────────────────────────
        // Counter is stored as (value XOR ObfuscationMask) in lowercase hex.
        // The file also carries a "ts" timestamp to look like a telemetry record.

        private static LicenseInfo FullLicense() => new LicenseInfo
        {
            IsTrial        = false,
            IsActive       = true,
            ExpirationDate = DateTimeOffset.MaxValue,
            IsExpired      = false
        };

        private static int ReadGraceCounter()
        {
            try
            {
                if (!File.Exists(CacheFile))
                    return 0;

                using var doc = JsonDocument.Parse(File.ReadAllText(CacheFile));
                if (doc.RootElement.TryGetProperty(CacheKey, out JsonElement el)
                    && el.ValueKind == JsonValueKind.String)
                {
                    if (uint.TryParse(el.GetString(),
                            System.Globalization.NumberStyles.HexNumber, null, out uint encoded))
                    {
                        return (int)(encoded ^ ObfuscationMask);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LicenseManager] ReadGraceCounter failed: {ex.Message}");
            }
            return 0;
        }

        private static void WriteGraceCounter(int count)
        {
            try
            {
                Directory.CreateDirectory(CacheDir);

                uint encoded = (uint)count ^ ObfuscationMask;
                // "ts" carries a Unix epoch millis value — looks like a last-updated timestamp.
                long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                string json = $"{{\n  \"{CacheKey}\": \"{encoded:x8}\",\n  \"ts\": {ts}\n}}";
                File.WriteAllText(CacheFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[LicenseManager] WriteGraceCounter failed: {ex.Message}");
            }
        }

        private static int IncrementGraceCounter()
        {
            int count = ReadGraceCounter() + 1;
            WriteGraceCounter(count);
            return count;
        }

        private static void ResetGraceCounter()
        {
            // Only write if there's something to reset (avoids unnecessary disk writes).
            if (ReadGraceCounter() != 0)
                WriteGraceCounter(0);
        }
    }

    /// <summary>
    /// Snapshot of the current app license state.
    /// </summary>
    public class LicenseInfo
    {
        public bool IsTrial { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }

        /// <summary>True when the app is in trial mode AND the trial period has passed.</summary>
        public bool IsExpired { get; set; }

        /// <summary>Convenience: app is usable (active and not expired).</summary>
        public bool CanRun => IsActive && !IsExpired;
    }
}
