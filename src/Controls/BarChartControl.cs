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

        public BarChartControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

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
                string labelStr = labelVal == 0 ? "0" : $"{labelVal}k";
                g.DrawString(labelStr, font, textBrush, 5, yVal - 7);
            }

            // Draw Bars
            int count = DataPoints.Length;
            float slotWidth = (float)chartWidth / count;
            float barWidth = Math.Max(6, slotWidth * 0.35f);

            for (int i = 0; i < count; i++)
            {
                var point = DataPoints[i];
                float barHeight = ((float)point.Value / MaxValue) * chartHeight;
                if (barHeight > chartHeight) barHeight = chartHeight;

                float barX = leftPadding + (i * slotWidth) + (slotWidth - barWidth) / 2f;
                float barY = topPadding + (chartHeight - barHeight);

                var barRect = new RectangleF(barX, barY, barWidth, barHeight);

                // Draw Rounded Bar
                using var barPath = CreatePillPath(barRect, barWidth / 2f);
                Color topColor = Color.FromArgb(100, 140, 255);
                Color bottomColor = Color.FromArgb(50, 80, 220);

                using (var brush = new LinearGradientBrush(new PointF(barX, barY), new PointF(barX, barY + barHeight + 1), topColor, bottomColor))
                {
                    g.FillPath(brush, barPath);
                }

                // Month text below bar
                var textRect = new RectangleF(leftPadding + (i * slotWidth), Height - bottomPadding + 4, slotWidth, bottomPadding);
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(point.Label, font, textBrush, textRect, sf);
            }
        }

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
    }
}
