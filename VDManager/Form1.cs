using System.Linq;
using VDManager.Models;
using VDManager.Services;

namespace VDManager;

public partial class Form1 : Form
{
    private IWindowManager windowManager;
    private IPersistenceService persistenceService;
    private IRulesManager rulesManager;
    private IWindowMonitor windowMonitor;
    private HotkeyManager hotkeyManager;
    private ILauncherService launcherService;
    private NotifyIcon? trayIcon;
    private AppSettings appSettings;
    private IThemeManager themeManager;
    private List<WindowInfo> currentWindows;
    private List<HotkeyConfig> hotkeyConfigs;
    private Dictionary<string, int> registeredHotkeyIds;
    private System.Windows.Forms.Timer? _startupMonitorDelayTimer;
    private bool _pendingStartupMonitorStart;
    private System.Windows.Forms.Timer? _applyBatchDesktopGuardTimer;
    private int _applyBatchDesktopBefore = -1;
    private string? _applyBatchSource;

    // Display change detection
    private System.Windows.Forms.Timer? _displayChangeDebounceTimer;

    // VirtualDesktopAccessor post-message hook message IDs
    // We pass VDA_MSG_BASE to RegisterPostMessageHook; the DLL posts (VDA_MSG_BASE + offset).
    private const int WM_USER             = 0x0400;
    private const int VDA_MSG_BASE        = WM_USER + 20;
    private const int VDA_DESKTOP_CHANGED = VDA_MSG_BASE + 0;  // wParam=oldIdx, lParam=newIdx
    private const int VDA_VD_DESTROYED    = VDA_MSG_BASE + 2;  // wParam=destroyedIdx, lParam=fallbackIdx
    private const int VDA_VD_DESTROY_BEGIN= VDA_MSG_BASE + 4;  // wParam=destroyedIdx, lParam=fallbackIdx
    private const int VDA_VD_CREATED      = VDA_MSG_BASE + 5;  // wParam=newIdx, lParam=0

    // Enforcement UI controls
    private CheckBox? chkEnforcementEnabled;
    private CheckBox? chkSkipMinimized;
    private NumericUpDown? nudGracePeriod;
    private NumericUpDown? nudCooldown;

    // Window detection timing controls
    private NumericUpDown? nudNewWindowDelay;

    // Layout test controls
    private Button? btnRunLayoutTest;
    private Label? lblLayoutTestStatus;
    private CancellationTokenSource? _layoutTestCts;

    // Delay startup profile auto-launch so window/rule tracking has time to warm up.
    private const int StartupAutoLaunchWarmupMs = 5000;
    private const int ApplyBatchDesktopRestoreDelayMs = 500;

