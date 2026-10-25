using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public class InvoiceDesignerForm : Form
    {
        private readonly BillingDataService _billingService = BillingDataService.Instance;
        private readonly InvoicePdfService _pdfService = InvoicePdfService.Instance;

        // Profiles: Saved DB state vs Live Working Editor state
        private BusinessProfile _savedProfile = new();
        private BusinessProfile _editProfile = new();
        private Invoice _activeInvoice = new();
        private List<Invoice> _availableInvoices = new();

        // Canvas & Zoom State
        private float _currentScale = 1.0f;
        private bool _fitToPage = true;
        private bool _isDirty = false;
        private bool _isPopulating = false;
        private readonly System.Windows.Forms.Timer _debounceTimer;

        // Layout Containers
        private Panel _toolbarPanel = null!;
        private Panel _leftEditorPanel = null!;
        private Panel _rightPreviewPanel = null!;
        private Panel _previewTopBar = null!;
        private Panel _canvasContainer = null!;
        private InvoiceCanvasControl _canvasControl = null!;

        // Action Toolbar Buttons
        private ModernButton _btnBack = null!;
        private ModernButton _btnReset = null!;
        private ModernButton _btnRefresh = null!;
        private ModernButton _btnExport = null!;
        private ModernButton _btnSave = null!;

        // Section 1: Business Information Controls
        private TextBox _txtName = null!;
        private TextBox _txtTagline = null!;
        private TextBox _txtAddress = null!;
        private TextBox _txtCity = null!;
        private TextBox _txtCountry = null!;
        private TextBox _txtPhone = null!;
        private TextBox _txtEmail = null!;
        private TextBox _txtGSTIN = null!;

        // Section 2: Bank Details Controls
        private TextBox _txtBankName = null!;
        private TextBox _txtBranchName = null!;
        private TextBox _txtAccountNo = null!;
        private TextBox _txtIFSC = null!;
        private TextBox _txtUPI = null!;

        // Section 3: Branding & Color Controls
        private TextBox _txtLogoPath = null!;
        private PictureBox _picLogoThumb = null!;
        private ModernButton _btnBrowseLogo = null!;
        private ModernButton _btnClearLogo = null!;
        private FlowLayoutPanel _pnlColorSwatches = null!;
        private Panel _pnlSelectedColorSwatch = null!;
        private Label _lblAccentColorHex = null!;
        private ModernButton _btnCustomColor = null!;
        private TextBox _txtFooterNote = null!;
        private TextBox _txtTerms = null!;

        // Preview Controls
        private ComboBox _cmbInvoicePicker = null!;
        private ModernButton _btnFitPage = null!;
        private ModernButton _btnZoomOut = null!;
        private Label _lblZoomLevel = null!;
        private ModernButton _btnZoomIn = null!;
        private Label _lblPageIndicator = null!;
        private readonly List<RoundedPanel> _editorCards = new();

        public InvoiceDesignerForm()
        {
            Text = "Invoice PDF Designer — Live Branding Studio";
            Size = new Size(1360, 880);
            MinimumSize = new Size(1120, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            _debounceTimer = new System.Windows.Forms.Timer { Interval = 250 };
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                UpdateProfileFromEditor();
                RenderPreview();
            };

            BuildLayout();
            LoadInitialData();
        }

        #region UI Layout Construction
        private void BuildLayout()
        {
            // 1. Top Action Toolbar (Height: 56px)
            BuildTopToolbar();

            // 2. Main Content Split View (Left Editor Panel + Right Preview Panel)
            var pnlMainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Background
            };
            Controls.Add(pnlMainContainer);

            // Left Editor Panel (Width: 430px)
            BuildLeftEditorPanel(pnlMainContainer);

            // Right Live Preview Panel (Fill)
            BuildRightPreviewPanel(pnlMainContainer);
        }

        private void BuildTopToolbar()
        {
            _toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(24, 27, 40) : Color.White,
                Padding = new Padding(16, 10, 16, 10)
            };
            Controls.Add(_toolbarPanel);

            // 1px Bottom Border
            _toolbarPanel.Paint += (s, e) =>
            {
                using var p = new Pen(ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240), 1f);
                e.Graphics.DrawLine(p, 0, _toolbarPanel.Height - 1, _toolbarPanel.Width, _toolbarPanel.Height - 1);
            };

            // Back to Settings Button
            _btnBack = new ModernButton
            {
                Text = "← Back to Settings",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(140, 36),
                Location = new Point(16, 12),
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 47, 65) : Color.FromArgb(241, 245, 249),
                HoverColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(55, 62, 85) : Color.FromArgb(226, 232, 240),
                PressedColor = Color.FromArgb(79, 70, 229),
                ForeColor = ThemeManager.TextPrimary,
                TextHoverColor = ThemeManager.TextPrimary,
                BorderRadius = 8
            };
            _toolbarPanel.Controls.Add(_btnBack);
            _btnBack.Click += (s, e) => HandleBackNavigation();

            // Title & Subtitle
            var lblTitle = new Label
            {
                Text = "🎨  Invoice PDF Designer",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(170, 9),
                AutoSize = true
            };
            _toolbarPanel.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "Single-screen live invoice branding editor & instant vector PDF preview",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(172, 33),
                AutoSize = true
            };
            _toolbarPanel.Controls.Add(lblSubtitle);

            // Action Buttons on Right
            _btnSave = new ModernButton
            {
                Text = "💾 Save Changes",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(135, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = Color.FromArgb(79, 70, 229), // Indigo 600
                HoverColor = Color.FromArgb(99, 102, 241),
                PressedColor = Color.FromArgb(67, 56, 202),
                ForeColor = Color.White,
                BorderRadius = 8
            };
            _btnSave.Location = new Point(_toolbarPanel.Width - _btnSave.Width - 16, 12);
            _toolbarPanel.Controls.Add(_btnSave);
            _btnSave.Click += async (s, e) => await SaveChangesAsync();

            _btnExport = new ModernButton
            {
                Text = "📄 Export PDF",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(115, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = Color.FromArgb(16, 185, 129), // Emerald 600
                HoverColor = Color.FromArgb(52, 211, 153),
                PressedColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White,
                BorderRadius = 8
            };
            _btnExport.Location = new Point(_btnSave.Left - _btnExport.Width - 10, 12);
            _toolbarPanel.Controls.Add(_btnExport);
            _btnExport.Click += (s, e) => ExportPdfWorkflow();

            _btnRefresh = new ModernButton
            {
                Text = "↻ Refresh",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(95, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249),
                HoverColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(226, 232, 240),
                ForeColor = ThemeManager.TextPrimary,
                TextHoverColor = ThemeManager.TextPrimary,
                BorderRadius = 8
            };
            _btnRefresh.Location = new Point(_btnExport.Left - _btnRefresh.Width - 10, 12);
            _toolbarPanel.Controls.Add(_btnRefresh);
            _btnRefresh.Click += (s, e) =>
            {
                UpdateProfileFromEditor();
                RenderPreview();
            };

            _btnReset = new ModernButton
            {
                Text = "↺ Reset",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(90, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249),
                HoverColor = Color.FromArgb(239, 68, 68),
                ForeColor = ThemeManager.TextPrimary,
                TextHoverColor = Color.White,
                BorderRadius = 8
            };
            _btnReset.Location = new Point(_btnRefresh.Left - _btnReset.Width - 10, 12);
            _toolbarPanel.Controls.Add(_btnReset);
            _btnReset.Click += (s, e) => ResetChangesWorkflow();
        }

        private void BuildLeftEditorPanel(Panel container)
        {
            _leftEditorPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 430,
                AutoScroll = true,
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(20, 22, 33) : Color.FromArgb(248, 250, 252),
                Padding = new Padding(16, 14, 16, 20)
            };
            container.Controls.Add(_leftEditorPanel);

            // Right vertical divider
            _leftEditorPanel.Paint += (s, e) =>
            {
                using var p = new Pen(ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240), 1f);
                e.Graphics.DrawLine(p, _leftEditorPanel.Width - 1, 0, _leftEditorPanel.Width - 1, _leftEditorPanel.Height);
            };

            int y = 10;
            int cardW = 398;
            _editorCards.Clear();

            // SECTION 1: Business Information
            var cardBusiness = CreateCollapsibleCard("🏢  Business Information", ref y, cardW, _leftEditorPanel, out Panel bodyBusiness);
            int by = 10;
            _txtName = AddFormField("Business / Brand Name *:", ref by, bodyBusiness);
            _txtTagline = AddFormField("Tagline / Subtitle:", ref by, bodyBusiness);
            _txtAddress = AddFormField("Business Address:", ref by, bodyBusiness, isAddress: true);
            _txtCity = AddFormField("City & State:", ref by, bodyBusiness);
            _txtCountry = AddFormField("Country:", ref by, bodyBusiness);
            _txtPhone = AddFormField("Phone Number:", ref by, bodyBusiness);
            _txtEmail = AddFormField("Email Address:", ref by, bodyBusiness);
            _txtGSTIN = AddFormField("GSTIN (Tax ID):", ref by, bodyBusiness);
            bodyBusiness.Height = by + 10;
            cardBusiness.Height = 38 + bodyBusiness.Height + 10;
            cardBusiness.Tag = cardBusiness.Height;
            y = cardBusiness.Bottom + 16;

            // SECTION 2: Bank Payment Details
            var cardBank = CreateCollapsibleCard("🏦  Bank Payment Details", ref y, cardW, _leftEditorPanel, out Panel bodyBank);
            int py = 10;
            _txtBankName = AddFormField("Bank Name:", ref py, bodyBank);
            _txtBranchName = AddFormField("Branch Name:", ref py, bodyBank);
            _txtAccountNo = AddFormField("Bank Account Number:", ref py, bodyBank);
            _txtIFSC = AddFormField("IFSC Code:", ref py, bodyBank);
            _txtUPI = AddFormField("UPI ID / VPA:", ref py, bodyBank);
            bodyBank.Height = py + 10;
            cardBank.Height = 38 + bodyBank.Height + 10;
            cardBank.Tag = cardBank.Height;
            y = cardBank.Bottom + 16;

            // SECTION 3: Invoice Branding & Colors
            var cardBranding = CreateCollapsibleCard("🎨  Invoice Branding & Colors", ref y, cardW, _leftEditorPanel, out Panel bodyBranding);
            int cy = 10;

            // Logo Picker
            var lblLogo = new Label
            {
                Text = "Business Logo (Printed on Header):",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(14, cy),
                AutoSize = true
            };
            bodyBranding.Controls.Add(lblLogo);
            cy += 20;

            _picLogoThumb = new PictureBox
            {
                Size = new Size(42, 42),
                Location = new Point(14, cy),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            bodyBranding.Controls.Add(_picLogoThumb);

            _txtLogoPath = new TextBox
            {
                Location = new Point(62, cy + 6),
                Size = new Size(180, 26),
                Font = new Font("Segoe UI", 9f),
                ReadOnly = true,
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary
            };
            bodyBranding.Controls.Add(_txtLogoPath);

            _btnBrowseLogo = new ModernButton
            {
                Text = "Browse...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(72, 28),
                Location = new Point(248, cy + 5),
                NormalColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                BorderRadius = 6
            };
            bodyBranding.Controls.Add(_btnBrowseLogo);
            _btnBrowseLogo.Click += (s, e) => BrowseLogoFile();

            _btnClearLogo = new ModernButton
            {
                Text = "✖",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(34, 28),
                Location = new Point(325, cy + 5),
                NormalColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                BorderRadius = 6
            };
            bodyBranding.Controls.Add(_btnClearLogo);
            _btnClearLogo.Click += (s, e) => ClearLogo();
            cy += 50;

            // Accent Color Palette
            var lblAccent = new Label
            {
                Text = "Invoice Accent Color:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(14, cy),
                AutoSize = true
            };
            bodyBranding.Controls.Add(lblAccent);
            cy += 22;

            _pnlColorSwatches = new FlowLayoutPanel
            {
                Location = new Point(14, cy),
                Size = new Size(345, 36),
                AutoSize = false
            };
            bodyBranding.Controls.Add(_pnlColorSwatches);

            string[] presetHexes = { "#3b82f6", "#6366f1", "#10b981", "#e11d48", "#8b5cf6", "#334155" };
            foreach (var hex in presetHexes)
            {
                var colorBtn = new Button
                {
                    Size = new Size(28, 28),
                    BackColor = ColorTranslator.FromHtml(hex),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 8, 2)
                };
                colorBtn.FlatAppearance.BorderSize = 1;
                colorBtn.FlatAppearance.BorderColor = Color.White;
                colorBtn.Click += (s, e) => SetAccentColor(hex);
                _pnlColorSwatches.Controls.Add(colorBtn);
            }

            _btnCustomColor = new ModernButton
            {
                Text = "🎨 Custom...",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Size = new Size(95, 28),
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 47, 65) : Color.FromArgb(226, 232, 240),
                ForeColor = ThemeManager.TextPrimary,
                TextHoverColor = ThemeManager.TextPrimary,
                BorderRadius = 6
            };
            _btnCustomColor.Click += (s, e) => PickCustomColor();
            _pnlColorSwatches.Controls.Add(_btnCustomColor);
            cy += 40;

            // Selected Color Preview Swatch & Hex Display
            _pnlSelectedColorSwatch = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(16, cy + 2),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ColorTranslator.FromHtml("#3b82f6")
            };
            bodyBranding.Controls.Add(_pnlSelectedColorSwatch);

            _lblAccentColorHex = new Label
            {
                Text = "Selected Color: #3b82f6",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(42, cy + 4),
                AutoSize = true
            };
            bodyBranding.Controls.Add(_lblAccentColorHex);
            cy += 32;

            // Footer Note
            var lblFooter = new Label
            {
                Text = "Footer Text (Printed at bottom):",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(14, cy),
                AutoSize = true
            };
            bodyBranding.Controls.Add(lblFooter);
            cy += 20;

            _txtFooterNote = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(345, 45),
                Location = new Point(14, cy),
                Font = new Font("Segoe UI", 9f),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtFooterNote.TextChanged += OnEditorFieldChanged;
            bodyBranding.Controls.Add(_txtFooterNote);
            cy += 54;

            // Terms & Conditions
            var lblTerms = new Label
            {
                Text = "Default Terms (Printed in Bank box):",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(14, cy),
                AutoSize = true
            };
            bodyBranding.Controls.Add(lblTerms);
            cy += 20;

            _txtTerms = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(345, 60),
                Location = new Point(14, cy),
                Font = new Font("Segoe UI", 9f),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtTerms.TextChanged += OnEditorFieldChanged;
            bodyBranding.Controls.Add(_txtTerms);
            cy += 68;

            bodyBranding.Height = cy + 10;
            cardBranding.Height = 38 + bodyBranding.Height + 10;
            cardBranding.Tag = cardBranding.Height;
        }

        private void BuildRightPreviewPanel(Panel container)
        {
            _rightPreviewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(15, 17, 26) : Color.FromArgb(241, 245, 249)
            };
            container.Controls.Add(_rightPreviewPanel);

            // Preview Toolbar Header Bar
            _previewTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(24, 27, 40) : Color.White,
                Padding = new Padding(16, 6, 16, 6)
            };
            _rightPreviewPanel.Controls.Add(_previewTopBar);

            _previewTopBar.Paint += (s, e) =>
            {
                using var p = new Pen(ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240), 1f);
                e.Graphics.DrawLine(p, 0, _previewTopBar.Height - 1, _previewTopBar.Width, _previewTopBar.Height - 1);
            };

            // Invoice Data Picker
            var lblPicker = new Label
            {
                Text = "📄 Invoice Data:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(14, 12),
                AutoSize = true
            };
            _previewTopBar.Controls.Add(lblPicker);

            _cmbInvoicePicker = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f),
                Location = new Point(120, 9),
                Size = new Size(240, 26)
            };
            _previewTopBar.Controls.Add(_cmbInvoicePicker);
            _cmbInvoicePicker.SelectedIndexChanged += (s, e) =>
            {
                if (_isPopulating) return;
                int idx = _cmbInvoicePicker.SelectedIndex;
                if (idx > 0 && idx - 1 < _availableInvoices.Count)
                {
                    _activeInvoice = _availableInvoices[idx - 1];
                }
                else
                {
                    _activeInvoice = CreateSampleInvoice();
                }
                RenderPreview();
            };

            // Zoom Controls on Right of Preview Bar
            _btnFitPage = new ModernButton
            {
                Text = "🔍 Fit to Page",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(95, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 47, 65) : Color.FromArgb(241, 245, 249),
                ForeColor = ThemeManager.TextPrimary,
                TextHoverColor = ThemeManager.TextPrimary,
                BorderRadius = 6
            };
            _btnFitPage.Location = new Point(_previewTopBar.Width - 365, 8);
            _previewTopBar.Controls.Add(_btnFitPage);
            _btnFitPage.Click += (s, e) =>
            {
                _fitToPage = true;
                RecalculateFitZoom();
                RenderPreview();
            };

            _btnZoomOut = new ModernButton
            {
                Text = "➖",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(32, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 47, 65) : Color.FromArgb(241, 245, 249),
                ForeColor = ThemeManager.TextPrimary,
                TextHoverColor = ThemeManager.TextPrimary,
                BorderRadius = 6
            };
            _btnZoomOut.Location = new Point(_btnFitPage.Right + 8, 8);
            _previewTopBar.Controls.Add(_btnZoomOut);
            _btnZoomOut.Click += (s, e) => AdjustZoom(-0.15f);

            _lblZoomLevel = new Label
            {
                Text = "100%",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(52, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(_btnZoomOut.Right + 4, 9)
            };
            _previewTopBar.Controls.Add(_lblZoomLevel);

            _btnZoomIn = new ModernButton
            {
                Text = "➕",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(32, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 47, 65) : Color.FromArgb(241, 245, 249),
                ForeColor = ThemeManager.TextPrimary,
                TextHoverColor = ThemeManager.TextPrimary,
                BorderRadius = 6
            };
            _btnZoomIn.Location = new Point(_lblZoomLevel.Right + 4, 8);
            _previewTopBar.Controls.Add(_btnZoomIn);
            _btnZoomIn.Click += (s, e) => AdjustZoom(0.15f);

            _lblPageIndicator = new Label
            {
                Text = "Page 1 of 1",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(80, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(_btnZoomIn.Right + 12, 9)
            };
            _previewTopBar.Controls.Add(_lblPageIndicator);

            // Canvas Scrollable Viewport
            _canvasContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(15, 17, 26) : Color.FromArgb(226, 232, 240),
                Padding = new Padding(24)
            };
            _rightPreviewPanel.Controls.Add(_canvasContainer);
            _previewTopBar.BringToFront();

            _canvasControl = new InvoiceCanvasControl
            {
                Location = new Point(24, 24),
                Size = new Size(595, 842)
            };
            _canvasContainer.Controls.Add(_canvasControl);
            _canvasContainer.Resize += (s, e) =>
            {
                if (_fitToPage)
                {
                    RecalculateFitZoom();
                    RenderPreview();
                }
                else
                {
                    CenterCanvas();
                }
            };
        }
        #endregion

        #region Helper Controls & Card Builder
        private RoundedPanel CreateCollapsibleCard(string title, ref int y, int width, Control parent, out Panel bodyPanel)
        {
            var card = new RoundedPanel
            {
                Location = new Point(16, y),
                Size = new Size(width, 100),
                BorderRadius = 10,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240)
            };
            parent.Controls.Add(card);
            _editorCards.Add(card);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 8, 12, 8)
            };
            card.Controls.Add(header);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(12, 8),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);

            var lblArrow = new Label
            {
                Text = "▼",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.TextSecondary,
                Dock = DockStyle.Right,
                Width = 24,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblArrow);

            var panel = new Panel
            {
                Location = new Point(0, 38),
                Width = width,
                Height = 100,
                Padding = new Padding(14, 6, 14, 10)
            };
            bodyPanel = panel;
            card.Controls.Add(panel);
            panel.BringToFront();

            bool isCollapsed = false;
            header.Click += (s, e) =>
            {
                isCollapsed = !isCollapsed;
                panel.Visible = !isCollapsed;
                lblArrow.Text = isCollapsed ? "▲" : "▼";
                card.Height = isCollapsed ? 38 : (card.Tag is int h ? h : 38 + panel.Height + 10);
                ReflowLeftCards();
            };

            return card;
        }

        private void ReflowLeftCards()
        {
            int y = 10;
            foreach (var card in _editorCards)
            {
                card.Location = new Point(16, y);
                y = card.Bottom + 16;
            }
        }

        private TextBox AddFormField(string labelText, ref int y, Control parent, bool isAddress = false)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(14, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            y += 18;

            int txtHeight = isAddress ? 44 : 26;
            var txt = new TextBox
            {
                Location = new Point(14, y),
                Size = new Size(368, txtHeight),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(30, 34, 50) : Color.White,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = isAddress
            };
            txt.TextChanged += OnEditorFieldChanged;
            parent.Controls.Add(txt);
            y += txtHeight + 8;

            return txt;
        }
        #endregion

        #region Data Loading & Binding
        private async void LoadInitialData()
        {
            _isPopulating = true;
            try
            {
                _savedProfile = await _billingService.GetBusinessProfileAsync();
                _editProfile = _savedProfile.Clone();

                // Populate Form Fields
                _txtName.Text = _editProfile.BusinessName;
                _txtTagline.Text = _editProfile.Tagline;
                _txtAddress.Text = _editProfile.Address;
                _txtCity.Text = _editProfile.City;
                _txtCountry.Text = _editProfile.Country;
                _txtPhone.Text = _editProfile.Phone;
                _txtEmail.Text = _editProfile.Email;
                _txtGSTIN.Text = _editProfile.GSTIN;

                _txtBankName.Text = _editProfile.BankName;
                _txtBranchName.Text = _editProfile.BranchName;
                _txtAccountNo.Text = _editProfile.AccountNumber;
                _txtIFSC.Text = _editProfile.IFSC;
                _txtUPI.Text = _editProfile.UPIId;

                _txtLogoPath.Text = _editProfile.LogoPath;
                UpdateLogoThumbnail(_editProfile.LogoPath);
                SetAccentColor(_editProfile.AccentColorHex, triggerDebounce: false);

                _txtFooterNote.Text = _editProfile.FooterNote;
                _txtTerms.Text = _editProfile.DefaultTerms;

                // Load invoices for live preview dropdown
                _availableInvoices = await _billingService.GetInvoicesAsync(limit: 10);
                _cmbInvoicePicker.Items.Clear();
                _cmbInvoicePicker.Items.Add("Default Sample Invoice (12 Records Template)");
                foreach (var inv in _availableInvoices)
                {
                    _cmbInvoicePicker.Items.Add($"{inv.InvoiceNumber} — {inv.CustomerName} (₹{inv.GrandTotal:N2})");
                }
                _cmbInvoicePicker.SelectedIndex = 0;
                _activeInvoice = CreateSampleInvoice();

                _isDirty = false;
                RecalculateFitZoom();
                RenderPreview();
            }
            finally
            {
                _isPopulating = false;
            }
        }

        private static Invoice CreateSampleInvoice()
        {
            var inv = new Invoice
            {
                InvoiceNumber = "INV-2026-0006",
                InvoiceDate = new DateTime(2026, 10, 14),
                DueDate = new DateTime(2026, 11, 13),
                PaymentTerms = "Net 30",
                Status = "Draft",
                CustomerName = "Aarav Sharma",
                CompanyName = "TechCorp Solutions Pvt Ltd",
                BillingAddress = "A-402 Sharath Hexa, SG Highway, Ahmedabad, India",
                CustomerPhone = "+91 98250 11223",
                CustomerEmail = "aarav@techcorp.in",
                CustomerGSTIN = "24AABCT1234F1Z1",
                TaxRate = 18m,
                IsInterState = false
            };

            inv.Items.Add(new InvoiceItem("Enterprise Software License", "Annual license for Cloud Suite", 1m, 45000m, 0m, 18m));
            inv.Items.Add(new InvoiceItem("Cloud Subscription", "Standard monthly cloud services", 1m, 5000m, 0m, 18m));
            inv.Items.Add(new InvoiceItem("Cloud Subscription", "Standard monthly cloud services", 1m, 5000m, 0m, 18m));
            inv.Items.Add(new InvoiceItem("Cloud Subscription", "Standard monthly cloud services", 1m, 5000m, 0m, 18m));
            inv.RecalculateTotals();

            return inv;
        }

        private void OnEditorFieldChanged(object? sender, EventArgs e)
        {
            if (_isPopulating) return;
            _isDirty = true;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void UpdateProfileFromEditor()
        {
            _editProfile.BusinessName = _txtName.Text.Trim();
            _editProfile.Tagline = _txtTagline.Text.Trim();
            _editProfile.Address = _txtAddress.Text.Trim();
            _editProfile.City = _txtCity.Text.Trim();
            _editProfile.Country = _txtCountry.Text.Trim();
            _editProfile.Phone = _txtPhone.Text.Trim();
            _editProfile.Email = _txtEmail.Text.Trim();
            _editProfile.GSTIN = _txtGSTIN.Text.Trim();

            _editProfile.BankName = _txtBankName.Text.Trim();
            _editProfile.BranchName = _txtBranchName.Text.Trim();
            _editProfile.AccountNumber = _txtAccountNo.Text.Trim();
            _editProfile.IFSC = _txtIFSC.Text.Trim();
            _editProfile.UPIId = _txtUPI.Text.Trim();

            _editProfile.LogoPath = _txtLogoPath.Text.Trim();
            _editProfile.FooterNote = _txtFooterNote.Text.Trim();
            _editProfile.DefaultTerms = _txtTerms.Text.Trim();
        }
        #endregion

        #region Logo & Accent Color Handling
        private void BrowseLogoFile()
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Select Business Logo Image"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                _txtLogoPath.Text = ofd.FileName;
                UpdateLogoThumbnail(ofd.FileName);
                _isDirty = true;
                UpdateProfileFromEditor();
                RenderPreview();
            }
        }

        private void ClearLogo()
        {
            _txtLogoPath.Text = "";
            UpdateLogoThumbnail("");
            _isDirty = true;
            UpdateProfileFromEditor();
            RenderPreview();
        }

        private void UpdateLogoThumbnail(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    _picLogoThumb.Image?.Dispose();
                    _picLogoThumb.Image = Image.FromStream(stream);
                    return;
                }
                catch { }
            }
            _picLogoThumb.Image?.Dispose();
            _picLogoThumb.Image = null;
        }

        private void SetAccentColor(string hex, bool triggerDebounce = true)
        {
            _editProfile.AccentColorHex = hex;
            try
            {
                var c = ColorTranslator.FromHtml(hex);
                _pnlSelectedColorSwatch.BackColor = c;
            }
            catch { }

            _lblAccentColorHex.Text = $"Selected Color: {hex}";
            if (triggerDebounce)
            {
                _isDirty = true;
                RenderPreview();
            }
        }

        private void PickCustomColor()
        {
            using var cd = new ColorDialog();
            try { cd.Color = ColorTranslator.FromHtml(_editProfile.AccentColorHex); } catch { }
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                string hex = $"#{cd.Color.R:X2}{cd.Color.G:X2}{cd.Color.B:X2}";
                SetAccentColor(hex);
            }
        }
        #endregion

        #region Zoom & Canvas Rendering
        private void RecalculateFitZoom()
        {
            if (_canvasContainer.ClientSize.Width <= 0 || _canvasContainer.ClientSize.Height <= 0) return;

            float availW = _canvasContainer.ClientSize.Width - 60f;
            float availH = _canvasContainer.ClientSize.Height - 60f;
            float scaleW = availW / 595.28f;
            float scaleH = availH / 841.89f;

            _currentScale = Math.Clamp(Math.Min(scaleW, scaleH), 0.4f, 1.8f);
            _lblZoomLevel.Text = $"{Math.Round(_currentScale * 100)}%";
        }

        private void AdjustZoom(float delta)
        {
            _fitToPage = false;
            _currentScale = Math.Clamp(_currentScale + delta, 0.4f, 2.2f);
            _lblZoomLevel.Text = $"{Math.Round(_currentScale * 100)}%";
            RenderPreview();
        }

        private void RenderPreview()
        {
            try
            {
                var bmp = _pdfService.RenderInvoiceBitmap(_activeInvoice, _editProfile, _currentScale);
                _canvasControl.SetInvoiceBitmap(bmp);
                CenterCanvas();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Preview render error: {ex.Message}");
            }
        }

        private void CenterCanvas()
        {
            int cx = Math.Max(24, (_canvasContainer.ClientSize.Width - _canvasControl.Width) / 2);
            int cy = Math.Max(24, (_canvasContainer.ClientSize.Height - _canvasControl.Height) / 2);
            _canvasControl.Location = new Point(cx, cy);
        }
        #endregion

        #region Top Toolbar Actions (Save, Reset, Export, Back)
        private async Task SaveChangesAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("Business / Brand Name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return;
            }

            UpdateProfileFromEditor();
            bool success = await _billingService.SaveBusinessProfileAsync(_editProfile);
            if (success)
            {
                _savedProfile = _editProfile.Clone();
                _isDirty = false;
                MessageBox.Show("Invoice branding and company settings have been saved successfully!\nAll future invoices and PDF previews will use these details.", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RenderPreview();
            }
            else
            {
                MessageBox.Show("Failed to save invoice settings. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetChangesWorkflow()
        {
            if (_isDirty)
            {
                var ask = MessageBox.Show("Are you sure you want to discard your unsaved edits and restore the last saved settings?", "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes) return;
            }

            _isPopulating = true;
            try
            {
                _editProfile = _savedProfile.Clone();
                _txtName.Text = _editProfile.BusinessName;
                _txtTagline.Text = _editProfile.Tagline;
                _txtAddress.Text = _editProfile.Address;
                _txtCity.Text = _editProfile.City;
                _txtCountry.Text = _editProfile.Country;
                _txtPhone.Text = _editProfile.Phone;
                _txtEmail.Text = _editProfile.Email;
                _txtGSTIN.Text = _editProfile.GSTIN;

                _txtBankName.Text = _editProfile.BankName;
                _txtBranchName.Text = _editProfile.BranchName;
                _txtAccountNo.Text = _editProfile.AccountNumber;
                _txtIFSC.Text = _editProfile.IFSC;
                _txtUPI.Text = _editProfile.UPIId;

                _txtLogoPath.Text = _editProfile.LogoPath;
                UpdateLogoThumbnail(_editProfile.LogoPath);
                SetAccentColor(_editProfile.AccentColorHex, triggerDebounce: false);

                _txtFooterNote.Text = _editProfile.FooterNote;
                _txtTerms.Text = _editProfile.DefaultTerms;

                _isDirty = false;
                RenderPreview();
            }
            finally
            {
                _isPopulating = false;
            }
        }

        private void ExportPdfWorkflow()
        {
            UpdateProfileFromEditor();
            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"{_activeInvoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf",
                Title = "Export Invoice PDF"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _pdfService.GenerateInvoicePdf(_activeInvoice, _editProfile, sfd.FileName);
                    var askOpen = MessageBox.Show($"Invoice PDF exported successfully to:\n{sfd.FileName}\n\nDo you want to open it now?", "Export Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (askOpen == DialogResult.Yes)
                    {
                        _pdfService.OpenPdf(sfd.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export PDF: {ex.Message}", "PDF Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void HandleBackNavigation()
        {
            if (_isDirty)
            {
                var ask = MessageBox.Show("You have unsaved invoice branding changes.\n\nDo you want to save before leaving?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (ask == DialogResult.Yes)
                {
                    _ = SaveChangesAsync();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if (ask == DialogResult.No)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
                // If Cancel, do nothing
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _debounceTimer?.Dispose();
        }
        #endregion
    }

    // =========================================================================================
    // CUSTOM DOUBLE-BUFFERED INVOICE CANVAS CONTROL (Renders Drop Shadow & Centered A4 Page)
    // =========================================================================================
    public class InvoiceCanvasControl : Control
    {
        private Bitmap? _invoiceBitmap;

        public InvoiceCanvasControl()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            UpdateStyles();
            BackColor = Color.Transparent;
        }

        public void SetInvoiceBitmap(Bitmap bmp)
        {
            _invoiceBitmap?.Dispose();
            _invoiceBitmap = bmp;
            Size = new Size(bmp.Width + 24, bmp.Height + 24);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_invoiceBitmap == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw Drop Shadow around A4 Paper
            var shadowRect = new Rectangle(12, 12, _invoiceBitmap.Width, _invoiceBitmap.Height);
            for (int i = 6; i >= 1; i--)
            {
                using var pShadow = new Pen(Color.FromArgb(18 - (i * 2), 0, 0, 0), i * 2f);
                g.DrawRectangle(pShadow, shadowRect.X - i, shadowRect.Y - i, shadowRect.Width + (i * 2), shadowRect.Height + (i * 2));
            }

            // Draw A4 Paper Bitmap
            var paperRect = new Rectangle(8, 8, _invoiceBitmap.Width, _invoiceBitmap.Height);
            g.DrawImage(_invoiceBitmap, paperRect);

            // Subtle 1px Paper Border
            using var pBorder = new Pen(Color.FromArgb(203, 213, 225), 1f);
            g.DrawRectangle(pBorder, paperRect);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _invoiceBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
