using System;
using System.Drawing;
using System.Windows.Forms;
using VDManager.Models;

namespace VDManager.Controls
{
    /// <summary>
    /// Visual control for selecting window quadrants and layouts.
    /// The grid size is scaled by <c>DeviceDpi</c> so it looks correct at any
    /// Windows display-scaling setting.
    /// </summary>
    public class QuadrantSelector : UserControl
    {
        private Quadrant  selectedQuadrant = Quadrant.TopLeft;
        private Quadrant? hoveredQuadrant  = null;

        private const int CellPadding   = 4;   // logical pixels at 96 DPI
        private const int BaseGridSize  = 120;  // logical pixels at 96 DPI

        public event EventHandler? QuadrantChanged;

        public Quadrant SelectedQuadrant
        {
            get => selectedQuadrant;
            set
            {
                if (selectedQuadrant != value)
                {
                    selectedQuadrant = value;
                    Invalidate();
                    QuadrantChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // ── DPI helpers ────────────────────────────────────────────────────────

        /// <summary>Effective DPI scale factor (1.0 = 96 DPI / 100%).</summary>
        private float DpiScale
        {
            get
            {
                try   { int d = DeviceDpi; return d > 0 ? d / 96f : 1f; }
                catch { return 1f; }
            }
        }

        private int Scale(int v) => (int)Math.Round(v * DpiScale);

        /// <summary>Current physical grid size in pixels.</summary>
        private int GridSize => Scale(BaseGridSize);

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public QuadrantSelector()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint            |
                     ControlStyles.DoubleBuffer, true);

            UpdateSize();
            Cursor = Cursors.Hand;
        }

        private void UpdateSize()
        {
            int pad  = Scale(CellPadding);
            int grid = GridSize;
            Size = new Size(grid + pad * 2, grid + pad * 2);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateSize();
            Invalidate();
        }

        // ── Mouse ─────────────────────────────────────────────────────────────

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var quad = GetQuadrantAtPoint(e.Location);
            if (quad != hoveredQuadrant)
            {
                hoveredQuadrant = quad;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredQuadrant = null;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            var quad = GetQuadrantAtPoint(e.Location);
            if (quad.HasValue)
                SelectedQuadrant = quad.Value;
        }

        private Quadrant? GetQuadrantAtPoint(Point point)
        {
            int pad  = Scale(CellPadding);
            int grid = GridSize;

            int x = point.X - pad;
            int y = point.Y - pad;

            if (x < 0 || y < 0 || x >= grid || y >= grid)
                return null;

            int third      = grid / 3;
            int half       = grid / 2;
            int twoThirds  = third * 2;

            // Top row
            if (y < half)
            {
                if (x < third)          return Quadrant.LeftThird;
                else if (x < half)      return Quadrant.TopLeft;
                else if (x < twoThirds) return Quadrant.TopRight;
                else                    return Quadrant.RightThird;
            }
            // Bottom row
            else
            {
                if (x < third)          return Quadrant.LeftThird;
                else if (x < half)      return Quadrant.BottomLeft;
                else if (x < twoThirds) return Quadrant.BottomRight;
                else                    return Quadrant.RightThird;
            }
        }

        // ── Paint ─────────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            bool isDark = BackColor.GetBrightness() < 0.5f;

            int pad  = Scale(CellPadding);
            int grid = GridSize;

            // Draw monitor background
            Rectangle monitorRect = new Rectangle(pad, pad, grid, grid);
            Color monitorBg     = isDark ? Color.FromArgb(55, 57, 60) : Color.White;
            Color monitorBorder = isDark ? Color.FromArgb(80, 82, 85) : Color.Gray;
            using (var monBrush = new SolidBrush(monitorBg))
            using (var monPen   = new Pen(monitorBorder))
            {
                g.FillRectangle(monBrush, monitorRect);
                g.DrawRectangle(monPen,   monitorRect);
            }

            // Draw all layout options
            DrawLayoutOption(g, Quadrant.TopLeft);
            DrawLayoutOption(g, Quadrant.TopRight);
            DrawLayoutOption(g, Quadrant.BottomLeft);
            DrawLayoutOption(g, Quadrant.BottomRight);
            DrawLayoutOption(g, Quadrant.LeftThird);
            DrawLayoutOption(g, Quadrant.RightThird);
            DrawLayoutOption(g, Quadrant.LeftTwoThirds);
            DrawLayoutOption(g, Quadrant.RightTwoThirds);
            DrawLayoutOption(g, Quadrant.TopHalf);
            DrawLayoutOption(g, Quadrant.BottomHalf);
            DrawLayoutOption(g, Quadrant.CenterHalf);
            DrawLayoutOption(g, Quadrant.CenterThird);
            DrawLayoutOption(g, Quadrant.LeftQuarter);
            DrawLayoutOption(g, Quadrant.CenterLeftQuarter);
            DrawLayoutOption(g, Quadrant.CenterRightQuarter);
            DrawLayoutOption(g, Quadrant.RightQuarter);
            DrawLayoutOption(g, Quadrant.LeftThreeQuarters);
            DrawLayoutOption(g, Quadrant.RightThreeQuarters);
        }

