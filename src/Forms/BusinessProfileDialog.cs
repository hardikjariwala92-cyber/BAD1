using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public class BusinessProfileDialog : Form
    {
        private readonly BillingDataService _billingService = BillingDataService.Instance;
        private BusinessProfile _profile = new();

        private TextBox _txtName = null!;
        private TextBox _txtTagline = null!;
        private TextBox _txtAddress = null!;
        private TextBox _txtCity = null!;
        private TextBox _txtCountry = null!;
        private TextBox _txtPhone = null!;
        private TextBox _txtEmail = null!;
        private TextBox _txtGSTIN = null!;
        private TextBox _txtBankName = null!;
        private TextBox _txtAccountNo = null!;
        private TextBox _txtIFSC = null!;
        private TextBox _txtUPI = null!;
        private TextBox _txtTerms = null!;

        public BusinessProfileDialog()
        {
            Text = "Business Profile & Invoice Settings";
            Size = new Size(680, 720);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            BuildUI();
            LoadProfile();
        }

        private void BuildUI()
        {
            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(25, 20, 25, 20)
            };
            Controls.Add(pnlContainer);

            var lblHeader = new Label
            {
                Text = "🏢  Business & Invoice Branding Settings",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlContainer.Controls.Add(lblHeader);

            int y = 55;

            // Business Details Group
            _txtName = AddLabeledInput("Business / Brand Name *:", y, pnlContainer); y += 60;
            _txtTagline = AddLabeledInput("Tagline / Subtitle:", y, pnlContainer); y += 60;
            _txtAddress = AddLabeledInput("Business Address:", y, pnlContainer); y += 60;
            _txtCity = AddLabeledInput("City & State:", y, pnlContainer); y += 60;
            _txtCountry = AddLabeledInput("Country:", y, pnlContainer); y += 60;
            _txtPhone = AddLabeledInput("Phone Number:", y, pnlContainer); y += 60;
            _txtEmail = AddLabeledInput("Email Address:", y, pnlContainer); y += 60;
            _txtGSTIN = AddLabeledInput("GSTIN (Tax Identification Number):", y, pnlContainer); y += 60;

            // Bank Details Group
            var lblBankHeader = new Label
            {
                Text = "🏦  Bank & Payment Details (Printed on Invoices)",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                Location = new Point(20, y + 10),
                AutoSize = true
            };
            pnlContainer.Controls.Add(lblBankHeader);
            y += 40;

            _txtBankName = AddLabeledInput("Bank Name:", y, pnlContainer); y += 60;
            _txtAccountNo = AddLabeledInput("Bank Account Number:", y, pnlContainer); y += 60;
            _txtIFSC = AddLabeledInput("IFSC Code:", y, pnlContainer); y += 60;
            _txtUPI = AddLabeledInput("UPI ID / VPA:", y, pnlContainer); y += 60;

            // Default Terms
            var lblTerms = new Label
            {
                Text = "Default Terms & Conditions:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(20, y),
                AutoSize = true
            };
            pnlContainer.Controls.Add(lblTerms);

            _txtTerms = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(615, 75),
                Location = new Point(20, y + 22),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlContainer.Controls.Add(_txtTerms);
            y += 110;

            // Buttons
            var btnSave = new ModernButton
            {
                Text = "💾 Save Settings",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(180, 40),
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BorderRadius = 10
            };
            pnlContainer.Controls.Add(btnSave);

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
            pnlContainer.Controls.Add(btnCancel);
            btnCancel.Click += (s, e) => Close();

            btnSave.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtName.Text))
                {
                    MessageBox.Show("Business Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _profile.BusinessName = _txtName.Text.Trim();
                _profile.Tagline = _txtTagline.Text.Trim();
                _profile.Address = _txtAddress.Text.Trim();
                _profile.City = _txtCity.Text.Trim();
                _profile.Country = _txtCountry.Text.Trim();
                _profile.Phone = _txtPhone.Text.Trim();
                _profile.Email = _txtEmail.Text.Trim();
                _profile.GSTIN = _txtGSTIN.Text.Trim();
                _profile.BankName = _txtBankName.Text.Trim();
                _profile.AccountNumber = _txtAccountNo.Text.Trim();
                _profile.IFSC = _txtIFSC.Text.Trim();
                _profile.UPIId = _txtUPI.Text.Trim();
                _profile.DefaultTerms = _txtTerms.Text.Trim();

                await _billingService.SaveBusinessProfileAsync(_profile);
                MessageBox.Show("Business profile and invoice settings saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            };
        }

        private async void LoadProfile()
        {
            _profile = await _billingService.GetBusinessProfileAsync();
            _txtName.Text = _profile.BusinessName;
            _txtTagline.Text = _profile.Tagline;
            _txtAddress.Text = _profile.Address;
            _txtCity.Text = _profile.City;
            _txtCountry.Text = _profile.Country;
            _txtPhone.Text = _profile.Phone;
            _txtEmail.Text = _profile.Email;
            _txtGSTIN.Text = _profile.GSTIN;
            _txtBankName.Text = _profile.BankName;
            _txtAccountNo.Text = _profile.AccountNumber;
            _txtIFSC.Text = _profile.IFSC;
            _txtUPI.Text = _profile.UPIId;
            _txtTerms.Text = _profile.DefaultTerms;
        }

        private TextBox AddLabeledInput(string label, int y, Panel parent)
        {
            var lbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(20, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);

            var txt = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Size = new Size(615, 30),
                Location = new Point(20, y + 22),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }
    }
}
