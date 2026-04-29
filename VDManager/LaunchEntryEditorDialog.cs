using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VDManager.Models;
using VDManager.Services;

namespace VDManager
{
    /// <summary>
    /// Dialog for creating or editing a single AppLaunchEntry.
    /// Each entry must be linked to a WindowRule so that the launcher can use the
    /// rule's window claim (maintained by WindowInstanceTracker) to decide whether
    /// the app is already running — no separate process scan is required.
    /// </summary>
    public partial class LaunchEntryEditorDialog : Form
    {
        public AppLaunchEntry Entry { get; private set; }

        private readonly int _desktopCount;
        private readonly IReadOnlyList<WindowRule> _rules;

        public LaunchEntryEditorDialog(int desktopCount, IReadOnlyList<WindowRule> rules, AppLaunchEntry? existing = null, IThemeManager? themeManager = null)
        {
            _desktopCount = desktopCount;
            _rules = rules ?? new List<WindowRule>();

            Entry = existing != null
                ? new AppLaunchEntry
                {
                    Id = existing.Id,
                    Name = existing.Name,
                    ExecutablePath = existing.ExecutablePath,
                    Arguments = existing.Arguments,
                    WorkingDirectory = existing.WorkingDirectory,
                    DelaySeconds = existing.DelaySeconds,
                    TargetDesktopIndex = existing.TargetDesktopIndex,
                    SortOrder = existing.SortOrder,
                    LinkedRuleId = existing.LinkedRuleId
                }
                : new AppLaunchEntry();

            InitializeComponent();
            PopulateFields();
            themeManager?.ApplyTheme(this);
        }