        private void DrawLayoutOption(Graphics g, Quadrant quadrant)
        {
            Rectangle rect = GetQuadrantRectangle(quadrant);

            bool isSelected = (quadrant == selectedQuadrant);
            bool isHovered  = (quadrant == hoveredQuadrant);

            Color fillColor, borderColor;

            if (isSelected)
            {
                fillColor   = Color.FromArgb(100, Color.DodgerBlue);
                borderColor = Color.DodgerBlue;
            }
            else if (isHovered)
            {
                fillColor   = Color.FromArgb(50, Color.LightSkyBlue);
                borderColor = Color.SkyBlue;
            }
            else
            {
                fillColor   = Color.FromArgb(30, Color.LightGray);
                borderColor = Color.LightGray;
            }

            using (Brush brush = new SolidBrush(fillColor))
            using (Pen   pen   = new Pen(borderColor, isSelected ? 2 : 1))
            {
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen,   rect);
            }
        }

        private Rectangle GetQuadrantRectangle(Quadrant quadrant)
        {
            int pad  = Scale(CellPadding);
            int grid = GridSize;

            int x = pad;
            int y = pad;
            int w = grid;
            int h = grid;

            int half       = grid / 2;
            int third      = grid / 3;
            int twoThirds  = third * 2;
            int quarter    = grid / 4;

            return quadrant switch
            {
                Quadrant.TopLeft             => new Rectangle(x,              y,      half,       half),
                Quadrant.TopRight            => new Rectangle(x + half,       y,      half,       half),
                Quadrant.BottomLeft          => new Rectangle(x,              y+half, half,       half),
                Quadrant.BottomRight         => new Rectangle(x + half,       y+half, half,       half),

                Quadrant.LeftThird           => new Rectangle(x,              y,      third,      h),
                Quadrant.CenterThird         => new Rectangle(x + third,      y,      third,      h),
                Quadrant.RightThird          => new Rectangle(x + twoThirds,  y,      third,      h),

                Quadrant.LeftTwoThirds       => new Rectangle(x,              y,      twoThirds,  h),
                Quadrant.RightTwoThirds      => new Rectangle(x + third,      y,      twoThirds,  h),

                Quadrant.TopHalf             => new Rectangle(x,              y,      w,          half),
                Quadrant.BottomHalf          => new Rectangle(x,              y+half, w,          half),

                Quadrant.CenterHalf          => new Rectangle(x + grid / 4,   y,      half,       h),

                Quadrant.LeftQuarter         => new Rectangle(x,              y,      quarter,    h),
                Quadrant.CenterLeftQuarter   => new Rectangle(x + quarter,    y,      quarter,    h),
                Quadrant.CenterRightQuarter  => new Rectangle(x + quarter*2,  y,      quarter,    h),
                Quadrant.RightQuarter        => new Rectangle(x + quarter*3,  y,      quarter,    h),
                Quadrant.LeftThreeQuarters   => new Rectangle(x,              y,      quarter*3,  h),
                Quadrant.RightThreeQuarters  => new Rectangle(x + quarter,    y,      quarter*3,  h),

                Quadrant.Maximized           => new Rectangle(x,              y,      w,          h),

                _                            => new Rectangle(x,              y,      half,       half),
            };
        }
    }
}
