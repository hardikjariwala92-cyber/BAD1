using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// Modern, responsive dialog for adding new customer orders with full validation,
    /// customer autocompletion, real-time theming, and database persistence.
    /// </summary>
    public partial class AddCustomerOrder : Form
    {
        private readonly DatabaseService _dbService = DatabaseService.Instance;
        private readonly Dictionary<string, string> _customerAddresses = new(StringComparer.OrdinalIgnoreCase);
        private bool _isSaving = false;

        public CustomerOrder? CreatedOrder { get; private set; }

        public AddCustomerOrder(DateTime? defaultDate = null)
        {
            InitializeComponent();

            // Populate Status options (Processed is default)
            _cmbStatus.Items.AddRange(new object[] { "Processed", "Delivered", "Cancelled" });
            _cmbStatus.SelectedIndex = 0;

            // Default Order Date
            _dtpOrderDate.Value = defaultDate ?? DateTime.Today;

            // Custom Icon Badge Painting
            _pnlIconBadge.Paint += DrawIconBadge;

            // Wire input events
            _cmbCustomer.SelectedIndexChanged += OnCustomerSelected;
            _cmbCustomer.TextChanged += (s, e) => ClearValidationError();
            _txtAddress.TextChanged += (s, e) => ClearValidationError();
            _txtPrice.TextChanged += (s, e) => ClearValidationError();
            _txtPrice.KeyPress += OnPriceKeyPress;

            _btnSave.Click += async (s, e) => await SaveOrderAsync();
            _btnCancel.Click += (s, e) => CancelDialog();
            _btnClear.Click += (s, e) => ClearFields();

            // Theming support
            ThemeManager.OnThemeChanged += HandleThemeChanged;
            ApplyCurrentTheme();

            // Load customer suggestions asynchronously
            Load += async (s, e) => await LoadCustomerSuggestionsAsync();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ThemeManager.OnThemeChanged -= HandleThemeChanged;
            base.OnFormClosed(e);
        }

        private void HandleThemeChanged(AppTheme newTheme)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ApplyCurrentTheme()));
                return;
            }
            ApplyCurrentTheme();
        }

        public void ApplyCurrentTheme()
        {
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
            BackColor = ThemeManager.Background;

            // Panels
            _pnlHeader.CustomBackgroundColor = ThemeManager.CardBg;
            _pnlHeader.CustomBorderColor = ThemeManager.CardBorder;
            _pnlOrderDetails.CustomBackgroundColor = ThemeManager.CardBg;
            _pnlOrderDetails.CustomBorderColor = ThemeManager.CardBorder;

            // Header Typography
            _lblHeaderTitle.ForeColor = ThemeManager.TextPrimary;
            _lblHeaderSubtitle.ForeColor = ThemeManager.TextSecondary;
            _lblSectionTag.ForeColor = ThemeManager.AccentBlue;

            // Input Fields
            Color inputBg = isDark ? Color.FromArgb(24, 26, 38) : Color.White;
            Color inputFg = ThemeManager.TextPrimary;
            Color hintFg = ThemeManager.TextSecondary;

            _lblCustomerName.ForeColor = inputFg;
            _lblCustomerHint.ForeColor = hintFg;
            _cmbCustomer.BackColor = inputBg;
            _cmbCustomer.ForeColor = inputFg;

            _lblAddress.ForeColor = inputFg;
            _lblAddressHint.ForeColor = hintFg;
            _txtAddress.BackColor = inputBg;
            _txtAddress.ForeColor = inputFg;

            _lblPrice.ForeColor = inputFg;
            _lblPriceHint.ForeColor = hintFg;
            _txtPrice.BackColor = inputBg;
            _txtPrice.ForeColor = inputFg;

            _lblStatus.ForeColor = inputFg;
            _lblStatusHint.ForeColor = hintFg;
            _cmbStatus.BackColor = inputBg;
            _cmbStatus.ForeColor = inputFg;

            _lblOrderDate.ForeColor = inputFg;
            _lblDateHint.ForeColor = hintFg;
            _dtpOrderDate.CalendarMonthBackground = inputBg;
            _dtpOrderDate.CalendarForeColor = inputFg;
            _dtpOrderDate.CalendarTitleBackColor = ThemeManager.AccentBlue;
            _dtpOrderDate.CalendarTitleForeColor = Color.White;

            // Validation Banner
            _lblValidation.BackColor = isDark ? Color.FromArgb(48, 25, 30) : Color.FromArgb(254, 242, 242);
            _lblValidation.ForeColor = Color.FromArgb(239, 68, 68);

            // Buttons
            _btnSave.NormalColor = ThemeManager.AccentBlue;
            _btnSave.HoverColor = Color.FromArgb(98, 133, 255);
            _btnSave.PressedColor = Color.FromArgb(60, 95, 220);
            _btnSave.ForeColor = Color.White;

            _btnCancel.NormalColor = isDark ? Color.FromArgb(38, 42, 58) : Color.FromArgb(241, 245, 249);
            _btnCancel.HoverColor = isDark ? Color.FromArgb(52, 58, 80) : Color.FromArgb(226, 232, 240);
            _btnCancel.BorderOutlineColor = isDark ? Color.FromArgb(55, 60, 84) : Color.FromArgb(215, 222, 232);
            _btnCancel.ForeColor = ThemeManager.TextPrimary;

            _btnClear.NormalColor = isDark ? Color.FromArgb(30, 33, 46) : Color.FromArgb(241, 245, 249);
            _btnClear.HoverColor = isDark ? Color.FromArgb(44, 48, 68) : Color.FromArgb(226, 232, 240);
            _btnClear.BorderOutlineColor = isDark ? Color.FromArgb(48, 52, 72) : Color.FromArgb(215, 222, 232);
            _btnClear.ForeColor = ThemeManager.TextSecondary;

            _pnlIconBadge.Invalidate();
            _pnlHeader.Invalidate();
            _pnlOrderDetails.Invalidate();
            _btnClear.Invalidate();
            _btnCancel.Invalidate();
            _btnSave.Invalidate();
        }

        private void DrawIconBadge(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, _pnlIconBadge.Width - 1, _pnlIconBadge.Height - 1);
            using (var brush = new LinearGradientBrush(rect, ThemeManager.AccentBlue, Color.FromArgb(120, 80, 255), LinearGradientMode.ForwardDiagonal))
            {
                using var path = CreateRoundedRectanglePath(rect, 12);
                g.FillPath(brush, path);
            }

            // Draw Order / Package Icon Glyph
            using var font = new Font("Segoe UI Emoji", 16f);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("📦", font, Brushes.White, rect, sf);
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private async Task LoadCustomerSuggestionsAsync()
        {
            try
            {
                var customers = await _dbService.GetCustomersAsync();
                var customerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var c in customers)
                {
                    if (!string.IsNullOrWhiteSpace(c.Name))
                    {
                        customerNames.Add(c.Name.Trim());
                        string fullAddr = !string.IsNullOrWhiteSpace(c.City) ? c.City : c.Address;
                        if (!string.IsNullOrWhiteSpace(fullAddr) && !_customerAddresses.ContainsKey(c.Name.Trim()))
                        {
                            _customerAddresses[c.Name.Trim()] = fullAddr.Trim();
                        }
                    }
                }

                // Also supplement from existing orders
                var existingOrders = await _dbService.GetOrdersAsync();
                foreach (var o in existingOrders)
                {
                    if (!string.IsNullOrWhiteSpace(o.ProfileName))
                    {
                        customerNames.Add(o.ProfileName.Trim());
                        if (!string.IsNullOrWhiteSpace(o.Address) && !_customerAddresses.ContainsKey(o.ProfileName.Trim()))
                        {
                            _customerAddresses[o.ProfileName.Trim()] = o.Address.Trim();
                        }
                    }
                }

                var autoSource = new AutoCompleteStringCollection();
                _cmbCustomer.Items.Clear();
                foreach (var name in customerNames.OrderBy(n => n))
                {
                    _cmbCustomer.Items.Add(name);
                    autoSource.Add(name);
                }

                if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
                {
                    try
                    {
                        _cmbCustomer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        _cmbCustomer.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        _cmbCustomer.AutoCompleteCustomSource = autoSource;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AddCustomerOrder] Failed to load suggestions: {ex.Message}");
            }
        }

        private void OnCustomerSelected(object? sender, EventArgs e)
        {
            ClearValidationError();
            string selectedName = _cmbCustomer.Text.Trim();
            if (!string.IsNullOrEmpty(selectedName) && _customerAddresses.TryGetValue(selectedName, out var knownAddress))
            {
                // Auto-fill address if currently empty
                if (string.IsNullOrWhiteSpace(_txtAddress.Text))
                {
                    _txtAddress.Text = knownAddress;
                }
            }
        }

        private void OnPriceKeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow control keys (backspace, delete, copy/paste)
            if (char.IsControl(e.KeyChar)) return;

            // Allow digits and single decimal separator
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar == '.' && !_txtPrice.Text.Contains('.')) return;

            // Suppress invalid keystrokes
            e.Handled = true;
        }

        private void ClearValidationError()
        {
            if (_lblValidation.Visible)
            {
                _lblValidation.Text = string.Empty;
                _lblValidation.Visible = false;
            }
        }

        private void ShowValidationError(string message, Control focusTarget)
        {
            _lblValidation.Text = "⚠️  " + message;
            _lblValidation.Visible = true;
            focusTarget.Focus();
            if (focusTarget is TextBox tb) tb.SelectAll();
            System.Media.SystemSounds.Beep.Play();
        }

        public bool ValidateInputs(out string errorMessage, out Control? focusControl)
        {
            errorMessage = string.Empty;
            focusControl = null;

            string customerName = _cmbCustomer.Text.Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                errorMessage = "Please enter or select a customer name.";
                focusControl = _cmbCustomer;
                return false;
            }

            string address = _txtAddress.Text.Trim();
            if (string.IsNullOrWhiteSpace(address))
            {
                errorMessage = "Please enter a valid city or delivery address.";
                focusControl = _txtAddress;
                return false;
            }

            string rawPrice = _txtPrice.Text.Trim().Replace("₹", "").Replace("$", "").Replace(",", "").Trim();
            if (string.IsNullOrWhiteSpace(rawPrice))
            {
                errorMessage = "Please enter the order price.";
                focusControl = _txtPrice;
                return false;
            }

            if (!decimal.TryParse(rawPrice, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal priceVal) &&
                !decimal.TryParse(rawPrice, out priceVal))
            {
                errorMessage = "Order price must be a valid numeric value.";
                focusControl = _txtPrice;
                return false;
            }

            if (priceVal <= 0)
            {
                errorMessage = "Order price must be a positive amount greater than zero.";
                focusControl = _txtPrice;
                return false;
            }

            return true;
        }

        private async Task SaveOrderAsync()
        {
            if (_isSaving) return;

            if (!ValidateInputs(out string error, out Control? focusCtrl))
            {
                if (focusCtrl != null) ShowValidationError(error, focusCtrl);
                return;
            }

            _isSaving = true;
            _btnSave.Enabled = false;
            _btnSave.Text = "Saving...";
            Cursor = Cursors.WaitCursor;

            try
            {
                string name = _cmbCustomer.Text.Trim();
                string address = _txtAddress.Text.Trim();
                string status = _cmbStatus.SelectedItem?.ToString() ?? "Processed";
                string dateStr = _dtpOrderDate.Value.ToString("dd.MM.yyyy");

                string rawPrice = _txtPrice.Text.Trim().Replace("₹", "").Replace("$", "").Replace(",", "").Trim();
                decimal.TryParse(rawPrice, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal priceVal);
                string formattedPrice = "₹" + priceVal.ToString("N0", new CultureInfo("en-IN"));

                var order = new CustomerOrder(name, address, dateStr, status, formattedPrice);

                bool saved = await _dbService.AddOrderAsync(order);
                if (saved)
                {
                    CreatedOrder = order;

                    using (var successDlg = new OrderSuccessDialog(order))
                    {
                        successDlg.ShowDialog(this);
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    ShowValidationError("Failed to save order to the database. Please try again.", _btnSave);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"An error occurred while saving the customer order:\n{ex.Message}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                _btnSave.Enabled = true;
                _btnSave.Text = "Save Order";
                Cursor = Cursors.Default;
            }
        }

        private void CancelDialog()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ClearFields()
        {
            _cmbCustomer.Text = string.Empty;
            _cmbCustomer.SelectedIndex = -1;
            _txtAddress.Text = string.Empty;
            _txtPrice.Text = string.Empty;
            _cmbStatus.SelectedIndex = 0; // "Processed"
            _dtpOrderDate.Value = DateTime.Today;
            ClearValidationError();
            _cmbCustomer.Focus();
        }
    }
}
