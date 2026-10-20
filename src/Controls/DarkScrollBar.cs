using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Controls
{
    /// <summary>
    /// Sleek, modern custom dark scrollbar for DataGridView or standalone panels.
    /// Replaces the default clunky white WinForms scrollbar with a slim, rounded, dark-themed pill scrollbar.
    /// </summary>
    public class DarkScrollBar : Control
    {
        private DataGridView? _grid;
        private bool _isHovered = false;
        private bool _isDragging = false;
        private int _dragStartY = 0;
        private int _dragStartValue = 0;

        public Color TrackColor { get; set; } = Color.FromArgb(20, 22, 33);
        public Color ThumbColor { get; set; } = Color.FromArgb(65, 75, 110);
        public Color ThumbHoverColor { get; set; } = Color.FromArgb(95, 110, 160);
        public Color ThumbDragColor { get; set; } = Color.FromArgb(120, 140, 205);

        public int Minimum { get; set; } = 0;
        public int Maximum { get; set; } = 100;
        public int LargeChange { get; set; } = 10;

        private int _value = 0;
        public int Value
        {
            get => _value;
            set
            {
                int maxVal = Math.Max(Minimum, Maximum - LargeChange);
                int clamped = Math.Max(Minimum, Math.Min(maxVal, value));
                if (_value != clamped)
                {
                    _value = clamped;
                    Invalidate();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? ValueChanged;

        public DarkScrollBar()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            UpdateStyles();

            Width = 8;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;

            ApplyTheme(ThemeManager.CurrentTheme);
            ThemeManager.OnThemeChanged += ApplyTheme;
        }

        public void ApplyTheme(AppTheme theme)
        {
            if (theme == AppTheme.Light)
            {
                TrackColor = Color.FromArgb(241, 245, 249);
                ThumbColor = Color.FromArgb(203, 213, 225);
                ThumbHoverColor = Color.FromArgb(148, 163, 184);
                ThumbDragColor = Color.FromArgb(100, 116, 139);
            }
            else
            {
                TrackColor = Color.FromArgb(20, 22, 33);
                ThumbColor = Color.FromArgb(65, 75, 110);
                ThumbHoverColor = Color.FromArgb(95, 110, 160);
                ThumbDragColor = Color.FromArgb(120, 140, 205);
            }
            Invalidate();
        }


        /// <summary>
        /// Automatically binds this scrollbar to a DataGridView, keeping it in sync and replacing the native white scrollbar.
        /// </summary>
        public void AttachToGrid(DataGridView grid)
        {
            if (_grid != null)
            {
                _grid.Scroll -= HandleGridScroll;
                _grid.RowsAdded -= HandleGridRowsChanged;
                _grid.RowsRemoved -= HandleGridRowsChanged;
                _grid.SizeChanged -= HandleGridSizeChanged;
                _grid.MouseWheel -= HandleGridMouseWheel;
            }

            _grid = grid;
            _grid.ScrollBars = ScrollBars.None;

            _grid.Scroll += HandleGridScroll;
            _grid.RowsAdded += HandleGridRowsChanged;
            _grid.RowsRemoved += HandleGridRowsChanged;
            _grid.SizeChanged += HandleGridSizeChanged;
            _grid.MouseWheel += HandleGridMouseWheel;

            ValueChanged += (s, e) =>
            {
                if (_grid != null && _grid.RowCount > 0)
                {
                    try
                    {
                        int targetRow = Math.Max(0, Math.Min(_grid.RowCount - 1, Value));
                        if (_grid.FirstDisplayedScrollingRowIndex != targetRow)
                        {
                            _grid.FirstDisplayedScrollingRowIndex = targetRow;
                        }
                    }
                    catch
                    {
                        // Defensive against transient grid layouts
                    }
                }
            };

            SyncWithGrid();
        }

        public void SyncWithGrid()
        {
            if (_grid == null || !_grid.IsHandleCreated) return;

            int totalRows = _grid.RowCount;
            int visibleRows = Math.Max(1, _grid.DisplayedRowCount(false));

            if (totalRows <= visibleRows)
            {
                Visible = false;
                return;
            }

            Visible = true;
            Minimum = 0;
            Maximum = totalRows;
            LargeChange = visibleRows;

            int firstVisible = Math.Max(0, _grid.FirstDisplayedScrollingRowIndex);
            _value = Math.Max(0, Math.Min(Maximum - LargeChange, firstVisible));
            Invalidate();
        }

        private void HandleGridScroll(object? sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                SyncWithGrid();
            }
        }

        private void HandleGridRowsChanged(object? sender, EventArgs e) => SyncWithGrid();
        private void HandleGridSizeChanged(object? sender, EventArgs e) => SyncWithGrid();

        private void HandleGridMouseWheel(object? sender, MouseEventArgs e)
        {
            if (!Visible) return;
            int scrollStep = 2;
            if (e.Delta < 0)
            {
                Value += scrollStep;
            }
            else if (e.Delta > 0)
            {
                Value -= scrollStep;
            }
        }

        private Rectangle GetThumbRectangle()
        {
            int trackH = Height;
            if (trackH <= 0 || Maximum <= Minimum) return Rectangle.Empty;

            int range = Math.Max(1, Maximum - Minimum);
            float ratio = (float)LargeChange / range;
            int thumbH = Math.Max(24, (int)(trackH * ratio));
            int availableH = trackH - thumbH;

            int maxVal = Math.Max(1, Maximum - LargeChange);
            float posRatio = (float)(Value - Minimum) / maxVal;
            int thumbY = Math.Max(0, Math.Min(availableH, (int)(availableH * posRatio)));

            int pad = 1;
            return new Rectangle(pad, thumbY, Width - (pad * 2), thumbH);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw track
            using (var trackBrush = new SolidBrush(TrackColor))
            {
                using var path = CreateRoundedRect(ClientRectangle, 4);
                e.Graphics.FillPath(trackBrush, path);
            }

            // Draw thumb
            var thumbRect = GetThumbRectangle();
            if (thumbRect.Width > 0 && thumbRect.Height > 0)
            {
                Color thumbColor = _isDragging ? ThumbDragColor : (_isHovered ? ThumbHoverColor : ThumbColor);
                using (var thumbBrush = new SolidBrush(thumbColor))
                {
                    using var path = CreateRoundedRect(thumbRect, 3);
                    e.Graphics.FillPath(thumbBrush, path);
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            var thumbRect = GetThumbRectangle();
            if (thumbRect.Contains(e.Location))
            {
                _isDragging = true;
                _dragStartY = e.Y;
                _dragStartValue = Value;
                Capture = true;
                Invalidate();
            }
            else
            {
                // Clicked on track above or below thumb -> jump
                if (e.Y < thumbRect.Y)
                {
                    Value -= LargeChange;
                }
                else
                {
                    Value += LargeChange;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                int trackH = Height;
                var thumbRect = GetThumbRectangle();
                int availableH = trackH - thumbRect.Height;
                if (availableH > 0)
                {
                    int deltaY = e.Y - _dragStartY;
                    int maxVal = Math.Max(1, Maximum - LargeChange);
                    int deltaVal = (int)((float)deltaY / availableH * maxVal);
                    Value = _dragStartValue + deltaVal;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isDragging)
            {
                _isDragging = false;
                Capture = false;
                Invalidate();
            }
        }

        private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;

            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
