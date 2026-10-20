using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class ThemeToggle : Control
    {
        public bool IsDarkMode => ThemeManager.CurrentTheme == AppTheme.Dark;

        private readonly System.Windows.Forms.Timer _animTimer;
        private DateTime _animStartTime;
        private float _startProgress;
        private float _targetProgress;
        private float _currentProgress;
        private bool _isAnimating;
        private bool _isHovered;

        public bool IsAnimating => _isAnimating;
        public float AnimationProgress => _currentProgress;

        private const double AnimationDurationMs = 220.0;

        public ThemeToggle()
        {
            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.ResizeRedraw | 
                ControlStyles.SupportsTransparentBackColor, 
                true);
            UpdateStyles();

            Size = new Size(64, 30);
            Cursor = Cursors.Hand;

            // Initialize progress to match current theme (1 = Dark, 0 = Light)
            _currentProgress = IsDarkMode ? 1f : 0f;
            _targetProgress = _currentProgress;
            _startProgress = _currentProgress;

            _animTimer = new System.Windows.Forms.Timer
            {
                Interval = 15 // ~60 FPS smooth rendering
            };
            _animTimer.Tick += OnAnimationTick;

            BackColor = ThemeManager.Background;
            ThemeManager.OnThemeChanged += HandleThemeChanged;
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            double elapsed = (DateTime.UtcNow - _animStartTime).TotalMilliseconds;
            float t = (float)Math.Min(1.0, elapsed / AnimationDurationMs);
            float easedT = EaseInOutCubic(t);

            _currentProgress = _startProgress + ((_targetProgress - _startProgress) * easedT);
            Invalidate();

            if (t >= 1.0f)
            {
                _currentProgress = _targetProgress;
                _isAnimating = false;
                _animTimer.Stop();
                Invalidate();
            }
        }

        private static float EaseInOutCubic(float t)
        {
            return t < 0.5f 
                ? 4f * t * t * t 
                : 1f - ((float)Math.Pow((-2f * t) + 2f, 3) / 2f);
        }

        private static Color LerpColor(Color c1, Color c2, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            int a = (int)(c1.A + ((c2.A - c1.A) * t));
            int r = (int)(c1.R + ((c2.R - c1.R) * t));
            int g = (int)(c1.G + ((c2.G - c1.G) * t));
            int b = (int)(c1.B + ((c2.B - c1.B) * t));
            return Color.FromArgb(a, r, g, b);
        }

        private void HandleThemeChanged(AppTheme theme)
        {
            BackColor = Parent?.BackColor != Color.Transparent && Parent?.BackColor != null && !Parent.BackColor.IsEmpty
                ? Parent.BackColor
                : ThemeManager.Background;

            float expected = theme == AppTheme.Dark ? 1f : 0f;
            if (Math.Abs(_targetProgress - expected) > 0.01f)
            {
                _targetProgress = expected;
                _startProgress = _currentProgress;
                _animStartTime = DateTime.UtcNow;
                _isAnimating = true;
                _animTimer.Start();
            }
            Invalidate();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null && Parent.BackColor != Color.Transparent && !Parent.BackColor.IsEmpty)
            {
                BackColor = Parent.BackColor;
            }
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
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
                ThemeManager.OnThemeChanged -= HandleThemeChanged;
            }
            base.Dispose(disposing);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ToggleWithSmoothAnimation();
        }

        public void ToggleWithSmoothAnimation()
        {
            // Reverse target
            _targetProgress = _targetProgress > 0.5f ? 0f : 1f;
            _startProgress = _currentProgress;
            _animStartTime = DateTime.UtcNow;
            _isAnimating = true;

            // Trigger theme manager toggle
            ThemeManager.ToggleTheme();

            _animTimer.Start();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Color bg = Parent?.BackColor != Color.Transparent && Parent?.BackColor != null && !Parent.BackColor.IsEmpty
                ? Parent.BackColor
                : ThemeManager.Background;
            using var bgBrush = new SolidBrush(bg);
            pevent.Graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            int radius = rect.Height / 2;
            int diameter = radius * 2;

            using var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 90, 180);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 180);
            path.CloseFigure();

            // 1. Background Pill (Smoothly interpolated between Light and Dark track)
            Color lightTrack = Color.FromArgb(226, 232, 240);
            Color darkTrack = Color.FromArgb(30, 34, 52);
            Color pillBg = LerpColor(lightTrack, darkTrack, _currentProgress);

            using (var bgBrush = new SolidBrush(pillBg))
            {
                g.FillPath(bgBrush, path);
            }

            // 2. Track Border (Subtle border with hover glow)
            Color lightBorder = Color.FromArgb(203, 213, 225);
            Color darkBorder = Color.FromArgb(50, 58, 85);
            Color borderColor = LerpColor(lightBorder, darkBorder, _currentProgress);
            if (_isHovered)
            {
                borderColor = LerpColor(borderColor, ThemeManager.AccentBlue, 0.55f);
            }
            using (var borderPen = new Pen(borderColor, 1.2f))
            {
                g.DrawPath(borderPen, path);
            }

            // 3. Icons: Sun on Left (Light indicator), Moon on Right (Dark indicator)
            // Sun Icon (Left)
            float sunOpacity = Math.Max(0.2f, 1f - (_currentProgress * 0.75f));
            Color sunColor = Color.FromArgb((int)(255 * sunOpacity), 245, 158, 11);
            using (var sunBrush = new SolidBrush(sunColor))
            {
                int sunCenterX = rect.X + 13;
                int sunCenterY = rect.Y + (rect.Height / 2);
                g.FillEllipse(sunBrush, sunCenterX - 4, sunCenterY - 4, 8, 8);

                // Sun micro rays
                if (sunOpacity > 0.55f)
                {
                    using var rayPen = new Pen(sunColor, 1.2f);
                    g.DrawLine(rayPen, sunCenterX - 6, sunCenterY, sunCenterX - 7, sunCenterY);
                    g.DrawLine(rayPen, sunCenterX + 6, sunCenterY, sunCenterX + 7, sunCenterY);
                    g.DrawLine(rayPen, sunCenterX, sunCenterY - 6, sunCenterX, sunCenterY - 7);
                    g.DrawLine(rayPen, sunCenterX, sunCenterY + 6, sunCenterX, sunCenterY + 7);
                }
            }

            // Moon Icon (Right)
            float moonOpacity = Math.Max(0.25f, 0.25f + (_currentProgress * 0.75f));
            Color moonColor = Color.FromArgb((int)(255 * moonOpacity), 165, 180, 252);
            using (var moonBrush = new SolidBrush(moonColor))
            {
                int moonX = rect.Right - 18;
                int moonY = rect.Y + (rect.Height / 2) - 5;
                using var moonPath = new GraphicsPath();
                moonPath.AddEllipse(moonX, moonY, 10, 10);
                using var cutPath = new GraphicsPath();
                cutPath.AddEllipse(moonX - 3, moonY - 2, 9, 9);
                using var moonRegion = new Region(moonPath);
                moonRegion.Exclude(cutPath);
                g.FillRegion(moonBrush, moonRegion);
            }

            // 4. Smoothly Animated Sliding Thumb Knob
            int thumbSize = rect.Height - 6;
            int thumbY = rect.Y + 3;
            float minThumbX = rect.X + 3;
            float maxThumbX = rect.Right - thumbSize - 3;
            float thumbX = minThumbX + (_currentProgress * (maxThumbX - minThumbX));

            // Soft drop shadow under knob
            var shadowRect = new RectangleF(thumbX, thumbY + 1.5f, thumbSize, thumbSize);
            using (var shadowBrush = new SolidBrush(Color.FromArgb(45, 0, 0, 0)))
            {
                g.FillEllipse(shadowBrush, shadowRect);
            }

            // Knob Base (Smooth blend from Pure White to Vibrant Accent Blue)
            Color lightThumb = Color.White;
            Color darkThumb = ThemeManager.AccentBlue;
            Color thumbColor = LerpColor(lightThumb, darkThumb, _currentProgress);

            var knobRect = new RectangleF(thumbX, thumbY, thumbSize, thumbSize);
            using (var thumbBrush = new SolidBrush(thumbColor))
            {
                g.FillEllipse(thumbBrush, knobRect);
            }

            // Knob Highlight Ring
            Color ringColor = _currentProgress > 0.5f 
                ? Color.FromArgb(70, 255, 255, 255) 
                : Color.FromArgb(30, 0, 0, 0);
            using (var knobPen = new Pen(ringColor, 1f))
            {
                g.DrawEllipse(knobPen, knobRect);
            }

            // Tactile Center Micro-Core (Amber in light mode, Glowing white/cyan in dark mode)
            Color coreColor = _currentProgress > 0.5f 
                ? Color.FromArgb(220, 255, 255, 255) 
                : Color.FromArgb(245, 158, 11);
            using (var coreBrush = new SolidBrush(coreColor))
            {
                float coreSize = 4f;
                float coreX = thumbX + ((thumbSize - coreSize) / 2f);
                float coreY = thumbY + ((thumbSize - coreSize) / 2f);
                g.FillEllipse(coreBrush, coreX, coreY, coreSize, coreSize);
            }
        }
    }
}
