using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public enum BarcodeScannerMode
    {
        InvoiceEntry,
        InventoryLookup
    }

    public class BarcodeScannerDialog : Form
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private readonly Action<Product> _onProductScanned;
        private readonly BarcodeScannerMode _mode;
        private readonly Action<string>? _onRegisterNewRequested;
        private readonly Action<Product>? _onEditProductRequested;

        private TextBox _txtBarcode = null!;
        private Label _lblStatus = null!;
        private ModernButton? _btnQuickAction;
        private RoundedPanel _pnlScannerView = null!;
        private System.Windows.Forms.Timer _laserTimer = null!;
        private float _laserY = 10f;
        private bool _laserDown = true;
        private int _scannedCount = 0;
        private Label _lblScannedCount = null!;
        private FlowLayoutPanel _pnlSampleChips = null!;

        public int ScannedCount => _scannedCount;
        public BarcodeScannerMode Mode => _mode;

        public BarcodeScannerDialog(Action<Product> onProductScanned)
            : this(onProductScanned, BarcodeScannerMode.InvoiceEntry, null, null)
        {
        }

        public BarcodeScannerDialog(
            Action<Product> onProductScanned,
            BarcodeScannerMode mode,
            Action<string>? onRegisterNewRequested = null,
            Action<Product>? onEditProductRequested = null)
        {
            _onProductScanned = onProductScanned ?? throw new ArgumentNullException(nameof(onProductScanned));
            _mode = mode;
            _onRegisterNewRequested = onRegisterNewRequested;
            _onEditProductRequested = onEditProductRequested;

            Text = _mode == BarcodeScannerMode.InventoryLookup
                ? "Barcode Scanner & Product Inventory — S TOM'S"
                : "Barcode Scanner & Rapid Item Entry — S TOM'S";
            Size = new Size(620, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            BuildUI();
            StartLaserAnimation();
            _ = LoadSampleBarcodesAsync();
        }

        private void BuildUI()
        {
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

            // 1. Header Banner
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = ThemeManager.CardBg,
                Padding = new Padding(20, 10, 20, 8)
            };
            Controls.Add(pnlHeader);

            var lblTitle = new Label
            {
                Text = _mode == BarcodeScannerMode.InventoryLookup
                    ? "📷  Barcode Scanner & Inventory Lookup"
                    : "📷  Barcode Scanner & Rapid Entry",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 10),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = _mode == BarcodeScannerMode.InventoryLookup
                    ? "Scan with barcode gun, type code & press Enter, or click samples to find in inventory"
                    : "Scan with USB barcode gun, type code & press Enter, or click samples below",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(18, 36),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblSub);

            // 2. Animated Laser Scanner Viewfinder Box
            _pnlScannerView = new RoundedPanel
            {
                Location = new Point(25, 80),
                Size = new Size(555, 150),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = isDark ? Color.FromArgb(18, 22, 35) : Color.FromArgb(243, 246, 255),
                CustomBorderColor = ThemeManager.AccentBlue
            };
            _pnlScannerView.Paint += OnPaintScannerView;
            Controls.Add(_pnlScannerView);

            // 3. Status / Feedback Banner & Quick Action
            _lblStatus = new Label
            {
                Text = "Ready to scan barcode...",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                Location = new Point(25, 238),
                Size = new Size(390, 26),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(_lblStatus);

            _btnQuickAction = new ModernButton
            {
                Text = "Action",
                Size = new Size(160, 28),
                Location = new Point(420, 237),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BorderRadius = 6,
                ForeColor = Color.White,
                Visible = false
            };
            _btnQuickAction.Click += (s, e) =>
            {
                if (_btnQuickAction.Tag is Product p)
                {
                    _onEditProductRequested?.Invoke(p);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if (_btnQuickAction.Tag is string unregCode)
                {
                    _onRegisterNewRequested?.Invoke(unregCode);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
            Controls.Add(_btnQuickAction);

            // 4. Barcode Input Container
            var lblInputTitle = new Label
            {
                Text = "Barcode / SKU / Product ID:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(25, 268),
                AutoSize = true
            };
            Controls.Add(lblInputTitle);

            _txtBarcode = new TextBox
            {
                Font = new Font("Consolas", 12f, FontStyle.Bold),
                Location = new Point(25, 290),
                Size = new Size(420, 32),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Scan or type barcode (e.g. 890123400001)..."
            };
            _txtBarcode.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await ProcessBarcodeAsync(_txtBarcode.Text);
                }
            };
            Controls.Add(_txtBarcode);

            var btnLookup = new ModernButton
            {
                Text = _mode == BarcodeScannerMode.InventoryLookup ? "⚡ Lookup" : "⚡ Add",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(79, 70, 229),
                PressedColor = Color.FromArgb(67, 56, 202),
                Size = new Size(125, 32),
                Location = new Point(455, 290),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 6,
                ForeColor = Color.White
            };
            btnLookup.Click += async (s, e) => await ProcessBarcodeAsync(_txtBarcode.Text);
            Controls.Add(btnLookup);

            // 5. Sample Barcode Chips Header
            var lblSampleTitle = new Label
            {
                Text = "💡 Quick Sample Barcodes (Click to test instant scan):",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(25, 332),
                AutoSize = true
            };
            Controls.Add(lblSampleTitle);

            // 6. FlowLayoutPanel for quick sample chips
            _pnlSampleChips = new FlowLayoutPanel
            {
                Location = new Point(25, 355),
                Size = new Size(555, 140),
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            Controls.Add(_pnlSampleChips);

            // 7. Footer: Scanned counter & Done Button
            _lblScannedCount = new Label
            {
                Text = _mode == BarcodeScannerMode.InventoryLookup ? "Products scanned: 0" : "Items added in this session: 0",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(25, 515),
                AutoSize = true
            };
            Controls.Add(_lblScannedCount);

            var btnDone = new ModernButton
            {
                Text = _mode == BarcodeScannerMode.InventoryLookup ? "✓ Done / Close" : "✓ Return to Invoice",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(16, 185, 129),
                PressedColor = Color.FromArgb(5, 150, 105),
                Size = new Size(160, 36),
                Location = new Point(420, 510),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            btnDone.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btnDone);
        }

        private void StartLaserAnimation()
        {
            _laserTimer = new System.Windows.Forms.Timer
            {
                Interval = 25 // Smooth ~40 FPS animation
            };
            _laserTimer.Tick += (s, e) =>
            {
                if (_laserDown)
                {
                    _laserY += 3f;
                    if (_laserY >= 135f) _laserDown = false;
                }
                else
                {
                    _laserY -= 3f;
                    if (_laserY <= 15f) _laserDown = true;
                }
                _pnlScannerView.Invalidate();
            };
            _laserTimer.Start();
        }

        private void OnPaintScannerView(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = _pnlScannerView.Width;
            int h = _pnlScannerView.Height;

            // 1. Draw Viewfinder Corner Crosshairs ┌ ┐ └ ┘
            int cw = 20;
            int pad = 12;
            using var crosshairPen = new Pen(ThemeManager.AccentBlue, 2.5f);
            // Top-left
            g.DrawLine(crosshairPen, pad, pad, pad + cw, pad);
            g.DrawLine(crosshairPen, pad, pad, pad, pad + cw);
            // Top-right
            g.DrawLine(crosshairPen, w - pad, pad, w - pad - cw, pad);
            g.DrawLine(crosshairPen, w - pad, pad, w - pad, pad + cw);
            // Bottom-left
            g.DrawLine(crosshairPen, pad, h - pad, pad + cw, h - pad);
            g.DrawLine(crosshairPen, pad, h - pad, pad, pad - cw);
            // Bottom-right
            g.DrawLine(crosshairPen, w - pad, h - pad, w - pad - cw, h - pad);
            g.DrawLine(crosshairPen, w - pad, h - pad, w - pad, h - pad - cw);

            // 2. Draw Stylized Barcode in the Center
            int barStartX = (w - 220) / 2;
            int barY = 35;
            int barH = 75;
            Color barColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(160, 175, 210) : Color.FromArgb(80, 95, 125);
            using var barBrush = new SolidBrush(barColor);

            int[] widths = { 2, 4, 1, 3, 2, 5, 2, 3, 4, 1, 3, 2, 4, 2, 1, 3, 4, 2, 5, 2, 3, 1, 4, 2, 3, 1, 4 };
            int curX = barStartX;
            for (int i = 0; i < widths.Length; i++)
            {
                int bWidth = widths[i];
                if (i % 2 == 0)
                {
                    g.FillRectangle(barBrush, curX, barY, bWidth, barH);
                }
                curX += bWidth + 3;
            }

            // 3. Draw Laser Line & Neon Scan Glow
            using (var laserGlowPen = new Pen(Color.FromArgb(80, 239, 68, 68), 5f))
            {
                g.DrawLine(laserGlowPen, 35, _laserY, w - 35, _laserY);
            }
            using (var laserPen = new Pen(Color.FromArgb(239, 68, 68), 2f))
            {
                g.DrawLine(laserPen, 30, _laserY, w - 30, _laserY);
            }
        }

        private async Task LoadSampleBarcodesAsync()
        {
            try
            {
                var products = await _db.GetProductsAsync();
                _pnlSampleChips.Controls.Clear();

                foreach (var p in products)
                {
                    string barcode = string.IsNullOrEmpty(p.Barcode) ? $"890123400{p.ProductId:D3}" : p.Barcode;
                    var btnChip = new Button
                    {
                        Text = $"🏷️ {barcode} — {p.Name} (₹{p.Price:N0})",
                        Font = new Font("Segoe UI", 8.5f),
                        Size = new Size(265, 34),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand,
                        BackColor = ThemeManager.CardBg,
                        ForeColor = ThemeManager.TextPrimary,
                        Tag = barcode,
                        Margin = new Padding(3, 3, 3, 3)
                    };
                    btnChip.FlatAppearance.BorderColor = ThemeManager.CardBorder;
                    btnChip.Click += async (s, e) =>
                    {
                        if (s is Button b && b.Tag is string code)
                        {
                            _txtBarcode.Text = code;
                            await ProcessBarcodeAsync(code);
                        }
                    };
                    _pnlSampleChips.Controls.Add(btnChip);
                }
            }
            catch
            {
                // Fallback gracefully
            }
        }

        private async Task ProcessBarcodeAsync(string? rawBarcode)
        {
            if (string.IsNullOrWhiteSpace(rawBarcode))
            {
                _lblStatus.Text = "⚠️ Please enter or scan a barcode.";
                _lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                if (_btnQuickAction != null) _btnQuickAction.Visible = false;
                _txtBarcode.Focus();
                return;
            }

            string code = rawBarcode.Trim();
            _lblStatus.Text = $"Searching for barcode '{code}'...";
            _lblStatus.ForeColor = ThemeManager.AccentBlue;
            if (_btnQuickAction != null) _btnQuickAction.Visible = false;

            var product = await _db.GetProductByBarcodeAsync(code);
            if (product != null)
            {
                _scannedCount++;
                _onProductScanned(product);

                if (_mode == BarcodeScannerMode.InventoryLookup)
                {
                    _lblStatus.Text = $"✓ Found: {product.Name} (₹{product.Price:N0}, Stock: {product.StockQuantity})";
                    _lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
                    _lblScannedCount.Text = $"Products scanned: {_scannedCount}";

                    if (_btnQuickAction != null && _onEditProductRequested != null)
                    {
                        _btnQuickAction.Text = "✏️ Edit Product";
                        _btnQuickAction.NormalColor = Color.FromArgb(37, 99, 235);
                        _btnQuickAction.HoverColor = Color.FromArgb(29, 78, 216);
                        _btnQuickAction.PressedColor = Color.FromArgb(30, 64, 175);
                        _btnQuickAction.Tag = product;
                        _btnQuickAction.Visible = true;
                    }
                }
                else
                {
                    _lblStatus.Text = $"✓ Added: {product.Name} (₹{product.Price:N2})";
                    _lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
                    _lblScannedCount.Text = $"Items added in this session: {_scannedCount}";
                    if (_btnQuickAction != null) _btnQuickAction.Visible = false;
                }

                _txtBarcode.Clear();
                _txtBarcode.Focus();
            }
            else
            {
                _lblStatus.Text = $"⚠️ No product found for barcode '{code}'.";
                _lblStatus.ForeColor = Color.FromArgb(239, 68, 68);

                if (_mode == BarcodeScannerMode.InventoryLookup && _onRegisterNewRequested != null && _btnQuickAction != null)
                {
                    _btnQuickAction.Text = "+ Add New Product";
                    _btnQuickAction.NormalColor = Color.FromArgb(16, 185, 129);
                    _btnQuickAction.HoverColor = Color.FromArgb(5, 150, 105);
                    _btnQuickAction.PressedColor = Color.FromArgb(4, 120, 87);
                    _btnQuickAction.Tag = code;
                    _btnQuickAction.Visible = true;
                }
                else
                {
                    if (_btnQuickAction != null) _btnQuickAction.Visible = false;
                }

                _txtBarcode.SelectAll();
                _txtBarcode.Focus();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtBarcode?.Focus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _laserTimer?.Stop();
                _laserTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
