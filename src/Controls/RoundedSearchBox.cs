using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class RoundedSearchBox : Panel
    {
        private readonly TextBox _textBox;
        private readonly Button _btnClear;
        private readonly Button _btnSearchIcon;
        private bool _isFocused = false;
        private bool _isHovered = false;

        public int BorderRadius { get; set; } = 15;
        public Color BoxBackgroundColor { get; set; } = Color.FromArgb(24, 26, 38);
        public Color BorderColor { get; set; } = Color.FromArgb(46, 53, 79);
        public Color FocusBorderColor { get; set; } = Color.FromArgb(99, 102, 241);
        public Color HoverBorderColor { get; set; } = Color.FromArgb(70, 80, 115);

        public TextBox InnerTextBox => _textBox;

        public string PlaceholderText
        {
            get => _textBox.PlaceholderText;
            set => _textBox.PlaceholderText = value;
        }

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string Text
        {
            get => _textBox?.Text ?? string.Empty;
            set
            {
                if (_textBox != null)
                {
                    _textBox.Text = value ?? string.Empty;
                }
            }
        }

        public event EventHandler? SearchTriggered;

        public RoundedSearchBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            UpdateStyles();

            Size = new Size(205, 30);
            BackColor = Color.Transparent;

            // 1. Inner Borderless TextBox (Initialized first so button handlers can safely reference it)
            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9f),
                Location = new Point(32, 6),
                Size = new Size(Width - 60, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                PlaceholderText = "Search customer, city, status..."
            };

            // 2. Clear Button (Round pill '✕' button)
            _btnClear = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Size = new Size(18, 18),
                Location = new Point(Width - 24, 6),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false,
                BackColor = Color.Transparent
            };
            _btnClear.FlatAppearance.BorderSize = 0;
            _btnClear.FlatAppearance.MouseDownBackColor = Color.Transparent;
            _btnClear.FlatAppearance.MouseOverBackColor = Color.Transparent;
            _btnClear.Click += (s, e) =>
            {
                _textBox.Text = string.Empty;
                _textBox.Focus();
            };

            // 3. Search Icon Button (interactive round lens button)
            _btnSearchIcon = new Button
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 8f),
                Size = new Size(22, 22),
                Location = new Point(6, 4),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            _btnSearchIcon.FlatAppearance.BorderSize = 0;
            _btnSearchIcon.FlatAppearance.MouseDownBackColor = Color.Transparent;
            _btnSearchIcon.FlatAppearance.MouseOverBackColor = Color.Transparent;
            _btnSearchIcon.Click += (s, e) =>
            {
                _textBox.Focus();
                SearchTriggered?.Invoke(this, EventArgs.Empty);
            };

            _textBox.TextChanged += (s, e) =>
            {
                UpdateClearButtonVisibility();
                OnTextChanged(e);
            };
            _textBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _textBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };

            Controls.Add(_btnSearchIcon);
            Controls.Add(_textBox);
            Controls.Add(_btnClear);

            ApplyTheme(ThemeManager.CurrentTheme);
            ThemeManager.OnThemeChanged += ApplyTheme;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.OnThemeChanged -= ApplyTheme;
            }
            base.Dispose(disposing);
        }

        private void UpdateClearButtonVisibility()
        {
            _btnClear.Visible = !string.IsNullOrEmpty(_textBox.Text);
            AdjustTextBoxWidth();
        }

        private void AdjustTextBoxWidth()
        {
            if (_textBox == null) return;
            int rightPad = _btnClear != null && _btnClear.Visible ? 28 : 12;
            _textBox.Location = new Point(30, (Height - _textBox.PreferredHeight) / 2);
            _textBox.Width = Math.Max(20, Width - 32 - rightPad);
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
            Point pt = PointToClient(Cursor.Position);
            if (!ClientRectangle.Contains(pt))
            {
                _isHovered = false;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            AdjustTextBoxWidth();
            if (_btnClear != null)
            {
                _btnClear.Location = new Point(Width - 24, (Height - 18) / 2);
            }
            if (_btnSearchIcon != null)
            {
                _btnSearchIcon.Location = new Point(6, (Height - 22) / 2);
            }
            Invalidate();
        }

        public void ApplyTheme(AppTheme theme)
        {
            bool isDark = theme == AppTheme.Dark;
            BoxBackgroundColor = isDark ? Color.FromArgb(22, 25, 37) : Color.FromArgb(248, 250, 252);
            BorderColor = isDark ? Color.FromArgb(46, 53, 79) : Color.FromArgb(226, 232, 240);
            FocusBorderColor = isDark ? Color.FromArgb(99, 102, 241) : Color.FromArgb(79, 70, 229);
            HoverBorderColor = isDark ? Color.FromArgb(70, 80, 115) : Color.FromArgb(203, 213, 225);

            _textBox.BackColor = BoxBackgroundColor;
            _textBox.ForeColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);

            _btnSearchIcon.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
            _btnClear.ForeColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Color parentBg = Parent != null && Parent.BackColor != Color.Transparent
                ? Parent.BackColor
                : ThemeManager.CardBg;
            using var bgBrush = new SolidBrush(parentBg);
            pevent.Graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            int radius = BorderRadius;
            using var path = CreateRoundedRectanglePath(rect, radius);

            // Fill Pill Background
            using (var fillBrush = new SolidBrush(BoxBackgroundColor))
            {
                g.FillPath(fillBrush, path);
            }

            // Draw CSS-like focus glow ring or hover border
            if (_isFocused)
            {
                // Outer glow halo (simulating CSS box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.25))
                using var haloPen = new Pen(Color.FromArgb(60, FocusBorderColor), 3.5f);
                g.DrawPath(haloPen, path);

                // Sharp inner accent border
                using var borderPen = new Pen(FocusBorderColor, 1.5f);
                g.DrawPath(borderPen, path);
            }
            else if (_isHovered)
            {
                using var hoverPen = new Pen(HoverBorderColor, 1.2f);
                g.DrawPath(hoverPen, path);
            }
            else
            {
                using var borderPen = new Pen(BorderColor, 1f);
                g.DrawPath(borderPen, path);
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
