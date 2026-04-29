using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VDManager.Controls
{
    /// <summary>
    /// Visual panel showing all available window layout options in a grid.
    /// All pixel dimensions are scaled by the form's current DeviceDpi so the
    /// control looks correct at any Windows display-scaling setting.
    /// </summary>
    public class VisualQuadrantPanel : Panel
    {
        private Quadrant selectedQuadrant = Quadrant.None;
        private List<LayoutButton> layoutButtons = new List<LayoutButton>();

        // Design-time reference sizes (at 96 DPI / 100% scaling)
        private const int BaseButtonSize = 70;
        private const int BaseSpacing    = 6;

        public event EventHandler? QuadrantChanged;

        public Quadrant SelectedQuadrant
        {
            get => selectedQuadrant;
            set
            {
                if (selectedQuadrant != value)
                {
                    selectedQuadrant = value;
                    UpdateButtonStates();
                    QuadrantChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public VisualQuadrantPanel()
        {
            AutoScroll    = false;
            BorderStyle   = BorderStyle.FixedSingle;
            Padding       = new Padding(8);
            BackColor     = System.Drawing.SystemColors.Control;

            InitializeLayoutButtons();
        }

        /// <summary>
        /// Returns the effective DPI scale factor for this control.
        /// Uses DeviceDpi (available from .NET 5+ / WinForms on .NET Core) when
        /// it returns a useful value, otherwise falls back to 1.0.
        /// </summary>
        private float DpiScale
        {
            get
            {
                try
                {
                    int dpi = DeviceDpi; // inherited from Control (WinForms .NET 5+)
                    return dpi > 0 ? dpi / 96f : 1f;
                }
                catch
                {
                    return 1f;
                }
            }
        }

        private int Scale(int baseValue) => (int)Math.Round(baseValue * DpiScale);

        private void InitializeLayoutButtons()
        {
            // Sizes are computed lazily at layout time (OnLayout) so the initial
            // call during construction uses a scale of 1.0 — the correct positions
            // are recalculated once the control is parented to a real window.
            RebuildButtons();
        }

        private void RebuildButtons()
        {
            // Remove existing buttons
            foreach (var btn in layoutButtons)
            {
                Controls.Remove(btn);
                btn.Dispose();
            }
            layoutButtons.Clear();

            int buttonSize = Scale(BaseButtonSize);
            int spacing    = Scale(BaseSpacing);

            // Calculate how many columns fit in the available client width.
            // At least 1 column so the panel always shows something.
            int availableWidth = ClientSize.Width - Padding.Left - Padding.Right;
            int cols = Math.Max(1, (availableWidth + spacing) / (buttonSize + spacing));

            int x = Padding.Left;
            int y = Padding.Top;
            int col = 0;

            // Define all layouts
            var layouts = new[]
            {
                (Quadrant.None,               "None",          "No snap"),
                (Quadrant.TopLeft,            "Top Left",      "Quarter"),
                (Quadrant.TopRight,           "Top Right",     "Quarter"),
                (Quadrant.BottomLeft,         "Bottom Left",   "Quarter"),
                (Quadrant.BottomRight,        "Bottom Right",  "Quarter"),
                (Quadrant.LeftHalf,           "Left Half",     "50%"),
                (Quadrant.RightHalf,          "Right Half",    "50%"),
                (Quadrant.TopHalf,            "Top Half",      "50%"),
                (Quadrant.BottomHalf,         "Bottom Half",   "50%"),
                (Quadrant.LeftThird,          "Left Third",    "33%"),
                (Quadrant.CenterThird,        "Center Third",  "33%"),
                (Quadrant.RightThird,         "Right Third",   "33%"),
                (Quadrant.LeftTwoThirds,      "Left 2/3",      "66%"),
                (Quadrant.RightTwoThirds,     "Right 2/3",     "66%"),
                (Quadrant.CenterHalf,         "Center Half",   "50%"),
                (Quadrant.LeftQuarter,        "Left Quarter",  "25%"),
                (Quadrant.CenterLeftQuarter,  "Ctr-Left",      "25%"),
                (Quadrant.CenterRightQuarter, "Ctr-Right",     "25%"),
                (Quadrant.RightQuarter,       "Right Quarter", "25%"),
                (Quadrant.LeftThreeQuarters,  "Left 3/4",      "75%"),
                (Quadrant.RightThreeQuarters, "Right 3/4",     "75%"),
                (Quadrant.Maximized,          "Maximized",     "Full"),
            };

            foreach (var (quadrant, name, description) in layouts)
            {
                var btn = new LayoutButton(quadrant, name, description);
                btn.Size     = new Size(buttonSize, buttonSize);
                btn.Location = new Point(x, y);
                btn.Click   += (s, e) => SelectedQuadrant = quadrant;

                Controls.Add(btn);
                layoutButtons.Add(btn);

                col++;
                if (col >= cols)
                {
                    col = 0;
                    x   = Padding.Left;
                    y  += buttonSize + spacing;
                }
                else
                {
                    x += buttonSize + spacing;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RebuildButtons();
            UpdateButtonStates();
        }

        /// <summary>
        /// Rebuild buttons whenever the DPI changes (e.g. window dragged to a
        /// different monitor or Windows display-scaling changed at runtime).
        /// </summary>
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            RebuildButtons();
            UpdateButtonStates();
            Invalidate();
        }

        private void UpdateButtonStates()
        {
            foreach (var btn in layoutButtons)
            {
                btn.IsSelected = (btn.Quadrant == selectedQuadrant);
            }
        }

        // ── Inner control ──────────────────────────────────────────────────────

        /// <summary>
        /// Button that visually represents a layout option.
        /// All internal pixel values are scaled via the inherited <c>DeviceDpi</c>.
        /// </summary>
        private class LayoutButton : Control
        {
            public Quadrant Quadrant      { get; }
            private string  layoutName;
            private string  layoutDescription;
            private bool    isSelected;
            private bool    isHovered;

            // Design-time constants (at 96 DPI)
            private const int BaseMonitorPadding = 8;
            private const int BaseMonitorHeight  = 34;
            private const int BaseCornerRadius   = 6;
            private const int BaseTextGap        = 4;
            private const int BaseNameFontPt     = 7;   // points — already device-independent
            private const int BaseDescFontPt     = 6;   // points — already device-independent
            private const int BaseNameLineHeight  = 14; // pixels at 96 DPI
            private const int BaseDescLineHeight  = 10; // pixels at 96 DPI

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        Invalidate();
                    }
                }
            }

            public LayoutButton(Quadrant quadrant, string name, string description)
            {
                Quadrant           = quadrant;
                layoutName         = name;
                layoutDescription  = description;

                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint           |
                         ControlStyles.DoubleBuffer        |
                         ControlStyles.Selectable, true);

                Cursor = Cursors.Hand;
            }

            private float DpiScale
            {
                get
                {
                    try   { int d = DeviceDpi; return d > 0 ? d / 96f : 1f; }
                    catch { return 1f; }
                }
            }

            private int Scale(int v) => (int)Math.Round(v * DpiScale);

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                isHovered = true;
                Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                isHovered = false;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Graphics g = e.Graphics;
                g.SmoothingMode       = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint   = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Detect dark mode from parent's BackColor
                bool isDark = (Parent?.BackColor.GetBrightness() ?? 1f) < 0.5f;

                int cornerRadius   = Scale(BaseCornerRadius);
                int monitorPadding = Scale(BaseMonitorPadding);
                int monitorHeight  = Scale(BaseMonitorHeight);
                int textGap        = Scale(BaseTextGap);
                int nameLineH      = Scale(BaseNameLineHeight);
                int descLineH      = Scale(BaseDescLineHeight);

                // Create rounded rectangle path
                using (var path = GetRoundedRectangle(ClientRectangle, cornerRadius))
                {
                    // Background with subtle gradient
                    Color bgColor1, bgColor2;
                    if (isSelected)
                    {
                        bgColor1 = isDark ? Color.FromArgb(30, 60, 100)  : Color.FromArgb(240, 248, 255);
                        bgColor2 = isDark ? Color.FromArgb(25, 50, 90)   : Color.FromArgb(225, 240, 255);
                    }
                    else if (isHovered)
                    {
                        bgColor1 = isDark ? Color.FromArgb(60, 62, 65)   : Color.FromArgb(250, 252, 255);
                        bgColor2 = isDark ? Color.FromArgb(55, 57, 60)   : Color.FromArgb(245, 248, 252);
                    }
                    else
                    {
                        bgColor1 = isDark ? Color.FromArgb(50, 52, 55)   : Color.FromArgb(252, 252, 252);
                        bgColor2 = isDark ? Color.FromArgb(45, 47, 50)   : Color.FromArgb(248, 248, 248);
                    }

                    using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        ClientRectangle, bgColor1, bgColor2, 90f))
                    {
                        g.FillPath(bgBrush, path);
                    }

                    // Border
                    Color borderColor = isSelected
                        ? Color.FromArgb(0, 120, 215)
                        : isHovered
                            ? (isDark ? Color.FromArgb(90, 140, 200)  : Color.FromArgb(100, 160, 220))
                            : (isDark ? Color.FromArgb(70, 72, 75)    : Color.FromArgb(200, 200, 200));
                    int borderWidth = isSelected ? 2 : 1;
                    using (var borderPen = new Pen(borderColor, borderWidth))
                    {
                        borderPen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                        g.DrawPath(borderPen, path);
                    }

                    // Draw miniature monitor with shadow
                    Rectangle monitorRect = new Rectangle(
                        monitorPadding,
                        monitorPadding,
                        Width - monitorPadding * 2,
                        monitorHeight);

                    Rectangle shadowRect = monitorRect;
                    shadowRect.Offset(1, 1);
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(isDark ? 60 : 30, 0, 0, 0)))
                        g.FillRectangle(shadowBrush, shadowRect);

                    Color monBg1 = isDark ? Color.FromArgb(55, 57, 60) : Color.FromArgb(250, 250, 250);
                    Color monBg2 = isDark ? Color.FromArgb(45, 47, 50) : Color.FromArgb(235, 235, 235);
                    using (var monitorBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        monitorRect, monBg1, monBg2, 90f))
                    {
                        g.FillRectangle(monitorBrush, monitorRect);
                    }

                    Color monBorderColor = isDark ? Color.FromArgb(80, 82, 85) : Color.FromArgb(160, 160, 160);
                    using (var monitorPen = new Pen(monBorderColor, 1))
                        g.DrawRectangle(monitorPen, monitorRect);

                    DrawLayoutRepresentation(g, monitorRect);

                    // Text — font size is in points so it is already DPI-independent.
                    // Only the pixel-based vertical positions are scaled.
                    using (var nameFont = new Font("Segoe UI", BaseNameFontPt, FontStyle.Regular))
                    using (var descFont = new Font("Segoe UI", BaseDescFontPt))
                    {
                        Color textColor = isSelected
                            ? (isDark ? Color.FromArgb(110, 185, 255) : Color.FromArgb(0, 80, 160))
                            : (isDark ? Color.FromArgb(200, 200, 200) : Color.FromArgb(70, 70, 70));

                        using (var textBrush = new SolidBrush(textColor))
                        using (var sf = new StringFormat
                        {
                            Alignment     = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near,
                            Trimming      = StringTrimming.EllipsisCharacter,
                            FormatFlags   = StringFormatFlags.NoWrap
                        })
                        {
                            int textY = monitorRect.Bottom + textGap;
                            g.DrawString(layoutName, nameFont, textBrush,
                                new RectangleF(0, textY, Width, nameLineH), sf);

                            Color descColor = isDark
                                ? Color.FromArgb(130, 130, 130)
                                : Color.FromArgb(120, 120, 120);
                            using (var descBrush = new SolidBrush(descColor))
                            {
                                g.DrawString(layoutDescription, descFont, descBrush,
                                    new RectangleF(0, textY + nameLineH, Width, descLineH), sf);
                            }
                        }
                    }
                }
            }

            private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
            {
                int diameter = radius * 2;
                var arc  = new Rectangle(bounds.Location, new Size(diameter, diameter));
                var path = new System.Drawing.Drawing2D.GraphicsPath();

                path.AddArc(arc, 180, 90);
                arc.X = bounds.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = bounds.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = bounds.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
                return path;
            }

            private void DrawLayoutRepresentation(Graphics g, Rectangle monitorRect)
            {
                if (Quadrant == Quadrant.None)
                {
                    Color noneLineColor = isSelected ? Color.DodgerBlue : Color.SteelBlue;
                    using (var linePen = new Pen(noneLineColor, isSelected ? 2f : 1.5f))
                    {
                        linePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        g.DrawLine(linePen, monitorRect.Left + 2, monitorRect.Top + 2, monitorRect.Right - 2, monitorRect.Bottom - 2);
                        g.DrawLine(linePen, monitorRect.Right - 2, monitorRect.Top + 2, monitorRect.Left + 2, monitorRect.Bottom - 2);
                    }
                    return;
                }

                Rectangle layoutRect = GetLayoutRectangle(monitorRect);

                Color fillColor = isSelected
                    ? Color.FromArgb(150, Color.DodgerBlue)
                    : Color.FromArgb(100, Color.SteelBlue);
                Color lineColor = isSelected ? Color.DodgerBlue : Color.SteelBlue;

                using (Brush fillBrush = new SolidBrush(fillColor))
                using (Pen   linePen  = new Pen(lineColor, 1.5f))
                {
                    g.FillRectangle(fillBrush, layoutRect);
                    g.DrawRectangle(linePen,   layoutRect);
                }
            }

            private Rectangle GetLayoutRectangle(Rectangle monitorRect)
            {
                int x = monitorRect.X;
                int y = monitorRect.Y;
                int w = monitorRect.Width;
                int h = monitorRect.Height;

                int halfW      = w / 2;
                int halfH      = h / 2;
                int thirdW     = w / 3;
                int twoThirdsW = (w * 2) / 3;
                int quarterW   = w / 4;

                return Quadrant switch
                {
                    Quadrant.TopLeft             => new Rectangle(x,              y,      halfW,      halfH),
                    Quadrant.TopRight            => new Rectangle(x + halfW,      y,      halfW,      halfH),
                    Quadrant.BottomLeft          => new Rectangle(x,              y+halfH, halfW,     halfH),
                    Quadrant.BottomRight         => new Rectangle(x + halfW,      y+halfH, halfW,     halfH),
                    Quadrant.LeftHalf            => new Rectangle(x,              y,      halfW,      h),
                    Quadrant.RightHalf           => new Rectangle(x + halfW,      y,      halfW,      h),
                    Quadrant.TopHalf             => new Rectangle(x,              y,      w,          halfH),
                    Quadrant.BottomHalf          => new Rectangle(x,              y+halfH, w,         halfH),
                    Quadrant.LeftThird           => new Rectangle(x,              y,      thirdW,     h),
                    Quadrant.CenterThird         => new Rectangle(x + thirdW,     y,      thirdW,     h),
                    Quadrant.RightThird          => new Rectangle(x + twoThirdsW, y,      thirdW,     h),
                    Quadrant.LeftTwoThirds       => new Rectangle(x,              y,      twoThirdsW, h),
                    Quadrant.RightTwoThirds      => new Rectangle(x + thirdW,     y,      twoThirdsW, h),
                    Quadrant.CenterHalf          => new Rectangle(x + quarterW,   y,      halfW,      h),
                    Quadrant.LeftQuarter         => new Rectangle(x,              y,      quarterW,   h),
                    Quadrant.CenterLeftQuarter   => new Rectangle(x + quarterW,   y,      quarterW,   h),
                    Quadrant.CenterRightQuarter  => new Rectangle(x + quarterW*2, y,      quarterW,   h),
                    Quadrant.RightQuarter        => new Rectangle(x + quarterW*3, y,      quarterW,   h),
                    Quadrant.LeftThreeQuarters   => new Rectangle(x,              y,      quarterW*3, h),
                    Quadrant.RightThreeQuarters  => new Rectangle(x + quarterW,   y,      quarterW*3, h),
                    Quadrant.Maximized           => new Rectangle(x,              y,      w,          h),
                    _                            => new Rectangle(x,              y,      halfW,      halfH),
                };
            }
        }
    }
}
