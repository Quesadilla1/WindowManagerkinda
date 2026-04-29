using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VDManager.Models;
using VDManager.Services;

namespace VDManager
{
    /// <summary>
    /// Dialog for creating or editing a LaunchProfile (name, description, entries, hotkey, startup).
    /// </summary>
    public partial class LaunchProfileEditorDialog : Form
    {
        public LaunchProfile Profile { get; private set; }

        private readonly int _desktopCount;
        private readonly IReadOnlyList<WindowRule> _rules;
        private readonly IThemeManager? _themeManager;

        public LaunchProfileEditorDialog(int desktopCount, IReadOnlyList<WindowRule> rules, LaunchProfile? existing = null, IThemeManager? themeManager = null)
        {
            _desktopCount = desktopCount;
            _rules = rules ?? new List<WindowRule>();
            _themeManager = themeManager;

            // Deep-copy so Cancel truly cancels
            Profile = existing != null
                ? CloneProfile(existing)
                : new LaunchProfile { Name = "New Profile" };

            InitializeComponent();
            SetupEntriesGrid();
            PopulateFields();
            RefreshEntriesGrid();
            _themeManager?.ApplyTheme(this);
        }

        private static LaunchProfile CloneProfile(LaunchProfile src)
        {
            var p = new LaunchProfile
            {
                Id = src.Id,
                Name = src.Name,
                Description = src.Description,
                LaunchOnStartup = src.LaunchOnStartup,
                HotkeyModifiers = src.HotkeyModifiers,
                HotkeyKey = src.HotkeyKey
            };
            foreach (var e in src.Entries)
            {
                p.Entries.Add(new AppLaunchEntry
                {
                    Id = e.Id,
                    Name = e.Name,
                    ExecutablePath = e.ExecutablePath,
                    Arguments = e.Arguments,
                    WorkingDirectory = e.WorkingDirectory,
                    DelaySeconds = e.DelaySeconds,
                    TargetDesktopIndex = e.TargetDesktopIndex,
                    SortOrder = e.SortOrder,
                    LinkedRuleId = e.LinkedRuleId
                });
            }
            return p;
        }

        private static System.Windows.Forms.Label MakeLabel(string text, int x, int y) =>
            new System.Windows.Forms.Label { Text = text, Location = new System.Drawing.Point(x, y), AutoSize = true };

        private void SetupEntriesGrid()
        {
            dgvEntries.Columns.Clear();
            dgvEntries.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",    HeaderText = "Name",         Width = 120, ReadOnly = true });
            dgvEntries.Columns.Add(new DataGridViewTextBoxColumn { Name = "Exe",     HeaderText = "Executable",   Width = 150, ReadOnly = true });
            dgvEntries.Columns.Add(new DataGridViewTextBoxColumn { Name = "Args",    HeaderText = "Arguments",    Width = 120, ReadOnly = true });
            dgvEntries.Columns.Add(new DataGridViewTextBoxColumn { Name = "Delay",   HeaderText = "Delay (s)",    Width = 60,  ReadOnly = true });
            dgvEntries.Columns.Add(new DataGridViewTextBoxColumn { Name = "Desktop", HeaderText = "Desktop",      Width = 70,  ReadOnly = true });
            dgvEntries.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rule",    HeaderText = "Linked Rule",  Width = 100, ReadOnly = true });
        }

        private void RefreshEntriesGrid()
        {
            dgvEntries.Rows.Clear();
            var ordered = Profile.Entries.OrderBy(e => e.SortOrder).ToList();
            foreach (var e in ordered)
            {
                string desktop = e.TargetDesktopIndex >= 0 ? $"Desktop {e.TargetDesktopIndex + 1}" : "(none)";
                string ruleName = "(none)";
                if (!string.IsNullOrEmpty(e.LinkedRuleId))
                {
                    var matched = _rules.FirstOrDefault(r => r.Id == e.LinkedRuleId);
                    ruleName = matched != null ? matched.ToString() : "(deleted)";
                }
                int idx = dgvEntries.Rows.Add(e.Name, e.ExecutablePath, e.Arguments, e.DelaySeconds, desktop, ruleName);
                dgvEntries.Rows[idx].Tag = e;
            }
        }

