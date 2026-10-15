using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class ThemeToggle : Control
    {
        public bool IsDarkMode => ThemeManager.CurrentTheme == AppTheme.Dark;

        public ThemeToggle()
        {
            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw | 
                ControlStyles.SupportsTransparentBackColor, 
                true);
            UpdateStyles();

            Size = new Size(64, 30);
            Cursor = Cursors.Hand;
            BackColor = ThemeManager.Background;
            ThemeManager.OnThemeChanged += HandleThemeChanged;
        }

        private void HandleThemeChanged(AppTheme theme)
        {
            BackColor = Parent?.BackColor != Color.Transparent && Parent?.BackColor != null && !Parent.BackColor.IsEmpty
                ? Parent.BackColor
                : ThemeManager.Background;
            Invalidate();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null && Parent.BackColor != Color.Transparent && !Parent.BackColor.IsEmpty)
            {
                BackColor = Parent.BackColor;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.OnThemeChanged -= HandleThemeChanged;
            }
            base.Dispose(disposing);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ThemeManager.ToggleTheme();
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Color bg = Parent?.BackColor != Color.Transparent && Parent?.BackColor != null && !Parent.BackColor.IsEmpty
                ? Parent.BackColor
                : ThemeManager.Background;
            using var bgBrush = new SolidBrush(bg);
            pevent.Graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            int radius = rect.Height / 2;

            using var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 90, 180);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 180);
            path.CloseFigure();

            // Background pill
            Color pillBg = IsDarkMode ? Color.FromArgb(38, 41, 56) : Color.FromArgb(220, 226, 238);
            using (var bgBrush = new SolidBrush(pillBg))
            {
                g.FillPath(bgBrush, path);
            }

            // Draw Sun & Moon icons or text indicators
            int thumbSize = rect.Height - 6;
            int thumbY = rect.Y + 3;
            int thumbX = IsDarkMode ? rect.Right - thumbSize - 3 : rect.X + 3;

            // Draw icons on sides
            // Sun on left (Light)
            g.FillEllipse(Brushes.Gold, rect.X + 6, rect.Y + 7, 10, 10);

            // Moon on right (Dark)
            using (var moonBrush = new SolidBrush(Color.FromArgb(180, 190, 230)))
            {
                g.FillEllipse(moonBrush, rect.Right - 16, rect.Y + 7, 10, 10);
            }

            // Draw moving slider knob
            Color thumbColor = IsDarkMode ? ThemeManager.AccentBlue : Color.White;
            using (var thumbBrush = new SolidBrush(thumbColor))
            {
                g.FillEllipse(thumbBrush, thumbX, thumbY, thumbSize, thumbSize);
            }

            // Knob border / glow
            using (var thumbPen = new Pen(Color.FromArgb(50, 0, 0, 0), 1f))
            {
                g.DrawEllipse(thumbPen, thumbX, thumbY, thumbSize, thumbSize);
            }
        }
    }
}