        private void PopulateFields()
        {
            // Populate preset combo
            cmbPreset.Items.AddRange(new object[]
            {
                "(Custom)",
                "Windows App Example", "Edge", "Chrome", "Firefox",
                "VS Code", "Visual Studio 2022", "Cursor",
                "Windows Terminal", "PowerShell",
                "Notepad++", "File Explorer",
                "Slack", "Discord", "Microsoft Teams",
                "Spotify", "Obsidian"
            });
            cmbPreset.SelectedIndex = 0;

            // Populate desktop combo
            cmbDesktop.Items.Add("(Don't switch)");
            for (int i = 0; i < _desktopCount; i++)
                cmbDesktop.Items.Add($"Desktop {i + 1}");
            cmbDesktop.SelectedIndex = 0;

            // Populate rules combo
            PopulateRulesCombo();

            // Load entry values
            txtName.Text = Entry.Name;
            txtExe.Text = Entry.ExecutablePath;
            txtArgs.Text = Entry.Arguments;
            txtWorkDir.Text = Entry.WorkingDirectory;
            nudDelay.Value = Math.Max(0, Math.Min(300, Entry.DelaySeconds));

            // Desktop combo: index 0 = "(Don't switch)", index 1+ = Desktop 1..N
            int desktopSel = Entry.TargetDesktopIndex >= 0 ? Entry.TargetDesktopIndex + 1 : 0;
            cmbDesktop.SelectedIndex = Math.Min(desktopSel, cmbDesktop.Items.Count - 1);

            // Select the linked rule if it exists
            if (!string.IsNullOrEmpty(Entry.LinkedRuleId))
            {
                for (int i = 0; i < cmbLinkedRule.Items.Count; i++)
                {
                    if (cmbLinkedRule.Items[i] is WindowRule r && r.Id == Entry.LinkedRuleId)
                    {
                        cmbLinkedRule.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void PopulateRulesCombo()
        {
            cmbLinkedRule.Items.Clear();
            foreach (var rule in _rules)
                cmbLinkedRule.Items.Add(rule);
        }

        /// <summary>
        /// Returns true if the current name text looks like it was auto-filled by a previous preset
        /// (so we can safely overwrite it when a new preset is picked).
        /// </summary>
        private static readonly string[] KnownPresetNames =
        {
            "Windows App Example", "Edge", "Chrome", "Firefox",
            "VS Code", "Visual Studio 2022", "Cursor",
            "Windows Terminal", "PowerShell",
            "Notepad++", "File Explorer",
            "Slack", "Discord", "Microsoft Teams",
            "Spotify", "Obsidian"
        };

        private bool NameIsPresetDefault() =>
            string.IsNullOrWhiteSpace(txtName.Text) ||
            Array.Exists(KnownPresetNames, n => n == txtName.Text);

        private static string Expand(string path) =>
            Environment.ExpandEnvironmentVariables(path);

        private void ApplyPreset(string name, string exe, string args = "")
        {
            if (NameIsPresetDefault()) txtName.Text = name;
            txtExe.Text = exe;
            if (string.IsNullOrWhiteSpace(txtArgs.Text))
                txtArgs.Text = args;
        }

        private void CmbPreset_SelectedIndexChanged(object? sender, EventArgs e)
        {
            switch (cmbPreset.SelectedItem?.ToString())
            {
                // ── Browsers ──────────────────────────────────────────────────
                case "Windows App Example":
                    ApplyPreset("Windows App Example", "Calculator:");
                    break;
                case "Edge":
                    ApplyPreset("Edge", "msedge.exe", "--new-window https://");
                    break;
                case "Chrome":
                    ApplyPreset("Chrome", "chrome.exe", "--new-window https://");
                    break;
                case "Firefox":
                    ApplyPreset("Firefox", "firefox.exe", "-new-window https://");
                    break;

                // ── Dev tools ─────────────────────────────────────────────────
                case "VS Code":
                    ApplyPreset("VS Code", LauncherService.FindVSCode());
                    break;
                case "Visual Studio 2022":
                    ApplyPreset("Visual Studio 2022", LauncherService.FindVisualStudio2022());
                    break;
                case "Cursor":
                    ApplyPreset("Cursor", Expand(@"%LOCALAPPDATA%\Programs\cursor\Cursor.exe"));
                    break;

                // ── Terminals ─────────────────────────────────────────────────
                case "Windows Terminal":
                    ApplyPreset("Windows Terminal", "wt.exe");
                    break;
                case "PowerShell":
                    // Prefer modern PowerShell 7 (pwsh), fall back to Windows PowerShell
                    ApplyPreset("PowerShell", "pwsh.exe");
                    break;

                // ── Editors ───────────────────────────────────────────────────
                case "Notepad++":
                    ApplyPreset("Notepad++", "notepad++.exe");
                    break;

                // ── System ────────────────────────────────────────────────────
                case "File Explorer":
                    ApplyPreset("File Explorer", "explorer.exe");
                    break;

                // ── Communication ─────────────────────────────────────────────
                case "Slack":
                    ApplyPreset("Slack", Expand(@"%LOCALAPPDATA%\slack\slack.exe"));
                    break;
                case "Discord":
                    ApplyPreset("Discord",
                        Expand(@"%LOCALAPPDATA%\Discord\Update.exe"),
                        "--processStart Discord.exe");
                    break;
                case "Microsoft Teams":
                    ApplyPreset("Microsoft Teams", "ms-teams.exe");
                    break;

                // ── Media / Productivity ──────────────────────────────────────
                case "Spotify":
                    ApplyPreset("Spotify", Expand(@"%APPDATA%\Spotify\Spotify.exe"));
                    break;
                case "Obsidian":
                    ApplyPreset("Obsidian", Expand(@"%LOCALAPPDATA%\Obsidian\Obsidian.exe"));
                    break;
            }
        }

        private void BtnBrowseExe_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select Executable",
                Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (!string.IsNullOrWhiteSpace(txtExe.Text) && File.Exists(txtExe.Text))
                dlg.InitialDirectory = Path.GetDirectoryName(txtExe.Text);

            if (dlg.ShowDialog() == DialogResult.OK)
                txtExe.Text = dlg.FileName;
        }

        private void BtnBrowseWorkDir_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select Working Directory",
                UseDescriptionForTitle = true
            };

            if (!string.IsNullOrWhiteSpace(txtWorkDir.Text) && Directory.Exists(txtWorkDir.Text))
                dlg.SelectedPath = txtWorkDir.Text;

            if (dlg.ShowDialog() == DialogResult.OK)
                txtWorkDir.Text = dlg.SelectedPath;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExe.Text))
            {
                MessageBox.Show("Please specify an executable.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (cmbLinkedRule.SelectedItem is not WindowRule selectedRule)
            {
                MessageBox.Show(
                    "Please select a Window Rule to link to this entry.\n\n" +
                    "A linked rule lets the launcher detect whether this app is already running\n" +
                    "without doing a separate process scan. Create a rule first if none exist.",
                    "Linked Rule Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
                txtName.Text = Path.GetFileNameWithoutExtension(txtExe.Text);

            Entry.Name = txtName.Text.Trim();
            Entry.ExecutablePath = txtExe.Text.Trim();
            Entry.Arguments = txtArgs.Text.Trim();
            Entry.WorkingDirectory = txtWorkDir.Text.Trim();
            Entry.DelaySeconds = (int)nudDelay.Value;
            Entry.TargetDesktopIndex = cmbDesktop.SelectedIndex > 0 ? cmbDesktop.SelectedIndex - 1 : -1;
            Entry.LinkedRuleId = selectedRule.Id;
        }
    }
}
