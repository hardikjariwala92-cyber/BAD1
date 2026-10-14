using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class ModernButton : Button
    {
        public int BorderRadius { get; set; } = 10;
        public Color NormalColor { get; set; } = Color.FromArgb(76, 111, 255);
        public Color HoverColor { get; set; } = Color.FromArgb(100, 130, 255);
        public Color PressedColor { get; set; } = Color.FromArgb(60, 90, 220);
        public Color BorderOutlineColor { get; set; } = Color.Empty;
        public bool IsOutlined { get; set; } = false;

        private bool _isHovered = false;
        private bool _isPressed = false;

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var path = CreateRoundedPath(rect, BorderRadius);

            Color currentBg = _isPressed ? PressedColor : (_isHovered ? HoverColor : NormalColor);

            if (IsOutlined)
            {
                using var brush = new SolidBrush(_isHovered ? Color.FromArgb(20, currentBg) : Color.Transparent);
                g.FillPath(brush, path);

                Color borderCol = BorderOutlineColor != Color.Empty ? BorderOutlineColor : currentBg;
                using var pen = new Pen(borderCol, 1.5f);
                g.DrawPath(pen, path);
            }
            else
            {
                using var brush = new SolidBrush(currentBg);
                g.FillPath(brush, path);
            }

            // Draw Text
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                rect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
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
