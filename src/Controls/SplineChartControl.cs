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
        public string Unit { get; set; } = "k";
        public string[] MonthLabels { get; set; } = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

        private int _hoveredIndex = -1;
        private PointF[] _cachedPoints = Array.Empty<PointF>();

        public event Action<int, string, float>? OnPointClicked;

        public SplineChartControl()
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

        public void SetData(float[] points, int maxValue = 400)
        {
            NormalizedPoints = points;
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

            int newHoveredIndex = -1;
            if (_cachedPoints.Length > 0)
            {
                float minDistance = float.MaxValue;
                for (int i = 0; i < _cachedPoints.Length; i++)
                {
                    float dx = Math.Abs(e.X - _cachedPoints[i].X);
                    if (dx < minDistance && dx < 30) // Within 30px horizontally
                    {
                        minDistance = dx;
                        newHoveredIndex = i;
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
            if (_hoveredIndex >= 0 && _hoveredIndex < NormalizedPoints.Length)
            {
                string label = _hoveredIndex < MonthLabels.Length ? MonthLabels[_hoveredIndex] : $"M{_hoveredIndex + 1}";
                OnPointClicked?.Invoke(_hoveredIndex, label, NormalizedPoints[_hoveredIndex]);
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
                string labelStr = labelVal == 0 ? "0" : $"{labelVal}{Unit}";
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
            _cachedPoints = points;

            // Fill area under spline curve with gradient
            using (var areaPath = new GraphicsPath())
            {
                areaPath.AddCurve(points, 0.4f);
                areaPath.AddLine(points[^1].X, topPadding + chartHeight, points[0].X, topPadding + chartHeight);
                areaPath.CloseFigure();

                Color topGrad = Color.FromArgb(45, LineColor);
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

            // Interactive Hover Display
            if (_hoveredIndex >= 0 && _hoveredIndex < points.Length)
            {
                var hp = points[_hoveredIndex];
                float normVal = NormalizedPoints[_hoveredIndex];
                int actualVal = (int)(normVal * MaxValue);
                string mLabel = _hoveredIndex < MonthLabels.Length ? MonthLabels[_hoveredIndex] : $"M{_hoveredIndex + 1}";

                // 1. Vertical guideline
                using var guidePen = new Pen(Color.FromArgb(90, LineColor), 1f) { DashStyle = DashStyle.Dash };
                g.DrawLine(guidePen, hp.X, topPadding, hp.X, topPadding + chartHeight);

                // 2. Halo circle
                using var haloBrush = new SolidBrush(Color.FromArgb(60, LineColor));
                g.FillEllipse(haloBrush, hp.X - 10, hp.Y - 10, 20, 20);

                // 3. Inner dot
                using var pointBrush = new SolidBrush(LineColor);
                g.FillEllipse(pointBrush, hp.X - 5, hp.Y - 5, 10, 10);
                using var whiteRing = new Pen(Color.White, 2f);
                g.DrawEllipse(whiteRing, hp.X - 5, hp.Y - 5, 10, 10);

                // 4. Floating Tooltip
                DrawSplineTooltip(g, hp, mLabel, actualVal, normVal);
            }
        }

        private void DrawSplineTooltip(Graphics g, PointF pt, string month, int value, float norm)
        {
            int tipWidth = 145;
            int tipHeight = 56;

            float tipX = pt.X - (tipWidth / 2f);
            float tipY = pt.Y - tipHeight - 12;

            if (tipX < 45) tipX = 45;
            if (tipX + tipWidth > Width - 10) tipX = Width - tipWidth - 10;
            if (tipY < 4) tipY = pt.Y + 14;

            var tipRect = new Rectangle((int)tipX, (int)tipY, tipWidth, tipHeight);
            using var tipPath = CreateRoundedRectanglePath(tipRect, 8);

            using (var tipBg = new SolidBrush(Color.FromArgb(240, 22, 23, 34)))
            {
                g.FillPath(tipBg, tipPath);
            }
            using (var tipBorder = new Pen(LineColor, 1.2f))
            {
                g.DrawPath(tipBorder, tipPath);
            }

            using var fontTitle = new Font("Segoe UI", 8f, FontStyle.Bold);
            using var fontVal = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var fontSub = new Font("Segoe UI", 7.5f, FontStyle.Regular);

            using (var titleBrush = new SolidBrush(LineColor))
            {
                g.DrawString($"{month} Activity", fontTitle, titleBrush, tipX + 10, tipY + 6);
            }

            using (var valBrush = new SolidBrush(Color.White))
            {
                g.DrawString($"{value * 1000:N0} Users", fontVal, valBrush, tipX + 10, tipY + 21);
            }

            using (var subBrush = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.DrawString($"{(int)(norm * 100)}% of annual peak", fontSub, subBrush, tipX + 10, tipY + 38);
            }
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
