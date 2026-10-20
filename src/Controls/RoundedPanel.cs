using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Models;
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

        // Hover & Lift Features
        public bool EnableHoverEffect { get; set; } = false;
        public Color HoverBorderColor { get; set; } = Color.Empty;
        public Color HoverBackgroundColor { get; set; } = Color.Empty;
        public MetricDetailInfo? MetricInfo { get; set; }
        public Action<RoundedPanel>? OnCardClicked { get; set; }
        public Action<MouseEventArgs>? OnCardMouseWheel { get; set; }

        private bool _isHovered = false;
        public bool IsHovered => _isHovered;

        public RoundedPanel()
        {
            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw | 
                ControlStyles.SupportsTransparentBackColor, 
                true);
            UpdateStyles();

            BackColor = ThemeManager.CardBg;
            ThemeManager.OnThemeChanged += HandleThemeChanged;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            AttachChildMouseEvents(e.Control);
        }

        private void AttachChildMouseEvents(Control? ctrl)
        {
            if (ctrl == null) return;
            ctrl.MouseEnter += (s, e) => TriggerMouseEnter();
            ctrl.MouseLeave += (s, e) => CheckMouseLeave();
            ctrl.Click += (s, e) => OnCardClicked?.Invoke(this);
            ctrl.MouseWheel += (s, e) => OnMouseWheel(e);

            foreach (Control sub in ctrl.Controls)
            {
                AttachChildMouseEvents(sub);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            OnCardMouseWheel?.Invoke(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            TriggerMouseEnter();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            CheckMouseLeave();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            OnCardClicked?.Invoke(this);
            if (MetricInfo != null)
            {
                MetricHoverPopup.Instance.ShowDetails(this, MetricInfo);
            }
        }

        private void TriggerMouseEnter()
        {
            if (!EnableHoverEffect) return;
            if (!_isHovered)
            {
                _isHovered = true;
                Invalidate();
                if (MetricInfo != null)
                {
                    MetricHoverPopup.Instance.ShowDetails(this, MetricInfo);
                }
            }
        }

        private void CheckMouseLeave()
        {
            if (!EnableHoverEffect) return;
            Point clientMouse = PointToClient(Cursor.Position);
            if (!ClientRectangle.Contains(clientMouse))
            {
                _isHovered = false;
                Invalidate();
                MetricHoverPopup.Instance.HideDetails();
            }
        }

        private void HandleThemeChanged(AppTheme theme)
        {
            if (UseThemeBackground)
            {
                BackColor = ThemeManager.CardBg;
                if (CustomBackgroundColor != Color.Empty)
                {
                    CustomBackgroundColor = ThemeManager.CardBg;
                }
                if (CustomBorderColor != Color.Empty && CustomBorderColor != Color.Transparent)
                {
                    CustomBorderColor = ThemeManager.CardBorder;
                }
            }
            Invalidate(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.OnThemeChanged -= HandleThemeChanged;
            }
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Color parentBg = Parent?.BackColor != Color.Transparent && Parent?.BackColor != null && !Parent.BackColor.IsEmpty
                ? Parent.BackColor
                : ThemeManager.Background;
            using var brush = new SolidBrush(parentBg);
            pevent.Graphics.FillRectangle(brush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color bgColor = CustomBackgroundColor != Color.Empty 
                ? CustomBackgroundColor 
                : (UseThemeBackground ? ThemeManager.CardBg : BackColor);

            if (_isHovered && EnableHoverEffect)
            {
                if (HoverBackgroundColor != Color.Empty)
                    bgColor = HoverBackgroundColor;
                else
                    bgColor = ThemeManager.CurrentTheme == AppTheme.Dark
                        ? Color.FromArgb(38, 40, 56)
                        : Color.FromArgb(248, 250, 255);
            }

            Color borderColor = CustomBorderColor != Color.Empty 
                ? CustomBorderColor 
                : ThemeManager.CardBorder;

            if (_isHovered && EnableHoverEffect)
            {
                borderColor = HoverBorderColor != Color.Empty
                    ? HoverBorderColor
                    : ThemeManager.AccentBlue;
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var path = CreateRoundedRectanglePath(rect, BorderRadius);

            // Fill background
            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw border with lift glow
            int currentBorderWidth = (_isHovered && EnableHoverEffect) ? Math.Max(2, BorderWidth) : BorderWidth;
            if (currentBorderWidth > 0 && borderColor != Color.Transparent && !borderColor.IsEmpty)
            {
                using var pen = new Pen(borderColor, currentBorderWidth);
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
