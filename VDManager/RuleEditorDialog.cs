using System;
using System.Linq;
using System.Windows.Forms;
using VDManager.Controls;
using VDManager.Models;
using VDManager.Services;

namespace VDManager
{
    public partial class RuleEditorDialog : Form
    {
        private IWindowManager windowManager = null!;

        public WindowRule Rule { get; private set; } = null!;

        public RuleEditorDialog(int desktopCount, WindowRule? existingRule = null, IWindowManager? wm = null, IThemeManager? themeManager = null)
        {
            windowManager = wm ?? new WindowManager();
            InitializeComponent();
            PopulateComboBoxes(desktopCount);
            LoadRunningProcesses();

            if (existingRule != null)
            {
                // Edit mode
                this.Text = "Edit Rule";
                LoadRule(existingRule);
            }
            else
            {
                // Add mode
                this.Text = "Add Rule";
                Rule = new WindowRule();
                visualQuadrantPanel.SelectedQuadrant = Quadrant.None;
            }

            themeManager?.ApplyTheme(this);
        }

        private void btnRefreshProcesses_Click(object? sender, EventArgs e) => LoadRunningProcesses();

        private void PopulateComboBoxes(int desktopCount)
        {
            for (int i = 0; i < desktopCount; i++)
                cmbDesktop.Items.Add($"Desktop {i + 1}");
            if (cmbDesktop.Items.Count > 0)
                cmbDesktop.SelectedIndex = 0;

            var monitorNames = QuadrantLayout.GetMonitorNames();
            foreach (var name in monitorNames)
                cmbMonitor.Items.Add(name);
            if (cmbMonitor.Items.Count > 0)
                cmbMonitor.SelectedIndex = 0;
        }

        private void LoadRunningProcesses()
        {
            // Save current selection if any
            string currentSelection = cmbProcessName.Text;

            cmbProcessName.Items.Clear();

            // Get all unique process names from running windows
            var windows = windowManager.GetAllWindows();
            var processNames = windows
                .Select(w => w.ProcessName)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            foreach (var processName in processNames)
            {
                cmbProcessName.Items.Add(processName);
            }

            // Restore selection if it still exists
            if (!string.IsNullOrEmpty(currentSelection))
            {
                int index = cmbProcessName.Items.IndexOf(currentSelection);
                if (index >= 0)
                {
                    cmbProcessName.SelectedIndex = index;
                }
                else
                {
                    cmbProcessName.Text = currentSelection;
                }
            }
        }

        private void LoadRule(WindowRule rule)
        {
            Rule = rule;
            cmbProcessName.Text = rule.ProcessName;
            txtTitlePattern.Text = rule.WindowTitlePattern ?? "";
            chkUseRegex.Checked = rule.UseRegex;
            nudInstanceNumber.Value = Math.Max(nudInstanceNumber.Minimum,
                                     Math.Min(nudInstanceNumber.Maximum, rule.InstanceNumber));
            nudPriority.Value = Math.Max(nudPriority.Minimum,
                                Math.Min(nudPriority.Maximum, rule.Priority));

            // Guard against saved index being out of range when the user has
            // fewer desktops or monitors than when the rule was created (#6, #7).
            if (rule.DesktopIndex >= 0 && rule.DesktopIndex < cmbDesktop.Items.Count)
                cmbDesktop.SelectedIndex = rule.DesktopIndex;
            else
                cmbDesktop.SelectedIndex = Math.Max(0, cmbDesktop.Items.Count - 1);

            visualQuadrantPanel.SelectedQuadrant = rule.Quadrant;

            if (rule.MonitorIndex >= 0 && rule.MonitorIndex < cmbMonitor.Items.Count)
                cmbMonitor.SelectedIndex = rule.MonitorIndex;
            else
                cmbMonitor.SelectedIndex = 0;

            txtDescription.Text = rule.Description;
            chkEnabled.Checked = rule.Enabled;
            chkEnforcePosition.Checked = rule.EnforcePosition;
        }

        private void btnOK_Click(object? sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(cmbProcessName.Text))
            {
                MessageBox.Show("Please select or enter a process name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Validate regex pattern before saving so invalid patterns are caught early
            if (chkUseRegex.Checked && !string.IsNullOrWhiteSpace(txtTitlePattern.Text))
            {
                try
                {
                    _ = new System.Text.RegularExpressions.Regex(txtTitlePattern.Text.Trim());
                }
                catch (System.Text.RegularExpressions.RegexParseException ex)
                {
                    // Offer to auto-escape the pattern so it matches literally instead
                    string escaped = System.Text.RegularExpressions.Regex.Escape(txtTitlePattern.Text.Trim());
                    var fix = MessageBox.Show(
                        $"The window title pattern is not a valid regular expression:\n\n{ex.Message}\n\n" +
                        $"Would you like to automatically escape special characters so it matches the text literally?\n\n" +
                        $"Escaped pattern:  {escaped}",
                        "Invalid Regex Pattern",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (fix == DialogResult.Yes)
                    {
                        txtTitlePattern.Text = escaped;
                        // Pattern is now valid — continue saving
                    }
                    else
                    {
                        this.DialogResult = DialogResult.None;
                        txtTitlePattern.Focus();
                        return;
                    }
                }
            }

            // Create or update rule
            if (Rule == null)
            {
                Rule = new WindowRule();
            }

            Rule.ProcessName = cmbProcessName.Text.Trim();
            Rule.WindowTitlePattern = string.IsNullOrWhiteSpace(txtTitlePattern.Text) ? null : txtTitlePattern.Text.Trim();
            Rule.UseRegex = chkUseRegex.Checked;
            Rule.InstanceNumber = (int)nudInstanceNumber.Value;
            Rule.Priority = (int)nudPriority.Value;
            Rule.DesktopIndex = cmbDesktop.SelectedIndex;
            Rule.Quadrant = visualQuadrantPanel.SelectedQuadrant;
            Rule.MonitorIndex = cmbMonitor.SelectedIndex;
            Rule.Description = txtDescription.Text.Trim();
            Rule.Enabled = chkEnabled.Checked;
            Rule.EnforcePosition = chkEnforcePosition.Checked;
        }
    }
}
