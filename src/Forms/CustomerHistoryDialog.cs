using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public class CustomerHistoryDialog : Form
    {
        private readonly Customer _customer;
        private readonly BillingDataService _billingService = BillingDataService.Instance;
        private DataGridView _gridInvoices = null!;
        private Label _lblBalance = null!;

        public CustomerHistoryDialog(Customer customer)
        {
            _customer = customer;

            Text = $"Customer History — {customer.Name}";
            Size = new Size(950, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            // Header Card
            var pnlHeader = new RoundedPanel
            {
                Location = new Point(20, 15),
                Size = new Size(895, 110),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            Controls.Add(pnlHeader);

            var lblName = new Label
            {
                Text = $"👤  {_customer.Name}  {(!string.IsNullOrWhiteSpace(_customer.CompanyName) ? $"({_customer.CompanyName})" : "")}",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 14),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblName);

            var lblDetails = new Label
            {
                Text = $"Email: {_customer.Email}   |   Phone: {_customer.Phone}   |   City: {_customer.City}, {_customer.Country}   |   GSTIN: {(!string.IsNullOrWhiteSpace(_customer.GSTIN) ? _customer.GSTIN : "N/A")}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(16, 42),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblDetails);

            _lblBalance = new Label
            {
                Text = "Outstanding Balance: Calculating...",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68),
                Location = new Point(16, 72),
                AutoSize = true
            };
            pnlHeader.Controls.Add(_lblBalance);

            // Invoices Table Card
            var pnlGrid = new RoundedPanel
            {
                Location = new Point(20, 135),
                Size = new Size(895, 385),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            Controls.Add(pnlGrid);

            var lblGridTitle = new Label
            {
                Text = "Invoice & Billing History",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true
            };
            pnlGrid.Controls.Add(lblGridTitle);

            _gridInvoices = new DataGridView
            {
                Location = new Point(16, 42),
                Size = new Size(863, 325),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9f),
                RowTemplate = { Height = 34 },
                BackgroundColor = ThemeManager.CardBg,
                GridColor = ThemeManager.CardBorder
            };

            _gridInvoices.DefaultCellStyle.BackColor = ThemeManager.CardBg;
            _gridInvoices.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            _gridInvoices.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TableHeaderBg;
            _gridInvoices.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            _gridInvoices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _gridInvoices.ColumnHeadersHeight = 36;

            _gridInvoices.Columns.Add("InvoiceNo", "Invoice #");
            _gridInvoices.Columns.Add("Date", "Date");
            _gridInvoices.Columns.Add("DueDate", "Due Date");
            _gridInvoices.Columns.Add("Total", "Grand Total (₹)");
            _gridInvoices.Columns.Add("Paid", "Paid (₹)");
            _gridInvoices.Columns.Add("Balance", "Balance (₹)");
            _gridInvoices.Columns.Add("Status", "Status");

            _gridInvoices.Columns[0].Width = 130;
            _gridInvoices.Columns[1].Width = 100;
            _gridInvoices.Columns[2].Width = 100;
            _gridInvoices.Columns[3].Width = 120;
            _gridInvoices.Columns[4].Width = 120;
            _gridInvoices.Columns[5].Width = 120;
            _gridInvoices.Columns[6].Width = 120;

            _gridInvoices.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 6 && e.Value != null)
                {
                    string status = e.Value.ToString() ?? "";
                    e.CellStyle!.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle!.ForeColor = status switch
                    {
                        "Paid" => Color.FromArgb(16, 185, 129),
                        "Partially Paid" => Color.FromArgb(245, 158, 11),
                        "Overdue" => Color.FromArgb(239, 68, 68),
                        "Cancelled" => Color.FromArgb(100, 116, 139),
                        _ => Color.FromArgb(249, 115, 22)
                    };
                }
            };

            pnlGrid.Controls.Add(_gridInvoices);

            // Close button
            var btnClose = new ModernButton
            {
                Text = "Close",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(120, 36),
                Location = new Point(795, 530),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 10
            };
            Controls.Add(btnClose);
            btnClose.Click += (s, e) => Close();
        }

        private async void LoadData()
        {
            var invoices = await _billingService.GetCustomerInvoicesAsync(_customer.CustomerId);
            decimal balance = await _billingService.GetCustomerOutstandingBalanceAsync(_customer.CustomerId);

            _lblBalance.Text = $"Outstanding Balance: ₹{balance:N2}";
            _lblBalance.ForeColor = balance > 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129);

            _gridInvoices.Rows.Clear();
            foreach (var inv in invoices)
            {
                _gridInvoices.Rows.Add(
                    inv.InvoiceNumber,
                    inv.InvoiceDate.ToString("dd MMM yyyy"),
                    inv.DueDate.ToString("dd MMM yyyy"),
                    $"₹{inv.GrandTotal:N2}",
                    $"₹{inv.AmountPaid:N2}",
                    $"₹{inv.BalanceDue:N2}",
                    inv.Status
                );
            }
        }
    }
}
