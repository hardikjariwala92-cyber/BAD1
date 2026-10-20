using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public class ManualAddItemDialog : Form
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private List<Product> _catalogProducts = new();

        private ComboBox _cmbCatalog = null!;
        private TextBox _txtName = null!;
        private TextBox _txtDesc = null!;
        private TextBox _txtQty = null!;
        private TextBox _txtRate = null!;
        private TextBox _txtDisc = null!;
        private ComboBox _cmbTax = null!;
        private Label _lblLineTotalPreview = null!;
        private RoundedPanel _pnlCalculation = null!;

        public InvoiceItem? CreatedItem { get; private set; }
        public bool AddBlankRowRequested { get; private set; }

        public ManualAddItemDialog()
        {
            Text = "Add Product / Service Item — S TOM'S";
            Size = new Size(540, 610);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            BuildUI();
            _ = LoadCatalogAsync();
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
                Padding = new Padding(20, 12, 20, 8)
            };
            Controls.Add(pnlHeader);

            var lblTitle = new Label
            {
                Text = "📦  Add Product / Service Item",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 10),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Select from product catalog or enter custom service details manually",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(18, 36),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblSub);

            // Separator line
            pnlHeader.Paint += (s, e) =>
            {
                using var p = new Pen(ThemeManager.CardBorder, 1f);
                e.Graphics.DrawLine(p, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            int y = 80;

            // 2. Quick Catalog Dropdown
            CreateLabel("Select from Product Catalog (Auto-fill):", 25, y);
            _cmbCatalog = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(475, 28),
                Location = new Point(25, y + 20),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary
            };
            _cmbCatalog.Items.Add("-- Or Enter Custom Manual Service Below --");
            _cmbCatalog.SelectedIndex = 0;
            _cmbCatalog.SelectedIndexChanged += OnCatalogSelected;
            Controls.Add(_cmbCatalog);
            y += 58;

            // 3. Name (Required)
            CreateLabel("Service / Product Name *:", 25, y);
            _txtName = CreateInput(25, y + 20, 475, 26);
            _txtName.TextChanged += (s, e) => UpdateCalculationPreview();
            Controls.Add(_txtName);
            y += 54;

            // 4. Description
            CreateLabel("Description (Optional):", 25, y);
            _txtDesc = CreateInput(25, y + 20, 475, 26);
            Controls.Add(_txtDesc);
            y += 54;

            // 5. Quantity & Rate (₹)
            int halfW = 230;
            CreateLabel("Quantity *:", 25, y);
            _txtQty = CreateInput(25, y + 20, halfW, 26, "1");
            _txtQty.TextChanged += (s, e) => UpdateCalculationPreview();
            Controls.Add(_txtQty);

            CreateLabel("Rate / Price (₹) *:", 270, y);
            _txtRate = CreateInput(270, y + 20, halfW, 26, "0.00");
            _txtRate.TextChanged += (s, e) => UpdateCalculationPreview();
            Controls.Add(_txtRate);
            y += 54;

            // 6. Discount & Tax
            CreateLabel("Discount (%):", 25, y);
            _txtDisc = CreateInput(25, y + 20, halfW, 26, "0");
            _txtDisc.TextChanged += (s, e) => UpdateCalculationPreview();
            Controls.Add(_txtDisc);

            CreateLabel("GST Tax Rate (%):", 270, y);
            _cmbTax = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(halfW, 28),
                Location = new Point(270, y + 20),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary
            };
            _cmbTax.Items.AddRange(new object[] { "0%", "5%", "12%", "18%", "28%" });
            _cmbTax.SelectedIndex = 3; // 18% default
            _cmbTax.SelectedIndexChanged += (s, e) => UpdateCalculationPreview();
            Controls.Add(_cmbTax);
            y += 58;

            // 7. Live Calculation Preview Pill
            _pnlCalculation = new RoundedPanel
            {
                Location = new Point(25, y),
                Size = new Size(475, 42),
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = isDark ? Color.FromArgb(30, 36, 56) : Color.FromArgb(240, 245, 255),
                CustomBorderColor = ThemeManager.AccentBlue
            };
            Controls.Add(_pnlCalculation);

            _lblLineTotalPreview = new Label
            {
                Text = "Line Total: ₹0.00",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _pnlCalculation.Controls.Add(_lblLineTotalPreview);
            y += 54;

            // 8. Action Buttons
            var btnAdd = new ModernButton
            {
                Text = "✅ Add to Invoice",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(16, 185, 129),
                PressedColor = Color.FromArgb(5, 150, 105),
                Size = new Size(155, 36),
                Location = new Point(25, y),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            Controls.Add(btnAdd);
            btnAdd.Click += OnAddClicked;

            var btnBlank = new ModernButton
            {
                Text = "+ Blank Row",
                NormalColor = isDark ? Color.FromArgb(45, 52, 75) : Color.FromArgb(226, 232, 240),
                HoverColor = isDark ? Color.FromArgb(60, 70, 100) : Color.FromArgb(203, 213, 225),
                PressedColor = isDark ? Color.FromArgb(35, 40, 60) : Color.FromArgb(148, 163, 184),
                Size = new Size(130, 36),
                Location = new Point(190, y),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = isDark ? Color.White : Color.FromArgb(15, 23, 42)
            };
            Controls.Add(btnBlank);
            btnBlank.Click += (s, e) =>
            {
                AddBlankRowRequested = true;
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = new ModernButton
            {
                Text = "✕ Cancel",
                NormalColor = isDark ? Color.FromArgb(40, 44, 60) : Color.FromArgb(241, 245, 249),
                HoverColor = isDark ? Color.FromArgb(55, 60, 80) : Color.FromArgb(226, 232, 240),
                PressedColor = isDark ? Color.FromArgb(30, 34, 48) : Color.FromArgb(203, 213, 225),
                Size = new Size(100, 36),
                Location = new Point(400, y),
                Font = new Font("Segoe UI", 9f),
                BorderRadius = 8,
                ForeColor = ThemeManager.TextSecondary
            };
            Controls.Add(btnCancel);
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            UpdateCalculationPreview();
        }

        private async Task LoadCatalogAsync()
        {
            try
            {
                _catalogProducts = await _db.GetProductsAsync();
                foreach (var p in _catalogProducts)
                {
                    _cmbCatalog.Items.Add($"{p.Name} — ₹{p.Price:N2} ({p.Category})");
                }
            }
            catch
            {
                // Fallback gracefully
            }
        }

        private void OnCatalogSelected(object? sender, EventArgs e)
        {
            if (_cmbCatalog.SelectedIndex > 0 && _cmbCatalog.SelectedIndex - 1 < _catalogProducts.Count)
            {
                var p = _catalogProducts[_cmbCatalog.SelectedIndex - 1];
                _txtName.Text = p.Name;
                _txtDesc.Text = $"{p.Category} — In stock: {p.StockQuantity} units";
                _txtRate.Text = p.Price.ToString("F2");
                _txtQty.Text = "1";
                _txtDisc.Text = "0";
                _cmbTax.SelectedIndex = 3; // 18%
                UpdateCalculationPreview();
            }
        }

        private void UpdateCalculationPreview()
        {
            if (_txtQty == null || _txtRate == null || _txtDisc == null || _cmbTax == null || _lblLineTotalPreview == null) return;

            decimal qty = decimal.TryParse(_txtQty.Text, out decimal q) ? q : 1m;
            decimal rate = decimal.TryParse(_txtRate.Text, out decimal r) ? r : 0m;
            decimal disc = decimal.TryParse(_txtDisc.Text, out decimal d) ? d : 0m;

            string taxStr = _cmbTax.SelectedItem?.ToString()?.Replace("%", "") ?? "18";
            decimal taxRate = decimal.TryParse(taxStr, out decimal t) ? t : 18m;

            decimal gross = qty * rate;
            decimal discAmount = gross * (disc / 100m);
            decimal taxable = Math.Max(0m, gross - discAmount);
            decimal taxAmount = taxable * (taxRate / 100m);
            decimal total = Math.Round(taxable + taxAmount, 2);

            _lblLineTotalPreview.Text = $"Subtotal: ₹{gross:N2}  |  Tax: ₹{taxAmount:N2}  |  Line Total: ₹{total:N2}";
        }

        private void OnAddClicked(object? sender, EventArgs e)
        {
            string name = _txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a valid Product or Service Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return;
            }

            if (!decimal.TryParse(_txtQty.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Quantity must be a positive number greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtQty.Focus();
                return;
            }

            if (!decimal.TryParse(_txtRate.Text, out decimal rate) || rate < 0)
            {
                MessageBox.Show("Rate / Price must be a valid non-negative number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtRate.Focus();
                return;
            }

            decimal disc = decimal.TryParse(_txtDisc.Text, out decimal d) ? d : 0m;
            string taxStr = _cmbTax.SelectedItem?.ToString()?.Replace("%", "") ?? "18";
            decimal tax = decimal.TryParse(taxStr, out decimal t) ? t : 18m;

            CreatedItem = new InvoiceItem(name, _txtDesc.Text.Trim(), qty, rate, disc, tax);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CreateLabel(string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            };
            Controls.Add(lbl);
        }

        private TextBox CreateInput(int x, int y, int w, int h, string defaultVal = "")
        {
            return new TextBox
            {
                Text = defaultVal,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
        }
    }
}
