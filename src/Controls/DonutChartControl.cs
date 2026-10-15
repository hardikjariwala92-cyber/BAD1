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
        public string TotalBaseText { get; set; } = "";

        private int _hoveredSliceIndex = -1;
        private Point _mousePoint = Point.Empty;

        public event Action<int, DonutSlice>? OnSliceClicked;

        public DonutChartControl()
        {
            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw | 
                ControlStyles.SupportsTransparentBackColor, 
                true);
            UpdateStyles();

            Size = new Size(160, 90);
            BackColor = ThemeManager.CardBg;

            ThemeManager.OnThemeChanged += HandleThemeChanged;
        }

        public void SetSlices(List<DonutSlice> slices)
        {
            Slices = slices;
            _hoveredSliceIndex = -1;
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
            if (Slices != null && Slices.Count > 0)
            {
                int chartDiameter = Math.Min(Height - 4, (ShowLegend ? Width / 2 : Width) - 4);
                if (chartDiameter <= 10) chartDiameter = 20;

                int chartX = 2;
                int chartY = (Height - chartDiameter) / 2;
                float centerX = chartX + chartDiameter / 2f;
                float centerY = chartY + chartDiameter / 2f;

                float dx = e.X - centerX;
                float dy = e.Y - centerY;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                float outerRadius = chartDiameter / 2f;
                float innerRadius = (outerRadius * HoleSizePercentage) / 100f;

                if (dist >= innerRadius && dist <= outerRadius + 6)
                {
                    // Calculate angle: 0 at top (-90 degrees)
                    float angle = (float)(Math.Atan2(dy, dx) * 180f / Math.PI) + 90f;
                    if (angle < 0) angle += 360f;

                    float startAngle = 0f;
                    for (int i = 0; i < Slices.Count; i++)
                    {
                        float sweep = (Slices[i].Percentage / 100f) * 360f;
                        if (angle >= startAngle && angle < startAngle + sweep)
                        {
                            newHoveredIndex = i;
                            break;
                        }
                        startAngle += sweep;
                    }
                }
            }

            if (newHoveredIndex != _hoveredSliceIndex)
            {
                _hoveredSliceIndex = newHoveredIndex;
                Cursor = _hoveredSliceIndex >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredSliceIndex != -1)
            {
                _hoveredSliceIndex = -1;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_hoveredSliceIndex >= 0 && _hoveredSliceIndex < Slices.Count)
            {
                OnSliceClicked?.Invoke(_hoveredSliceIndex, Slices[_hoveredSliceIndex]);
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
            for (int i = 0; i < Slices.Count; i++)
            {
                var slice = Slices[i];
                float sweepAngle = (slice.Percentage / 100f) * 360f;
                bool isHovered = (i == _hoveredSliceIndex);

                using var path = new GraphicsPath();

                if (isHovered)
                {
                    // Slightly expanded arc
                    var expOuterRect = new Rectangle(chartX - 2, chartY - 2, chartDiameter + 4, chartDiameter + 4);
                    path.AddArc(expOuterRect, startAngle, sweepAngle);
                    path.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
                }
                else
                {
                    path.AddArc(outerRect, startAngle, sweepAngle);
                    path.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
                }
                path.CloseFigure();

                Color sliceColor = isHovered ? BrightenColor(slice.SliceColor, 30) : slice.SliceColor;
                using (var brush = new SolidBrush(sliceColor))
                {
                    g.FillPath(brush, path);
                }

                if (isHovered)
                {
                    using var glowPen = new Pen(Color.White, 1.5f);
                    g.DrawPath(glowPen, path);
                }

                startAngle += sweepAngle;
            }

            // Draw Legend if enabled
            if (ShowLegend)
            {
                int legendX = chartX + chartDiameter + 10;
                int legendY = chartY;
                using var font = new Font("Segoe UI", 7.5f, FontStyle.Regular);
                using var fontBold = new Font("Segoe UI", 7.5f, FontStyle.Bold);

                for (int i = 0; i < Slices.Count; i++)
                {
                    var slice = Slices[i];
                    if (legendY + 14 > Height) break;

                    bool isHovered = (i == _hoveredSliceIndex);

                    // Bullet dot
                    using (var bBrush = new SolidBrush(slice.SliceColor))
                    {
                        g.FillEllipse(bBrush, legendX, legendY + 2, isHovered ? 8 : 7, isHovered ? 8 : 7);
                    }

                    // Text (e.g. "63% New")
                    string txt = $"{(int)slice.Percentage}% {slice.Label}";
                    Color txtColor = isHovered ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
                    using (var tBrush = new SolidBrush(txtColor))
                    {
                        g.DrawString(txt, isHovered ? fontBold : font, tBrush, legendX + 11, legendY);
                    }

                    legendY += 15;
                }
            }
        }

        private static Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Clamp(color.R + amount, 0, 255),
                Math.Clamp(color.G + amount, 0, 255),
                Math.Clamp(color.B + amount, 0, 255)
            );
        }
    }
}
