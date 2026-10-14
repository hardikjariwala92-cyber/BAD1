using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class DonutSlice
    {
        public string Label { get; set; } = string.Empty;
        public float Percentage { get; set; }
        public Color SliceColor { get; set; }

        public DonutSlice(string label, float percentage, Color color)
        {
            Label = label;
            Percentage = percentage;
            SliceColor = color;
        }
    }

    public class DonutChartControl : Control
    {
        public List<DonutSlice> Slices { get; set; } = new();
        public int HoleSizePercentage { get; set; } = 60; // Inner circle %
        public bool ShowLegend { get; set; } = true;

        public DonutChartControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(160, 90);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Slices == null || Slices.Count == 0) return;

            int chartDiameter = Math.Min(Height - 4, (ShowLegend ? Width / 2 : Width) - 4);
            if (chartDiameter <= 10) chartDiameter = 20;

            int chartX = 2;
            int chartY = (Height - chartDiameter) / 2;

            var outerRect = new Rectangle(chartX, chartY, chartDiameter, chartDiameter);
            int innerHole = (chartDiameter * HoleSizePercentage) / 100;
            var innerRect = new Rectangle(
                chartX + (chartDiameter - innerHole) / 2,
                chartY + (chartDiameter - innerHole) / 2,
                innerHole,
                innerHole
            );

            float startAngle = -90f;
            foreach (var slice in Slices)
            {
                float sweepAngle = (slice.Percentage / 100f) * 360f;
                using var path = new GraphicsPath();
                path.AddArc(outerRect, startAngle, sweepAngle);
                path.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
                path.CloseFigure();

                using (var brush = new SolidBrush(slice.SliceColor))
                {
                    g.FillPath(brush, path);
                }

                startAngle += sweepAngle;
            }

            // Draw Legend if enabled
            if (ShowLegend)
            {
                int legendX = chartX + chartDiameter + 10;
                int legendY = chartY;
                using var font = new Font("Segoe UI", 7.5f, FontStyle.Regular);

                foreach (var slice in Slices)
                {
                    if (legendY + 14 > Height) break;

                    // Bullet dot
                    using (var bBrush = new SolidBrush(slice.SliceColor))
                    {
                        g.FillEllipse(bBrush, legendX, legendY + 2, 7, 7);
                    }

                    // Text (e.g. "63% New")
                    string txt = $"{(int)slice.Percentage}% {slice.Label}";
                    using (var tBrush = new SolidBrush(ThemeManager.TextSecondary))
                    {
                        g.DrawString(txt, font, tBrush, legendX + 11, legendY);
                    }

                    legendY += 15;
                }
            }
        }
    }
}
