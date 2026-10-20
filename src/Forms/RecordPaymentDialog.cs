using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public class RecordPaymentDialog : Form
    {
        private readonly Invoice _invoice;
        private readonly BillingDataService _billingService = BillingDataService.Instance;

        private TextBox _txtAmount = null!;
        private ComboBox _cmbMethod = null!;
        private TextBox _txtRef = null!;
        private TextBox _txtNotes = null!;
        private DateTimePicker _dtpDate = null!;

        public RecordPaymentDialog(Invoice invoice)
        {
            _invoice = invoice;

            Text = $"Record Payment — {invoice.InvoiceNumber}";
            Size = new Size(500, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            BuildUI();
        }

        private void BuildUI()
        {
            // Header Panel
            var pnlHeader = new RoundedPanel
            {
                Location = new Point(20, 15),
                Size = new Size(445, 115),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            Controls.Add(pnlHeader);

            var lblTitle = new Label
            {
                Text = "💳  Record Payment",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(14, 12),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            var lblCustomer = new Label
            {
                Text = $"Customer: {_invoice.CustomerName} ({_invoice.CompanyName})",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(14, 38),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblCustomer);

            var lblFinancials = new Label
            {
                Text = $"Total: ₹{_invoice.GrandTotal:N2}   |   Paid: ₹{_invoice.AmountPaid:N2}   |   Balance: ₹{_invoice.BalanceDue:N2}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _invoice.BalanceDue > 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129),
                Location = new Point(14, 62),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblFinancials);

            int y = 145;

            // Payment Amount
            CreateLabel("Payment Amount (₹) *:", y);
            _txtAmount = new TextBox
            {
                Text = _invoice.BalanceDue.ToString("F2"),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Size = new Size(445, 30),
                Location = new Point(20, y + 22),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.AccentBlue,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(_txtAmount);
            y += 62;

            // Payment Date
            CreateLabel("Payment Date:", y);
            _dtpDate = new DateTimePicker
            {
                Value = DateTime.Now,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10f),
                Size = new Size(445, 28),
                Location = new Point(20, y + 22)
            };
            Controls.Add(_dtpDate);
            y += 62;

            // Payment Method
            CreateLabel("Payment Method:", y);
            _cmbMethod = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f),
                Size = new Size(445, 30),
                Location = new Point(20, y + 22),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary
            };
            _cmbMethod.Items.AddRange(new object[] { "UPI", "Bank Transfer", "Cash", "Card", "Cheque", "Other" });
            _cmbMethod.SelectedIndex = 0;
            Controls.Add(_cmbMethod);
            y += 62;

            // Reference Number
            CreateLabel("Reference / Transaction ID / UTR:", y);
            _txtRef = new TextBox
            {
                PlaceholderText = "e.g. UPI/20261014/12345 or HDFC/NEFT/009182",
                Font = new Font("Segoe UI", 10f),
                Size = new Size(445, 30),
                Location = new Point(20, y + 22),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(_txtRef);
            y += 62;

            // Notes
            CreateLabel("Notes / Remarks:", y);
            _txtNotes = new TextBox
            {
                PlaceholderText = "Optional notes or remarks regarding this transaction...",
                Font = new Font("Segoe UI", 10f),
                Size = new Size(445, 30),
                Location = new Point(20, y + 22),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(_txtNotes);
            y += 68;

            // Action Buttons
            var btnSave = new ModernButton
            {
                Text = "💾 Save Payment",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(180, 40),
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BorderRadius = 10
            };
            Controls.Add(btnSave);

            var btnCancel = new ModernButton
            {
                Text = "Cancel",
                NormalColor = Color.FromArgb(80, 85, 100),
                HoverColor = Color.FromArgb(100, 105, 120),
                Size = new Size(130, 40),
                Location = new Point(215, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BorderRadius = 10
            };
            Controls.Add(btnCancel);
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave.Click += async (s, e) =>
            {
                if (!decimal.TryParse(_txtAmount.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Please enter a valid positive payment amount greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtAmount.Focus();
                    return;
                }

                if (amount > _invoice.BalanceDue)
                {
                    MessageBox.Show(
                        $"Payment amount (₹{amount:N2}) cannot exceed the current outstanding balance due of ₹{_invoice.BalanceDue:N2}.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtAmount.Focus();
                    return;
                }

                btnSave.Enabled = false;
                try
                {
                    var payment = new Payment(
                        _invoice.InvoiceId,
                        _invoice.InvoiceNumber,
                        _invoice.CustomerName,
                        amount,
                        _cmbMethod.SelectedItem?.ToString() ?? "UPI",
                        _txtRef.Text.Trim(),
                        _txtNotes.Text.Trim()
                    )
                    {
                        PaymentDate = _dtpDate.Value
                    };

                    await _billingService.RecordPaymentAsync(payment);

                    // Synchronize in-memory invoice entity
                    _invoice.Payments.Add(payment);
                    _invoice.AmountPaid += amount;
                    _invoice.BalanceDue = Math.Max(0m, _invoice.GrandTotal - _invoice.AmountPaid);
                    _invoice.RecalculateTotals();

                    MessageBox.Show($"Payment of ₹{amount:N2} recorded successfully for invoice {_invoice.InvoiceNumber}!", "Payment Recorded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to record payment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnSave.Enabled = true;
                }
            };
        }

        private void CreateLabel(string text, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(lbl);
        }
    }
}
