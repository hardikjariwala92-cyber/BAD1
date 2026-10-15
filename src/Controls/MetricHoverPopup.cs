using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class MetricHoverPopup : Form
    {
        private static MetricHoverPopup? _instance;
        public static MetricHoverPopup Instance => _instance ??= new MetricHoverPopup();

        private MetricDetailInfo? _info;
        private readonly System.Windows.Forms.Timer _dismissTimer;

        public MetricHoverPopup()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            Size = new Size(310, 220);

            _dismissTimer = new System.Windows.Forms.Timer { Interval = 250 };
            _dismissTimer.Tick += (s, e) =>
            {
                if (!Visible) return;
                Point cursor = Cursor.Position;
                Rectangle bounds = Bounds;
                bounds.Inflate(40, 40);
                if (!bounds.Contains(cursor))
                {
                    _dismissTimer.Stop();
                    Hide();
                }
            };

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
            UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE (does not steal focus)
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW (does not show in Alt-Tab)
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW (native OS shadow)
                return cp;
            }
        }

        public void ShowDetails(Control anchor, MetricDetailInfo info)
        {
            _info = info;

            // Calculate dynamic height based on stats count
            int baseHeight = 175;
            int statsHeight = (info.Stats != null && info.Stats.Count > 0) ? info.Stats.Count * 22 : 0;
            Size = new Size(320, baseHeight + statsHeight);

            // Determine position relative to anchor control
            Point screenPt = anchor.PointToScreen(new Point(0, 0));
            int targetX = screenPt.X + (anchor.Width - Width) / 2;
            int targetY = screenPt.Y + anchor.Height + 8; // Below card

            // Adjust if goes off-screen
            var screen = Screen.FromControl(anchor).WorkingArea;
            if (targetY + Height > screen.Bottom)
            {
                targetY = screenPt.Y - Height - 8; // Flip above card if bottom space is insufficient
            }
            if (targetX + Width > screen.Right)
            {
                targetX = screen.Right - Width - 12;
            }
            if (targetX < screen.Left)
            {
                targetX = screen.Left + 12;
            }

            Location = new Point(targetX, targetY);
            if (!Visible)
            {
                Show();
            }
            Invalidate();
            _dismissTimer.Start();
        }

        public void HideDetails()
        {
            _dismissTimer.Stop();
            Hide();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = CreateRoundedPath(rect, 12);

            // 1. Background fill
            Color bg = ThemeManager.CurrentTheme == AppTheme.Dark
                ? Color.FromArgb(24, 25, 36)
                : Color.FromArgb(255, 255, 255);
            using (var brush = new SolidBrush(bg))
            {
                g.FillPath(brush, path);
            }

            // 2. Border glow
            Color borderColor = ThemeManager.AccentBlue;
            using (var pen = new Pen(borderColor, 1.5f))
            {
                g.DrawPath(pen, path);
            }

            // 3. Top accent bar
            using (var barPath = new GraphicsPath())
            {
                barPath.AddArc(0, 0, 24, 24, 180, 90);
                barPath.AddArc(Width - 25, 0, 24, 24, 270, 90);
                barPath.AddLine(Width - 1, 4, 0, 4);
                barPath.CloseFigure();
                using var barBrush = new LinearGradientBrush(
                    new Point(0, 0), new Point(Width, 0),
                    ThemeManager.AccentBlue, ThemeManager.AccentCyan);
                g.FillPath(barBrush, barPath);
            }

            if (_info == null) return;

            // 4. Header: Icon + Category + Title
            int curY = 14;
            using var fontIcon = new Font("Segoe UI Emoji", 13f);
            using var fontCat = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            using var fontTitle = new Font("Segoe UI", 10.5f, FontStyle.Bold);

            // Icon
            if (!string.IsNullOrEmpty(_info.Icon))
            {
                g.DrawString(_info.Icon, fontIcon, Brushes.White, 16, curY - 2);
            }

            // Category tag
            using var catBrush = new SolidBrush(ThemeManager.AccentCyan);
            g.DrawString(_info.Category.ToUpperInvariant(), fontCat, catBrush, 46, curY);

            // Title
            curY += 16;
            using var titleBrush = new SolidBrush(ThemeManager.TextPrimary);
            g.DrawString(_info.Title, fontTitle, titleBrush, 46, curY);

            // Divider
            curY += 28;
            using var divPen = new Pen(ThemeManager.CardBorder, 1f);
            g.DrawLine(divPen, 16, curY, Width - 16, curY);
            curY += 10;

            // 5. Metric Values Row (Current vs Previous + Growth Badge)
            using var fontValLabel = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            using var fontCurVal = new Font("Segoe UI", 16f, FontStyle.Bold);
            using var fontPrevVal = new Font("Segoe UI", 10f, FontStyle.Regular);
            using var subBrush = new SolidBrush(ThemeManager.TextSecondary);

            // Current Value
            g.DrawString("CURRENT", fontValLabel, subBrush, 16, curY);
            g.DrawString(_info.CurrentValue, fontCurVal, titleBrush, 14, curY + 12);

            // Previous Value
            if (!string.IsNullOrEmpty(_info.PreviousValue))
            {
                int prevX = 170;
                g.DrawString("PREVIOUS", fontValLabel, subBrush, prevX, curY);
                g.DrawString(_info.PreviousValue, fontPrevVal, subBrush, prevX, curY + 14);
            }

            curY += 46;

            // Growth Badge Pill
            if (!string.IsNullOrEmpty(_info.GrowthText))
            {
                Color badgeBg = _info.IsPositiveGrowth
                    ? Color.FromArgb(30, 16, 185, 129)
                    : Color.FromArgb(30, 239, 68, 68);
                Color badgeText = _info.IsPositiveGrowth
                    ? ThemeManager.AccentGreen
                    : ThemeManager.AccentRed;

                using var badgeFont = new Font("Segoe UI", 8f, FontStyle.Bold);
                var textSize = g.MeasureString(_info.GrowthText, badgeFont);
                var badgeRect = new RectangleF(16, curY, textSize.Width + 14, 20);

                using (var bb = new SolidBrush(badgeBg))
                {
                    using var bp = CreateRoundedPath(Rectangle.Round(badgeRect), 6);
                    g.FillPath(bb, bp);
                }
                using (var bt = new SolidBrush(badgeText))
                {
                    g.DrawString(_info.GrowthText, badgeFont, bt, 23, curY + 2);
                }
                curY += 28;
            }

            // 6. Statistics breakdown rows
            if (_info.Stats != null && _info.Stats.Count > 0)
            {
                using var statFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                using var valFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);

                foreach (var (lbl, val) in _info.Stats)
                {
                    // Label
                    g.DrawString("•  " + lbl, statFont, subBrush, 18, curY);
                    // Value right-aligned
                    var valSize = g.MeasureString(val, valFont);
                    g.DrawString(val, valFont, titleBrush, Width - 18 - valSize.Width, curY);
                    curY += 22;
                }
            }

            // 7. Footer: Last Updated timestamp
            curY += 6;
            g.DrawLine(divPen, 16, curY, Width - 16, curY);
            curY += 8;

            using var fontFooter = new Font("Segoe UI", 7.5f, FontStyle.Italic);
            string timeStr = $"🕒 Updated: {_info.LastUpdated:MMM dd, yyyy HH:mm:ss}";
            g.DrawString(timeStr, fontFooter, subBrush, 16, curY);
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
