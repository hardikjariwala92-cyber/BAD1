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
        public bool UseGradient { get; set; } = false;
        public Color GradientColorStart { get; set; } = Color.FromArgb(79, 70, 229);
        public Color GradientColorEnd { get; set; } = Color.FromArgb(99, 102, 241);
        public Color HoverGradientColorStart { get; set; } = Color.FromArgb(99, 102, 241);
        public Color HoverGradientColorEnd { get; set; } = Color.FromArgb(129, 140, 248);
        public bool HasGlowOnHover { get; set; } = false;
        public Color GlowColor { get; set; } = Color.FromArgb(99, 102, 241);
        public Color TextHoverColor { get; set; } = Color.Empty;

        private bool _isHovered = false;
        private bool _isPressed = false;

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.SupportsTransparentBackColor, 
                true);
            UpdateStyles();
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
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Paint parent background first to eliminate black corner artifacts
            Color parentBg = Parent != null && Parent.BackColor != Color.Transparent ? Parent.BackColor : ThemeManager.Background;
            using (var bgBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var path = CreateRoundedPath(rect, BorderRadius);

            Color currentBg = _isPressed ? PressedColor : (_isHovered ? HoverColor : NormalColor);

            if (IsOutlined)
            {
                Color fillCol = _isHovered ? Color.FromArgb(28, currentBg) : parentBg;
                using var brush = new SolidBrush(fillCol);
                g.FillPath(brush, path);

                Color borderCol = BorderOutlineColor != Color.Empty ? BorderOutlineColor : currentBg;
                using var pen = new Pen(borderCol, 1.5f);
                g.DrawPath(pen, path);
            }
            else if (UseGradient && !_isPressed)
            {
                Color gStart = _isHovered && HoverGradientColorStart != Color.Empty ? HoverGradientColorStart : GradientColorStart;
                Color gEnd = _isHovered && HoverGradientColorEnd != Color.Empty ? HoverGradientColorEnd : GradientColorEnd;
                using var gradBrush = new LinearGradientBrush(rect, gStart, gEnd, LinearGradientMode.ForwardDiagonal);
                g.FillPath(gradBrush, path);

                // Subtle top highlight border for modern CSS sheen
                using var sheenPen = new Pen(Color.FromArgb(45, 255, 255, 255), 1f);
                g.DrawPath(sheenPen, path);
            }
            else
            {
                using var brush = new SolidBrush(currentBg);
                g.FillPath(brush, path);
            }

            if (HasGlowOnHover && _isHovered && GlowColor != Color.Empty)
            {
                using var glowPen = new Pen(Color.FromArgb(90, GlowColor), 2f);
                g.DrawPath(glowPen, path);
            }
            else if (BorderOutlineColor != Color.Empty && !IsOutlined)
            {
                using var borderPen = new Pen(BorderOutlineColor, 1f);
                g.DrawPath(borderPen, path);
            }

            // Draw Text
            Color textCol = _isHovered && TextHoverColor != Color.Empty ? TextHoverColor : ForeColor;
            if (IsOutlined && textCol == Color.White && ThemeManager.CurrentTheme == AppTheme.Light)
            {
                textCol = Color.FromArgb(51, 65, 85);
            }
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                rect,
                textCol,
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
