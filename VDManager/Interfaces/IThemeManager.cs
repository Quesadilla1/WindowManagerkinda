using System;
using System.Drawing;
using System.Windows.Forms;

namespace VDManager.Services
{
    public interface IThemeManager : IDisposable
    {
        event EventHandler? ThemeChanged;

        void SetTheme(ThemeManager.Theme theme);
        ThemeManager.Theme GetCurrentTheme();
        bool IsDarkMode();
        Color GetSecondaryTextColor();

        void ApplyTheme(Form form);
        void ApplyTheme(ContextMenuStrip menu);
        void ApplySecondaryTextColor(Label label);
    }
}
