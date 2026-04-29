using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace VDManager.Services
{
    /// <summary>
    /// Manages application theme (light/dark mode)
    /// </summary>
    public class ThemeManager : IThemeManager
    {
        private bool _disposed;
        public enum Theme
        {
            Light,
            Dark,
            System
        }

        private Theme currentTheme = Theme.System;
        private bool isSystemDarkMode = false;

        public event EventHandler? ThemeChanged;

        public ThemeManager()
        {
            DetectSystemTheme();

            // Listen for system theme changes
            SystemEvents.UserPreferenceChanged += OnSystemPreferenceChanged;
        }

        private void OnSystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                bool wasDark = isSystemDarkMode;
                DetectSystemTheme();

                if (wasDark != isSystemDarkMode && currentTheme == Theme.System)
                {
                    ThemeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void DetectSystemTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            isSystemDarkMode = (int)value == 0;
                        }
                    }
                }
            }
            catch
            {
                isSystemDarkMode = false;
            }
        }

        public void SetTheme(Theme theme)
        {
            currentTheme = theme;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public Theme GetCurrentTheme()
        {
            return currentTheme;
        }

        public bool IsDarkMode()
        {
            return currentTheme switch
            {
                Theme.Dark => true,
                Theme.Light => false,
                Theme.System => isSystemDarkMode,
                _ => false
            };
        }

        public void ApplyTheme(Form form)
        {
            bool isDark = IsDarkMode();
            Win32API.SetTitleBarDarkMode(form.Handle, isDark);

            if (isDark)
            {
                ApplyDarkTheme(form);
            }
            else
            {
                ApplyLightTheme(form);
            }
        }

        private void ApplyDarkTheme(Control control)
        {
            // Enhanced dark theme colors
            control.BackColor = Color.FromArgb(30, 30, 30);  // Darkest
            control.ForeColor = Color.FromArgb(240, 240, 240);  // Off-white

            foreach (Control child in control.Controls)
            {
                if (child is TabControl tabControl)
                {
                    tabControl.BackColor = Color.FromArgb(45, 45, 45);
                    tabControl.ForeColor = Color.FromArgb(240, 240, 240);
                    tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                    tabControl.DrawItem -= DarkTabControl_DrawItem;
                    tabControl.DrawItem += DarkTabControl_DrawItem;
                }
                else if (child is TabPage tabPage)
                {
                    tabPage.BackColor = Color.FromArgb(30, 30, 30);
                    tabPage.ForeColor = Color.FromArgb(240, 240, 240);
                }
                else if (child is Button button)
                {
                    button.BackColor = Color.FromArgb(50, 50, 50);
                    button.ForeColor = Color.FromArgb(240, 240, 240);
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
                }
                else if (child is TextBox textBox)
                {
                    textBox.BackColor = Color.FromArgb(40, 40, 40);
                    textBox.ForeColor = Color.FromArgb(240, 240, 240);
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (child is ListBox listBox)
                {
                    listBox.BackColor = Color.FromArgb(40, 40, 40);
                    listBox.ForeColor = Color.FromArgb(240, 240, 240);
                }
                else if (child is ComboBox comboBox)
                {
                    comboBox.BackColor = Color.FromArgb(40, 40, 40);
                    comboBox.ForeColor = Color.FromArgb(240, 240, 240);
                    comboBox.FlatStyle = FlatStyle.Flat;
                }
                else if (child is DataGridView dgv)
                {
                    dgv.BackgroundColor = Color.FromArgb(30, 30, 30);
                    dgv.ForeColor = Color.FromArgb(240, 240, 240);
                    dgv.GridColor = Color.FromArgb(60, 60, 60);

                    // Default cell style
                    dgv.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
                    dgv.DefaultCellStyle.ForeColor = Color.FromArgb(240, 240, 240);
                    dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);  // Windows blue
                    dgv.DefaultCellStyle.SelectionForeColor = Color.White;

                    // Alternating row color for better readability
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
                    dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(240, 240, 240);

                    // Header styling
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(240, 240, 240);
                    dgv.EnableHeadersVisualStyles = false;
                }
                else if (child is Label label)
                {
                    label.ForeColor = Color.FromArgb(240, 240, 240);
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.ForeColor = Color.FromArgb(240, 240, 240);
                }
                else if (child is Panel panel)
                {
                    // Panel background slightly lighter than form
                    panel.BackColor = Color.FromArgb(40, 40, 40);
                    // Add border if panel has FixedSingle style
                    if (panel.BorderStyle == BorderStyle.FixedSingle)
                    {
                        panel.ForeColor = Color.FromArgb(60, 60, 60);  // Border color via parent
                    }
                }
                else if (child is GroupBox groupBox)
                {
                    // GroupBox ForeColor controls both the title text AND the border outline.
                    // Use a medium gray so the border isn't a harsh white, while title stays readable.
                    groupBox.ForeColor = Color.FromArgb(160, 160, 160);
                }
                else if (child is RadioButton radioButton)
                {
                    radioButton.BackColor = Color.FromArgb(30, 30, 30);
                    radioButton.ForeColor = Color.FromArgb(240, 240, 240);
                }
                else if (child is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = Color.FromArgb(50, 50, 50);
                    numericUpDown.ForeColor = Color.FromArgb(240, 240, 240);
                }
                else if (child is RichTextBox richTextBox)
                {
                    richTextBox.BackColor = Color.FromArgb(30, 30, 30);
                    richTextBox.ForeColor = Color.FromArgb(240, 240, 240);
                }

                // Recursively apply to child controls
                if (child.Controls.Count > 0)
                {
                    ApplyDarkTheme(child);
                }
            }
        }

        private void ApplyLightTheme(Control control)
        {
            // Enhanced light theme colors
            control.BackColor = Color.FromArgb(240, 240, 240);  // Very light gray
            control.ForeColor = Color.Black;

            foreach (Control child in control.Controls)
            {
                if (child is TabControl tabControl)
                {
                    tabControl.DrawMode = TabDrawMode.Normal;
                    tabControl.DrawItem -= DarkTabControl_DrawItem;
                    tabControl.BackColor = SystemColors.Control;
                    tabControl.ForeColor = SystemColors.ControlText;
                }
                else if (child is TabPage tabPage)
                {
                    tabPage.BackColor = SystemColors.Control;
                    tabPage.ForeColor = SystemColors.ControlText;
                }
                else if (child is Button button)
                {
                    button.BackColor = SystemColors.Control;
                    button.ForeColor = SystemColors.ControlText;
                    button.FlatStyle = FlatStyle.Standard;
                }
                else if (child is TextBox textBox)
                {
                    textBox.BackColor = SystemColors.Window;
                    textBox.ForeColor = SystemColors.WindowText;
                }
                else if (child is ListBox listBox)
                {
                    listBox.BackColor = SystemColors.Window;
                    listBox.ForeColor = SystemColors.WindowText;
                }
                else if (child is ComboBox comboBox)
                {
                    comboBox.BackColor = SystemColors.Window;
                    comboBox.ForeColor = SystemColors.WindowText;
                    comboBox.FlatStyle = FlatStyle.Standard;
                }
                else if (child is DataGridView dgv)
                {
                    dgv.BackgroundColor = Color.White;
                    dgv.ForeColor = Color.Black;
                    dgv.GridColor = Color.FromArgb(220, 220, 220);  // Light gray grid lines

                    // Default cell style
                    dgv.DefaultCellStyle.BackColor = Color.White;
                    dgv.DefaultCellStyle.ForeColor = Color.Black;
                    dgv.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                    dgv.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;

                    // Alternating row color for better readability
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);  // Very subtle gray
                    dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black;

                    // Header styling
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    dgv.EnableHeadersVisualStyles = false;  // Use custom colors
                }
                else if (child is Label label)
                {
                    label.ForeColor = SystemColors.ControlText;
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.ForeColor = SystemColors.ControlText;
                }
                else if (child is Panel panel)
                {
                    // Panel background is white for clean look
                    panel.BackColor = Color.White;
                    // Add subtle border if panel has FixedSingle style
                    if (panel.BorderStyle == BorderStyle.FixedSingle)
                    {
                        panel.ForeColor = Color.FromArgb(200, 200, 200);  // Light gray border
                    }
                }
                else if (child is GroupBox groupBox)
                {
                    groupBox.ForeColor = SystemColors.ControlText;
                }
                else if (child is RadioButton radioButton)
                {
                    radioButton.BackColor = SystemColors.Control;
                    radioButton.ForeColor = SystemColors.ControlText;
                }
                else if (child is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = SystemColors.Window;
                    numericUpDown.ForeColor = SystemColors.WindowText;
                }
                else if (child is RichTextBox richTextBox)
                {
                    richTextBox.BackColor = SystemColors.Window;
                    richTextBox.ForeColor = SystemColors.WindowText;
                }

                // Recursively apply to child controls
                if (child.Controls.Count > 0)
                {
                    ApplyLightTheme(child);
                }
            }
        }

        /// <summary>
        /// Get the secondary text color for the current theme (for hints, descriptions)
        /// </summary>
        public Color GetSecondaryTextColor()
        {
            return IsDarkMode()
                ? Color.FromArgb(180, 180, 180)  // Light gray for dark theme
                : Color.FromArgb(96, 96, 96);    // Medium gray for light theme
        }

        /// <summary>
        /// Apply secondary text color to a label (for hints, descriptions)
        /// </summary>
        public void ApplySecondaryTextColor(Label label)
        {
            label.ForeColor = GetSecondaryTextColor();
        }

        private static void DarkTabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tc) return;

            var tab = tc.TabPages[e.Index];
            var bounds = tc.GetTabRect(e.Index);
            bool selected = e.Index == tc.SelectedIndex;

            var stripColor = Color.FromArgb(28, 28, 28);

            // Fill leftward gap before the first tab (left edge → tab start)
            if (e.Index == 0 && bounds.Left > 0)
            {
                using var gapBrush = new SolidBrush(stripColor);
                e.Graphics.FillRectangle(gapBrush, 0, 0, bounds.Left, bounds.Bottom);
            }

            // Fill rightward gap after the last tab (tab end → control right edge)
            if (e.Index == tc.TabCount - 1 && bounds.Right < tc.Width)
            {
                using var gapBrush = new SolidBrush(stripColor);
                e.Graphics.FillRectangle(gapBrush, bounds.Right, 0, tc.Width - bounds.Right, bounds.Bottom);
            }

            // Tab background
            Color bg = selected ? Color.FromArgb(40, 40, 40) : stripColor;
            using var bgBrush = new SolidBrush(bg);
            e.Graphics.FillRectangle(bgBrush, bounds);

            // Blue accent bar on the bottom edge of the selected tab
            if (selected)
            {
                using var accentBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                e.Graphics.FillRectangle(accentBrush, bounds.Left, bounds.Bottom - 2, bounds.Width, 2);
            }

            // Text
            using var textBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(tab.Text, e.Font ?? tc.Font, textBrush, (RectangleF)bounds, sf);
        }

        /// <summary>
        /// Apply theme to a ContextMenuStrip. Must be called explicitly since
        /// ContextMenuStrip is not part of the Control tree and won't be reached
        /// by the recursive ApplyTheme(Form) walk. Safe to call again after
        /// adding new items (e.g. dynamic submenus).
        /// </summary>
        public void ApplyTheme(ContextMenuStrip menu)
        {
            if (IsDarkMode())
            {
                menu.BackColor = Color.FromArgb(45, 45, 45);
                menu.ForeColor = Color.FromArgb(240, 240, 240);
                menu.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());
                ApplyThemeToStripItems(menu.Items, dark: true);
            }
            else
            {
                menu.BackColor = SystemColors.Menu;
                menu.ForeColor = SystemColors.MenuText;
                menu.Renderer = new ToolStripProfessionalRenderer();
                ApplyThemeToStripItems(menu.Items, dark: false);
            }
        }

        private static void ApplyThemeToStripItems(ToolStripItemCollection items, bool dark)
        {
            foreach (ToolStripItem item in items)
            {
                item.BackColor = dark ? Color.FromArgb(45, 45, 45) : SystemColors.Menu;
                item.ForeColor = dark ? Color.FromArgb(240, 240, 240) : SystemColors.MenuText;

                if (item is ToolStripMenuItem menuItem && menuItem.DropDownItems.Count > 0)
                    ApplyThemeToStripItems(menuItem.DropDownItems, dark);
            }
        }

        private sealed class DarkMenuColorTable : ProfessionalColorTable
        {
            public override Color MenuBorder => Color.FromArgb(70, 70, 70);
            public override Color MenuItemBorder => Color.FromArgb(70, 120, 180);
            public override Color MenuItemSelected => Color.FromArgb(60, 60, 60);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 60);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, 60, 60);
            public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 45);
            public override Color ImageMarginGradientBegin => Color.FromArgb(38, 38, 38);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(38, 38, 38);
            public override Color ImageMarginGradientEnd => Color.FromArgb(38, 38, 38);
            public override Color CheckBackground => Color.FromArgb(60, 60, 60);
            public override Color CheckSelectedBackground => Color.FromArgb(70, 120, 180);
            public override Color SeparatorDark => Color.FromArgb(70, 70, 70);
            public override Color SeparatorLight => Color.FromArgb(55, 55, 55);
        }

        public void Dispose()
        {
            if (_disposed) return;
            SystemEvents.UserPreferenceChanged -= OnSystemPreferenceChanged;
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
