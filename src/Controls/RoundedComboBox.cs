using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    public class RoundedComboBox : Control, IMessageFilter
    {
        private readonly List<string> _items = new();
        private int _selectedIndex = -1;
        private bool _isHovered = false;
        private bool _isOpen = false;
        private ContextMenuStrip? _dropdownMenu;

        public int BorderRadius { get; set; } = 15;
        public bool EnableMouseWheelScroll { get; set; } = true;
        public Color BoxBackgroundColor { get; set; } = Color.FromArgb(22, 25, 37);
        public Color BorderColor { get; set; } = Color.FromArgb(46, 53, 79);
        public Color HoverBorderColor { get; set; } = Color.FromArgb(99, 102, 241);
        public Color FocusBorderColor { get; set; } = Color.FromArgb(99, 102, 241);
        public Color TextColor { get; set; } = Color.FromArgb(241, 245, 249);
        public Color ChevronColor { get; set; } = Color.FromArgb(148, 163, 184);

        public List<string> Items => _items;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                int clamped = value;
                if (clamped < -1 || clamped >= _items.Count) clamped = -1;
                if (_selectedIndex != clamped)
                {
                    _selectedIndex = clamped;
                    Invalidate();
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public object? SelectedItem
        {
            get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
            set
            {
                if (value == null)
                {
                    SelectedIndex = -1;
                    return;
                }
                string str = value.ToString() ?? "";
                int idx = _items.FindIndex(i => string.Equals(i, str, StringComparison.OrdinalIgnoreCase));
                SelectedIndex = idx;
            }
        }

        public event EventHandler? SelectedIndexChanged;

        public RoundedComboBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            UpdateStyles();

            Size = new Size(125, 30);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            BackColor = Color.Transparent;

            ApplyTheme(ThemeManager.CurrentTheme);
            ThemeManager.OnThemeChanged += ApplyTheme;
            Application.AddMessageFilter(this);
        }

        public void HandleWheelScroll(int delta)
        {
            if (_items.Count <= 1 || !EnableMouseWheelScroll) return;

            if (delta < 0)
            {
                // Scrolled Down -> next item (cycles: All -> Delivered -> Processed -> Cancelled -> All)
                int next = (_selectedIndex + 1) % _items.Count;
                SelectedIndex = next;
            }
            else if (delta > 0)
            {
                // Scrolled Up -> previous item
                int prev = _selectedIndex <= 0 ? _items.Count - 1 : _selectedIndex - 1;
                SelectedIndex = prev;
            }

            if (_isOpen && _dropdownMenu != null && _dropdownMenu.Visible)
            {
                for (int i = 0; i < _dropdownMenu.Items.Count; i++)
                {
                    if (_dropdownMenu.Items[i] is ToolStripMenuItem item)
                    {
                        item.Font = i == _selectedIndex ? new Font("Segoe UI", 9f, FontStyle.Bold) : new Font("Segoe UI", 9f);
                    }
                }
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x020A;
            if (m.Msg == WM_MOUSEWHEEL && EnableMouseWheelScroll)
            {
                if (!Visible || !IsHandleCreated || Disposing || IsDisposed)
                    return false;

                Point screenPt = Cursor.Position;
                bool overControl = ClientRectangle.Contains(PointToClient(screenPt));
                bool overMenu = _isOpen && _dropdownMenu != null && _dropdownMenu.Visible &&
                                _dropdownMenu.ClientRectangle.Contains(_dropdownMenu.PointToClient(screenPt));

                if (overControl || overMenu)
                {
                    int delta = unchecked((short)((long)m.WParam >> 16));
                    HandleWheelScroll(delta);
                    return true;
                }
            }
            return false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (EnableMouseWheelScroll)
            {
                HandleWheelScroll(e.Delta);
                if (e is HandledMouseEventArgs h)
                {
                    h.Handled = true;
                }
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

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ShowDropdown();
        }

        private void ShowDropdown()
        {
            if (_items.Count == 0) return;

            _dropdownMenu?.Dispose();
            _dropdownMenu = new ContextMenuStrip
            {
                ShowImageMargin = false,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Renderer = new ModernDropdownRenderer(ThemeManager.CurrentTheme)
            };

            for (int i = 0; i < _items.Count; i++)
            {
                int capturedIndex = i;
                string itemText = _items[i];
                string prefix = GetStatusPrefix(itemText);
                string displayLabel = $"{prefix} {itemText}".TrimStart();

                var menuItem = new ToolStripMenuItem(displayLabel)
                {
                    Font = i == _selectedIndex ? new Font("Segoe UI", 9f, FontStyle.Bold) : new Font("Segoe UI", 9f),
                    Padding = new Padding(12, 6, 12, 6)
                };

                menuItem.Click += (s, ev) =>
                {
                    SelectedIndex = capturedIndex;
                };

                _dropdownMenu.Items.Add(menuItem);
            }

            _dropdownMenu.Opened += (s, ev) =>
            {
                _isOpen = true;
                Invalidate();
            };

            _dropdownMenu.Closed += (s, ev) =>
            {
                _isOpen = false;
                Invalidate();
            };

            _dropdownMenu.Show(this, new Point(0, Height + 2));
        }

        private static string GetStatusPrefix(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "delivered" or "paid" => "🟢",
                "processed" or "partially paid" => "🟡",
                "unpaid" => "🟠",
                "cancelled" => "🔘",
                "overdue" => "🔴",
                "draft" => "⚪",
                "all" => "🌐",
                _ => "•"
            };
        }

        public void ApplyTheme(AppTheme theme)
        {
            bool isDark = theme == AppTheme.Dark;
            BoxBackgroundColor = isDark ? Color.FromArgb(22, 25, 37) : Color.FromArgb(248, 250, 252);
            BorderColor = isDark ? Color.FromArgb(46, 53, 79) : Color.FromArgb(226, 232, 240);
            HoverBorderColor = isDark ? Color.FromArgb(99, 102, 241) : Color.FromArgb(79, 70, 229);
            FocusBorderColor = isDark ? Color.FromArgb(99, 102, 241) : Color.FromArgb(79, 70, 229);
            TextColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
            ChevronColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.OnThemeChanged -= ApplyTheme;
                Application.RemoveMessageFilter(this);
                _dropdownMenu?.Dispose();
            }
            base.Dispose(disposing);
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
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var path = CreateRoundedRectanglePath(rect, BorderRadius);

            // Fill Pill Background
            using (var fillBrush = new SolidBrush(BoxBackgroundColor))
            {
                g.FillPath(fillBrush, path);
            }

            // Draw CSS focus glow or hover border
            if (_isOpen)
            {
                // Multi-pass CSS focus halo (box-shadow: 0 0 0 3.5px rgba(99, 102, 241, 0.25))
                using var haloPen = new Pen(Color.FromArgb(60, FocusBorderColor), 3.5f);
                g.DrawPath(haloPen, path);

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

            // Draw Status Indicator Dot
            string selectedText = SelectedItem?.ToString() ?? "All";
            Color dotColor = selectedText.ToLowerInvariant() switch
            {
                "delivered" or "paid" => Color.FromArgb(16, 185, 129),
                "processed" or "partially paid" => Color.FromArgb(245, 158, 11),
                "unpaid" => Color.FromArgb(249, 115, 22),
                "overdue" => Color.FromArgb(239, 68, 68),
                "cancelled" => Color.FromArgb(100, 116, 139),
                "draft" => Color.FromArgb(148, 163, 184),
                _ => Color.FromArgb(99, 102, 241)
            };

            int dotSize = 8;
            int dotX = 14;
            int dotY = (Height - dotSize) / 2;

            using (var dotGlowBrush = new SolidBrush(Color.FromArgb(50, dotColor)))
            {
                g.FillEllipse(dotGlowBrush, dotX - 2, dotY - 2, dotSize + 4, dotSize + 4);
            }
            using (var dotBrush = new SolidBrush(dotColor))
            {
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            // Draw Selected Text
            int textX = dotX + dotSize + 8;
            int textW = Width - textX - 26;
            var textRect = new Rectangle(textX, 0, textW, Height);

            TextRenderer.DrawText(
                g,
                selectedText,
                Font,
                textRect,
                TextColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            );

            // Draw Vector Down Chevron (▾)
            int chevX = Width - 16;
            int chevY = Height / 2;
            using var chevPen = new Pen(ChevronColor, 1.8f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            g.DrawLine(chevPen, chevX - 4, chevY - 2, chevX, chevY + 2);
            g.DrawLine(chevPen, chevX, chevY + 2, chevX + 4, chevY - 2);
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

        private sealed class ModernDropdownRenderer : ToolStripProfessionalRenderer
        {
            private readonly AppTheme _theme;

            public ModernDropdownRenderer(AppTheme theme) : base(new ModernDropdownColorTable(theme))
            {
                _theme = theme;
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    Color highlight = _theme == AppTheme.Dark
                        ? Color.FromArgb(45, 53, 83)
                        : Color.FromArgb(224, 231, 255);
                    using var brush = new SolidBrush(highlight);
                    e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
                }
                else
                {
                    Color bg = _theme == AppTheme.Dark
                        ? Color.FromArgb(22, 25, 37)
                        : Color.FromArgb(255, 255, 255);
                    using var brush = new SolidBrush(bg);
                    e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = _theme == AppTheme.Dark
                    ? Color.FromArgb(241, 245, 249)
                    : Color.FromArgb(15, 23, 42);
                base.OnRenderItemText(e);
            }
        }

        private sealed class ModernDropdownColorTable : ProfessionalColorTable
        {
            private readonly AppTheme _theme;

            public ModernDropdownColorTable(AppTheme theme)
            {
                _theme = theme;
            }

            public override Color ToolStripDropDownBackground => _theme == AppTheme.Dark
                ? Color.FromArgb(22, 25, 37)
                : Color.FromArgb(255, 255, 255);

            public override Color MenuBorder => _theme == AppTheme.Dark
                ? Color.FromArgb(46, 53, 79)
                : Color.FromArgb(226, 232, 240);

            public override Color MenuItemSelected => _theme == AppTheme.Dark
                ? Color.FromArgb(45, 53, 83)
                : Color.FromArgb(224, 231, 255);
        }
    }
}
