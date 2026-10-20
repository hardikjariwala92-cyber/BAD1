using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public class InvoiceForm : Form
    {
        private readonly BillingDataService _billingService = BillingDataService.Instance;
        private readonly InvoicePdfService _pdfService = InvoicePdfService.Instance;
        private Invoice _invoice;
        private BusinessProfile _profile = new();
        private List<Customer> _existingCustomers = new();
        private bool _isSaving = false;

        // Form Controls
        private TextBox _txtInvoiceNo = null!;
        private DateTimePicker _dtpInvoiceDate = null!;
        private DateTimePicker _dtpDueDate = null!;
        private ComboBox _cmbTerms = null!;
        private ComboBox _cmbStatus = null!;
        private CheckBox _chkInterState = null!;

        // Customer Controls
        private ComboBox _cmbCustomers = null!;
        private TextBox _txtCustName = null!;
        private TextBox _txtCompName = null!;
        private TextBox _txtCustPhone = null!;
        private TextBox _txtCustEmail = null!;
        private TextBox _txtCustAddress = null!;
        private TextBox _txtCustGSTIN = null!;

        // Items Grid
        private DataGridView _gridItems = null!;

        // Calculation Summary Labels
        private Label _lblSubTotal = null!;
        private Label _lblItemDiscount = null!;
        private TextBox _txtOverallDiscount = null!;
        private Label _lblTaxableAmount = null!;
        private Label _lblTaxDetails = null!;
        private Label _lblGrandTotal = null!;
        private Label _lblAmountPaid = null!;
        private Label _lblBalanceDue = null!;

        // Notes and Terms
        private TextBox _txtNotes = null!;
        private TextBox _txtTerms = null!;

        // Panels & Layout Controls for Dynamic Full-Screen Responsiveness
        private Panel _pnlMain = null!;
        private RoundedPanel _headerPanel = null!;
        private RoundedPanel _cardInvoice = null!;
        private RoundedPanel _cardCust = null!;
        private RoundedPanel _cardItems = null!;
        private RoundedPanel _cardNotes = null!;
        private RoundedPanel _cardSummary = null!;
        private RoundedPanel _pnlGrandTotal = null!;
        private ModernButton _btnSave = null!;
        private ModernButton _btnRecordPayment = null!;
        private ModernButton _btnPreview = null!;
        private ModernButton _btnExport = null!;
        private ModernButton _btnClose = null!;
        private ModernButton _btnNewCust = null!;
        private ModernButton _btnAddItem = null!;
        private ModernButton _btnScanBarcode = null!;
        private ModernButton _btnRemoveItem = null!;

        // Right column label references for responsive alignment
        private Label? _lblDueDateLabel;
        private Label? _lblStatusLabel;
        private Label? _lblCompNameLabel;
        private Label? _lblCustEmailLabel;
        private Label? _lblCustGSTINLabel;

        public InvoiceForm(Invoice? existingInvoice = null)
        {
            _invoice = existingInvoice ?? new Invoice();

            Text = _invoice.InvoiceId > 0 ? $"Edit Invoice — {_invoice.InvoiceNumber}" : "Create New Invoice — S TOM'S Platform";
            Size = new Size(1180, 850);
            MinimumSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            BuildUI();
            LoadInitialData();
        }

        private void BuildUI()
        {
            _pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(25, 20, 25, 25)
            };
            Controls.Add(_pnlMain);
            _pnlMain.Resize += (s, e) => UpdateInvoiceLayout();

            // 1. TOP HEADER & ACTION BUTTONS
            int y = 10;
            _headerPanel = new RoundedPanel
            {
                Location = new Point(0, y),
                Size = new Size(1110, 65),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _pnlMain.Controls.Add(_headerPanel);

            var lblHeader = new Label
            {
                Text = _invoice.InvoiceId > 0 ? $"✏️  Edit Invoice: {_invoice.InvoiceNumber}" : "🧾  Create New Service Invoice",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 18),
                AutoSize = true
            };
            _headerPanel.Controls.Add(lblHeader);

            // Action Buttons on Top Right
            int btnX = 1110 - 720;
            _btnSave = new ModernButton
            {
                Text = "💾 Save Invoice",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(130, 34),
                Location = new Point(btnX, 15),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8
            };
            _headerPanel.Controls.Add(_btnSave);
            _btnSave.Click += async (s, e) => await SaveInvoiceWorkflowAsync(showSuccessPrompt: true);
            btnX += 140;

            _btnRecordPayment = new ModernButton
            {
                Text = "💳 Record Payment",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(140, 34),
                Location = new Point(btnX, 15),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8
            };
            _headerPanel.Controls.Add(_btnRecordPayment);
            _btnRecordPayment.Click += async (s, e) =>
            {
                if (_invoice.InvoiceId <= 0)
                {
                    bool saved = await SaveInvoiceWorkflowAsync(showSuccessPrompt: false);
                    if (!saved) return;
                }

                if (_invoice.BalanceDue <= 0)
                {
                    MessageBox.Show("This invoice has no outstanding balance.", "Payment Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var payDlg = new RecordPaymentDialog(_invoice);
                if (payDlg.ShowDialog(this) == DialogResult.OK)
                {
                    var updated = await _billingService.GetInvoiceByIdAsync(_invoice.InvoiceId);
                    if (updated != null)
                    {
                        _invoice.Payments = updated.Payments;
                        _invoice.AmountPaid = updated.AmountPaid;
                        _invoice.BalanceDue = updated.BalanceDue;
                        _invoice.Status = updated.Status;
                        _cmbStatus.SelectedItem = updated.Status;
                        RecalculateLiveSummary();
                    }
                }
            };
            btnX += 150;

            _btnPreview = new ModernButton
            {
                Text = "👁️ Preview PDF",
                NormalColor = ThemeManager.AccentPurple,
                HoverColor = Color.FromArgb(200, 100, 255),
                Size = new Size(125, 34),
                Location = new Point(btnX, 15),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8
            };
            _headerPanel.Controls.Add(_btnPreview);
            _btnPreview.Click += async (s, e) => await PreviewPdfWorkflowAsync();
            btnX += 135;

            _btnExport = new ModernButton
            {
                Text = "📄 Export PDF",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(120, 34),
                Location = new Point(btnX, 15),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8
            };
            _headerPanel.Controls.Add(_btnExport);
            _btnExport.Click += async (s, e) => await ExportPdfWorkflowAsync();
            btnX += 130;

            _btnClose = new ModernButton
            {
                Text = "Close",
                NormalColor = Color.FromArgb(80, 85, 100),
                HoverColor = Color.FromArgb(100, 105, 120),
                Size = new Size(90, 34),
                Location = new Point(btnX, 15),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8
            };
            _headerPanel.Controls.Add(_btnClose);
            _btnClose.Click += (s, e) => Close();

            y += 75;

            // 2. METADATA: Invoice Details (Left: 545px) & Customer Details (Right: 545px)
            int colW = 545;
            int gap = 20;

            // --- INVOICE DETAILS CARD ---
            _cardInvoice = new RoundedPanel
            {
                Location = new Point(0, y),
                Size = new Size(colW, 250),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _pnlMain.Controls.Add(_cardInvoice);

            var lblInvTitle = new Label
            {
                Text = "📄  Invoice Information",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true
            };
            _cardInvoice.Controls.Add(lblInvTitle);

            // Row 1: Invoice # and Inter-State Tax Checkbox
            CreateFieldLabel("Invoice Number *:", 16, 42, _cardInvoice);
            _txtInvoiceNo = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(240, 28),
                Location = new Point(16, 62),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.AccentBlue,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardInvoice.Controls.Add(_txtInvoiceNo);

            _chkInterState = new CheckBox
            {
                Text = "Inter-State (IGST 18%)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(280, 62),
                AutoSize = true
            };
            _chkInterState.CheckedChanged += (s, e) => RecalculateLiveSummary();
            _cardInvoice.Controls.Add(_chkInterState);

            // Row 2: Invoice Date & Due Date
            CreateFieldLabel("Invoice Date:", 16, 100, _cardInvoice);
            _dtpInvoiceDate = new DateTimePicker
            {
                Value = _invoice.InvoiceDate,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(240, 26),
                Location = new Point(16, 120)
            };
            _cardInvoice.Controls.Add(_dtpInvoiceDate);

            _lblDueDateLabel = CreateFieldLabel("Due Date:", 280, 100, _cardInvoice);
            _dtpDueDate = new DateTimePicker
            {
                Value = _invoice.DueDate,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(240, 26),
                Location = new Point(280, 120)
            };
            _cardInvoice.Controls.Add(_dtpDueDate);

            // Row 3: Payment Terms & Status
            CreateFieldLabel("Payment Terms:", 16, 160, _cardInvoice);
            _cmbTerms = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(240, 28),
                Location = new Point(16, 180),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary
            };
            _cmbTerms.Items.AddRange(new object[] { "Due on Receipt", "Net 15", "Net 30", "Net 45", "Net 60" });
            _cmbTerms.SelectedItem = _invoice.PaymentTerms;
            _cmbTerms.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbTerms.SelectedItem?.ToString() == "Net 15") _dtpDueDate.Value = _dtpInvoiceDate.Value.AddDays(15);
                else if (_cmbTerms.SelectedItem?.ToString() == "Net 30") _dtpDueDate.Value = _dtpInvoiceDate.Value.AddDays(30);
                else if (_cmbTerms.SelectedItem?.ToString() == "Net 60") _dtpDueDate.Value = _dtpInvoiceDate.Value.AddDays(60);
                else if (_cmbTerms.SelectedItem?.ToString() == "Due on Receipt") _dtpDueDate.Value = _dtpInvoiceDate.Value;
            };
            _cardInvoice.Controls.Add(_cmbTerms);

            _lblStatusLabel = CreateFieldLabel("Invoice Status:", 280, 160, _cardInvoice);
            _cmbStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(240, 28),
                Location = new Point(280, 180),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary
            };
            _cmbStatus.Items.AddRange(new object[] { "Unpaid", "Partially Paid", "Paid", "Draft", "Overdue", "Cancelled" });
            _cmbStatus.SelectedItem = _invoice.Status;
            _cardInvoice.Controls.Add(_cmbStatus);

            // --- CUSTOMER DETAILS CARD ---
            _cardCust = new RoundedPanel
            {
                Location = new Point(colW + gap, y),
                Size = new Size(colW, 250),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _pnlMain.Controls.Add(_cardCust);

            var lblCustTitle = new Label
            {
                Text = "👤  Customer Details",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true
            };
            _cardCust.Controls.Add(lblCustTitle);

            // Select Existing Customer Dropdown + Quick Add Button
            CreateFieldLabel("Select Saved Customer:", 16, 42, _cardCust);
            _cmbCustomers = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(360, 28),
                Location = new Point(16, 62),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary
            };
            _cmbCustomers.SelectedIndexChanged += CustomerSelectedFromDropdown;
            _cardCust.Controls.Add(_cmbCustomers);

            _btnNewCust = new ModernButton
            {
                Text = "+ New",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(130, 28),
                Location = new Point(390, 62),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BorderRadius = 6
            };
            _cardCust.Controls.Add(_btnNewCust);
            _btnNewCust.Click += QuickAddNewCustomer_Click;

            // Row 2: Customer Name & Company Name
            CreateFieldLabel("Customer Name *:", 16, 100, _cardCust);
            _txtCustName = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(240, 26),
                Location = new Point(16, 120),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardCust.Controls.Add(_txtCustName);

            _lblCompNameLabel = CreateFieldLabel("Company Name:", 280, 100, _cardCust);
            _txtCompName = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(240, 26),
                Location = new Point(280, 120),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardCust.Controls.Add(_txtCompName);

            // Row 3: Phone & Email
            CreateFieldLabel("Phone Number:", 16, 155, _cardCust);
            _txtCustPhone = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(160, 26),
                Location = new Point(16, 175),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardCust.Controls.Add(_txtCustPhone);

            _lblCustEmailLabel = CreateFieldLabel("Email Address:", 190, 155, _cardCust);
            _txtCustEmail = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(180, 26),
                Location = new Point(190, 175),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardCust.Controls.Add(_txtCustEmail);

            _lblCustGSTINLabel = CreateFieldLabel("GSTIN (Optional):", 385, 155, _cardCust);
            _txtCustGSTIN = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(135, 26),
                Location = new Point(385, 175),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardCust.Controls.Add(_txtCustGSTIN);

            // Row 4: Billing Address
            CreateFieldLabel("Billing Address:", 16, 205, _cardCust);
            _txtCustAddress = new TextBox
            {
                Font = new Font("Segoe UI", 9f),
                Size = new Size(504, 24),
                Location = new Point(16, 222),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardCust.Controls.Add(_txtCustAddress);

            y += 265;

            // 3. ITEM & SERVICES SECTION
            _cardItems = new RoundedPanel
            {
                Location = new Point(0, y),
                Size = new Size(1110, 290),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _pnlMain.Controls.Add(_cardItems);

            var lblItemsTitle = new Label
            {
                Text = "📦  Itemized Services & Software Products",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true
            };
            _cardItems.Controls.Add(lblItemsTitle);

            // Add, Scan & Remove Item buttons
            _btnAddItem = new ModernButton
            {
                Text = "+ Add Service / Item",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                PressedColor = Color.FromArgb(10, 160, 110),
                Size = new Size(165, 30),
                Location = new Point(625, 10),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 6,
                ForeColor = Color.White
            };
            _cardItems.Controls.Add(_btnAddItem);
            _btnAddItem.Click += (s, e) => ShowManualAddItemDialog();

            _btnScanBarcode = new ModernButton
            {
                Text = "📷 Scan Barcode",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(79, 70, 229),
                PressedColor = Color.FromArgb(67, 56, 202),
                Size = new Size(150, 30),
                Location = new Point(798, 10),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 6,
                ForeColor = Color.White
            };
            _cardItems.Controls.Add(_btnScanBarcode);
            _btnScanBarcode.Click += (s, e) => ShowBarcodeScannerDialog();

            _btnRemoveItem = new ModernButton
            {
                Text = "🗑️ Remove Item",
                NormalColor = Color.FromArgb(220, 53, 69),
                HoverColor = Color.FromArgb(240, 70, 85),
                PressedColor = Color.FromArgb(180, 40, 55),
                Size = new Size(130, 30),
                Location = new Point(955, 10),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 6,
                ForeColor = Color.White
            };
            _cardItems.Controls.Add(_btnRemoveItem);
            _btnRemoveItem.Click += (s, e) => RemoveSelectedItemRow();

            // DataGridView for Invoice Items
            _gridItems = new DataGridView
            {
                Location = new Point(16, 48),
                Size = new Size(1078, 225),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9.5f),
                RowTemplate = { Height = 34 },
                BackgroundColor = ThemeManager.CardBg,
                GridColor = ThemeManager.CardBorder
            };

            _gridItems.DefaultCellStyle.BackColor = ThemeManager.CardBg;
            _gridItems.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            _gridItems.DefaultCellStyle.SelectionBackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(45, 50, 75) : Color.FromArgb(230, 235, 255);
            _gridItems.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            _gridItems.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TableHeaderBg;
            _gridItems.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            _gridItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _gridItems.ColumnHeadersHeight = 36;

            // Columns definition
            _gridItems.Columns.Add("Service", "Service / Product Name *");
            _gridItems.Columns.Add("Description", "Description");
            _gridItems.Columns.Add("Quantity", "Qty");
            _gridItems.Columns.Add("UnitPrice", "Rate (₹)");
            _gridItems.Columns.Add("Discount", "Disc %");
            _gridItems.Columns.Add("Tax", "Tax %");
            _gridItems.Columns.Add("LineTotal", "Line Total (₹)");

            _gridItems.Columns[0].Width = 240;
            _gridItems.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridItems.Columns[1].MinimumWidth = 250;
            _gridItems.Columns[2].Width = 75;
            _gridItems.Columns[3].Width = 120;
            _gridItems.Columns[4].Width = 90;
            _gridItems.Columns[5].Width = 90;
            _gridItems.Columns[6].Width = 140;
            _gridItems.Columns[6].ReadOnly = true; // Auto-calculated!
            _gridItems.ScrollBars = ScrollBars.Vertical;

            // Trigger real-time calculation whenever cell values change
            _gridItems.CellValueChanged += (s, e) => RecalculateLiveSummary();
            _gridItems.RowsRemoved += (s, e) => RecalculateLiveSummary();

            _cardItems.Controls.Add(_gridItems);
            y += 305;

            // 4. BOTTOM SECTION: Notes & Terms & Live Financial Summary
            _cardNotes = new RoundedPanel
            {
                Location = new Point(0, y),
                Size = new Size(590, 220),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _pnlMain.Controls.Add(_cardNotes);

            var lblNotesTitle = new Label
            {
                Text = "📝  Notes & Payment Instructions",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true
            };
            _cardNotes.Controls.Add(lblNotesTitle);

            _txtNotes = new TextBox
            {
                PlaceholderText = "Add private notes, project milestones, or payment reference instructions...",
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9f),
                Size = new Size(558, 65),
                Location = new Point(16, 36),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardNotes.Controls.Add(_txtNotes);

            var lblTermsTitle = new Label
            {
                Text = "📜  Terms & Conditions",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 110),
                AutoSize = true
            };
            _cardNotes.Controls.Add(lblTermsTitle);

            _txtTerms = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9f),
                Size = new Size(558, 75),
                Location = new Point(16, 134),
                BackColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _cardNotes.Controls.Add(_txtTerms);

            // --- FINANCIAL SUMMARY CARD (LIVE BILL CALCULATION) ---
            _cardSummary = new RoundedPanel
            {
                Location = new Point(610, y),
                Size = new Size(500, 220),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _pnlMain.Controls.Add(_cardSummary);

            int sumY = 12;
            _lblSubTotal = AddSummaryRow("Subtotal:", "₹0.00", ref sumY, _cardSummary);
            _lblItemDiscount = AddSummaryRow("Item Discounts:", "₹0.00", ref sumY, _cardSummary);

            // Overall Discount (Editable input)
            var lblOverall = new Label
            {
                Text = "Overall Discount (₹):",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(16, sumY + 3),
                AutoSize = true
            };
            _cardSummary.Controls.Add(lblOverall);

            _txtOverallDiscount = new TextBox
            {
                Text = "0.00",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(110, 24),
                Location = new Point(365, sumY),
                BackColor = ThemeManager.Background,
                ForeColor = Color.FromArgb(16, 185, 129),
                TextAlign = HorizontalAlignment.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtOverallDiscount.TextChanged += (s, e) => RecalculateLiveSummary();
            _cardSummary.Controls.Add(_txtOverallDiscount);
            sumY += 28;

            _lblTaxableAmount = AddSummaryRow("Taxable Amount:", "₹0.00", ref sumY, _cardSummary);
            _lblTaxDetails = AddSummaryRow("GST (CGST+SGST):", "₹0.00", ref sumY, _cardSummary);

            // Grand Total (Highlighted Pill)
            _pnlGrandTotal = new RoundedPanel
            {
                Location = new Point(16, sumY),
                Size = new Size(468, 38),
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(40, 48, 75) : Color.FromArgb(235, 242, 255),
                CustomBorderColor = ThemeManager.AccentBlue
            };
            _cardSummary.Controls.Add(_pnlGrandTotal);

            var lblGrandTitle = new Label
            {
                Text = "GRAND TOTAL:",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                Location = new Point(12, 9),
                AutoSize = true
            };
            _pnlGrandTotal.Controls.Add(lblGrandTitle);

            _lblGrandTotal = new Label
            {
                Text = "₹0.00",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                Location = new Point(250, 7),
                Size = new Size(205, 26),
                TextAlign = ContentAlignment.MiddleRight
            };
            _pnlGrandTotal.Controls.Add(_lblGrandTotal);
            sumY += 46;

            // Paid & Balance Due
            _lblAmountPaid = AddSummaryRow("Amount Paid:", "₹0.00", ref sumY, _cardSummary, isBold: true, color: Color.FromArgb(16, 185, 129));
            _lblBalanceDue = AddSummaryRow("Balance Due:", "₹0.00", ref sumY, _cardSummary, isBold: true, color: Color.FromArgb(239, 68, 68));

            UpdateInvoiceLayout();
        }

        private async void LoadInitialData()
        {
            _profile = await _billingService.GetBusinessProfileAsync();
            _existingCustomers = await _billingService.GetAllCustomersAsync();

            _cmbCustomers.Items.Clear();
            _cmbCustomers.Items.Add("-- Select Existing Customer --");
            foreach (var c in _existingCustomers)
            {
                string display = string.IsNullOrWhiteSpace(c.CompanyName) ? c.Name : $"{c.Name} ({c.CompanyName})";
                _cmbCustomers.Items.Add(display);
            }
            _cmbCustomers.SelectedIndex = 0;

            if (_invoice.InvoiceId > 0)
            {
                // Editing existing invoice
                _txtInvoiceNo.Text = _invoice.InvoiceNumber;
                _txtInvoiceNo.ReadOnly = true;
                _dtpInvoiceDate.Value = _invoice.InvoiceDate;
                _dtpDueDate.Value = _invoice.DueDate;
                _cmbTerms.SelectedItem = _invoice.PaymentTerms;
                _cmbStatus.SelectedItem = _invoice.Status;
                _chkInterState.Checked = _invoice.IsInterState;

                _txtCustName.Text = _invoice.CustomerName;
                _txtCompName.Text = _invoice.CompanyName;
                _txtCustPhone.Text = _invoice.CustomerPhone;
                _txtCustEmail.Text = _invoice.CustomerEmail;
                _txtCustAddress.Text = _invoice.BillingAddress;
                _txtCustGSTIN.Text = _invoice.CustomerGSTIN;

                _txtOverallDiscount.Text = _invoice.OverallDiscount.ToString("F2");
                _txtNotes.Text = _invoice.Notes;
                _txtTerms.Text = _invoice.TermsAndConditions;

                // Populate Items
                _gridItems.Rows.Clear();
                foreach (var item in _invoice.Items)
                {
                    _gridItems.Rows.Add(item.ServiceName, item.Description, item.Quantity, item.UnitPrice, item.DiscountPercent, item.TaxPercent, item.LineTotal);
                }
            }
            else
            {
                // New Invoice: auto-generate number and add a default service line
                _txtInvoiceNo.Text = await _billingService.GetNextInvoiceNumberAsync();
                _txtTerms.Text = _profile.DefaultTerms;

                // Add default preset service rows for quick onboarding
                _gridItems.Rows.Add("Enterprise Software License", "Annual license for Cloud Suite (10 Users)", 1, 45000.00, 0, 18, 53100.00);
            }

            RecalculateLiveSummary();
        }

        private void CustomerSelectedFromDropdown(object? sender, EventArgs e)
        {
            int index = _cmbCustomers.SelectedIndex - 1;
            if (index >= 0 && index < _existingCustomers.Count)
            {
                var cust = _existingCustomers[index];
                _invoice.CustomerId = cust.CustomerId;
                _txtCustName.Text = cust.Name;
                _txtCompName.Text = cust.CompanyName;
                _txtCustPhone.Text = cust.Phone;
                _txtCustEmail.Text = cust.Email;
                _txtCustAddress.Text = $"{cust.Address}, {cust.City}, {cust.Country}";
                _txtCustGSTIN.Text = cust.GSTIN;
            }
        }

        private void QuickAddNewCustomer_Click(object? sender, EventArgs e)
        {
            var dlg = new Form
            {
                Text = "Quick Add Customer",
                Size = new Size(420, 480),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeManager.Background
            };

            int y = 15;
            CreateFieldLabel("Customer Name *:", 20, y, dlg);
            var txtN = new TextBox { Location = new Point(20, y + 20), Size = new Size(360, 26), BackColor = ThemeManager.CardBg, ForeColor = ThemeManager.TextPrimary, BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(txtN); y += 55;

            CreateFieldLabel("Company Name:", 20, y, dlg);
            var txtC = new TextBox { Location = new Point(20, y + 20), Size = new Size(360, 26), BackColor = ThemeManager.CardBg, ForeColor = ThemeManager.TextPrimary, BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(txtC); y += 55;

            CreateFieldLabel("Phone:", 20, y, dlg);
            var txtP = new TextBox { Location = new Point(20, y + 20), Size = new Size(360, 26), BackColor = ThemeManager.CardBg, ForeColor = ThemeManager.TextPrimary, BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(txtP); y += 55;

            CreateFieldLabel("Email:", 20, y, dlg);
            var txtE = new TextBox { Location = new Point(20, y + 20), Size = new Size(360, 26), BackColor = ThemeManager.CardBg, ForeColor = ThemeManager.TextPrimary, BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(txtE); y += 55;

            CreateFieldLabel("Billing Address & City:", 20, y, dlg);
            var txtA = new TextBox { Location = new Point(20, y + 20), Size = new Size(360, 26), BackColor = ThemeManager.CardBg, ForeColor = ThemeManager.TextPrimary, BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(txtA); y += 55;

            CreateFieldLabel("GSTIN:", 20, y, dlg);
            var txtG = new TextBox { Location = new Point(20, y + 20), Size = new Size(360, 26), BackColor = ThemeManager.CardBg, ForeColor = ThemeManager.TextPrimary, BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(txtG); y += 60;

            var btnAdd = new ModernButton
            {
                Text = "✅ Add Customer",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(160, 36),
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 8
            };
            dlg.Controls.Add(btnAdd);

            btnAdd.Click += async (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtN.Text))
                {
                    MessageBox.Show("Customer name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newCust = new Customer
                {
                    Name = txtN.Text.Trim(),
                    CompanyName = txtC.Text.Trim(),
                    Phone = txtP.Text.Trim(),
                    Email = txtE.Text.Trim(),
                    Address = txtA.Text.Trim(),
                    City = "Ahmedabad",
                    Country = "India",
                    GSTIN = txtG.Text.Trim()
                };

                await _billingService.AddCustomerAsync(newCust);
                _existingCustomers = await _billingService.GetAllCustomersAsync();

                _cmbCustomers.Items.Clear();
                _cmbCustomers.Items.Add("-- Select Existing Customer --");
                int selectIdx = 0;
                for (int i = 0; i < _existingCustomers.Count; i++)
                {
                    var c = _existingCustomers[i];
                    string display = string.IsNullOrWhiteSpace(c.CompanyName) ? c.Name : $"{c.Name} ({c.CompanyName})";
                    _cmbCustomers.Items.Add(display);
                    if (c.CustomerId == newCust.CustomerId) selectIdx = i + 1;
                }
                _cmbCustomers.SelectedIndex = selectIdx;
                dlg.Close();
            };

            dlg.ShowDialog(this);
        }

        private void ShowManualAddItemDialog()
        {
            using var dlg = new ManualAddItemDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (dlg.AddBlankRowRequested)
                {
                    _gridItems.Rows.Add("", "", 1, 0.00, 0, 18, 0.00);
                    if (_gridItems.Rows.Count > 0)
                    {
                        _gridItems.CurrentCell = _gridItems.Rows[_gridItems.Rows.Count - 1].Cells[0];
                        _gridItems.BeginEdit(true);
                    }
                }
                else if (dlg.CreatedItem != null)
                {
                    var itm = dlg.CreatedItem;
                    _gridItems.Rows.Add(itm.ServiceName, itm.Description, itm.Quantity, itm.UnitPrice, itm.DiscountPercent, itm.TaxPercent, itm.LineTotal);
                }
                RecalculateLiveSummary();
            }
        }

        private void ShowBarcodeScannerDialog()
        {
            using var dlg = new BarcodeScannerDialog(product =>
            {
                AddOrIncrementProductItem(product);
            });
            dlg.ShowDialog(this);
        }

        public void AddOrIncrementProductItem(Product product)
        {
            if (product == null) return;

            DataGridViewRow? existingRow = null;
            foreach (DataGridViewRow row in _gridItems.Rows)
            {
                if (row.Cells[0].Value != null && string.Equals(row.Cells[0].Value.ToString(), product.Name, StringComparison.OrdinalIgnoreCase))
                {
                    existingRow = row;
                    break;
                }
            }

            if (existingRow != null)
            {
                decimal curQty = decimal.TryParse(existingRow.Cells[2].Value?.ToString(), out decimal q) ? q : 1m;
                existingRow.Cells[2].Value = curQty + 1;
            }
            else
            {
                decimal tax = 18m;
                decimal qty = 1m;
                decimal gross = qty * product.Price;
                decimal lineTotal = Math.Round(gross + (gross * (tax / 100m)), 2);
                string desc = $"{product.Category} (SKU: {product.Barcode})";
                _gridItems.Rows.Add(product.Name, desc, qty, product.Price, 0, tax, lineTotal);
            }

            RecalculateLiveSummary();
        }

        public void AddNewItemRow(string name = "Cloud Subscription", string desc = "Standard monthly cloud services", decimal qty = 1, decimal rate = 5000.00m, decimal disc = 0, decimal tax = 18)
        {
            decimal gross = qty * rate;
            decimal discAmt = gross * (disc / 100m);
            decimal taxable = gross - discAmt;
            decimal lineTotal = Math.Round(taxable + (taxable * (tax / 100m)), 2);
            _gridItems.Rows.Add(name, desc, qty, rate, disc, tax, lineTotal);
            RecalculateLiveSummary();
        }

        private void RemoveSelectedItemRow()
        {
            if (_gridItems.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in _gridItems.SelectedRows)
                {
                    _gridItems.Rows.Remove(row);
                }
            }
            else if (_gridItems.CurrentRow != null)
            {
                _gridItems.Rows.Remove(_gridItems.CurrentRow);
            }
            RecalculateLiveSummary();
        }

        private void RecalculateLiveSummary()
        {
            decimal subtotal = 0m;
            decimal itemDiscounts = 0m;
            decimal taxableSum = 0m;
            decimal totalTax = 0m;

            foreach (DataGridViewRow row in _gridItems.Rows)
            {
                if (row.IsNewRow) continue;

                string name = row.Cells[0].Value?.ToString() ?? "";
                decimal qty = decimal.TryParse(row.Cells[2].Value?.ToString(), out decimal q) ? q : 1m;
                decimal rate = decimal.TryParse(row.Cells[3].Value?.ToString(), out decimal r) ? r : 0m;
                decimal disc = decimal.TryParse(row.Cells[4].Value?.ToString(), out decimal d) ? d : 0m;
                decimal tax = decimal.TryParse(row.Cells[5].Value?.ToString(), out decimal t) ? t : 18m;

                decimal gross = qty * rate;
                decimal itemDisc = gross * (disc / 100m);
                decimal taxable = Math.Max(0m, gross - itemDisc);
                decimal itemTax = taxable * (tax / 100m);
                decimal lineTotal = Math.Round(taxable + itemTax, 2);

                row.Cells[6].Value = lineTotal;

                subtotal += gross;
                itemDiscounts += itemDisc;
                taxableSum += taxable;
                totalTax += itemTax;
            }

            decimal overallDisc = decimal.TryParse(_txtOverallDiscount?.Text, out decimal od) ? od : 0m;
            decimal taxableAfterOverall = Math.Max(0m, taxableSum - overallDisc);

            // Re-apply tax proportionally if overall discount changed
            if (overallDisc > 0 && taxableSum > 0)
            {
                decimal effectiveRate = totalTax / taxableSum;
                totalTax = Math.Round(taxableAfterOverall * effectiveRate, 2);
            }

            decimal grandTotal = Math.Round(taxableAfterOverall + totalTax, 2);
            decimal amountPaid = _invoice.Payments.Sum(p => p.Amount);
            decimal balanceDue = Math.Max(0m, grandTotal - amountPaid);

            if (_lblSubTotal != null) _lblSubTotal.Text = $"₹{subtotal:N2}";
            if (_lblItemDiscount != null) _lblItemDiscount.Text = $"-₹{itemDiscounts:N2}";
            if (_lblTaxableAmount != null) _lblTaxableAmount.Text = $"₹{taxableAfterOverall:N2}";

            bool isInterState = _chkInterState?.Checked ?? false;
            if (_lblTaxDetails != null)
            {
                if (isInterState)
                    _lblTaxDetails.Text = $"IGST (18%): ₹{totalTax:N2}";
                else
                    _lblTaxDetails.Text = $"CGST+SGST (9%+9%): ₹{totalTax:N2}";
            }

            if (_lblGrandTotal != null) _lblGrandTotal.Text = $"₹{grandTotal:N2}";
            if (_lblAmountPaid != null) _lblAmountPaid.Text = $"₹{amountPaid:N2}";
            if (_lblBalanceDue != null)
            {
                _lblBalanceDue.Text = $"₹{balanceDue:N2}";
                _lblBalanceDue.ForeColor = balanceDue > 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129);
            }
        }

        private async Task<bool> SaveInvoiceWorkflowAsync(bool showSuccessPrompt = true)
        {
            if (_isSaving) return false;

            // 1. Validations
            if (string.IsNullOrWhiteSpace(_txtInvoiceNo.Text))
            {
                MessageBox.Show("Invoice number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtInvoiceNo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtCustName.Text))
            {
                MessageBox.Show("Customer Name is required. Please enter or select a customer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCustName.Focus();
                return false;
            }

            var validItems = new List<InvoiceItem>();
            foreach (DataGridViewRow row in _gridItems.Rows)
            {
                if (row.IsNewRow) continue;
                string service = row.Cells[0].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(service)) continue;

                decimal qty = decimal.TryParse(row.Cells[2].Value?.ToString(), out decimal q) ? q : 1m;
                decimal rate = decimal.TryParse(row.Cells[3].Value?.ToString(), out decimal r) ? r : 0m;
                decimal disc = decimal.TryParse(row.Cells[4].Value?.ToString(), out decimal d) ? d : 0m;
                decimal tax = decimal.TryParse(row.Cells[5].Value?.ToString(), out decimal t) ? t : 18m;

                if (qty <= 0)
                {
                    MessageBox.Show($"Quantity for '{service}' must be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (rate < 0)
                {
                    MessageBox.Show($"Price for '{service}' cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                validItems.Add(new InvoiceItem(service, row.Cells[1].Value?.ToString() ?? "", qty, rate, disc, tax));
            }

            if (validItems.Count == 0)
            {
                MessageBox.Show("Please add at least one valid item or service to the invoice.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            _isSaving = true;
            try
            {
                string selectedStatus = _cmbStatus.SelectedItem?.ToString() ?? "Unpaid";

                _invoice.InvoiceNumber = _txtInvoiceNo.Text.Trim();
                _invoice.InvoiceDate = _dtpInvoiceDate.Value;
                _invoice.DueDate = _dtpDueDate.Value;
                _invoice.PaymentTerms = _cmbTerms.SelectedItem?.ToString() ?? "Net 15";
                _invoice.IsInterState = _chkInterState.Checked;

                _invoice.CustomerName = _txtCustName.Text.Trim();
                _invoice.CompanyName = _txtCompName.Text.Trim();
                _invoice.CustomerPhone = _txtCustPhone.Text.Trim();
                _invoice.CustomerEmail = _txtCustEmail.Text.Trim();
                _invoice.BillingAddress = _txtCustAddress.Text.Trim();
                _invoice.CustomerGSTIN = _txtCustGSTIN.Text.Trim();

                decimal overallDisc = decimal.TryParse(_txtOverallDiscount.Text, out decimal od) ? od : 0m;
                _invoice.OverallDiscount = overallDisc;
                _invoice.Notes = _txtNotes.Text.Trim();
                _invoice.TermsAndConditions = _txtTerms.Text.Trim();

                _invoice.Items = validItems;
                _invoice.Status = selectedStatus;
                _invoice.RecalculateTotals();

                // If user selected Paid, but outstanding balance remains:
                if (selectedStatus == "Paid" && _invoice.BalanceDue > 0)
                {
                    var recordAns = MessageBox.Show(
                        $"This invoice currently has an outstanding balance of ₹{_invoice.BalanceDue:N2}.\n\nTo set the status to 'Paid', a full settlement payment of ₹{_invoice.BalanceDue:N2} will be recorded. Do you want to proceed?",
                        "Confirm Full Settlement", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (recordAns == DialogResult.Yes)
                    {
                        var payment = new Payment(
                            _invoice.InvoiceId,
                            _invoice.InvoiceNumber,
                            _invoice.CustomerName,
                            _invoice.BalanceDue,
                            "Cash",
                            "Direct-Settled",
                            "Settled upon saving as Paid"
                        )
                        {
                            PaymentDate = DateTime.Now
                        };
                        _invoice.Payments.Add(payment);
                        _invoice.AmountPaid = _invoice.GrandTotal;
                        _invoice.BalanceDue = 0m;
                        _invoice.Status = "Paid";
                    }
                    else
                    {
                        // Reset to actual calculated status
                        _invoice.RecalculateTotals();
                        _cmbStatus.SelectedItem = _invoice.Status;
                    }
                }
                else if (selectedStatus == "Draft" && _invoice.AmountPaid == 0)
                {
                    _invoice.Status = "Draft";
                }
                else if (selectedStatus == "Cancelled")
                {
                    _invoice.Status = "Cancelled";
                }

                await _billingService.SaveInvoiceAsync(_invoice);

                // If any newly recorded in-memory payment exists, persist it
                if (_invoice.InvoiceId > 0 && _invoice.Payments.Count > 0)
                {
                    foreach (var p in _invoice.Payments)
                    {
                        if (p.PaymentId == 0)
                        {
                            p.InvoiceId = _invoice.InvoiceId;
                            p.InvoiceNumber = _invoice.InvoiceNumber;
                            await _billingService.RecordPaymentAsync(p);
                        }
                    }
                }

                if (showSuccessPrompt)
                {
                    MessageBox.Show($"Invoice {_invoice.InvoiceNumber} saved successfully to persistent database!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database save failed: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                _isSaving = false;
            }
        }

        private async Task PreviewPdfWorkflowAsync()
        {
            bool saved = await SaveInvoiceWorkflowAsync(showSuccessPrompt: false);
            if (!saved) return;

            try
            {
                string pdfPath = _pdfService.GenerateInvoicePdf(_invoice, _profile);
                _pdfService.OpenPdf(pdfPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF Generation failed: {ex.Message}", "PDF Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ExportPdfWorkflowAsync()
        {
            bool saved = await SaveInvoiceWorkflowAsync(showSuccessPrompt: false);
            if (!saved) return;

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"{_invoice.InvoiceNumber}.pdf",
                Title = "Export Invoice PDF"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _pdfService.GenerateInvoicePdf(_invoice, _profile, sfd.FileName);
                    var askOpen = MessageBox.Show($"PDF saved successfully to:\n{sfd.FileName}\n\nDo you want to open it now?", "Export Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (askOpen == DialogResult.Yes)
                    {
                        _pdfService.OpenPdf(sfd.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Helper Methods
        private Label AddSummaryRow(string label, string initialValue, ref int y, Control parent, bool isBold = false, Color? color = null)
        {
            var lblL = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9.5f, isBold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(16, y),
                AutoSize = true
            };
            parent.Controls.Add(lblL);

            var lblV = new Label
            {
                Text = initialValue,
                Font = new Font("Segoe UI", 9.5f, isBold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color ?? ThemeManager.TextPrimary,
                Location = new Point(310, y),
                Size = new Size(165, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            parent.Controls.Add(lblV);

            y += 24;
            return lblV;
        }

        private Label CreateFieldLabel(string text, int x, int y, Control parent)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private void UpdateInvoiceLayout()
        {
            if (_pnlMain == null || _headerPanel == null || _cardInvoice == null || _cardCust == null ||
                _cardItems == null || _cardNotes == null || _cardSummary == null) return;

            _pnlMain.SuspendLayout();

            int startX = 25;
            int contentWidth = Math.Max(1000, _pnlMain.ClientSize.Width - 50);

            // 1. Header & Action buttons
            _headerPanel.Location = new Point(startX, 15);
            _headerPanel.Size = new Size(contentWidth, 65);

            int btnRight = contentWidth - 16;
            _btnClose.Location = new Point(btnRight - 90, 15);
            btnRight -= (90 + 10);
            _btnExport.Location = new Point(btnRight - 120, 15);
            btnRight -= (120 + 10);
            _btnPreview.Location = new Point(btnRight - 125, 15);
            btnRight -= (125 + 10);
            _btnRecordPayment.Location = new Point(btnRight - 145, 15);
            btnRight -= (145 + 10);
            _btnSave.Location = new Point(btnRight - 130, 15);

            // 2. Metadata Cards (Invoice Details & Customer Details)
            int row1Y = _headerPanel.Bottom + 15;
            int gap = 20;
            int colW = (contentWidth - gap) / 2;

            _cardInvoice.Location = new Point(startX, row1Y);
            _cardInvoice.Size = new Size(colW, 250);

            int cardCustX = startX + colW + gap;
            int cardCustW = contentWidth - colW - gap;
            _cardCust.Location = new Point(cardCustX, row1Y);
            _cardCust.Size = new Size(cardCustW, 250);

            // Reposition controls inside _cardInvoice
            int invFieldW = (colW - 52) / 2;
            int invCol2X = 16 + invFieldW + 20;

            _txtInvoiceNo.Size = new Size(invFieldW, 28);
            _chkInterState.Location = new Point(invCol2X, 64);

            _dtpInvoiceDate.Size = new Size(invFieldW, 26);
            if (_lblDueDateLabel != null) _lblDueDateLabel.Location = new Point(invCol2X, 100);
            _dtpDueDate.Location = new Point(invCol2X, 120);
            _dtpDueDate.Size = new Size(invFieldW, 26);

            _cmbTerms.Size = new Size(invFieldW, 28);
            if (_lblStatusLabel != null) _lblStatusLabel.Location = new Point(invCol2X, 160);
            _cmbStatus.Location = new Point(invCol2X, 180);
            _cmbStatus.Size = new Size(invFieldW, 28);

            // Reposition controls inside _cardCust
            _btnNewCust.Location = new Point(cardCustW - 16 - 110, 62);
            _btnNewCust.Size = new Size(110, 28);
            _cmbCustomers.Size = new Size(cardCustW - 16 - 110 - 10 - 16, 28);

            int custColW = (cardCustW - 52) / 2;
            int custCol2X = 16 + custColW + 20;

            _txtCustName.Size = new Size(custColW, 26);
            if (_lblCompNameLabel != null) _lblCompNameLabel.Location = new Point(custCol2X, 100);
            _txtCompName.Location = new Point(custCol2X, 120);
            _txtCompName.Size = new Size(custColW, 26);

            int threeColGap = 15;
            int threeColW = (cardCustW - 32 - (2 * threeColGap)) / 3;
            int col2EmailX = 16 + threeColW + threeColGap;
            int col3GstinX = col2EmailX + threeColW + threeColGap;

            _txtCustPhone.Size = new Size(threeColW, 26);
            if (_lblCustEmailLabel != null) _lblCustEmailLabel.Location = new Point(col2EmailX, 155);
            _txtCustEmail.Location = new Point(col2EmailX, 175);
            _txtCustEmail.Size = new Size(threeColW, 26);

            if (_lblCustGSTINLabel != null) _lblCustGSTINLabel.Location = new Point(col3GstinX, 155);
            _txtCustGSTIN.Location = new Point(col3GstinX, 175);
            _txtCustGSTIN.Size = new Size(cardCustW - 16 - col3GstinX, 26);

            _txtCustAddress.Size = new Size(cardCustW - 32, 24);

            // 3. Itemized Services Card
            int row2Y = _cardInvoice.Bottom + 15;
            _cardItems.Location = new Point(startX, row2Y);
            _cardItems.Size = new Size(contentWidth, 290);

            _btnRemoveItem.Location = new Point(contentWidth - 16 - 130, 10);
            _btnScanBarcode.Location = new Point(_btnRemoveItem.Left - 10 - 150, 10);
            _btnAddItem.Location = new Point(_btnScanBarcode.Left - 10 - 165, 10);

            _gridItems.Size = new Size(contentWidth - 32, 225);

            // 4. Notes & Summary Cards
            int row3Y = _cardItems.Bottom + 15;
            int summaryW = Math.Clamp(contentWidth * 38 / 100, 480, 560);
            int notesW = contentWidth - summaryW - gap;

            _cardNotes.Location = new Point(startX, row3Y);
            _cardNotes.Size = new Size(notesW, 230);

            _txtNotes.Size = new Size(notesW - 32, 65);
            _txtTerms.Size = new Size(notesW - 32, 75);

            int cardSummaryX = startX + notesW + gap;
            _cardSummary.Location = new Point(cardSummaryX, row3Y);
            _cardSummary.Size = new Size(summaryW, 230);

            int valX = summaryW - 16 - 165;
            if (_lblSubTotal != null) _lblSubTotal.Location = new Point(valX, _lblSubTotal.Location.Y);
            if (_lblItemDiscount != null) _lblItemDiscount.Location = new Point(valX, _lblItemDiscount.Location.Y);
            if (_txtOverallDiscount != null) _txtOverallDiscount.Location = new Point(summaryW - 16 - 110, _txtOverallDiscount.Location.Y);
            if (_lblTaxableAmount != null) _lblTaxableAmount.Location = new Point(valX, _lblTaxableAmount.Location.Y);
            if (_lblTaxDetails != null) _lblTaxDetails.Location = new Point(valX, _lblTaxDetails.Location.Y);
            if (_pnlGrandTotal != null)
            {
                _pnlGrandTotal.Size = new Size(summaryW - 32, 38);
                if (_lblGrandTotal != null)
                {
                    _lblGrandTotal.Location = new Point(_pnlGrandTotal.Width - 12 - 220, 7);
                }
            }
            if (_lblAmountPaid != null) _lblAmountPaid.Location = new Point(valX, _lblAmountPaid.Location.Y);
            if (_lblBalanceDue != null) _lblBalanceDue.Location = new Point(valX, _lblBalanceDue.Location.Y);

            _pnlMain.ResumeLayout();
        }
        #endregion
    }
}
