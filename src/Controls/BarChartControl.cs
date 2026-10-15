using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class BarChartControl : Control
    {
        public (string Label, int Value)[] DataPoints { get; set; } = Array.Empty<(string, int)>();
        public int MaxValue { get; set; } = 400;
        public string CurrencySymbol { get; set; } = "$";
        public string Unit { get; set; } = "k";

        private int _hoveredIndex = -1;
        private Point _mousePoint = Point.Empty;

        public event Action<int, string, int>? OnBarClicked;

        public BarChartControl()
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

        public void SetData((string Label, int Value)[] points, int maxValue = 400)
        {
            DataPoints = points;
            MaxValue = maxValue;
            _hoveredIndex = -1;
            Invalidate();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            BackColor = ThemeManager.CardBg;
            Invalidate();
        }

        private void HandleThemeChanged(AppTheme theme)
        {
            BackColor = ThemeManager.CardBg;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.OnThemeChanged -= HandleThemeChanged;
            }
            base.Dispose(disposing);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mousePoint = e.Location;

            int newHoveredIndex = -1;
            if (DataPoints != null && DataPoints.Length > 0)
            {
                int leftPadding = 45;
                int rightPadding = 15;
                int chartWidth = Width - leftPadding - rightPadding;

                if (chartWidth > 0 && e.X >= leftPadding && e.X <= Width - rightPadding)
                {
                    float slotWidth = (float)chartWidth / DataPoints.Length;
                    int idx = (int)((e.X - leftPadding) / slotWidth);
                    if (idx >= 0 && idx < DataPoints.Length)
                    {
                        newHoveredIndex = idx;
                    }
                }
            }

            if (newHoveredIndex != _hoveredIndex)
            {
                _hoveredIndex = newHoveredIndex;
                Cursor = _hoveredIndex >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredIndex != -1)
            {
                _hoveredIndex = -1;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_hoveredIndex >= 0 && _hoveredIndex < DataPoints.Length)
            {
                var pt = DataPoints[_hoveredIndex];
                OnBarClicked?.Invoke(_hoveredIndex, pt.Label, pt.Value);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            using var bgBrush = new SolidBrush(ThemeManager.CardBg);
            pevent.Graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fill background with theme card background color
            Color bg = ThemeManager.CardBg;
            using (var bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            if (DataPoints == null || DataPoints.Length == 0) return;

            int leftPadding = 45;
            int bottomPadding = 25;
            int topPadding = 15;
            int rightPadding = 15;

            int chartWidth = Width - leftPadding - rightPadding;
            int chartHeight = Height - topPadding - bottomPadding;

            if (chartWidth <= 0 || chartHeight <= 0) return;

            // Draw horizontal grid lines & Y-axis labels (0, 100k, 200k, 300k, 400k)
            using var gridPen = new Pen(ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(35, 38, 52) : Color.FromArgb(235, 238, 245), 1f);
            using var font = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            using var textBrush = new SolidBrush(ThemeManager.TextSecondary);

            int gridSteps = 4;
            for (int i = 0; i <= gridSteps; i++)
            {
                int yVal = topPadding + (int)((float)i / gridSteps * chartHeight);
                g.DrawLine(gridPen, leftPadding, yVal, Width - rightPadding, yVal);

                int labelVal = MaxValue - (int)((float)i / gridSteps * MaxValue);
                string labelStr = labelVal == 0 ? "0" : $"{labelVal}{Unit}";
                g.DrawString(labelStr, font, textBrush, 5, yVal - 7);
            }

            // Draw Bars
            int count = DataPoints.Length;
            float slotWidth = (float)chartWidth / count;
            float barWidth = Math.Max(7, slotWidth * 0.35f);

            RectangleF hoveredBarRect = RectangleF.Empty;
            (string Label, int Value) hoveredPoint = default;
            string prevGrowthStr = string.Empty;

            for (int i = 0; i < count; i++)
            {
                var point = DataPoints[i];
                float barHeight = ((float)point.Value / MaxValue) * chartHeight;
                if (barHeight > chartHeight) barHeight = chartHeight;

                float barX = leftPadding + (i * slotWidth) + (slotWidth - barWidth) / 2f;
                float barY = topPadding + (chartHeight - barHeight);

                var barRect = new RectangleF(barX, barY, barWidth, barHeight);
                bool isHovered = (i == _hoveredIndex);

                // Draw Rounded Bar
                using var barPath = CreatePillPath(barRect, barWidth / 2f);

                Color topColor = isHovered ? Color.FromArgb(145, 185, 255) : Color.FromArgb(100, 140, 255);
                Color bottomColor = isHovered ? Color.FromArgb(70, 110, 255) : Color.FromArgb(50, 80, 220);

                using (var brush = new LinearGradientBrush(new PointF(barX, barY), new PointF(barX, barY + barHeight + 1), topColor, bottomColor))
                {
                    g.FillPath(brush, barPath);
                }

                // If hovered, draw glowing border
                if (isHovered)
                {
                    hoveredBarRect = barRect;
                    hoveredPoint = point;

                    // Calculate growth compared to previous month
                    if (i > 0)
                    {
                        int prevVal = DataPoints[i - 1].Value;
                        if (prevVal > 0)
                        {
                            float diff = ((float)(point.Value - prevVal) / prevVal) * 100f;
                            prevGrowthStr = diff >= 0 ? $"+{diff:0.0}% vs {DataPoints[i - 1].Label}" : $"{diff:0.0}% vs {DataPoints[i - 1].Label}";
                        }
                    }
                    else
                    {
                        prevGrowthStr = "Base Period";
                    }

                    using var glowPen = new Pen(Color.FromArgb(200, 255, 255, 255), 1.5f);
                    g.DrawPath(glowPen, barPath);
                }

                // Month text below bar
                var textRect = new RectangleF(leftPadding + (i * slotWidth), Height - bottomPadding + 4, slotWidth, bottomPadding);
                var sf = new StringFormat { Alignment = StringAlignment.Center };

                if (isHovered)
                {
                    using var activeMonthFont = new Font("Segoe UI", 8f, FontStyle.Bold);
                    using var activeTextBrush = new SolidBrush(ThemeManager.TextPrimary);
                    g.DrawString(point.Label, activeMonthFont, activeTextBrush, textRect, sf);
                }
                else
                {
                    g.DrawString(point.Label, font, textBrush, textRect, sf);
                }
            }

            // Draw Floating Tooltip above hovered bar
            if (_hoveredIndex >= 0 && !hoveredBarRect.IsEmpty)
            {
                DrawBarTooltip(g, hoveredBarRect, hoveredPoint, prevGrowthStr);
            }
        }

        private void DrawBarTooltip(Graphics g, RectangleF barRect, (string Label, int Value) point, string growthStr)
        {
            string monthFull = GetMonthFullName(point.Label);
            string valText = $"{CurrencySymbol}{point.Value * 1000:N0}";

            using var fontTitle = new Font("Segoe UI", 8f, FontStyle.Bold);
            using var fontVal = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var fontGrowth = new Font("Segoe UI", 7.5f, FontStyle.Regular);

            int tipWidth = 135;
            int tipHeight = string.IsNullOrEmpty(growthStr) ? 46 : 60;

            float tipX = barRect.X + (barRect.Width / 2f) - (tipWidth / 2f);
            float tipY = barRect.Y - tipHeight - 8;

            // Clamping inside control
            if (tipX < 45) tipX = 45;
            if (tipX + tipWidth > Width - 10) tipX = Width - tipWidth - 10;
            if (tipY < 4) tipY = barRect.Y + 12; // Flip below top if clipped

            var tipRect = new Rectangle((int)tipX, (int)tipY, tipWidth, tipHeight);
            using var tipPath = CreateRoundedRectanglePath(tipRect, 8);

            // Background
            using (var tipBg = new SolidBrush(Color.FromArgb(240, 20, 23, 34)))
            {
                g.FillPath(tipBg, tipPath);
            }

            // Border
            using (var tipBorder = new Pen(ThemeManager.AccentBlue, 1.2f))
            {
                g.DrawPath(tipBorder, tipPath);
            }

            // Text
            using (var titleBrush = new SolidBrush(ThemeManager.AccentCyan))
            {
                g.DrawString(monthFull, fontTitle, titleBrush, tipX + 10, tipY + 6);
            }

            using (var valBrush = new SolidBrush(Color.White))
            {
                g.DrawString(valText, fontVal, valBrush, tipX + 10, tipY + 22);
            }

            if (!string.IsNullOrEmpty(growthStr))
            {
                bool isPositive = growthStr.StartsWith("+");
                Color growthColor = isPositive ? ThemeManager.AccentGreen : (growthStr.StartsWith("-") ? ThemeManager.AccentRed : ThemeManager.TextSecondary);
                using var growthBrush = new SolidBrush(growthColor);
                g.DrawString(growthStr, fontGrowth, growthBrush, tipX + 10, tipY + 41);
            }
        }

        private static string GetMonthFullName(string code) => code.ToUpperInvariant() switch
        {
            "JAN" => "January",
            "FEB" => "February",
            "MAR" => "March",
            "APR" => "April",
            "MAY" => "May",
            "JUN" => "June",
            "JUL" => "July",
            "AUG" => "August",
            "SEP" => "September",
            "OCT" => "October",
            "NOV" => "November",
            "DEC" => "December",
            _ => code
        };

        private static GraphicsPath CreatePillPath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            if (rect.Height <= radius * 2)
            {
                path.AddEllipse(rect);
                return path;
            }

            float d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 180);
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + radius);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
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
