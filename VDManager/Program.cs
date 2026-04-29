using VDManager.Services;

namespace VDManager;

static class Program
{
    private const string MutexName = "Global\\DeskBulldozer_SingleInstance_B7E3A1";

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <remarks>
    /// Must be a synchronous void method so that [STAThread] is honoured for
    /// the entire lifetime of the process.  Using "async Task Main" causes the
    /// continuation after the first await to run on a ThreadPool (MTA) thread,
    /// which breaks WinForms COM calls (AutoCompleteMode, drag-drop, etc.) and
    /// raises ThreadStateException.  Async license work is therefore blocked
    /// synchronously with GetAwaiter().GetResult() while staying on the STA thread.
    /// </remarks>
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "DeskBulldozer is already running.\nCheck the system tray.",
                "DeskBulldozer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // ── License check ────────────────────────────────────────────────────
        // Unpackaged (.exe dev run) → LicenseManager returns a full active stub.
        // Packaged (Store MSIX)     → real Store API call with grace-counter fallback.
        // Block synchronously so the STA thread is never abandoned to a ThreadPool thread.
        var licenseManager = new LicenseManager();
        LicenseInfo license;

        try
        {
            license = licenseManager.GetLicenseInfoAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Outer safety net — GetLicenseInfoAsync already handles exceptions
            // internally, but just in case something slips through, fail open.
            license = new LicenseInfo
            {
                IsTrial        = false,
                IsActive       = true,
                ExpirationDate = DateTimeOffset.MaxValue,
                IsExpired      = false
            };
        }

        if (!license.CanRun)
        {
            if (MessageBox.Show(
                "Your DeskBulldozer beta trial has expired.\n\nBuy the full version now?",
                "Trial Expired",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                bool purchased = licenseManager.RequestPurchaseAsync().GetAwaiter().GetResult();

                if (purchased)
                {
                    // Re-check license after purchase — Store sometimes activates immediately.
                    license = licenseManager.GetLicenseInfoAsync().GetAwaiter().GetResult();
                    if (license.CanRun)
                    {
                        MessageBox.Show(
                            "Thank you for purchasing DeskBulldozer!\n\nRestarting now...",
                            "Purchase Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        // Fall through to normal startup below.
                    }
                    else
                    {
                        MessageBox.Show(
                            "Purchase completed, but the license is not yet active.\n\n" +
                            "Please restart the app to continue.",
                            "Almost There",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                }
                else
                {
                    // Purchase was cancelled or failed — exit.
                    return;
                }
            }
            else
            {
                // User chose No — exit.
                return;
            }
        }

        // ── Normal startup ───────────────────────────────────────────────────
        // Opt out of Windows Power Throttling so the message pump keeps running
        // at full speed while the app is hidden in the system tray.  Without this
        // Windows 11 may suspend/throttle the process after a period of inactivity,
        // which causes WM_HOTKEY messages to stop arriving.
        Win32API.DisablePowerThrottling();

        ApplicationConfiguration.Initialize();

        // ── Composition root — wire all services here ────────────────────────
        IWindowManager windowManager = new WindowManager();
        IPersistenceService persistenceService = new PersistenceService();
        IWindowInstanceTracker instanceTracker = new WindowInstanceTracker();
        IRulesManager rulesManager = new RulesManager(windowManager, persistenceService, instanceTracker);
        IWindowMonitor windowMonitor = new WindowMonitor(windowManager, rulesManager, instanceTracker);
        IThemeManager themeManager = new ThemeManager();
        ILauncherService launcherService = new LauncherService(windowManager, instanceTracker, rulesManager);

        Application.Run(new Form1(windowManager, persistenceService, rulesManager,
                                  windowMonitor, launcherService, themeManager));
    }
}