        private void PopulateFields()
        {
            txtName.Text = Profile.Name;
            txtDesc.Text = Profile.Description;
            chkStartup.Checked = Profile.LaunchOnStartup;
            UpdateHotkeyDisplay();
        }

        private void UpdateHotkeyDisplay()
        {
            txtHotkey.Text = Profile.GetHotkeyDisplayString();
        }

        // ─── Hotkey ────────────────────────────────────────────────────────────

        private void BtnEditHotkey_Click(object? sender, EventArgs e)
        {
            using var dlg = new HotkeyEditorDialog("Launch Profile: " + Profile.Name, Profile.HotkeyModifiers, Profile.HotkeyKey, _themeManager);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Profile.HotkeyModifiers = dlg.Modifiers;
                Profile.HotkeyKey = dlg.Key;
                UpdateHotkeyDisplay();
            }
        }

        private void BtnClearHotkey_Click(object? sender, EventArgs e)
        {
            Profile.HotkeyModifiers = 0;
            Profile.HotkeyKey = Keys.None;
            UpdateHotkeyDisplay();
        }

        // ─── Entry CRUD ────────────────────────────────────────────────────────

        private void BtnAddEntry_Click(object? sender, EventArgs e)
        {
            using var dlg = new LaunchEntryEditorDialog(_desktopCount, _rules, themeManager: _themeManager);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dlg.Entry.SortOrder = Profile.Entries.Count;
                Profile.Entries.Add(dlg.Entry);
                RefreshEntriesGrid();
            }
        }

        private void BtnEditEntry_Click(object? sender, EventArgs e)
        {
            if (GetSelectedEntry() is not AppLaunchEntry entry) return;

            using var dlg = new LaunchEntryEditorDialog(_desktopCount, _rules, entry, _themeManager);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int idx = Profile.Entries.IndexOf(entry);
                if (idx >= 0)
                    Profile.Entries[idx] = dlg.Entry;
                RefreshEntriesGrid();
            }
        }

        private void BtnDeleteEntry_Click(object? sender, EventArgs e)
        {
            if (GetSelectedEntry() is not AppLaunchEntry entry) return;

            var result = MessageBox.Show(
                $"Delete entry '{entry.Name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Profile.Entries.Remove(entry);
                NormalizeSortOrders();
                RefreshEntriesGrid();
            }
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            if (dgvEntries.SelectedRows.Count == 0) return;
            int rowIdx = dgvEntries.SelectedRows[0].Index;
            if (rowIdx <= 0) return;

            var ordered = Profile.Entries.OrderBy(x => x.SortOrder).ToList();
            var cur = ordered[rowIdx];
            var prev = ordered[rowIdx - 1];
            (cur.SortOrder, prev.SortOrder) = (prev.SortOrder, cur.SortOrder);

            RefreshEntriesGrid();
            if (rowIdx - 1 < dgvEntries.Rows.Count)
                dgvEntries.Rows[rowIdx - 1].Selected = true;
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            if (dgvEntries.SelectedRows.Count == 0) return;
            int rowIdx = dgvEntries.SelectedRows[0].Index;
            var ordered = Profile.Entries.OrderBy(x => x.SortOrder).ToList();
            if (rowIdx >= ordered.Count - 1) return;

            var cur = ordered[rowIdx];
            var next = ordered[rowIdx + 1];
            (cur.SortOrder, next.SortOrder) = (next.SortOrder, cur.SortOrder);

            RefreshEntriesGrid();
            if (rowIdx + 1 < dgvEntries.Rows.Count)
                dgvEntries.Rows[rowIdx + 1].Selected = true;
        }

        private void NormalizeSortOrders()
        {
            var ordered = Profile.Entries.OrderBy(e => e.SortOrder).ToList();
            for (int i = 0; i < ordered.Count; i++)
                ordered[i].SortOrder = i;
        }

        private AppLaunchEntry? GetSelectedEntry()
        {
            if (dgvEntries.SelectedRows.Count == 0) return null;
            return dgvEntries.SelectedRows[0].Tag as AppLaunchEntry;
        }

        // ─── OK ────────────────────────────────────────────────────────────────

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a profile name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            Profile.Name = txtName.Text.Trim();
            Profile.Description = txtDesc.Text.Trim();
            Profile.LaunchOnStartup = chkStartup.Checked;
            // HotkeyModifiers and HotkeyKey are already updated live by the editor dialog
        }
    }
}
