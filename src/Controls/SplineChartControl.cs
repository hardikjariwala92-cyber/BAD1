using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class SplineChartControl : Control
    {
        public float[] NormalizedPoints { get; set; } = Array.Empty<float>();
        public Color LineColor { get; set; } = Color.FromArgb(190, 80, 240); // Vibrant purple/magenta
        public int MaxValue { get; set; } = 400;

        public SplineChartControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (NormalizedPoints == null || NormalizedPoints.Length < 2) return;

            int leftPadding = 45;
            int bottomPadding = 20;
            int topPadding = 15;
            int rightPadding = 15;

            int chartWidth = Width - leftPadding - rightPadding;
            int chartHeight = Height - topPadding - bottomPadding;

            if (chartWidth <= 0 || chartHeight <= 0) return;

            // Draw horizontal grid lines & Y labels
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

            // Calculate points for the spline
            int count = NormalizedPoints.Length;
            var points = new PointF[count];
            for (int i = 0; i < count; i++)
            {
                float x = leftPadding + (float)i / (count - 1) * chartWidth;
                float y = topPadding + (chartHeight - (NormalizedPoints[i] * chartHeight));
                points[i] = new PointF(x, y);
            }

            // Fill area under spline curve with gradient
            using (var areaPath = new GraphicsPath())
            {
                areaPath.AddCurve(points, 0.4f);
                areaPath.AddLine(points[^1].X, topPadding + chartHeight, points[0].X, topPadding + chartHeight);
                areaPath.CloseFigure();

                Color topGrad = Color.FromArgb(40, LineColor);
                Color botGrad = Color.FromArgb(0, LineColor);
                using var gradBrush = new LinearGradientBrush(
                    new PointF(0, topPadding),
                    new PointF(0, topPadding + chartHeight),
                    topGrad,
                    botGrad
                );
                g.FillPath(gradBrush, areaPath);
            }

            // Draw smooth spline line
            using (var linePen = new Pen(LineColor, 2.8f))
            {
                linePen.LineJoin = LineJoin.Round;
                g.DrawCurve(linePen, points, 0.4f);
            }

            // Draw glowing end dot
            var lastPt = points[^1];
            using (var dotBrush = new SolidBrush(LineColor))
            {
                g.FillEllipse(dotBrush, lastPt.X - 4, lastPt.Y - 4, 8, 8);
            }
            using (var ringPen = new Pen(Color.White, 1.5f))
            {
                g.DrawEllipse(ringPen, lastPt.X - 4, lastPt.Y - 4, 8, 8);
            }
        }
    }
}