    public Form1(
        IWindowManager windowManager,
        IPersistenceService persistenceService,
        IRulesManager rulesManager,
        IWindowMonitor windowMonitor,
        ILauncherService launcherService,
        IThemeManager themeManager)
    {
        this.windowManager = windowManager;
        this.persistenceService = persistenceService;
        this.rulesManager = rulesManager;
        this.windowMonitor = windowMonitor;
        this.launcherService = launcherService;
        this.themeManager = themeManager;

        InitializeComponent();

        // HotkeyManager requires this.Handle, which is valid after InitializeComponent
        hotkeyManager = new HotkeyManager(this.Handle);
        appSettings = new AppSettings();
        currentWindows = new List<WindowInfo>();
        hotkeyConfigs = new List<HotkeyConfig>();
        registeredHotkeyIds = new Dictionary<string, int>();

        // Subscribe to window monitor events
        windowMonitor.RuleApplied += OnRuleApplied;
        windowMonitor.EnforcementSkipped += OnEnforcementSkipped;

        // Subscribe to theme changes
        themeManager.ThemeChanged += OnThemeChanged;

        InitializeUI();
        CheckVirtualDesktopAPI();

        // Register for virtual desktop topology change notifications.
        if (VirtualDesktopAPI.IsAvailable())
            VirtualDesktopAPI.RegisterPostMessageHook(this.Handle, VDA_MSG_BASE);

        LoadRules();
        LoadAutoApplySettings();
        LoadSettings();
        LoadHotkeys();
        RegisterHotkeys();
        SetupSystemTray();
        LoadLauncher();
        ApplyTheme();

        // Display change detection: debounce timer + dual event subscription
        _displayChangeDebounceTimer = new System.Windows.Forms.Timer { Interval = 750 };
        _displayChangeDebounceTimer.Tick += OnDisplayChangeDebounced;
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnSystemDisplaySettingsChanged;

        // Pin this window to all virtual desktops after it's fully loaded,
        // and do a one-time apply of all rules on first launch.
        this.Shown += (s, e) =>
        {
            windowManager.PinWindow(this.Handle);

            // Apply all rules once on launch (only fires on first show, not tray restore)
            if (rulesManager.GetAllRules().Count > 0)
            {
                int count = ApplyRulesToAllWindowsWithDesktopGuard("StartupShownInitial");
                UpdateStatus($"Applied {count} rule(s) on launch");
            }

            // Delay launcher startup profiles until initial window/rule state is stable.
            ScheduleStartupAutoLaunch();

            // Start monitor scheduling for startup. If startup profiles exist, monitor
            // is now started immediately before auto-launch so new windows are observed.
            ScheduleStartupMonitoringIfNeeded();
        };
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Close to tray if enabled (unless explicitly closing from tray menu)
        if (e.CloseReason == CloseReason.UserClosing && appSettings.CloseToTray && trayIcon != null)
        {
            e.Cancel = true;
            Hide();
            trayIcon.Visible = true;
            if (appSettings.ShowBalloonTips)
            {
                trayIcon.ShowBalloonTip(1000, "DeskBulldozer", "Application minimized to system tray. Right-click to exit.", ToolTipIcon.Info);
            }
            return;
        }

        // Stop monitoring and cleanup
        if (VirtualDesktopAPI.IsAvailable())
            VirtualDesktopAPI.UnregisterPostMessageHook(this.Handle);
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= OnSystemDisplaySettingsChanged;
        _displayChangeDebounceTimer?.Stop();
        _displayChangeDebounceTimer?.Dispose();
        _startupMonitorDelayTimer?.Stop();
        _startupMonitorDelayTimer?.Dispose();
        _applyBatchDesktopGuardTimer?.Stop();
        _applyBatchDesktopGuardTimer?.Dispose();
        windowMonitor?.StopMonitoring();
        windowMonitor?.Dispose();
        launcherService?.Dispose();
        hotkeyManager?.Dispose();
        trayIcon?.Dispose();
        themeManager?.Dispose();
        base.OnFormClosing(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // Minimize to tray (if enabled)
        if (WindowState == FormWindowState.Minimized && trayIcon != null && appSettings.MinimizeToTray)
        {
            Hide();
            trayIcon.Visible = true;
            if (appSettings.ShowBalloonTips)
            {
                trayIcon.ShowBalloonTip(2000, "DeskBulldozer", "Running in system tray. Double-click to restore.", ToolTipIcon.Info);
            }
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        // Always re-pin when becoming visible (e.g., after restoring from tray).
        // Windows removes the pin state when the window is hidden. We defer via
        // BeginInvoke so the call runs after the shell has fully shown the window —
        // calling PinWindow synchronously here can fail silently because the window
        // isn't yet "present" from the shell's perspective.
        if (Visible)
        {
            BeginInvoke(new Action(() => windowManager.PinWindow(this.Handle)));
        }
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        UpdateCurrentDesktop(windowManager.GetCurrentDesktop());
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_HOTKEY        = 0x0312;
        const int WM_DISPLAYCHANGE = 0x007E;

        if (m.Msg == WM_HOTKEY)
        {
            int hotkeyId = m.WParam.ToInt32();
            hotkeyManager?.ProcessHotkey(hotkeyId);
        }
        else if (m.Msg == WM_DISPLAYCHANGE)
        {
            OnDisplayChangeDetected();
        }
        else if (m.Msg == VDA_VD_DESTROYED || m.Msg == VDA_VD_DESTROY_BEGIN)
        {
            OnVirtualDesktopRemoved(m.WParam.ToInt32(), m.LParam.ToInt32());
        }
        else if (m.Msg == VDA_VD_CREATED)
        {
            OnVirtualDesktopCreated(m.WParam.ToInt32());
        }
        else if (m.Msg == VDA_DESKTOP_CHANGED)
        {
            UpdateCurrentDesktop(m.LParam.ToInt32());
        }

        base.WndProc(ref m);
    }

    private void OnSystemDisplaySettingsChanged(object? sender, EventArgs e)
    {
        // SystemEvents may fire on a background thread; always marshal to UI thread.
        if (InvokeRequired)
            BeginInvoke(new Action(OnDisplayChangeDetected));
        else
            OnDisplayChangeDetected();
    }

    private void OnDisplayChangeDetected()
    {
        // Immediately invalidate the stale screen cache so all downstream calls get fresh data.
        QuadrantLayout.InvalidateScreenCache();

        // Restart debounce timer — only the final event in a rapid burst triggers real work.
        _displayChangeDebounceTimer?.Stop();
        _displayChangeDebounceTimer?.Start();
    }

    private void OnDisplayChangeDebounced(object? sender, EventArgs e)
    {
        _displayChangeDebounceTimer!.Stop(); // One-shot: do not re-fire.

        int monitorCount = QuadrantLayout.GetMonitorCount();
        System.Diagnostics.Debug.WriteLine(
            $"[DisplayChange] Configuration settled. Now {monitorCount} monitor(s).");

        // Refresh monitor combobox with current screen list.
        RefreshMonitorList();

        // Repaint the visual quadrant panel (it renders based on the selected monitor).
        visualQuadrantPanel?.Invalidate();

        // Show balloon tip if the app is running hidden in the system tray.
        if (appSettings.ShowBalloonTips && trayIcon?.Visible == true)
        {
            trayIcon.ShowBalloonTip(3000, "Display Configuration Changed",
                $"{monitorCount} monitor(s) now active. Window rules may need re-applying.",
                ToolTipIcon.Info);
        }

        // Suspend enforcement and schedule rule re-application after OS settles.
        HandleDisplayChangeForEnforcement();
    }

    private void HandleDisplayChangeForEnforcement()
    {
        // Capture count BEFORE suspending so we can detect topology changes vs. resolution-only changes.
        int monitorCountBefore = QuadrantLayout.GetMonitorCount();
        windowMonitor.SuspendEnforcementForDisplayChange();

        // Wait 2000ms for Windows (and apps) to finish repositioning windows after the
        // display change before we place them according to rules.
        var reapplyTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        reapplyTimer.Tick += (s, e) =>
        {
            reapplyTimer.Stop();
            reapplyTimer.Dispose();

            int monitorCountAfter = QuadrantLayout.GetMonitorCount();
            bool countChanged = monitorCountAfter != monitorCountBefore;

            if (rulesManager.HasRules() && (countChanged || appSettings.AutoApplyOnLaunch))
            {
                // Topology change (monitor added/removed): always re-apply so windows return
                // to correct monitors regardless of the AutoApplyOnLaunch setting.
                // Resolution-only change: respect AutoApplyOnLaunch as usual.
                int count = rulesManager.ApplyRulesToAllWindows();
                UpdateStatus($"Display change: re-applied {count} rule(s) across {monitorCountAfter} monitor(s)");
            }
            else if (rulesManager.HasRules())
            {
                // Resolution/DPI change only with auto-apply off — monitors unchanged, no action needed.
                UpdateStatus($"Display configuration changed — {monitorCountAfter} monitor(s).");
            }
            else
            {
                UpdateStatus($"Display configuration changed — {monitorCountAfter} monitor(s) detected.");
            }

            windowMonitor.ResumeEnforcementAfterDisplayChange(countChanged);
        };
        reapplyTimer.Start();
    }

    private void OnVirtualDesktopRemoved(int removedIndex, int fallbackIndex)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[VD] Desktop {removedIndex} removed (fallback: {fallbackIndex}).");

        // Clear enforcement — indices have shifted for anything above removedIndex.
        windowMonitor.OnVirtualDesktopCountChanged();

        RefreshDesktopList();

        if (appSettings.ShowBalloonTips && trayIcon?.Visible == true)
            trayIcon.ShowBalloonTip(3000, "Virtual Desktop Removed",
                $"Desktop {removedIndex + 1} was closed. Rules may need re-applying.",
                ToolTipIcon.Info);

        if (rulesManager.HasRules() && appSettings.AutoApplyOnLaunch)
        {
            int count = rulesManager.ApplyRulesToAllWindows();
            UpdateStatus($"VD removed: re-applied {count} rule(s).");
        }
        else
        {
            UpdateStatus($"Virtual desktop {removedIndex + 1} removed.");
        }
    }

    private void OnVirtualDesktopCreated(int newIndex)
    {
        System.Diagnostics.Debug.WriteLine($"[VD] Desktop {newIndex} created.");
        RefreshDesktopList();
        UpdateStatus($"Virtual desktop {newIndex + 1} created.");
    }

    private void OnRuleApplied(object? sender, RuleAppliedEventArgs e)
    {
        // Update UI on main thread
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => OnRuleApplied(sender, e)));
            return;
        }

        if (e.Success)
        {
            UpdateStatus($"Auto-applied rule: {e.Window.ProcessName} → Desktop {e.Rule.DesktopIndex + 1}");
        }
    }

    private void OnEnforcementSkipped(object? sender, string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => OnEnforcementSkipped(sender, message)));
            return;
        }
        UpdateStatus(message);
    }

    private void InitializeUI()
    {
        this.Text = "DeskBulldozer";
        this.StartPosition = FormStartPosition.CenterScreen;

        // Load the Win32 resource icon embedded via <ApplicationIcon> in the .csproj.
        // This makes the title bar, taskbar button and Alt+Tab thumbnail all show app.ico.
        try
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
                        ?? SystemIcons.Application;
        }
        catch
        {
            this.Icon = SystemIcons.Application;
        }

        // Load desktop list
        RefreshDesktopList();

        // Initialize visual quadrant panel (default to TopLeft)
        visualQuadrantPanel.SelectedQuadrant = Quadrant.TopLeft;

        // Load monitor list
        RefreshMonitorList();

        // Load windows
        RefreshWindows();

        // Setup rules grid
        SetupRulesGrid();
        RefreshRulesGrid();

        // Setup enforcement UI in Settings tab
        SetupEnforcementUI();

        // Setup button hover effects
        SetupButtonHoverEffects();
    }

    private void SetupButtonHoverEffects()
    {
        // Get all buttons that should have hover effects
        var buttons = new[] { btnAddRule, btnEditRule, btnDeleteRule, btnMoveWindow, btnRefresh, btnAddAsRule, btnApplyRules };

        foreach (var btn in buttons)
        {
            // Capture one saved-color slot per button in the closure.
            // On MouseEnter we snapshot the current (theme-correct) BackColor before
            // lightening it; on MouseLeave we restore that exact snapshot.
            // This avoids the previous bug where ControlPaint.Dark(ControlPaint.Light(c,f),f) ≠ c,
            // which caused each hover to leave the button slightly darker until it turned black.
            Color? savedColor = null;

            btn.MouseEnter += (s, e) =>
            {
                if (btn.Enabled)
                {
                    savedColor = btn.BackColor;   // snapshot theme-correct colour
                    float lightFactor = themeManager.IsDarkMode() ? 0.15f : 0.1f;
                    btn.BackColor = ControlPaint.Light(savedColor.Value, lightFactor);
                }
            };

            btn.MouseLeave += (s, e) =>
            {
                if (savedColor.HasValue)
                {
                    btn.BackColor = savedColor.Value;   // restore exact saved colour
                    savedColor = null;
                }
            };
        }
    }

    private void visualQuadrantPanel_QuadrantChanged(object? sender, EventArgs e)
    {
        // Visual panel selection changed - no additional action needed
        // The selected quadrant is automatically stored in visualQuadrantPanel.SelectedQuadrant
        UpdateAddAsRuleButtonVisibility();
    }

    private void UpdateAddAsRuleButtonVisibility()
    {
        // Enable "Add as Rule" when a window is selected.
        // Any quadrant (including the default TopLeft) is a valid layout choice, so we
        // do NOT gate on the quadrant value.  The previous condition used || which made
        // hasSelectedQuadrant always true when a window was selected anyway — fixing #4.
        bool hasSelectedWindow = lstWindows.SelectedIndex >= 0;
        btnAddAsRule.Enabled = hasSelectedWindow;
    }

    private void RefreshMonitorList()
    {
        int previousIndex = cmbMonitor.SelectedIndex; // Save before clearing (-1 if none selected)
        cmbMonitor.Items.Clear();

        var monitorNames = QuadrantLayout.GetMonitorNames();
        foreach (var name in monitorNames)
        {
            cmbMonitor.Items.Add(name);
        }

        if (cmbMonitor.Items.Count > 0)
        {
            // Clamp to valid range; handles previousIndex == -1 (no prior selection) via Math.Max(0, ...).
            cmbMonitor.SelectedIndex = Math.Min(
                Math.Max(0, previousIndex),
                cmbMonitor.Items.Count - 1);
        }
    }

    private void CheckVirtualDesktopAPI()
    {
        // Check if VirtualDesktopAccessor.dll is available
        if (VirtualDesktopAPI.IsAvailable())
        {
            try
            {
                int desktopCount = VirtualDesktopAPI.GetDesktopCount();
                int currentDesktop = VirtualDesktopAPI.GetCurrentDesktopNumber();

                UpdateStatus($"API Ready - {desktopCount} desktops available");
                UpdateCurrentDesktop(currentDesktop);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"VirtualDesktopAccessor.dll found but error calling API:\n{ex.Message}",
                    "API Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
        else
        {
            string errorDetails = VirtualDesktopAPI.GetLoadError();
            MessageBox.Show(
                $"{errorDetails}\n\n" +
                "Please download the DLL from:\n" +
                "https://github.com/Ciantic/VirtualDesktopAccessor/releases\n\n" +
                "And place it in the Native folder, then rebuild the project.",
                "Missing Dependency",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            // Disable controls if API is not available
            btnMoveWindow.Enabled = false;
            cmbTargetDesktop.Enabled = false;
            visualQuadrantPanel.Enabled = false;
            cmbMonitor.Enabled = false;
        }
    }

    private void RefreshDesktopList()
    {
        cmbTargetDesktop.Items.Clear();

        int desktopCount = windowManager.GetDesktopCount();
        for (int i = 0; i < desktopCount; i++)
        {
            cmbTargetDesktop.Items.Add($"Desktop {i + 1}");
        }

        if (cmbTargetDesktop.Items.Count > 0)
        {
            cmbTargetDesktop.SelectedIndex = 0;
        }
    }

    private void RefreshWindows()
    {
        UpdateStatus("Refreshing windows...");

        lstWindows.Items.Clear();
        currentWindows = windowManager.GetAllWindows();

        foreach (var window in currentWindows)
        {
            lstWindows.Items.Add(window.GetDisplayName());
        }

        UpdateStatus($"Found {currentWindows.Count} windows");
        UpdateCurrentDesktop(windowManager.GetCurrentDesktop());
    }

    private void UpdateStatus(string message)
    {
        lblStatus.Text = $"Status: {message}";
    }

    private void UpdateCurrentDesktop(int desktopNumber)
    {
        lblCurrentDesktop.Text = $"Current Desktop: {desktopNumber + 1}";
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        RefreshWindows();
    }

    private void btnMoveWindow_Click(object? sender, EventArgs e)
    {
        if (lstWindows.SelectedIndex < 0)
        {
            MessageBox.Show(
                "Please select a window to move.",
                "No Window Selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return;
        }

        if (cmbTargetDesktop.SelectedIndex < 0)
        {
            MessageBox.Show(
                "Please select a target desktop.",
                "No Desktop Selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return;
        }

        var selectedWindow = currentWindows[lstWindows.SelectedIndex];
        int targetDesktop = cmbTargetDesktop.SelectedIndex;
        Quadrant quadrant = visualQuadrantPanel.SelectedQuadrant;
        int monitorIndex = cmbMonitor.SelectedIndex;

        // Validate window handle
        if (selectedWindow.Handle == IntPtr.Zero)
        {
            UpdateStatus("Invalid window handle");
            MessageBox.Show(
                "The selected window is no longer valid. Please refresh the window list.",
                "Invalid Window",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        // Validate monitor index
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (monitorIndex < 0 || monitorIndex >= screens.Length)
        {
            monitorIndex = 0;
        }

        // Build status message
        string quadrantText = quadrant == Quadrant.None
            ? ""
            : $" and positioning in {quadrant}";

        UpdateStatus($"Moving window to Desktop {targetDesktop + 1}{quadrantText}...");

        bool success;
        try
        {
            if (quadrant == Quadrant.None)
            {
                // Just move, no positioning
                success = windowManager.MoveWindowToDesktop(selectedWindow, targetDesktop);
            }
            else
            {
                // Move and position
                success = windowManager.MoveAndPositionWindow(selectedWindow, targetDesktop, quadrant, monitorIndex);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus("Error occurred");
            MessageBox.Show(
                $"An error occurred:\n{ex.Message}\n\nWindow: {selectedWindow.ProcessName}\nHandle: {selectedWindow.Handle}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (success)
        {
            string successMsg = quadrant == Quadrant.None
                ? $"Window moved to Desktop {targetDesktop + 1}"
                : $"Window moved to Desktop {targetDesktop + 1} and positioned in {quadrant}";

            UpdateStatus($"Successfully moved '{selectedWindow.ProcessName}'");
            MessageBox.Show(
                successMsg,
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // Refresh the list
            RefreshWindows();
        }
        else
        {
            UpdateStatus("Failed to move/position window");

            // Get diagnostics
            string diagnostics = DiagnosticHelper.GetWindowDiagnostics(selectedWindow);
            string apiDiag = DiagnosticHelper.GetVirtualDesktopDiagnostics();

            MessageBox.Show(
                "Failed to move or position the window.\n\n" +
                "Window Details:\n" + diagnostics + "\n" +
                "API Status:\n" + apiDiag,
                "Error - Detailed Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void lstWindows_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateAddAsRuleButtonVisibility();
    }

    private void btnAddAsRule_Click(object? sender, EventArgs e)
    {
        if (lstWindows.SelectedIndex < 0)
        {
            return;
        }

        var selectedWindow = currentWindows[lstWindows.SelectedIndex];
        int targetDesktop = cmbTargetDesktop.SelectedIndex >= 0 ? cmbTargetDesktop.SelectedIndex : 0;
        Quadrant quadrant = visualQuadrantPanel.SelectedQuadrant;
        int monitorIndex = cmbMonitor.SelectedIndex >= 0 ? cmbMonitor.SelectedIndex : 0;

        // Validate monitor index
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (monitorIndex < 0 || monitorIndex >= screens.Length)
        {
            monitorIndex = 0;
        }

        // Create a new rule based on the current selection
        var newRule = new WindowRule
        {
            ProcessName = selectedWindow.ProcessName,
            WindowTitlePattern = selectedWindow.Title,  // Pre-fill with current window title
            UseRegex = false,
            Description = GenerateRuleDescription(selectedWindow.Title),
            DesktopIndex = targetDesktop,
            Quadrant = quadrant,
            MonitorIndex = monitorIndex,
            Enabled = true,
            Priority = 0,
            InstanceNumber = 0  // 0 means all instances
        };

        // Open the rule editor dialog with the pre-filled rule
        using var ruleDialog = new RuleEditorDialog(windowManager.GetDesktopCount(), newRule, windowManager, themeManager);
        if (ruleDialog.ShowDialog() == DialogResult.OK)
        {
            rulesManager.AddRule(ruleDialog.Rule);
            RefreshRulesGrid();
            UpdateStatus($"Rule added for '{selectedWindow.ProcessName}'");

            // Show success message and ask if user wants to apply rules now
            var result = MessageBox.Show(
                $"Rule created for '{selectedWindow.ProcessName}'\n\n" +
                $"Desktop: {targetDesktop + 1}\n" +
                $"Position: {quadrant}\n" +
                $"Monitor: {monitorIndex + 1}\n\n" +
                "Would you like to apply all rules now?",
                "Rule Created",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes)
            {
                UpdateStatus("Applying all rules...");
                int count = rulesManager.ApplyRulesToAllWindows();
                UpdateStatus($"Applied {count} rules");
                RefreshWindows();
            }
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\p{C}")]
    private static partial System.Text.RegularExpressions.Regex InvisibleCharsRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+[–—-]\s+")]
    private static partial System.Text.RegularExpressions.Regex TitleSeparatorRegex();

    private static string GenerateRuleDescription(string windowTitle)
    {
        if (string.IsNullOrWhiteSpace(windowTitle))
            return string.Empty;

        // Strip invisible Unicode control/format characters (zero-width spaces, etc.)
        string cleaned = InvisibleCharsRegex().Replace(windowTitle, "").Trim();

        // Split on " – ", " — ", " - " (em dash, en dash, regular dash surrounded by spaces)
        var parts = TitleSeparatorRegex().Split(cleaned);

        // Take last non-empty segment (typically the app name)
        for (int i = parts.Length - 1; i >= 0; i--)
        {
            var part = parts[i].Trim();
            if (!string.IsNullOrWhiteSpace(part))
                return part;
        }

        return cleaned;
    }

    #region Rules Management

    private void SetupRulesGrid()
    {
        dgvRules.AutoGenerateColumns = false;
        dgvRules.Columns.Clear();

        dgvRules.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Enabled",
            HeaderText = "✓",
            DataPropertyName = "Enabled",
            Width = 40
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Priority",
            HeaderText = "Pri",
            DataPropertyName = "Priority",
            Width = 40
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ProcessName",
            HeaderText = "Process",
            DataPropertyName = "ProcessName",
            Width = 100
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "WindowTitlePattern",
            HeaderText = "Title Filter",
            DataPropertyName = "WindowTitlePattern",
            Width = 100
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "InstanceNumber",
            HeaderText = "Inst#",
            DataPropertyName = "InstanceNumber",
            Width = 45
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DesktopIndex",
            HeaderText = "Desk",
            DataPropertyName = "DesktopIndex",
            Width = 45
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Quadrant",
            HeaderText = "Layout",
            DataPropertyName = "Quadrant",
            Width = 90
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "MonitorIndex",
            HeaderText = "Mon",
            DataPropertyName = "MonitorIndex",
            Width = 40
        });

        dgvRules.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "EnforcePosition",
            HeaderText = "Enforce",
            DataPropertyName = "EnforcePosition",
            Width = 65
        });

        dgvRules.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Description",
            HeaderText = "Description",
            DataPropertyName = "Description",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    private void LoadRules()
    {
        rulesManager.LoadRules();
        RefreshRulesGrid();
    }

    private void RefreshRulesGrid()
    {
        dgvRules.DataSource = null;
        dgvRules.DataSource = rulesManager.GetAllRules();

        UpdateStatus($"{rulesManager.GetAllRules().Count} rules loaded");
    }

    private void btnAddRule_Click(object? sender, EventArgs e)
    {
        using var ruleDialog = new RuleEditorDialog(windowManager.GetDesktopCount(), null, windowManager, themeManager);
        if (ruleDialog.ShowDialog() == DialogResult.OK)
        {
            rulesManager.AddRule(ruleDialog.Rule);
            RefreshRulesGrid();
            UpdateStatus("Rule added");
        }
    }

    private void btnEditRule_Click(object? sender, EventArgs e)
    {
        if (dgvRules.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a rule to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var rule = (WindowRule)dgvRules.SelectedRows[0].DataBoundItem;
        using var ruleDialog = new RuleEditorDialog(windowManager.GetDesktopCount(), rule, windowManager, themeManager);

        if (ruleDialog.ShowDialog() == DialogResult.OK)
        {
            rulesManager.UpdateRule(rule.Id, ruleDialog.Rule);
            RefreshRulesGrid();
            UpdateStatus("Rule updated");
        }
    }

    private void btnDeleteRule_Click(object? sender, EventArgs e)
    {
        if (dgvRules.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a rule to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            "Are you sure you want to delete this rule?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            var rule = (WindowRule)dgvRules.SelectedRows[0].DataBoundItem;
            rulesManager.DeleteRule(rule.Id);
            RefreshRulesGrid();
            UpdateStatus("Rule deleted");
        }
    }

    private void btnApplyRules_Click(object? sender, EventArgs e)
    {
        UpdateStatus("Applying all rules...");
        int count = rulesManager.ApplyRulesToAllWindows();
        UpdateStatus($"Applied {count} rules");

        MessageBox.Show(
            $"Applied rules to {count} windows.",
            "Rules Applied",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );

        RefreshWindows();
    }

    private void LoadAutoApplySettings()
    {
        // Load settings from disk
        appSettings = persistenceService.LoadSettings();
        chkAutoApply.Checked = appSettings.AutoApplyOnLaunch;
        chkAutoApply.CheckedChanged += chkAutoApply_CheckedChanged;

        // Do not start monitoring in constructor startup path.
        // We defer to Shown() after initial ApplyRules + startup launcher scheduling.
        if (appSettings.AutoApplyOnLaunch)
        {
            _pendingStartupMonitorStart = true;
            UpdateStatus("Auto-apply enabled - monitoring will start after startup initialization");
        }
    }

    private void StartAutoApplyMonitoring()
    {
        if (windowMonitor.IsMonitoring)
            return;

        windowMonitor.StartMonitoring(appSettings.MonitoringInterval);
        UpdateStatus("Auto-apply enabled - monitoring for new windows");
    }

    private void ApplyStartupRulesBeforeMonitoring()
    {
        if (!rulesManager.HasRules())
        {
            System.Diagnostics.Debug.WriteLine("[Startup] No rules to apply before monitor start.");
            return;
        }

        System.Diagnostics.Debug.WriteLine(
            "[Startup] Running one-time post-launch ApplyRulesToAllWindows before monitor starts.");

        int applied = ApplyRulesToAllWindowsWithDesktopGuard("StartupPreMonitorApply");
        UpdateStatus($"Startup post-launch apply: {applied} rule(s)");
    }

    // Startup-only desktop guard: used around startup batch applies to snap back
    // to the original desktop if shell-follow occurred during rule application.
    private int ApplyRulesToAllWindowsWithDesktopGuard(string source)
    {
        int desktopBefore = windowManager.GetCurrentDesktop();
        System.Diagnostics.Debug.WriteLine(
            $"[DesktopGuard] {source}: before apply desktop={desktopBefore}");

        int appliedCount = rulesManager.ApplyRulesToAllWindows();

        if (desktopBefore < 0)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[DesktopGuard] {source}: skipping restore check (invalid desktop before apply).");
            return appliedCount;
        }

        _applyBatchDesktopBefore = desktopBefore;
        _applyBatchSource = source;

        _applyBatchDesktopGuardTimer?.Stop();
        _applyBatchDesktopGuardTimer?.Dispose();
        _applyBatchDesktopGuardTimer = new System.Windows.Forms.Timer { Interval = ApplyBatchDesktopRestoreDelayMs };
        _applyBatchDesktopGuardTimer.Tick += (s, e) =>
        {
            _applyBatchDesktopGuardTimer?.Stop();
            _applyBatchDesktopGuardTimer?.Dispose();
            _applyBatchDesktopGuardTimer = null;

            int expectedDesktop = _applyBatchDesktopBefore;
            string applySource = _applyBatchSource ?? "UnknownSource";
            _applyBatchDesktopBefore = -1;
            _applyBatchSource = null;

            int desktopAfterDelay = windowManager.GetCurrentDesktop();
            System.Diagnostics.Debug.WriteLine(
                $"[DesktopGuard] {applySource}: after {ApplyBatchDesktopRestoreDelayMs}ms desktop={desktopAfterDelay}, expected={expectedDesktop}");

            if (expectedDesktop >= 0 && desktopAfterDelay >= 0 && desktopAfterDelay != expectedDesktop)
            {
                bool switchedBack = windowManager.SwitchToDesktop(expectedDesktop);
                int finalDesktop = windowManager.GetCurrentDesktop();
                System.Diagnostics.Debug.WriteLine(
                    $"[DesktopGuard] {applySource}: restore attempted={switchedBack}, final desktop={finalDesktop}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[DesktopGuard] {applySource}: desktop unchanged; no restore needed.");
            }
        };
        _applyBatchDesktopGuardTimer.Start();

        return appliedCount;
    }

    private void ScheduleStartupMonitoringIfNeeded()
    {
        if (!_pendingStartupMonitorStart || !appSettings.AutoApplyOnLaunch)
            return;

        _pendingStartupMonitorStart = false;
        _startupMonitorDelayTimer?.Stop();
        _startupMonitorDelayTimer?.Dispose();
        _startupMonitorDelayTimer = null;

        int startupProfileCount = appSettings.LaunchProfiles.Count(p => p.LaunchOnStartup);
        if (startupProfileCount > 0)
        {
            // When startup profiles are configured, startup auto-launch timer now
            // starts monitor just before launching apps.
            System.Diagnostics.Debug.WriteLine(
                "[Startup] Startup profiles detected — monitor start is tied to startup auto-launch timer.");
            return;
        }

        // No startup launches pending: start monitor immediately.
        ApplyStartupRulesBeforeMonitoring();
        StartAutoApplyMonitoring();
    }

    private void chkAutoApply_CheckedChanged(object? sender, EventArgs e)
    {
        // Save the setting
        appSettings.AutoApplyOnLaunch = chkAutoApply.Checked;
        persistenceService.SaveSettings(appSettings);

        if (chkAutoApply.Checked)
        {
            _pendingStartupMonitorStart = false;
            _startupMonitorDelayTimer?.Stop();
            _startupMonitorDelayTimer?.Dispose();
            _startupMonitorDelayTimer = null;
            StartAutoApplyMonitoring();
        }
        else
        {
            _pendingStartupMonitorStart = false;
            _startupMonitorDelayTimer?.Stop();
            _startupMonitorDelayTimer?.Dispose();
            _startupMonitorDelayTimer = null;

            // Stop monitoring
            windowMonitor.StopMonitoring();
            UpdateStatus("Auto-apply disabled");
        }
    }

    #endregion

    #region Hotkey Management

    /// <summary>
    /// Load hotkey configurations from disk or create defaults
    /// </summary>
    private void LoadHotkeys()
    {
        hotkeyConfigs = persistenceService.LoadHotkeys();

        // First run: seed the default hotkey rows (all disabled / unassigned)
        // so users can see and configure them immediately in the Hotkeys tab.
        if (hotkeyConfigs.Count == 0)
        {
            hotkeyConfigs = BuildDefaultHotkeyConfigs();
            SaveHotkeys();
            System.Diagnostics.Debug.WriteLine("[HOTKEY] No saved hotkeys found. Seeded default hotkey list (all disabled).");
        }

        LoadHotkeysGrid();
    }

    /// <summary>
    /// Register all enabled hotkeys
    /// </summary>
    private void RegisterHotkeys()
    {
        // Unregister all existing hotkeys first
        hotkeyManager.UnregisterAll();
        registeredHotkeyIds.Clear();

        var failures = new List<string>();

        foreach (var config in hotkeyConfigs.Where(c => c.Enabled && c.Key != Keys.None))
        {
            int hotkeyId = hotkeyManager.RegisterHotkey(
                config.Modifiers,
                config.Key,
                () => ExecuteHotkeyAction(config)
            );

            if (hotkeyId == -1)
            {
                failures.Add($"{config.Name} ({config.GetDisplayString()})");
            }
            else
            {
                registeredHotkeyIds[config.Id] = hotkeyId;
            }
        }

        // Log results
        int registered = registeredHotkeyIds.Count;
        int enabled = hotkeyConfigs.Count(c => c.Enabled && c.Key != Keys.None);

        if (failures.Count > 0)
        {
            string message = $"[HOTKEY] Registered {registered}/{enabled} hotkeys. {failures.Count} failed:\n" +
                            string.Join("\n", failures.Select(f => $"  - {f}"));
            System.Diagnostics.Debug.WriteLine(message);
            UpdateStatus($"{registered}/{enabled} hotkeys registered ({failures.Count} conflicts)");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[HOTKEY] All {registered} enabled hotkeys registered successfully");
            UpdateStatus(registered > 0 ? $"{registered} hotkeys registered" : "No hotkeys registered");
        }
    }

    /// <summary>
    /// Execute the action for a hotkey
    /// </summary>
    private void ExecuteHotkeyAction(HotkeyConfig config)
    {
        try
        {
            switch (config.ActionType)
            {
                case HotkeyActionTypes.SwitchDesktop:
                    if (!string.IsNullOrEmpty(config.ActionParameters) &&
                        int.TryParse(config.ActionParameters, out int desktopIndex))
                    {
                        SwitchToDesktopHotkey(desktopIndex);
                    }
                    break;

                case HotkeyActionTypes.MoveActiveWindow:
                    if (!string.IsNullOrEmpty(config.ActionParameters) &&
                        int.TryParse(config.ActionParameters, out int targetDesktop))
                    {
                        MoveActiveWindowToDesktopHotkey(targetDesktop);
                    }
                    break;

                case HotkeyActionTypes.SwitchToPreviousDesktop:
                    SwitchToPreviousDesktop();
                    break;

                case HotkeyActionTypes.SwitchToNextDesktop:
                    SwitchToNextDesktop();
                    break;

                case HotkeyActionTypes.ApplyAllRules:
                    ApplyAllRulesHotkey();
                    break;

                case HotkeyActionTypes.ApplyRuleToActiveWindow:
                    ApplyRuleToActiveWindowHotkey();
                    break;

                case HotkeyActionTypes.ShowManager:
                    ShowManagerHotkey();
                    break;

                case HotkeyActionTypes.PinActiveWindow:
                    PinActiveWindow();
                    break;

                case HotkeyActionTypes.UnpinActiveWindow:
                    UnpinActiveWindow();
                    break;

                case HotkeyActionTypes.CreateNewDesktop:
                    CreateNewDesktop();
                    break;

                case HotkeyActionTypes.RemoveCurrentDesktop:
                    RemoveCurrentDesktop();
                    break;

                case HotkeyActionTypes.LaunchProfile:
                    if (!string.IsNullOrEmpty(config.ActionParameters))
                    {
                        var profileToLaunch = appSettings.LaunchProfiles.FirstOrDefault(p => p.Id == config.ActionParameters);
                        if (profileToLaunch != null)
                            launcherService.LaunchProfile(profileToLaunch);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HOTKEY] Error executing {config.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Load hotkeys into the DataGridView
    /// </summary>
    private void LoadHotkeysGrid()
    {
        dgvHotkeys.Rows.Clear();
        dgvHotkeys.Columns.Clear();

        // Setup columns
        dgvHotkeys.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Enabled",
            HeaderText = "Enabled",
            Width = 70,
            ReadOnly = false
        });

        dgvHotkeys.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Action",
            HeaderText = "Action",
            Width = 250,
            ReadOnly = true
        });

        dgvHotkeys.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Hotkey",
            HeaderText = "Hotkey",
            Width = 200,
            ReadOnly = true
        });

        dgvHotkeys.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "Status",
            Width = 150,
            ReadOnly = true
        });

        dgvHotkeys.CellValueChanged -= DgvHotkeys_CellValueChanged;
        dgvHotkeys.CellValueChanged += DgvHotkeys_CellValueChanged;
        dgvHotkeys.CurrentCellDirtyStateChanged -= DgvHotkeys_CurrentCellDirtyStateChanged;
        dgvHotkeys.CurrentCellDirtyStateChanged += DgvHotkeys_CurrentCellDirtyStateChanged;

        // Add rows
        foreach (var config in hotkeyConfigs)
        {
            string status = GetHotkeyStatus(config);
            dgvHotkeys.Rows.Add(config.Enabled, config.Name, config.GetDisplayString(), status);
            dgvHotkeys.Rows[dgvHotkeys.Rows.Count - 1].Tag = config;
        }
    }

    private string GetHotkeyStatus(HotkeyConfig config)
    {
        if (!config.Enabled)
            return "Disabled";
        if (config.Key == Keys.None)
            return "Not Set";
        if (registeredHotkeyIds.ContainsKey(config.Id))
            return "Active";
        return "Conflict";
    }

    /// <summary>
    /// Save hotkeys to disk
    /// </summary>
    private void SaveHotkeys()
    {
        persistenceService.SaveHotkeys(hotkeyConfigs);
    }

    /// <summary>
    /// Build the default hotkey list shown to users (all disabled / unassigned).
    /// </summary>
    private List<HotkeyConfig> BuildDefaultHotkeyConfigs()
    {
        var defaults = new List<HotkeyConfig>();

        // Note: All hotkeys are left empty by default as requested.
        // Users can configure them individually.

        // Switch to desktop 1-9
        for (int i = 0; i < 9; i++)
        {
            defaults.Add(new HotkeyConfig
            {
                Name = $"Switch to Desktop {i + 1}",
                ActionType = HotkeyActionTypes.SwitchDesktop,
                ActionParameters = i.ToString(),
                Modifiers = 0,
                Key = Keys.None,
                Enabled = false
            });
        }

        // Move window to desktop 1-9
        for (int i = 0; i < 9; i++)
        {
            defaults.Add(new HotkeyConfig
            {
                Name = $"Move Window to Desktop {i + 1}",
                ActionType = HotkeyActionTypes.MoveActiveWindow,
                ActionParameters = i.ToString(),
                Modifiers = 0,
                Key = Keys.None,
                Enabled = false
            });
        }

        // Previous/Next desktop
        defaults.Add(new HotkeyConfig
        {
            Name = "Switch to Previous Desktop",
            ActionType = HotkeyActionTypes.SwitchToPreviousDesktop,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        defaults.Add(new HotkeyConfig
        {
            Name = "Switch to Next Desktop",
            ActionType = HotkeyActionTypes.SwitchToNextDesktop,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        // Apply rules
        defaults.Add(new HotkeyConfig
        {
            Name = "Apply All Rules",
            ActionType = HotkeyActionTypes.ApplyAllRules,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        defaults.Add(new HotkeyConfig
        {
            Name = "Apply Rule to Active Window",
            ActionType = HotkeyActionTypes.ApplyRuleToActiveWindow,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        // Show manager
        defaults.Add(new HotkeyConfig
        {
            Name = "Show/Focus DeskBulldozer",
            ActionType = HotkeyActionTypes.ShowManager,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        // Pin/Unpin
        defaults.Add(new HotkeyConfig
        {
            Name = "Pin Active Window",
            ActionType = HotkeyActionTypes.PinActiveWindow,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        defaults.Add(new HotkeyConfig
        {
            Name = "Unpin Active Window",
            ActionType = HotkeyActionTypes.UnpinActiveWindow,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        // Create/Remove desktop
        defaults.Add(new HotkeyConfig
        {
            Name = "Create New Desktop",
            ActionType = HotkeyActionTypes.CreateNewDesktop,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        defaults.Add(new HotkeyConfig
        {
            Name = "Remove Current Desktop",
            ActionType = HotkeyActionTypes.RemoveCurrentDesktop,
            Modifiers = 0,
            Key = Keys.None,
            Enabled = false
        });

        return defaults;
    }

    /// <summary>
    /// Reset hotkeys to default configuration
    /// </summary>
    private void ResetHotkeysToDefaults()
    {
        hotkeyConfigs = BuildDefaultHotkeyConfigs();

        SaveHotkeys();
        LoadHotkeysGrid();
        RegisterHotkeys();
    }

    #endregion

    #region Hotkey UI Event Handlers

    private void DgvHotkeys_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        // Don't open editor for checkbox column
        if (dgvHotkeys.Columns[e.ColumnIndex].Name == "Enabled")
            return;

        var row = dgvHotkeys.Rows[e.RowIndex];
        var config = row.Tag as HotkeyConfig;
        if (config == null)
            return;

        using (var dialog = new HotkeyEditorDialog(config.Name, config.Modifiers, config.Key, themeManager))
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                config.Modifiers = dialog.Modifiers;
                config.Key = dialog.Key;

                // Auto-enable if a key was set
                if (config.Key != Keys.None && !config.Enabled)
                {
                    config.Enabled = true;
                }

                SaveHotkeys();
                LoadHotkeysGrid();
                RegisterHotkeys();
            }
        }
    }

    private void DgvHotkeys_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dgvHotkeys.IsCurrentCellDirty)
        {
            dgvHotkeys.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void DgvHotkeys_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        var row = dgvHotkeys.Rows[e.RowIndex];
        var config = row.Tag as HotkeyConfig;
        if (config == null)
            return;

        // Handle enabled checkbox toggle
        if (dgvHotkeys.Columns[e.ColumnIndex].Name == "Enabled")
        {
            config.Enabled = (bool)row.Cells["Enabled"].Value;
            SaveHotkeys();
            RegisterHotkeys();
            LoadHotkeysGrid(); // Refresh to update status
        }
    }

    private void BtnResetHotkeys_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "This will reset all hotkeys to their default state (all disabled).\n\n" +
            "You will need to configure each hotkey individually.\n\n" +
            "Continue?",
            "Reset Hotkeys",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (result == DialogResult.Yes)
        {
            ResetHotkeysToDefaults();
        }
    }

    #endregion

    private void SwitchToDesktopHotkey(int desktopIndex)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => SwitchToDesktopHotkey(desktopIndex)));
            return;
        }

        if (desktopIndex < windowManager.GetDesktopCount())
        {
            windowManager.SwitchToDesktop(desktopIndex);
            UpdateStatus($"Switched to Desktop {desktopIndex + 1}");
            UpdateCurrentDesktop(desktopIndex);
        }
    }

    private void MoveActiveWindowToDesktopHotkey(int desktopIndex)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => MoveActiveWindowToDesktopHotkey(desktopIndex)));
            return;
        }

        if (desktopIndex >= windowManager.GetDesktopCount())
            return;

        // Get the foreground window
        IntPtr hwnd = Win32API.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return;

        // Get window info
        var windows = windowManager.GetAllWindows();
        var window = windows.FirstOrDefault(w => w.Handle == hwnd);

        if (window != null)
        {
            windowManager.MoveWindowToDesktop(window, desktopIndex);
            UpdateStatus($"Moved '{window.ProcessName}' to Desktop {desktopIndex + 1}");
        }
    }

    private void SwitchToPreviousDesktop()
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(SwitchToPreviousDesktop));
            return;
        }

        // current is 0-based; no need to fetch the total count for the prev-desktop guard.
        int current = windowManager.GetCurrentDesktop();

        if (current > 0)
        {
            int target = current - 1; // 0-based index of the desktop to switch to
            windowManager.SwitchToDesktop(target);
            UpdateStatus($"Switched to Desktop {target + 1}"); // 1-based display
            UpdateCurrentDesktop(target);
        }
    }

    private void SwitchToNextDesktop()
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(SwitchToNextDesktop));
            return;
        }

        int current = windowManager.GetCurrentDesktop();
        int count = windowManager.GetDesktopCount();

        if (current < count - 1)
        {
            windowManager.SwitchToDesktop(current + 1);
            UpdateStatus($"Switched to Desktop {current + 2}");
            UpdateCurrentDesktop(current + 1);
        }
    }

    private void ApplyAllRulesHotkey()
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(ApplyAllRulesHotkey));
            return;
        }

        int count = rulesManager.ApplyRulesToAllWindows();
        UpdateStatus($"Applied {count} rules via hotkey");
    }

    private void ApplyRuleToActiveWindowHotkey()
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(ApplyRuleToActiveWindowHotkey));
            return;
        }

        // Get the foreground window
        IntPtr hwnd = Win32API.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return;

        var windows = windowManager.GetAllWindows();
        var window = windows.FirstOrDefault(w => w.Handle == hwnd);

        if (window != null)
        {
            if (rulesManager.ApplyRuleToWindow(window))
            {
                UpdateStatus($"Applied rule to '{window.ProcessName}'");
            }
            else
            {
                UpdateStatus($"No matching rule for '{window.ProcessName}'");
            }
        }
    }

    private void ShowManagerHotkey()
    {
        // Bring the VDManager window to the front
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(ShowManagerHotkey));
            return;
        }

        if (this.WindowState == FormWindowState.Minimized)
        {
            this.WindowState = FormWindowState.Normal;
        }

        this.Activate();
        this.BringToFront();
    }

    #region System Tray

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    private void SetupSystemTray()
    {
        try
        {
            trayIcon = new NotifyIcon
            {
                Text = "DeskBulldozer",
                Visible = false
            };

            // Reuse the form's icon (loaded from app.ico via Icon.ExtractAssociatedIcon in InitializeUI).
            // Fall back to SystemIcons.Application only if somehow not set yet.
            trayIcon.Icon = this.Icon ?? SystemIcons.Application;

            // Double-click to restore
            trayIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                Activate();
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                }
            };

            // Context menu
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add("Show DeskBulldozer", null, (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                Activate();
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                }
            });

            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Apply All Rules", null, (s, e) =>
            {
                int count = rulesManager.ApplyRulesToAllWindows();
                if (trayIcon != null)
                {
                    trayIcon.ShowBalloonTip(2000, "Rules Applied", $"Applied {count} rules to windows", ToolTipIcon.Info);
                }
            });

            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Exit", null, (s, e) =>
            {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                }
                Application.Exit();
            });

            trayIcon.ContextMenuStrip = contextMenu;
            themeManager.ApplyTheme(contextMenu);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TRAY] Error setting up tray icon: {ex.Message}");
        }
    }

    #endregion

    #region Advanced Features

    private void PinActiveWindow()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(PinActiveWindow));
            return;
        }

        IntPtr hwnd = Win32API.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return;

        try
        {
            int result = VirtualDesktopAPI.PinWindow(hwnd);
            var windows = windowManager.GetAllWindows();
            var window = windows.FirstOrDefault(w => w.Handle == hwnd);

            if (window != null)
            {
                UpdateStatus($"Pinned '{window.ProcessName}' to all desktops");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error pinning window: {ex.Message}");
        }
    }

    private void UnpinActiveWindow()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(UnpinActiveWindow));
            return;
        }

        IntPtr hwnd = Win32API.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return;

        try
        {
            int result = VirtualDesktopAPI.UnPinWindow(hwnd);
            var windows = windowManager.GetAllWindows();
            var window = windows.FirstOrDefault(w => w.Handle == hwnd);

            if (window != null)
            {
                UpdateStatus($"Unpinned '{window.ProcessName}' from all desktops");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error unpinning window: {ex.Message}");
        }
    }

    private void CreateNewDesktop()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(CreateNewDesktop));
            return;
        }

        try
        {
            int result = VirtualDesktopAPI.CreateDesktop();
            if (result >= 0)
            {
                RefreshDesktopList();
                UpdateStatus($"Created new desktop (total: {windowManager.GetDesktopCount()})");
            }
            else
            {
                UpdateStatus("Failed to create desktop (Windows 11 only)");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error creating desktop: {ex.Message}");
        }
    }

    private void RemoveCurrentDesktop()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(RemoveCurrentDesktop));
            return;
        }

        try
        {
            int current = windowManager.GetCurrentDesktop();
            int count = windowManager.GetDesktopCount();

            if (count <= 1)
            {
                MessageBox.Show("Cannot remove the last desktop.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Remove Desktop {current + 1}?\n\nWindows on this desktop will move to Desktop 1.",
                "Confirm Remove Desktop",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                int fallback = current > 0 ? current - 1 : 1;
                int removeResult = VirtualDesktopAPI.RemoveDesktop(current, fallback);

                if (removeResult >= 0)
                {
                    RefreshDesktopList();
                    UpdateStatus($"Removed desktop (remaining: {windowManager.GetDesktopCount()})");
                }
                else
                {
                    UpdateStatus("Failed to remove desktop (Windows 11 only)");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error removing desktop: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Launcher Management

    private void LoadLauncher()
    {
        SetupProfilesGrid();
        RefreshProfilesGrid();
        launcherService.RegisterProfileHotkeys(appSettings.LaunchProfiles, hotkeyManager);
        UpdateTrayLauncherSubmenu();
    }

    private void ScheduleStartupAutoLaunch()
    {
        int startupCount = appSettings.LaunchProfiles.Count(p => p.LaunchOnStartup);
        if (startupCount <= 0)
            return;

        System.Diagnostics.Debug.WriteLine(
            $"[Launcher] Scheduling startup auto-launch in {StartupAutoLaunchWarmupMs}ms for tracker warm-up.");

        if (appSettings.ShowBalloonTips)
        {
            trayIcon?.ShowBalloonTip(
                3000,
                "DeskBulldozer Launcher",
                $"Auto-launching {startupCount} profile(s) in {StartupAutoLaunchWarmupMs / 1000}s...",
                ToolTipIcon.Info);
        }

        var timer = new System.Windows.Forms.Timer { Interval = StartupAutoLaunchWarmupMs };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            timer.Dispose();

            // Reliability: ensure monitor is active before startup launches so
            // newly created windows are detected by the "new window" path.
            if (appSettings.AutoApplyOnLaunch && !windowMonitor.IsMonitoring)
            {
                ApplyStartupRulesBeforeMonitoring();
                StartAutoApplyMonitoring();
            }

            launcherService.AutoLaunchStartupProfiles(appSettings.LaunchProfiles);
            UpdateStatus($"Auto-launch queued for {startupCount} profile(s)");
        };
        timer.Start();
    }

    private void SetupProfilesGrid()
    {
        dgvProfiles.AutoGenerateColumns = false;
        dgvProfiles.Columns.Clear();
        dgvProfiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Profile Name", Width = 150, ReadOnly = true });
        dgvProfiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", Width = 200, ReadOnly = true });
        dgvProfiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hotkey", HeaderText = "Hotkey", Width = 120, ReadOnly = true });
        dgvProfiles.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Startup", HeaderText = "Auto-launch", Width = 90, ReadOnly = true });
        dgvProfiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Entries", HeaderText = "# Apps", Width = 60, ReadOnly = true });
    }

    private void RefreshProfilesGrid()
    {
        dgvProfiles.Rows.Clear();
        foreach (var profile in appSettings.LaunchProfiles)
        {
            int idx = dgvProfiles.Rows.Add(profile.Name, profile.Description, profile.GetHotkeyDisplayString(), profile.LaunchOnStartup, profile.Entries.Count);
            dgvProfiles.Rows[idx].Tag = profile;
        }
    }

    private void SaveProfiles()
    {
        persistenceService.SaveSettings(appSettings);
        launcherService.UnregisterProfileHotkeys(appSettings.LaunchProfiles, hotkeyManager);
        launcherService.RegisterProfileHotkeys(appSettings.LaunchProfiles, hotkeyManager);
        UpdateTrayLauncherSubmenu();
    }

    private void UpdateTrayLauncherSubmenu()
    {
        if (trayIcon?.ContextMenuStrip == null) return;
        ToolStripItem? existing = trayIcon.ContextMenuStrip.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Name == "mnuLaunchProfile");
        ToolStripItem? existingSep = trayIcon.ContextMenuStrip.Items.OfType<ToolStripItem>().FirstOrDefault(i => i.Name == "mnuLaunchProfileSep");
        if (existing != null) trayIcon.ContextMenuStrip.Items.Remove(existing);
        if (existingSep != null) trayIcon.ContextMenuStrip.Items.Remove(existingSep);
        if (appSettings.LaunchProfiles.Count == 0) return;
        var submenu = new ToolStripMenuItem("Launch Profile") { Name = "mnuLaunchProfile" };
        foreach (var profile in appSettings.LaunchProfiles)
        {
            var capturedProfile = profile;
            string label = profile.HotkeyKey != Keys.None ? $"{profile.Name}  [{profile.GetHotkeyDisplayString()}]" : profile.Name;
            submenu.DropDownItems.Add(label, null, (s, e) => { launcherService.LaunchProfile(capturedProfile); UpdateStatus($"Launched profile: {capturedProfile.Name}"); });
        }
        int exitIdx = trayIcon.ContextMenuStrip.Items.OfType<ToolStripItem>().ToList().FindIndex(i => i is ToolStripMenuItem m && m.Text == "Exit");
        int insertAt = exitIdx > 0 ? exitIdx - 1 : trayIcon.ContextMenuStrip.Items.Count - 1;
        var sep = new ToolStripSeparator { Name = "mnuLaunchProfileSep" };
        trayIcon.ContextMenuStrip.Items.Insert(insertAt, sep);
        trayIcon.ContextMenuStrip.Items.Insert(insertAt, submenu);
        themeManager.ApplyTheme(trayIcon.ContextMenuStrip);
    }

    private void BtnAddProfile_Click(object? sender, EventArgs e)
    {
        using var dlg = new LaunchProfileEditorDialog(windowManager.GetDesktopCount(), rulesManager.GetAllRules(), themeManager: themeManager);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            appSettings.LaunchProfiles.Add(dlg.Profile);
            SaveProfiles();
            RefreshProfilesGrid();
            UpdateStatus($"Profile '{dlg.Profile.Name}' added");
        }
    }

    private void BtnEditProfile_Click(object? sender, EventArgs e)
    {
        if (GetSelectedProfile() is not LaunchProfile profile) return;
        using var dlg = new LaunchProfileEditorDialog(windowManager.GetDesktopCount(), rulesManager.GetAllRules(), profile, themeManager);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            int idx = appSettings.LaunchProfiles.IndexOf(profile);
            if (idx >= 0) appSettings.LaunchProfiles[idx] = dlg.Profile;
            SaveProfiles();
            RefreshProfilesGrid();
            UpdateStatus($"Profile '{dlg.Profile.Name}' updated");
        }
    }

    private void BtnDeleteProfile_Click(object? sender, EventArgs e)
    {
        if (GetSelectedProfile() is not LaunchProfile profile) return;
        var result = MessageBox.Show($"Delete profile '{profile.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            if (profile.RegisteredHotkeyId >= 0) hotkeyManager.UnregisterHotkey(profile.RegisteredHotkeyId);
            appSettings.LaunchProfiles.Remove(profile);
            SaveProfiles();
            RefreshProfilesGrid();
            UpdateStatus($"Profile '{profile.Name}' deleted");
        }
    }

    private void BtnLaunchProfile_Click(object? sender, EventArgs e)
    {
        if (GetSelectedProfile() is not LaunchProfile profile) return;
        int count = launcherService.LaunchProfile(profile);
        UpdateStatus($"Launching '{profile.Name}' ({count} apps)...");
        if (appSettings.ShowBalloonTips)
            trayIcon?.ShowBalloonTip(2000, "Launching Profile", $"'{profile.Name}' — {count} app(s) queued", ToolTipIcon.Info);
    }

    private void DgvProfiles_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        BtnEditProfile_Click(sender, e);
    }

    private LaunchProfile? GetSelectedProfile()
    {
        if (dgvProfiles.SelectedRows.Count == 0) return null;
        return dgvProfiles.SelectedRows[0].Tag as LaunchProfile;
    }

    #endregion

    #region Settings & Theme

    private void LoadSettings()
    {
        // Settings are already loaded in LoadAutoApplySettings
        // Apply theme preference
        var theme = (ThemeManager.Theme)appSettings.ThemePreference;
        themeManager.SetTheme(theme);

        // Update UI to match settings
        rbThemeLight.Checked = (theme == ThemeManager.Theme.Light);
        rbThemeDark.Checked = (theme == ThemeManager.Theme.Dark);
        rbThemeSystem.Checked = (theme == ThemeManager.Theme.System);

        // Startup setting
        chkStartWithWindows.Checked = StartupManager.IsStartupEnabled();
        appSettings.StartWithWindows = chkStartWithWindows.Checked;

        // Tray settings
        chkMinimizeToTray.Checked = appSettings.MinimizeToTray;
        chkCloseToTray.Checked = appSettings.CloseToTray;
        chkShowBalloonTips.Checked = appSettings.ShowBalloonTips;

        // Load font preference
        if (!string.IsNullOrEmpty(appSettings.FontName))
        {
            cmbFont.SelectedItem = appSettings.FontName;
            ApplyFont(appSettings.FontName);
        }
        else
        {
            // Default to Segoe UI
            cmbFont.SelectedItem = "Segoe UI";
        }

        // Load desktop switch timeout (clamp to the spinner's valid range)
        int timeoutValue = Math.Max((int)nudDesktopSwitchTimeout.Minimum,
                            Math.Min((int)nudDesktopSwitchTimeout.Maximum,
                                     appSettings.DesktopSwitchTimeoutMs));
        nudDesktopSwitchTimeout.Value = timeoutValue;
        windowManager.DesktopSwitchTimeoutMs = timeoutValue;

        // Load enforcement settings
        if (chkEnforcementEnabled != null)
        {
            chkEnforcementEnabled.Checked = appSettings.PositionEnforcementEnabled;
            windowMonitor.EnforcementEnabled = appSettings.PositionEnforcementEnabled;
        }
        if (chkSkipMinimized != null)
        {
            chkSkipMinimized.Checked = appSettings.SkipEnforcementWhenMinimized;
            windowMonitor.SkipEnforcementWhenMinimized = appSettings.SkipEnforcementWhenMinimized;
        }
        if (nudGracePeriod != null)
        {
            nudGracePeriod.Value = Math.Max(nudGracePeriod.Minimum,
                Math.Min(nudGracePeriod.Maximum, appSettings.EnforcementGracePeriodMs));
            windowMonitor.GracePeriodMs = (int)nudGracePeriod.Value;
        }
        if (nudCooldown != null)
        {
            nudCooldown.Value = Math.Max(nudCooldown.Minimum,
                Math.Min(nudCooldown.Maximum, appSettings.EnforcementCooldownMs));
            windowMonitor.CooldownMs = (int)nudCooldown.Value;
        }

        // Load new-window rule delay
        if (nudNewWindowDelay != null)
        {
            nudNewWindowDelay.Value = Math.Max(nudNewWindowDelay.Minimum,
                Math.Min(nudNewWindowDelay.Maximum, appSettings.NewWindowRuleDelayMs));
            windowMonitor.NewWindowRuleDelayMs = (int)nudNewWindowDelay.Value;
        }
    }

    private void SetupEnforcementUI()
    {
        // All pixel values below are logical (96-DPI) values.
        // LogicalToDeviceUnits() converts them to physical pixels for the
        // current DPI, so the group box looks correct at any scaling level.
        int x = LogicalToDeviceUnits(340);
        int y = LogicalToDeviceUnits(20);
        int grpW = LogicalToDeviceUnits(380);
        int grpH = LogicalToDeviceUnits(155); // Increased height for new checkbox
        int innerX = LogicalToDeviceUnits(10);
        int lblW = LogicalToDeviceUnits(130);
        int lblH = LogicalToDeviceUnits(20);
        int nudX = LogicalToDeviceUnits(145);
        int nudW = LogicalToDeviceUnits(80);
        int nudH = LogicalToDeviceUnits(23);
        int chkW = LogicalToDeviceUnits(350);
        int chkH = LogicalToDeviceUnits(20);

        var grpEnforcement = new GroupBox
        {
            Text = "Position Enforcement (Snap-Back)",
            Location = new System.Drawing.Point(x, y),
            Size = new System.Drawing.Size(grpW, grpH),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        chkEnforcementEnabled = new CheckBox
        {
            Text = "Enable global position enforcement",
            Location = new System.Drawing.Point(innerX, LogicalToDeviceUnits(25)),
            Size = new System.Drawing.Size(chkW, chkH),
            Checked = true
        };
        chkEnforcementEnabled.CheckedChanged += chkEnforcementEnabled_CheckedChanged;

        chkSkipMinimized = new CheckBox
        {
            Text = "Skip enforcement for minimized windows",
            Location = new System.Drawing.Point(innerX, LogicalToDeviceUnits(50)),
            Size = new System.Drawing.Size(chkW, chkH),
            Checked = true
        };
        chkSkipMinimized.CheckedChanged += chkSkipMinimized_CheckedChanged;

        var lblGrace = new Label
        {
            Text = "Grace period (ms):",
            Location = new System.Drawing.Point(innerX, LogicalToDeviceUnits(80)),
            Size = new System.Drawing.Size(lblW, lblH),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        nudGracePeriod = new NumericUpDown
        {
            Location = new System.Drawing.Point(nudX, LogicalToDeviceUnits(78)),
            Size = new System.Drawing.Size(nudW, nudH),
            Minimum = 500,
            Maximum = 30000,
            Increment = 500,
            Value = 3000
        };
        nudGracePeriod.ValueChanged += nudGracePeriod_ValueChanged;

        var lblCooldown = new Label
        {
            Text = "Cooldown (ms):",
            Location = new System.Drawing.Point(innerX, LogicalToDeviceUnits(112)),
            Size = new System.Drawing.Size(lblW, lblH),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        nudCooldown = new NumericUpDown
        {
            Location = new System.Drawing.Point(nudX, LogicalToDeviceUnits(110)),
            Size = new System.Drawing.Size(nudW, nudH),
            Minimum = 200,
            Maximum = 30000,
            Increment = 200,
            Value = 1000
        };
        nudCooldown.ValueChanged += nudCooldown_ValueChanged;

        grpEnforcement.Controls.AddRange(new Control[]
        {
            chkEnforcementEnabled, chkSkipMinimized, lblGrace, nudGracePeriod, lblCooldown, nudCooldown
        });

        tabSettings.Controls.Add(grpEnforcement);

        // ── Window Detection Timing group box ────────────────────────────────
        int detY = y + grpH + LogicalToDeviceUnits(10);
        int detGrpH = LogicalToDeviceUnits(65);

        var grpDetection = new GroupBox
        {
            Text = "Window Detection Timing",
            Location = new System.Drawing.Point(x, detY),
            Size = new System.Drawing.Size(grpW, detGrpH),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        var lblNewWindowDelay = new Label
        {
            Text = "New window rule delay (ms):",
            Location = new System.Drawing.Point(innerX, LogicalToDeviceUnits(28)),
            Size = new System.Drawing.Size(LogicalToDeviceUnits(170), lblH),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        nudNewWindowDelay = new NumericUpDown
        {
            Location = new System.Drawing.Point(LogicalToDeviceUnits(185), LogicalToDeviceUnits(26)),
            Size = new System.Drawing.Size(nudW, nudH),
            Minimum = 50,
            Maximum = 5000,
            Increment = 50,
            Value = 500
        };
        nudNewWindowDelay.ValueChanged += nudNewWindowDelay_ValueChanged;

        grpDetection.Controls.AddRange(new Control[] { lblNewWindowDelay, nudNewWindowDelay });
        tabSettings.Controls.Add(grpDetection);

        CreateLayoutTestGroup();
    }

    private void CreateLayoutTestGroup()
    {
        // Position below all existing settings controls
        int lowestY = 0;
        foreach (Control c in tabSettings.Controls)
            lowestY = Math.Max(lowestY, c.Bottom);

        int x      = LogicalToDeviceUnits(20);
        int y      = lowestY + LogicalToDeviceUnits(10);
        int grpW   = LogicalToDeviceUnits(700);
        int grpH   = LogicalToDeviceUnits(80);
        int innerX = LogicalToDeviceUnits(10);

        var grpTest = new GroupBox
        {
            Text     = "Layout Test",
            Location = new System.Drawing.Point(x, y),
            Size     = new System.Drawing.Size(grpW, grpH),
            Anchor   = AnchorStyles.Top | AnchorStyles.Left
        };

        btnRunLayoutTest = new Button
        {
            Text     = "Run Layout Test",
            Location = new System.Drawing.Point(innerX, LogicalToDeviceUnits(25)),
            Size     = new System.Drawing.Size(LogicalToDeviceUnits(140), LogicalToDeviceUnits(30))
        };
        btnRunLayoutTest.Click += btnRunLayoutTest_Click;

        lblLayoutTestStatus = new Label
        {
            Text      = "Cycles through all monitors × virtual desktops, tiling 4 notepad windows per step.",
            Location  = new System.Drawing.Point(LogicalToDeviceUnits(160), LogicalToDeviceUnits(32)),
            Size      = new System.Drawing.Size(LogicalToDeviceUnits(520), LogicalToDeviceUnits(20)),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };

        grpTest.Controls.AddRange(new Control[] { btnRunLayoutTest, lblLayoutTestStatus });
        tabSettings.Controls.Add(grpTest);
    }

    private async void btnRunLayoutTest_Click(object? sender, EventArgs e)
    {
        if (_layoutTestCts != null)
        {
            _layoutTestCts.Cancel();
            return;
        }

        _layoutTestCts = new CancellationTokenSource();
        btnRunLayoutTest!.Text = "Stop Test";

        try
        {
            await RunLayoutTestAsync(_layoutTestCts.Token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            _layoutTestCts.Dispose();
            _layoutTestCts = null;
            btnRunLayoutTest!.Text = "Run Layout Test";
            SetLayoutTestStatus("Cycles through all monitors × virtual desktops, tiling 4 notepad windows per step.");
        }
    }

    private async Task RunLayoutTestAsync(CancellationToken ct)
    {
        int monitorCount    = QuadrantLayout.GetMonitorCount();
        int desktopCount    = windowManager.GetDesktopCount();
        int originalDesktop = windowManager.GetCurrentDesktop();

        var quadrants = new[]
        {
            Quadrant.TopLeft, Quadrant.TopRight,
            Quadrant.BottomLeft, Quadrant.BottomRight
        };

        // Snapshot existing Notepad handles before launching
        var existingHandles = windowManager.GetWindowsForProcess("Notepad")
            .Select(w => w.Handle)
            .ToHashSet();

        // Launch 4 notepads (stub processes — actual windows belong to different PIDs
        // because Windows 11 notepad is a packaged app activated via a System32 stub)
        SetLayoutTestStatus("Launching notepad windows…");
        var procs = new System.Diagnostics.Process[4];
        for (int i = 0; i < 4; i++)
            procs[i] = System.Diagnostics.Process.Start("notepad.exe")!;

        // Wait for 4 NEW Notepad windows (snapshot diff by process name, not PID)
        IntPtr[]? hwnds = await WaitForNewNotepadWindows(existingHandles, 4, 10000, ct);

        if (hwnds == null)
        {
            SetLayoutTestStatus("Could not find 4 new notepad windows. Test aborted.");
            foreach (var p in procs) try { p.Dispose(); } catch { }
            return;
        }

        try
        {
            for (int d = 0; d < desktopCount; d++)
            {
                ct.ThrowIfCancellationRequested();

                // Switch the active virtual desktop first
                windowManager.SwitchToDesktop(d);
                await Task.Delay(400, ct);

                // Move all 4 notepads to this desktop directly via VDA —
                // MoveWindowToDesktop has an IsWindowVisible guard that rejects
                // windows on non-active desktops, so we call the VDA API directly.
                for (int i = 0; i < 4; i++)
                    VirtualDesktopAPI.MoveWindowToDesktopNumber(hwnds[i], d);

                // Give the shell time to process the desktop reassignments
                await Task.Delay(300, ct);

                for (int m = 0; m < monitorCount; m++)
                {
                    ct.ThrowIfCancellationRequested();

                    SetLayoutTestStatus(
                        $"Desktop {d + 1}/{desktopCount}  ·  Monitor {m + 1}/{monitorCount}  —  positioning…");

                    for (int i = 0; i < 4; i++)
                        windowManager.PositionWindowInQuadrant(hwnds[i], quadrants[i], m);

                    SetLayoutTestStatus(
                        $"Desktop {d + 1}/{desktopCount}  ·  Monitor {m + 1}/{monitorCount}  —  5 s…");

                    await Task.Delay(5000, ct);
                }
            }

            SetLayoutTestStatus("Test complete.");
        }
        finally
        {
            // Kill by the window's actual owning process (not the now-dead stub procs)
            foreach (var hwnd in hwnds)
            {
                try
                {
                    Win32API.GetWindowThreadProcessId(hwnd, out uint pid);
                    if (pid != 0)
                        using (var p = System.Diagnostics.Process.GetProcessById((int)pid))
                            if (!p.HasExited) p.Kill();
                }
                catch { }
            }

            foreach (var p in procs) try { p.Dispose(); } catch { }

            if (originalDesktop >= 0)
                windowManager.SwitchToDesktop(originalDesktop);
        }
    }

    private void SetLayoutTestStatus(string text)
    {
        if (lblLayoutTestStatus!.InvokeRequired)
            lblLayoutTestStatus.Invoke((Action)(() => lblLayoutTestStatus.Text = text));
        else
            lblLayoutTestStatus.Text = text;
    }

    private async Task<IntPtr[]?> WaitForNewNotepadWindows(
        HashSet<IntPtr> existingHandles, int count, int timeoutMs, CancellationToken ct)
    {
        int elapsed = 0;
        while (elapsed < timeoutMs)
        {
            ct.ThrowIfCancellationRequested();
            var newHandles = windowManager.GetWindowsForProcess("Notepad")
                .Where(w => !existingHandles.Contains(w.Handle))
                .Select(w => w.Handle)
                .ToArray();
            if (newHandles.Length >= count)
                return newHandles.Take(count).ToArray();
            await Task.Delay(200, ct);
            elapsed += 200;
        }
        return null;
    }

    private void chkEnforcementEnabled_CheckedChanged(object? sender, EventArgs e)
    {
        bool enabled = chkEnforcementEnabled!.Checked;
        appSettings.PositionEnforcementEnabled = enabled;
        windowMonitor.EnforcementEnabled = enabled;
        if (!enabled)
            windowMonitor.ClearAllEnforcedWindows();
        persistenceService.SaveSettings(appSettings);
    }

    private void nudGracePeriod_ValueChanged(object? sender, EventArgs e)
    {
        int value = (int)nudGracePeriod!.Value;
        appSettings.EnforcementGracePeriodMs = value;
        windowMonitor.GracePeriodMs = value;
        persistenceService.SaveSettings(appSettings);
    }

    private void nudCooldown_ValueChanged(object? sender, EventArgs e)
    {
        int value = (int)nudCooldown!.Value;
        appSettings.EnforcementCooldownMs = value;
        windowMonitor.CooldownMs = value;
        persistenceService.SaveSettings(appSettings);
    }

    private void chkSkipMinimized_CheckedChanged(object? sender, EventArgs e)
    {
        bool enabled = chkSkipMinimized!.Checked;
        appSettings.SkipEnforcementWhenMinimized = enabled;
        windowMonitor.SkipEnforcementWhenMinimized = enabled;
        persistenceService.SaveSettings(appSettings);
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        themeManager.ApplyTheme(this);
    }

    private void ApplyFont(string fontName)
    {
        try
        {
            // Apply font globally to form (cascades to all controls)
            this.Font = new Font(fontName, 10F, FontStyle.Regular);

            // Update headers to use new font with Bold style
            lblWindowListHeader.Font = new Font(fontName, 11F, FontStyle.Bold);
            lblWindowLayoutHeader.Font = new Font(fontName, 11F, FontStyle.Bold);
            lblRulesHeader.Font = new Font(fontName, 11F, FontStyle.Bold);
            lblQuadrant.Font = new Font(fontName, 11F, FontStyle.Bold);
            lblCurrentDesktop.Font = new Font(fontName, 11F, FontStyle.Bold);

            // Keep About tab large font
            lblAbout.Font = new Font(fontName, 16F, FontStyle.Regular);
        }
        catch (Exception ex)
        {
            // If font fails to load, fall back to Segoe UI
            MessageBox.Show($"Failed to apply font '{fontName}'. Reverting to Segoe UI.\n\nError: {ex.Message}",
                "Font Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            appSettings.FontName = "Segoe UI";
            this.Font = new Font("Segoe UI", 10F);
        }
    }

    private void cmbFont_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbFont.SelectedItem == null)
            return;

        string selectedFont = cmbFont.SelectedItem.ToString()!;

        // Update settings
        appSettings.FontName = selectedFont;
        persistenceService.SaveSettings(appSettings);

        // Apply font immediately
        ApplyFont(selectedFont);
    }

    private void rbTheme_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is RadioButton rb && rb.Checked)
        {
            ThemeManager.Theme theme;
            if (rb == rbThemeLight)
                theme = ThemeManager.Theme.Light;
            else if (rb == rbThemeDark)
                theme = ThemeManager.Theme.Dark;
            else
                theme = ThemeManager.Theme.System;

            themeManager.SetTheme(theme);
            appSettings.ThemePreference = (int)theme;
            persistenceService.SaveSettings(appSettings);
        }
    }

    private void chkStartWithWindows_CheckedChanged(object? sender, EventArgs e)
    {
        bool enabled = chkStartWithWindows.Checked;
        if (StartupManager.SetStartupEnabled(enabled, out string errorMessage))
        {
            appSettings.StartWithWindows = enabled;
            persistenceService.SaveSettings(appSettings);
            UpdateStatus(enabled ? "Startup enabled" : "Startup disabled");
        }
        else
        {
            // Revert checkbox if failed
            chkStartWithWindows.CheckedChanged -= chkStartWithWindows_CheckedChanged;
            chkStartWithWindows.Checked = !enabled;
            chkStartWithWindows.CheckedChanged += chkStartWithWindows_CheckedChanged;

            MessageBox.Show($"Failed to update startup settings:\n{errorMessage}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void chkMinimizeToTray_CheckedChanged(object? sender, EventArgs e)
    {
        appSettings.MinimizeToTray = chkMinimizeToTray.Checked;
        persistenceService.SaveSettings(appSettings);
    }

    private void chkCloseToTray_CheckedChanged(object? sender, EventArgs e)
    {
        appSettings.CloseToTray = chkCloseToTray.Checked;
        persistenceService.SaveSettings(appSettings);
    }

    private void chkShowBalloonTips_CheckedChanged(object? sender, EventArgs e)
    {
        appSettings.ShowBalloonTips = chkShowBalloonTips.Checked;
        persistenceService.SaveSettings(appSettings);
    }

    private void nudDesktopSwitchTimeout_ValueChanged(object? sender, EventArgs e)
    {
        int newTimeout = (int)nudDesktopSwitchTimeout.Value;
        appSettings.DesktopSwitchTimeoutMs = newTimeout;
        windowManager.DesktopSwitchTimeoutMs = newTimeout;
        persistenceService.SaveSettings(appSettings);
    }

    private void nudNewWindowDelay_ValueChanged(object? sender, EventArgs e)
    {
        int value = (int)nudNewWindowDelay!.Value;
        appSettings.NewWindowRuleDelayMs = value;
        windowMonitor.NewWindowRuleDelayMs = value;
        persistenceService.SaveSettings(appSettings);
    }

    #endregion

    private void cmbMonitor_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void cmbTargetDesktop_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void lblTargetDesktop_Click(object sender, EventArgs e)
    {

    }

    private void lblAbout_Click(object sender, EventArgs e)
    {

    }
}