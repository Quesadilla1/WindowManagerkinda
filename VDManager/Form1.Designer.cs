namespace VDManager;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        tabControl = new TabControl();
        tabWindows = new TabPage();
        pnlWindowLayout = new Panel();
        lblTargetDesktop = new Label();
        cmbTargetDesktop = new ComboBox();
        visualQuadrantPanel = new VDManager.Controls.VisualQuadrantPanel();
        lblMonitor = new Label();
        cmbMonitor = new ComboBox();
        btnMoveWindow = new Button();
        lblQuadrant = new Label();
        btnAddAsRule = new Button();
        lblWindowLayoutHeader = new Label();
        pnlWindowList = new Panel();
        lstWindows = new ListBox();
        lblWindowListHeader = new Label();
        btnRefresh = new Button();
        tabRules = new TabPage();
        pnlRuleControls = new Panel();
        btnAddRule = new Button();
        btnEditRule = new Button();
        btnDeleteRule = new Button();
        chkAutoApply = new CheckBox();
        btnApplyRules = new Button();
        pnlRulesList = new Panel();
        dgvRules = new DataGridView();
        lblRulesHeader = new Label();
        tabHotkeys = new TabPage();
        dgvHotkeys = new DataGridView();
        btnResetHotkeys = new Button();
        lblHotkeysInfo = new Label();
        tabLauncher = new TabPage();
        lblLauncherInfo = new Label();
        dgvProfiles = new DataGridView();
        btnAddProfile = new Button();
        btnEditProfile = new Button();
        btnDeleteProfile = new Button();
        btnLaunchProfile = new Button();
        tabSettings = new TabPage();
        grpTheme = new GroupBox();
        rbThemeSystem = new RadioButton();
        rbThemeDark = new RadioButton();
        rbThemeLight = new RadioButton();
        grpStartup = new GroupBox();
        chkStartWithWindows = new CheckBox();
        grpTray = new GroupBox();
        chkMinimizeToTray = new CheckBox();
        chkCloseToTray = new CheckBox();
        chkShowBalloonTips = new CheckBox();
        grpFont = new GroupBox();
        lblFont = new Label();
        cmbFont = new ComboBox();
        grpAdvanced = new GroupBox();
        lblDesktopSwitchTimeout = new Label();
        nudDesktopSwitchTimeout = new NumericUpDown();
        tabAbout = new TabPage();
        lblAbout = new Label();
        lblThirdParty = new Label();
        rtbLicense = new RichTextBox();
        lblStatus = new Label();
        lblWindows = new Label();
        lblCurrentDesktop = new Label();
        tabControl.SuspendLayout();
        tabWindows.SuspendLayout();
        pnlWindowLayout.SuspendLayout();
        pnlWindowList.SuspendLayout();
        tabRules.SuspendLayout();
        pnlRuleControls.SuspendLayout();
        pnlRulesList.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRules).BeginInit();
        tabHotkeys.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvHotkeys).BeginInit();
        tabLauncher.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProfiles).BeginInit();
        tabSettings.SuspendLayout();
        grpTheme.SuspendLayout();
        grpStartup.SuspendLayout();
        grpTray.SuspendLayout();
        grpFont.SuspendLayout();
        grpAdvanced.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudDesktopSwitchTimeout).BeginInit();
        tabAbout.SuspendLayout();
        SuspendLayout();
        // 
        // tabControl
        // 
        tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabControl.Controls.Add(tabWindows);
        tabControl.Controls.Add(tabRules);
        tabControl.Controls.Add(tabHotkeys);
        tabControl.Controls.Add(tabLauncher);
        tabControl.Controls.Add(tabSettings);
        tabControl.Controls.Add(tabAbout);
        tabControl.Location = new Point(12, 41);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(766, 713);
        tabControl.TabIndex = 0;
        // 
        // tabWindows
        // 
        tabWindows.Controls.Add(pnlWindowLayout);
        tabWindows.Controls.Add(pnlWindowList);
        tabWindows.Location = new Point(4, 26);
        tabWindows.Name = "tabWindows";
        tabWindows.Padding = new Padding(12);
        tabWindows.Size = new Size(758, 683);
        tabWindows.TabIndex = 0;
        tabWindows.Text = "Windows";
        tabWindows.UseVisualStyleBackColor = true;
        // 
        // pnlWindowLayout
        // 
        pnlWindowLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlWindowLayout.BorderStyle = BorderStyle.FixedSingle;
        pnlWindowLayout.Controls.Add(lblTargetDesktop);
        pnlWindowLayout.Controls.Add(cmbTargetDesktop);
        pnlWindowLayout.Controls.Add(visualQuadrantPanel);
        pnlWindowLayout.Controls.Add(lblMonitor);
        pnlWindowLayout.Controls.Add(cmbMonitor);
        pnlWindowLayout.Controls.Add(btnMoveWindow);
        pnlWindowLayout.Controls.Add(lblQuadrant);
        pnlWindowLayout.Controls.Add(btnAddAsRule);
        pnlWindowLayout.Controls.Add(lblWindowLayoutHeader);
        pnlWindowLayout.Location = new Point(12, 263);
        pnlWindowLayout.Name = "pnlWindowLayout";
        pnlWindowLayout.Padding = new Padding(10);
        pnlWindowLayout.Size = new Size(718, 390);
        pnlWindowLayout.TabIndex = 1;
        // 
        // lblTargetDesktop
        // 
        lblTargetDesktop.AutoSize = true;
        lblTargetDesktop.Location = new Point(9, 48);
        lblTargetDesktop.Name = "lblTargetDesktop";
        lblTargetDesktop.Size = new Size(104, 19);
        lblTargetDesktop.TabIndex = 1;
        lblTargetDesktop.Text = "Target Desktop:";
        lblTargetDesktop.Click += lblTargetDesktop_Click;
        // 
        // cmbTargetDesktop
        // 
        cmbTargetDesktop.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTargetDesktop.FormattingEnabled = true;
        cmbTargetDesktop.Location = new Point(143, 48);
        cmbTargetDesktop.Name = "cmbTargetDesktop";
        cmbTargetDesktop.Size = new Size(140, 25);
        cmbTargetDesktop.TabIndex = 2;
        cmbTargetDesktop.SelectedIndexChanged += cmbTargetDesktop_SelectedIndexChanged;
        // 
        // visualQuadrantPanel
        // 
        visualQuadrantPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        visualQuadrantPanel.BackColor = SystemColors.Control;
        visualQuadrantPanel.BorderStyle = BorderStyle.FixedSingle;
        visualQuadrantPanel.Location = new Point(9, 122);
        visualQuadrantPanel.Name = "visualQuadrantPanel";
        visualQuadrantPanel.Padding = new Padding(8);
        visualQuadrantPanel.SelectedQuadrant = Quadrant.TopLeft;
        visualQuadrantPanel.Size = new Size(692, 253);
        visualQuadrantPanel.TabIndex = 6;
        visualQuadrantPanel.QuadrantChanged += visualQuadrantPanel_QuadrantChanged;
        // 
        // lblMonitor
        // 
        lblMonitor.AutoSize = true;
        lblMonitor.Location = new Point(9, 91);
        lblMonitor.Name = "lblMonitor";
        lblMonitor.Size = new Size(62, 19);
        lblMonitor.TabIndex = 3;
        lblMonitor.Text = "Monitor:";
        // 
        // cmbMonitor
        // 
        cmbMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbMonitor.FormattingEnabled = true;
        cmbMonitor.Location = new Point(143, 91);
        cmbMonitor.Name = "cmbMonitor";
        cmbMonitor.Size = new Size(232, 25);
        cmbMonitor.TabIndex = 4;
        cmbMonitor.SelectedIndexChanged += cmbMonitor_SelectedIndexChanged;
        // 
        // btnMoveWindow
        // 
        btnMoveWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnMoveWindow.Location = new Point(551, 13);
        btnMoveWindow.Name = "btnMoveWindow";
        btnMoveWindow.Size = new Size(150, 30);
        btnMoveWindow.TabIndex = 7;
        btnMoveWindow.Text = "Move && Arrange";
        btnMoveWindow.UseVisualStyleBackColor = true;
        btnMoveWindow.Click += btnMoveWindow_Click;
        // 
        // lblQuadrant
        // 
        lblQuadrant.AutoSize = true;
        lblQuadrant.Location = new Point(10, 180);
        lblQuadrant.Name = "lblQuadrant";
        lblQuadrant.Size = new Size(114, 19);
        lblQuadrant.TabIndex = 5;
        lblQuadrant.Text = "Window Position:";
        // 
        // btnAddAsRule
        // 
        btnAddAsRule.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAddAsRule.Enabled = false;
        btnAddAsRule.Location = new Point(551, 48);
        btnAddAsRule.Name = "btnAddAsRule";
        btnAddAsRule.Size = new Size(150, 30);
        btnAddAsRule.TabIndex = 9;
        btnAddAsRule.Text = "➕ Add as Rule";
        btnAddAsRule.UseVisualStyleBackColor = true;
        btnAddAsRule.Click += btnAddAsRule_Click;
        // 
        // lblWindowLayoutHeader
        // 
        lblWindowLayoutHeader.AutoSize = true;
        lblWindowLayoutHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblWindowLayoutHeader.Location = new Point(-1, 10);
        lblWindowLayoutHeader.Name = "lblWindowLayoutHeader";
        lblWindowLayoutHeader.Size = new Size(135, 20);
        lblWindowLayoutHeader.TabIndex = 0;
        lblWindowLayoutHeader.Text = "Position && Layout";
        // 
        // pnlWindowList
        // 
        pnlWindowList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlWindowList.BorderStyle = BorderStyle.FixedSingle;
        pnlWindowList.Controls.Add(lstWindows);
        pnlWindowList.Controls.Add(lblWindowListHeader);
        pnlWindowList.Controls.Add(btnRefresh);
        pnlWindowList.Location = new Point(12, 12);
        pnlWindowList.Name = "pnlWindowList";
        pnlWindowList.Padding = new Padding(10);
        pnlWindowList.Size = new Size(718, 245);
        pnlWindowList.TabIndex = 0;
        // 
        // lstWindows
        // 
        lstWindows.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lstWindows.FormattingEnabled = true;
        lstWindows.ItemHeight = 17;
        lstWindows.Location = new Point(10, 30);
        lstWindows.Name = "lstWindows";
        lstWindows.Size = new Size(695, 157);
        lstWindows.TabIndex = 1;
        lstWindows.SelectedIndexChanged += lstWindows_SelectedIndexChanged;
        // 
        // lblWindowListHeader
        // 
        lblWindowListHeader.AutoSize = true;
        lblWindowListHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblWindowListHeader.Location = new Point(0, 0);
        lblWindowListHeader.Name = "lblWindowListHeader";
        lblWindowListHeader.Size = new Size(115, 20);
        lblWindowListHeader.TabIndex = 0;
        lblWindowListHeader.Text = "Open Windows";
        // 
        // btnRefresh
        // 
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefresh.Location = new Point(551, 200);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(150, 30);
        btnRefresh.TabIndex = 8;
        btnRefresh.Text = "Refresh";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;
        // 
        // tabRules
        // 
        tabRules.Controls.Add(pnlRuleControls);
        tabRules.Controls.Add(pnlRulesList);
        tabRules.Location = new Point(4, 24);
        tabRules.Name = "tabRules";
        tabRules.Padding = new Padding(12);
        tabRules.Size = new Size(758, 685);
        tabRules.TabIndex = 1;
        tabRules.Text = "Rules";
        tabRules.UseVisualStyleBackColor = true;
        // 
        // pnlRuleControls
        // 
        pnlRuleControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlRuleControls.BorderStyle = BorderStyle.FixedSingle;
        pnlRuleControls.Controls.Add(btnAddRule);
        pnlRuleControls.Controls.Add(btnEditRule);
        pnlRuleControls.Controls.Add(btnDeleteRule);
        pnlRuleControls.Controls.Add(chkAutoApply);
        pnlRuleControls.Controls.Add(btnApplyRules);
        pnlRuleControls.Location = new Point(3, 616);
        pnlRuleControls.Name = "pnlRuleControls";
        pnlRuleControls.Padding = new Padding(10);
        pnlRuleControls.Size = new Size(718, 50);
        pnlRuleControls.TabIndex = 1;
        // 
        // btnAddRule
        // 
        btnAddRule.Location = new Point(10, 10);
        btnAddRule.Name = "btnAddRule";
        btnAddRule.Size = new Size(100, 30);
        btnAddRule.TabIndex = 0;
        btnAddRule.Text = "Add";
        btnAddRule.UseVisualStyleBackColor = true;
        btnAddRule.Click += btnAddRule_Click;
        // 
        // btnEditRule
        // 
        btnEditRule.Location = new Point(116, 10);
        btnEditRule.Name = "btnEditRule";
        btnEditRule.Size = new Size(100, 30);
        btnEditRule.TabIndex = 1;
        btnEditRule.Text = "Edit";
        btnEditRule.UseVisualStyleBackColor = true;
        btnEditRule.Click += btnEditRule_Click;
        // 
        // btnDeleteRule
        // 
        btnDeleteRule.Location = new Point(222, 10);
        btnDeleteRule.Name = "btnDeleteRule";
        btnDeleteRule.Size = new Size(100, 30);
        btnDeleteRule.TabIndex = 2;
        btnDeleteRule.Text = "Delete";
        btnDeleteRule.UseVisualStyleBackColor = true;
        btnDeleteRule.Click += btnDeleteRule_Click;
        // 
        // chkAutoApply
        // 
        chkAutoApply.AutoSize = true;
        chkAutoApply.Location = new Point(359, 15);
        chkAutoApply.Name = "chkAutoApply";
        chkAutoApply.Size = new Size(161, 23);
        chkAutoApply.TabIndex = 3;
        chkAutoApply.Text = "Auto-apply on launch";
        chkAutoApply.UseVisualStyleBackColor = true;
        // 
        // btnApplyRules
        // 
        btnApplyRules.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnApplyRules.Location = new Point(571, 10);
        btnApplyRules.Name = "btnApplyRules";
        btnApplyRules.Size = new Size(120, 30);
        btnApplyRules.TabIndex = 4;
        btnApplyRules.Text = "Apply All";
        btnApplyRules.UseVisualStyleBackColor = true;
        btnApplyRules.Click += btnApplyRules_Click;
        // 
        // pnlRulesList
        // 
        pnlRulesList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlRulesList.BorderStyle = BorderStyle.FixedSingle;
        pnlRulesList.Controls.Add(dgvRules);
        pnlRulesList.Controls.Add(lblRulesHeader);
        pnlRulesList.Location = new Point(12, 12);
        pnlRulesList.Name = "pnlRulesList";
        pnlRulesList.Padding = new Padding(10);
        pnlRulesList.Size = new Size(703, 598);
        pnlRulesList.TabIndex = 0;
        // 
        // dgvRules
        // 
        dgvRules.AllowUserToAddRows = false;
        dgvRules.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvRules.BorderStyle = BorderStyle.None;
        dgvRules.ColumnHeadersHeight = 32;
        dgvRules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvRules.Location = new Point(10, 30);
        dgvRules.Name = "dgvRules";
        dgvRules.RowHeadersVisible = false;
        dgvRules.RowTemplate.Height = 28;
        dgvRules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvRules.Size = new Size(681, 553);
        dgvRules.TabIndex = 1;
        // 
        // lblRulesHeader
        // 
        lblRulesHeader.AutoSize = true;
        lblRulesHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblRulesHeader.Location = new Point(0, 0);
        lblRulesHeader.Name = "lblRulesHeader";
        lblRulesHeader.Size = new Size(136, 20);
        lblRulesHeader.TabIndex = 0;
        lblRulesHeader.Text = "Automation Rules";
        // 
        // tabHotkeys
        // 
        tabHotkeys.Controls.Add(dgvHotkeys);
        tabHotkeys.Controls.Add(btnResetHotkeys);
        tabHotkeys.Controls.Add(lblHotkeysInfo);
        tabHotkeys.Location = new Point(4, 24);
        tabHotkeys.Name = "tabHotkeys";
        tabHotkeys.Padding = new Padding(12);
        tabHotkeys.Size = new Size(758, 685);
        tabHotkeys.TabIndex = 2;
        tabHotkeys.Text = "Hotkeys";
        tabHotkeys.UseVisualStyleBackColor = true;
        // 
        // dgvHotkeys
        // 
        dgvHotkeys.AllowUserToAddRows = false;
        dgvHotkeys.AllowUserToDeleteRows = false;
        dgvHotkeys.AllowUserToResizeRows = false;
        dgvHotkeys.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvHotkeys.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvHotkeys.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvHotkeys.Location = new Point(15, 56);
        dgvHotkeys.MultiSelect = false;
        dgvHotkeys.Name = "dgvHotkeys";
        dgvHotkeys.RowHeadersVisible = false;
        dgvHotkeys.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvHotkeys.Size = new Size(735, 576);
        dgvHotkeys.TabIndex = 0;
        dgvHotkeys.CellDoubleClick += DgvHotkeys_CellDoubleClick;
        // 
        // btnResetHotkeys
        // 
        btnResetHotkeys.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnResetHotkeys.Location = new Point(15, 638);
        btnResetHotkeys.Name = "btnResetHotkeys";
        btnResetHotkeys.Size = new Size(150, 28);
        btnResetHotkeys.TabIndex = 1;
        btnResetHotkeys.Text = "Reset to Defaults";
        btnResetHotkeys.UseVisualStyleBackColor = true;
        btnResetHotkeys.Click += BtnResetHotkeys_Click;
        // 
        // lblHotkeysInfo
        // 
        lblHotkeysInfo.AutoSize = true;
        lblHotkeysInfo.Location = new Point(15, 15);
        lblHotkeysInfo.Name = "lblHotkeysInfo";
        lblHotkeysInfo.Size = new Size(571, 38);
        lblHotkeysInfo.TabIndex = 2;
        lblHotkeysInfo.Text = "Double-click any hotkey to change it. Click 'Record' and press your desired key combination.\r\nLeave hotkeys empty (None) to disable them.";
        // 
        // tabLauncher
        // 
        tabLauncher.Controls.Add(lblLauncherInfo);
        tabLauncher.Controls.Add(dgvProfiles);
        tabLauncher.Controls.Add(btnAddProfile);
        tabLauncher.Controls.Add(btnEditProfile);
        tabLauncher.Controls.Add(btnDeleteProfile);
        tabLauncher.Controls.Add(btnLaunchProfile);
        tabLauncher.Location = new Point(4, 24);
        tabLauncher.Name = "tabLauncher";
        tabLauncher.Padding = new Padding(12);
        tabLauncher.Size = new Size(758, 685);
        tabLauncher.TabIndex = 5;
        tabLauncher.Text = "Launcher";
        tabLauncher.UseVisualStyleBackColor = true;
        // 
        // lblLauncherInfo
        // 
        lblLauncherInfo.AutoSize = true;
        lblLauncherInfo.Location = new Point(15, 15);
        lblLauncherInfo.Name = "lblLauncherInfo";
        lblLauncherInfo.Size = new Size(650, 19);
        lblLauncherInfo.TabIndex = 0;
        lblLauncherInfo.Text = "Create launch profiles to start groups of apps together. Assign hotkeys for instant access from anywhere.";
        // 
        // dgvProfiles
        // 
        dgvProfiles.AllowUserToAddRows = false;
        dgvProfiles.AllowUserToDeleteRows = false;
        dgvProfiles.AllowUserToResizeRows = false;
        dgvProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvProfiles.ColumnHeadersHeight = 28;
        dgvProfiles.Location = new Point(15, 46);
        dgvProfiles.MultiSelect = false;
        dgvProfiles.Name = "dgvProfiles";
        dgvProfiles.RowHeadersVisible = false;
        dgvProfiles.RowTemplate.Height = 26;
        dgvProfiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProfiles.Size = new Size(730, 468);
        dgvProfiles.TabIndex = 1;
        dgvProfiles.CellDoubleClick += DgvProfiles_CellDoubleClick;
        // 
        // btnAddProfile
        // 
        btnAddProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnAddProfile.Location = new Point(15, 523);
        btnAddProfile.Name = "btnAddProfile";
        btnAddProfile.Size = new Size(110, 30);
        btnAddProfile.TabIndex = 2;
        btnAddProfile.Text = "Add Profile";
        btnAddProfile.UseVisualStyleBackColor = true;
        btnAddProfile.Click += BtnAddProfile_Click;
        // 
        // btnEditProfile
        // 
        btnEditProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnEditProfile.Location = new Point(130, 523);
        btnEditProfile.Name = "btnEditProfile";
        btnEditProfile.Size = new Size(110, 30);
        btnEditProfile.TabIndex = 3;
        btnEditProfile.Text = "Edit Profile";
        btnEditProfile.UseVisualStyleBackColor = true;
        btnEditProfile.Click += BtnEditProfile_Click;
        // 
        // btnDeleteProfile
        // 
        btnDeleteProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnDeleteProfile.Location = new Point(245, 523);
        btnDeleteProfile.Name = "btnDeleteProfile";
        btnDeleteProfile.Size = new Size(110, 30);
        btnDeleteProfile.TabIndex = 4;
        btnDeleteProfile.Text = "Delete Profile";
        btnDeleteProfile.UseVisualStyleBackColor = true;
        btnDeleteProfile.Click += BtnDeleteProfile_Click;
        // 
        // btnLaunchProfile
        // 
        btnLaunchProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnLaunchProfile.Location = new Point(600, 523);
        btnLaunchProfile.Name = "btnLaunchProfile";
        btnLaunchProfile.Size = new Size(145, 30);
        btnLaunchProfile.TabIndex = 5;
        btnLaunchProfile.Text = "🚀 Launch Selected";
        btnLaunchProfile.UseVisualStyleBackColor = true;
        btnLaunchProfile.Click += BtnLaunchProfile_Click;
        // 
        // tabSettings
        // 
        tabSettings.Controls.Add(grpTheme);
        tabSettings.Controls.Add(grpStartup);
        tabSettings.Controls.Add(grpTray);
        tabSettings.Controls.Add(grpFont);
        tabSettings.Controls.Add(grpAdvanced);
        tabSettings.Location = new Point(4, 24);
        tabSettings.Name = "tabSettings";
        tabSettings.Padding = new Padding(12);
        tabSettings.Size = new Size(758, 685);
        tabSettings.TabIndex = 3;
        tabSettings.Text = "Settings";
        tabSettings.UseVisualStyleBackColor = true;
        // 
        // grpTheme
        // 
        grpTheme.Controls.Add(rbThemeSystem);
        grpTheme.Controls.Add(rbThemeDark);
        grpTheme.Controls.Add(rbThemeLight);
        grpTheme.Location = new Point(20, 20);
        grpTheme.Name = "grpTheme";
        grpTheme.Size = new Size(300, 120);
        grpTheme.TabIndex = 0;
        grpTheme.TabStop = false;
        grpTheme.Text = "Theme";
        // 
        // rbThemeSystem
        // 
        rbThemeSystem.AutoSize = true;
        rbThemeSystem.Checked = true;
        rbThemeSystem.Location = new Point(20, 80);
        rbThemeSystem.Name = "rbThemeSystem";
        rbThemeSystem.Size = new Size(182, 23);
        rbThemeSystem.TabIndex = 2;
        rbThemeSystem.TabStop = true;
        rbThemeSystem.Text = "System (Follow Windows)";
        rbThemeSystem.UseVisualStyleBackColor = true;
        rbThemeSystem.CheckedChanged += rbTheme_CheckedChanged;
        // 
        // rbThemeDark
        // 
        rbThemeDark.AutoSize = true;
        rbThemeDark.Location = new Point(20, 55);
        rbThemeDark.Name = "rbThemeDark";
        rbThemeDark.Size = new Size(56, 23);
        rbThemeDark.TabIndex = 1;
        rbThemeDark.Text = "Dark";
        rbThemeDark.UseVisualStyleBackColor = true;
        rbThemeDark.CheckedChanged += rbTheme_CheckedChanged;
        // 
        // rbThemeLight
        // 
        rbThemeLight.AutoSize = true;
        rbThemeLight.Location = new Point(20, 30);
        rbThemeLight.Name = "rbThemeLight";
        rbThemeLight.Size = new Size(58, 23);
        rbThemeLight.TabIndex = 0;
        rbThemeLight.Text = "Light";
        rbThemeLight.UseVisualStyleBackColor = true;
        rbThemeLight.CheckedChanged += rbTheme_CheckedChanged;
        // 
        // grpStartup
        // 
        grpStartup.Controls.Add(chkStartWithWindows);
        grpStartup.Location = new Point(20, 160);
        grpStartup.Name = "grpStartup";
        grpStartup.Size = new Size(300, 80);
        grpStartup.TabIndex = 1;
        grpStartup.TabStop = false;
        grpStartup.Text = "Startup";
        // 
        // chkStartWithWindows
        // 
        chkStartWithWindows.AutoSize = true;
        chkStartWithWindows.Location = new Point(20, 30);
        chkStartWithWindows.Name = "chkStartWithWindows";
        chkStartWithWindows.Size = new Size(147, 23);
        chkStartWithWindows.TabIndex = 0;
        chkStartWithWindows.Text = "Start with Windows";
        chkStartWithWindows.UseVisualStyleBackColor = true;
        chkStartWithWindows.CheckedChanged += chkStartWithWindows_CheckedChanged;
        // 
        // grpTray
        // 
        grpTray.Controls.Add(chkMinimizeToTray);
        grpTray.Controls.Add(chkCloseToTray);
        grpTray.Controls.Add(chkShowBalloonTips);
        grpTray.Location = new Point(20, 260);
        grpTray.Name = "grpTray";
        grpTray.Size = new Size(300, 130);
        grpTray.TabIndex = 2;
        grpTray.TabStop = false;
        grpTray.Text = "System Tray";
        // 
        // chkMinimizeToTray
        // 
        chkMinimizeToTray.AutoSize = true;
        chkMinimizeToTray.Checked = true;
        chkMinimizeToTray.CheckState = CheckState.Checked;
        chkMinimizeToTray.Location = new Point(20, 30);
        chkMinimizeToTray.Name = "chkMinimizeToTray";
        chkMinimizeToTray.Size = new Size(128, 23);
        chkMinimizeToTray.TabIndex = 0;
        chkMinimizeToTray.Text = "Minimize to tray";
        chkMinimizeToTray.UseVisualStyleBackColor = true;
        chkMinimizeToTray.CheckedChanged += chkMinimizeToTray_CheckedChanged;
        // 
        // chkCloseToTray
        // 
        chkCloseToTray.AutoSize = true;
        chkCloseToTray.Checked = true;
        chkCloseToTray.CheckState = CheckState.Checked;
        chkCloseToTray.Location = new Point(20, 55);
        chkCloseToTray.Name = "chkCloseToTray";
        chkCloseToTray.Size = new Size(106, 23);
        chkCloseToTray.TabIndex = 1;
        chkCloseToTray.Text = "Close to tray";
        chkCloseToTray.UseVisualStyleBackColor = true;
        chkCloseToTray.CheckedChanged += chkCloseToTray_CheckedChanged;
        // 
        // chkShowBalloonTips
        // 
        chkShowBalloonTips.AutoSize = true;
        chkShowBalloonTips.Checked = true;
        chkShowBalloonTips.CheckState = CheckState.Checked;
        chkShowBalloonTips.Location = new Point(20, 80);
        chkShowBalloonTips.Name = "chkShowBalloonTips";
        chkShowBalloonTips.Size = new Size(167, 23);
        chkShowBalloonTips.TabIndex = 2;
        chkShowBalloonTips.Text = "Show tray notifications";
        chkShowBalloonTips.UseVisualStyleBackColor = true;
        chkShowBalloonTips.CheckedChanged += chkShowBalloonTips_CheckedChanged;
        // 
        // grpFont
        // 
        grpFont.Controls.Add(lblFont);
        grpFont.Controls.Add(cmbFont);
        grpFont.Location = new Point(20, 400);
        grpFont.Name = "grpFont";
        grpFont.Size = new Size(300, 80);
        grpFont.TabIndex = 3;
        grpFont.TabStop = false;
        grpFont.Text = "Font";
        // 
        // lblFont
        // 
        lblFont.AutoSize = true;
        lblFont.Location = new Point(20, 30);
        lblFont.Name = "lblFont";
        lblFont.Size = new Size(112, 19);
        lblFont.TabIndex = 0;
        lblFont.Text = "Application Font:";
        // 
        // cmbFont
        // 
        cmbFont.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFont.FormattingEnabled = true;
        cmbFont.Items.AddRange(new object[] { "Segoe UI", "Arial", "Calibri", "Tahoma", "Verdana", "Consolas" });
        cmbFont.Location = new Point(140, 27);
        cmbFont.Name = "cmbFont";
        cmbFont.Size = new Size(140, 25);
        cmbFont.TabIndex = 1;
        cmbFont.SelectedIndexChanged += cmbFont_SelectedIndexChanged;
        // 
        // grpAdvanced
        // 
        grpAdvanced.Controls.Add(lblDesktopSwitchTimeout);
        grpAdvanced.Controls.Add(nudDesktopSwitchTimeout);
        grpAdvanced.Location = new Point(20, 490);
        grpAdvanced.Name = "grpAdvanced";
        grpAdvanced.Size = new Size(300, 70);
        grpAdvanced.TabIndex = 4;
        grpAdvanced.TabStop = false;
        grpAdvanced.Text = "Advanced";
        // 
        // lblDesktopSwitchTimeout
        // 
        lblDesktopSwitchTimeout.AutoSize = true;
        lblDesktopSwitchTimeout.Location = new Point(20, 32);
        lblDesktopSwitchTimeout.Name = "lblDesktopSwitchTimeout";
        lblDesktopSwitchTimeout.Size = new Size(187, 19);
        lblDesktopSwitchTimeout.TabIndex = 0;
        lblDesktopSwitchTimeout.Text = "Desktop switch timeout (ms):";
        // 
        // nudDesktopSwitchTimeout
        // 
        nudDesktopSwitchTimeout.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        nudDesktopSwitchTimeout.Location = new Point(210, 30);
        nudDesktopSwitchTimeout.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        nudDesktopSwitchTimeout.Minimum = new decimal(new int[] { 200, 0, 0, 0 });
        nudDesktopSwitchTimeout.Name = "nudDesktopSwitchTimeout";
        nudDesktopSwitchTimeout.Size = new Size(72, 25);
        nudDesktopSwitchTimeout.TabIndex = 1;
        nudDesktopSwitchTimeout.Value = new decimal(new int[] { 800, 0, 0, 0 });
        nudDesktopSwitchTimeout.ValueChanged += nudDesktopSwitchTimeout_ValueChanged;
        // 
        // tabAbout
        // 
        tabAbout.Controls.Add(lblAbout);
        tabAbout.Controls.Add(lblThirdParty);
        tabAbout.Controls.Add(rtbLicense);
        tabAbout.Location = new Point(4, 26);
        tabAbout.Name = "tabAbout";
        tabAbout.Padding = new Padding(12);
        tabAbout.Size = new Size(758, 683);
        tabAbout.TabIndex = 4;
        tabAbout.Text = "About";
        tabAbout.UseVisualStyleBackColor = true;
        // 
        // lblAbout
        // 
        lblAbout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblAbout.Font = new Font("Segoe UI", 16F);
        lblAbout.ForeColor = SystemColors.MenuText;
        lblAbout.Image = (Image)resources.GetObject("lblAbout.Image");
        lblAbout.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
        lblAbout.Location = new Point(6, 12);
        lblAbout.Name = "lblAbout";
        lblAbout.Size = new Size(737, 216);
        lblAbout.TabIndex = 0;
        lblAbout.Text = "DeskBulldozer By BulldozerLabs";
        lblAbout.TextAlign = ContentAlignment.BottomCenter;
        lblAbout.Click += lblAbout_Click;
        // 
        // lblThirdParty
        // 
        lblThirdParty.AutoSize = true;
        lblThirdParty.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblThirdParty.Location = new Point(6, 242);
        lblThirdParty.Name = "lblThirdParty";
        lblThirdParty.Size = new Size(144, 19);
        lblThirdParty.TabIndex = 1;
        lblThirdParty.Text = "Third-Party Licenses";
        // 
        // rtbLicense
        // 
        rtbLicense.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        rtbLicense.BackColor = SystemColors.Control;
        rtbLicense.BorderStyle = BorderStyle.FixedSingle;
        rtbLicense.Font = new Font("Consolas", 9F);
        rtbLicense.Location = new Point(6, 281);
        rtbLicense.Name = "rtbLicense";
        rtbLicense.ReadOnly = true;
        rtbLicense.ScrollBars = RichTextBoxScrollBars.Vertical;
        rtbLicense.Size = new Size(737, 383);
        rtbLicense.TabIndex = 2;
        rtbLicense.Text = resources.GetString("rtbLicense.Text");
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(16, 757);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(46, 19);
        lblStatus.TabIndex = 0;
        lblStatus.Text = "Ready";
        // 
        // lblWindows
        // 
        lblWindows.Location = new Point(0, 0);
        lblWindows.Name = "lblWindows";
        lblWindows.Size = new Size(100, 23);
        lblWindows.TabIndex = 0;
        // 
        // lblCurrentDesktop
        // 
        lblCurrentDesktop.AutoSize = true;
        lblCurrentDesktop.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblCurrentDesktop.Location = new Point(16, 12);
        lblCurrentDesktop.Name = "lblCurrentDesktop";
        lblCurrentDesktop.Size = new Size(141, 20);
        lblCurrentDesktop.TabIndex = 2;
        lblCurrentDesktop.Text = "Current Desktop: 1";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(804, 801);
        Controls.Add(lblStatus);
        Controls.Add(lblCurrentDesktop);
        Controls.Add(tabControl);
        Font = new Font("Segoe UI", 10F);
        MinimumSize = new Size(820, 840);
        Name = "Form1";
        Text = "DeskBulldozer";
        tabControl.ResumeLayout(false);
        tabWindows.ResumeLayout(false);
        pnlWindowLayout.ResumeLayout(false);
        pnlWindowLayout.PerformLayout();
        pnlWindowList.ResumeLayout(false);
        pnlWindowList.PerformLayout();
        tabRules.ResumeLayout(false);
        pnlRuleControls.ResumeLayout(false);
        pnlRuleControls.PerformLayout();
        pnlRulesList.ResumeLayout(false);
        pnlRulesList.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRules).EndInit();
        tabHotkeys.ResumeLayout(false);
        tabHotkeys.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvHotkeys).EndInit();
        tabLauncher.ResumeLayout(false);
        tabLauncher.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvProfiles).EndInit();
        tabSettings.ResumeLayout(false);
        grpTheme.ResumeLayout(false);
        grpTheme.PerformLayout();
        grpStartup.ResumeLayout(false);
        grpStartup.PerformLayout();
        grpTray.ResumeLayout(false);
        grpTray.PerformLayout();
        grpFont.ResumeLayout(false);
        grpFont.PerformLayout();
        grpAdvanced.ResumeLayout(false);
        grpAdvanced.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudDesktopSwitchTimeout).EndInit();
        tabAbout.ResumeLayout(false);
        tabAbout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabWindows;
    private System.Windows.Forms.TabPage tabRules;
    private System.Windows.Forms.TabPage tabHotkeys;
    private System.Windows.Forms.ListBox lstWindows;
    private System.Windows.Forms.Label lblWindows;
    private System.Windows.Forms.Label lblTargetDesktop;
    private System.Windows.Forms.ComboBox cmbTargetDesktop;
    private System.Windows.Forms.Label lblQuadrant;
    private VDManager.Controls.VisualQuadrantPanel visualQuadrantPanel;
    private System.Windows.Forms.Label lblMonitor;
    private System.Windows.Forms.ComboBox cmbMonitor;
    private System.Windows.Forms.Button btnMoveWindow;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.DataGridView dgvRules;
    private System.Windows.Forms.Button btnAddRule;
    private System.Windows.Forms.Button btnEditRule;
    private System.Windows.Forms.Button btnDeleteRule;
    private System.Windows.Forms.Button btnApplyRules;
    private System.Windows.Forms.CheckBox chkAutoApply;
    private System.Windows.Forms.DataGridView dgvHotkeys;
    private System.Windows.Forms.Button btnResetHotkeys;
    private System.Windows.Forms.Label lblHotkeysInfo;
    private System.Windows.Forms.TabPage tabSettings;
    private System.Windows.Forms.GroupBox grpTheme;
    private System.Windows.Forms.RadioButton rbThemeSystem;
    private System.Windows.Forms.RadioButton rbThemeDark;
    private System.Windows.Forms.RadioButton rbThemeLight;
    private System.Windows.Forms.GroupBox grpStartup;
    private System.Windows.Forms.CheckBox chkStartWithWindows;
    private System.Windows.Forms.GroupBox grpTray;
    private System.Windows.Forms.CheckBox chkMinimizeToTray;
    private System.Windows.Forms.CheckBox chkCloseToTray;
    private System.Windows.Forms.CheckBox chkShowBalloonTips;
    private System.Windows.Forms.TabPage tabAbout;
    private System.Windows.Forms.Label lblAbout;
    private System.Windows.Forms.Label lblThirdParty;
    private System.Windows.Forms.RichTextBox rtbLicense;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Label lblCurrentDesktop;
    private System.Windows.Forms.Button btnAddAsRule;
    private System.Windows.Forms.Panel pnlWindowList;
    private System.Windows.Forms.Label lblWindowListHeader;
    private System.Windows.Forms.Panel pnlWindowLayout;
    private System.Windows.Forms.Label lblWindowLayoutHeader;
    private System.Windows.Forms.Panel pnlRulesList;
    private System.Windows.Forms.Label lblRulesHeader;
    private System.Windows.Forms.Panel pnlRuleControls;
    private System.Windows.Forms.GroupBox grpFont;
    private System.Windows.Forms.Label lblFont;
    private System.Windows.Forms.ComboBox cmbFont;
    private System.Windows.Forms.GroupBox grpAdvanced;
    private System.Windows.Forms.Label lblDesktopSwitchTimeout;
    private System.Windows.Forms.NumericUpDown nudDesktopSwitchTimeout;
    // Launcher tab
    private System.Windows.Forms.TabPage tabLauncher;
    private System.Windows.Forms.Label lblLauncherInfo;
    private System.Windows.Forms.DataGridView dgvProfiles;
    private System.Windows.Forms.Button btnAddProfile;
    private System.Windows.Forms.Button btnEditProfile;
    private System.Windows.Forms.Button btnDeleteProfile;
    private System.Windows.Forms.Button btnLaunchProfile;
}
