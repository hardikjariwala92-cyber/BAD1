using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class RoundedPanel : Panel
    {
        public int BorderRadius { get; set; } = 16;
        public Color CustomBorderColor { get; set; } = Color.Empty;
        public Color CustomBackgroundColor { get; set; } = Color.Empty;
        public bool UseThemeBackground { get; set; } = true;
        public int BorderWidth { get; set; } = 1;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            UpdateStyles();
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color bgColor = CustomBackgroundColor != Color.Empty 
                ? CustomBackgroundColor 
                : (UseThemeBackground ? ThemeManager.CardBg : BackColor);

            Color borderColor = CustomBorderColor != Color.Empty 
                ? CustomBorderColor 
                : ThemeManager.CardBorder;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var path = CreateRoundedRectanglePath(rect, BorderRadius);

            // Fill background
            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw border
            if (BorderWidth > 0)
            {
                using var pen = new Pen(borderColor, BorderWidth);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
