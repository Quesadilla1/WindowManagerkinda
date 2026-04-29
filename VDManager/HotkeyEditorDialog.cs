using System;
using System.Windows.Forms;
using VDManager.Services;

namespace VDManager
{
    public partial class HotkeyEditorDialog : Form
    {
        public uint Modifiers { get; private set; }
        public Keys Key { get; private set; }
        public bool IsRecording { get; private set; }

        public HotkeyEditorDialog(string actionName, uint currentModifiers, Keys currentKey, IThemeManager? themeManager = null)
        {
            InitializeComponent();
            Text = $"Edit Hotkey: {actionName}";
            Modifiers = currentModifiers;
            Key = currentKey;
            UpdateHotkeyDisplay();
            themeManager?.ApplyTheme(this);
        }

        private void BtnRecord_Click(object? sender, EventArgs e)
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            Modifiers = 0;
            Key = Keys.None;
            UpdateHotkeyDisplay();
        }

        private void StartRecording()
        {
            IsRecording = true;
            btnRecord.Text = "Stop";
            txtHotkey.Text = "Press key combination...";
            txtHotkey.BackColor = System.Drawing.Color.LightYellow;
        }

        private void StopRecording()
        {
            IsRecording = false;
            btnRecord.Text = "Record";
            txtHotkey.BackColor = System.Drawing.SystemColors.Window;
            UpdateHotkeyDisplay();
        }

        private void HotkeyEditorDialog_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!IsRecording)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            // Ignore modifier-only keys
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey ||
                e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            {
                return;
            }

            // Build modifiers
            uint modifiers = 0;
            if (e.Control)
                modifiers |= HotkeyManager.MOD_CONTROL;
            if (e.Shift)
                modifiers |= HotkeyManager.MOD_SHIFT;
            if (e.Alt)
                modifiers |= HotkeyManager.MOD_ALT;

            // Check for Windows key (requires manual check)
            if ((Control.ModifierKeys & Keys.LWin) == Keys.LWin ||
                (Control.ModifierKeys & Keys.RWin) == Keys.RWin ||
                (GetAsyncKeyState(0x5B) & 0x8000) != 0 || // VK_LWIN
                (GetAsyncKeyState(0x5C) & 0x8000) != 0)   // VK_RWIN
            {
                modifiers |= HotkeyManager.MOD_WIN;
            }

            Modifiers = modifiers;
            Key = e.KeyCode;

            StopRecording();
        }

        private void UpdateHotkeyDisplay()
        {
            if (Modifiers == 0 && Key == Keys.None)
            {
                txtHotkey.Text = "None";
            }
            else
            {
                string modifierName = HotkeyManager.GetModifierName(Modifiers);
                txtHotkey.Text = string.IsNullOrEmpty(modifierName)
                    ? Key.ToString()
                    : $"{modifierName}+{Key}";
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
