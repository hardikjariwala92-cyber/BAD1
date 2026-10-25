using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    public class DashboardForm : Form
    {
        private readonly User _currentUser;
        private readonly DashboardDataService _dataService = new();
        private readonly DatabaseService _dbService = DatabaseService.Instance;
        private readonly BillingDataService _billingService = BillingDataService.Instance;
        private readonly InvoicePdfService _pdfService = InvoicePdfService.Instance;

        // Layout Panels
        private Panel _sidebarPanel = null!;
        private Panel _headerPanel = null!;
        private Panel _contentPanel = null!;
        private Label _lblHeaderTitle = null!;
        private Label _lblProfileName = null!;
        private RoundedPanel _datePill = null!;
        private ThemeToggle _themeToggle = null!;
        private Panel? _menuPanel;
        private Panel? _profileContainer;
        private Panel? _avatarBox;
        private Label? _lblDate;
        private Panel? _brandBox;

        // Sidebar menu tracking
        private readonly List<Button> _menuButtons = new();
        private int _activeMenuIndex = 0;
        private readonly string[] _menuItems = { "Analytics", "Sales & Billing", "Products", "Messages", "Customers", "Settings", "Sign Out" };
        private readonly string[] _menuIcons = { "📊", "🧾", "🛍️", "✉️", "👥", "⚙️", "🚪" };

        // Theme-tracked controls
        private readonly List<RoundedPanel> _themeCards = new();
        private readonly List<Label> _themeTitleLabels = new();
        private readonly List<Label> _themeSubLabels = new();
        private readonly List<DataGridView> _themeGrids = new();
        private readonly List<TextBox> _themeTextBoxes = new();
        private readonly List<ComboBox> _themeComboBoxes = new();

        // Billing Page Controls
        private DataGridView? _gridInvoices;
        private Label? _lblValTotalSales;
        private Label? _lblValTotalInvoices;
        private Label? _lblValPaidInvoices;
        private Label? _lblValUnpaidInvoices;
        private Label? _lblValOutstanding;
        private Label? _lblTrendTotalSales;
        private Label? _lblTrendTotalInvoices;
        private Label? _lblTrendPaidInvoices;
        private Label? _lblTrendUnpaidInvoices;
        private Label? _lblTrendOutstanding;
        private Label? _lblBillingTableTitle;
        private int _selectedBillingCardIndex = -1;
        private RoundedSearchBox? _txtSearchInvoice;
        private Label? _lblStatusFilter;
        private RoundedComboBox? _cmbFilterStatus;
        private readonly List<RoundedPanel> _billingMetricCards = new();
        private RoundedPanel? _billingToolbarCard;
        private RoundedPanel? _billingTableCard;
        private ModernButton? _btnCreateInvoice;
        private ModernButton? _btnViewInvoices;
        private ModernButton? _btnManageCustomers;
        private ModernButton? _btnProfileSettings;
        private ModernButton? _btnRefreshBilling;
        private RoundedPanel? _billingGridWrapper;
        private DarkScrollBar? _billingScrollBar;
        private Label? _lblBillingCountBadge;
        private Label? _lblBillingPaidBadge;
        private Label? _lblBillingDueBadge;
        private Label? _lblBillingHint;
        private string _billingSortColumn = "InvoiceDate";
        private bool _billingSortAscending = false;
        private int _hoveredBillingRowIndex = -1;
        private List<Invoice> _currentBillingInvoices = new();

        // Products Page Controls
        private readonly List<RoundedPanel> _productMetricCards = new();
        private int _selectedProductCardIndex = -1;
        private Label? _lblValTotalProducts;
        private Label? _lblValActiveProducts;
        private Label? _lblValLowStock;
        private Label? _lblValTotalStockValue;
        private Label? _lblTrendTotalProducts;
        private Label? _lblTrendActiveProducts;
        private Label? _lblTrendLowStock;
        private Label? _lblTrendTotalStockValue;

        private RoundedPanel? _productsToolbarCard;
        private RoundedSearchBox? _txtSearchProduct;
        private Label? _lblProductCategoryFilter;
        private RoundedComboBox? _cmbProductCategory;
        private Label? _lblProductStatusFilter;
        private RoundedComboBox? _cmbProductStatus;
        private ModernButton? _btnProductAdd;
        private ModernButton? _btnProductScanBarcode;
        private ModernButton? _btnProductRefresh;

        private RoundedPanel? _productsTableCard;
        private Label? _lblProductTableTitle;
        private Label? _lblProductHint;
        private RoundedPanel? _productsGridWrapper;
        private DataGridView? _gridProducts;
        private DarkScrollBar? _productScrollBar;
        private int _hoveredProductRowIndex = -1;
        private string _productSortColumn = "ID";
        private bool _productSortAscending = true;
        private List<Product> _allLoadedProducts = new();
        private List<Product> _currentFilteredProducts = new();

        // Customers Page Controls
        private readonly List<RoundedPanel> _customerMetricCards = new();
        private int _selectedCustomerCardIndex = -1;
        private Label? _lblValTotalCustomers;
        private Label? _lblValActiveCustomers;
        private Label? _lblValTotalCities;
        private Label? _lblValTotalCountries;
        private Label? _lblTrendTotalCustomers;
        private Label? _lblTrendActiveCustomers;
        private Label? _lblTrendTotalCities;
        private Label? _lblTrendTotalCountries;

        private RoundedPanel? _customersToolbarCard;
        private RoundedSearchBox? _txtSearchCustomer;
        private Label? _lblCustomerCountryFilter;
        private RoundedComboBox? _cmbCustomerCountry;
        private ModernButton? _btnCustomerAdd;
        private ModernButton? _btnCustomerRefresh;

        private RoundedPanel? _customersTableCard;
        private Label? _lblCustomerTableTitle;
        private Label? _lblCustomerHint;
        private RoundedPanel? _customersGridWrapper;
        private DataGridView? _gridCustomers;
        private DarkScrollBar? _customerScrollBar;
        private int _hoveredCustomerRowIndex = -1;
        private string _customerSortColumn = "ID";
        private bool _customerSortAscending = true;
        private List<Customer> _allLoadedCustomers = new();
        private List<Customer> _currentFilteredCustomers = new();

        // Messages Page Controls
        private RoundedPanel? _messagesHeaderCard;
        private RoundedPanel? _messagesListCard;
        private ModernButton? _btnMessagesMarkAll;

        // Settings Page Controls
        private RoundedPanel? _settingsThemeCard;
        private RoundedPanel? _settingsCompanyCard;
        private RoundedPanel? _settingsCompanyPreviewPanel;
        private Label? _lblSettingsCurrentTheme;
        private RoundedPanel? _settingsInfoCard;
        private RoundedPanel? _settingsSessionCard;

        // Analytics Page Controls
        private readonly List<RoundedPanel> _analyticsTopCards = new();
        private readonly List<RoundedPanel> _analyticsRow2Cards = new();
        private RoundedPanel? _analyticsSalesCard;
        private RoundedPanel? _analyticsActivityCard;
        private RoundedPanel? _analyticsTableCard;
        private Label? _lblAnalyticsTableTitle;
        private RoundedPanel? _analyticsGridWrapper;
        private BarChartControl? _analyticsBarChart;
        private SplineChartControl? _analyticsSplineChart;
        private DataGridView? _analyticsGridOrders;
        private DarkScrollBar? _analyticsScrollBar;
        private Label? _lblAnalyticsRevTotal;
        private Label? _lblAnalyticsDeliveredCount;
        private Label? _lblAnalyticsProcessedCount;
        private Label? _lblAnalyticsCancelledCount;
        private ModernButton? _btnAnalyticsAddOrder;
        private int _hoveredOrderRowIndex = -1;
        private string _analyticsSortColumn = "Profile";
        private bool _analyticsSortAscending = true;
        private List<CustomerOrder> _currentAnalyticsOrders = new();

        // Analytics dynamic value labels & controls
        private RoundedPanel? _cardOrders;
        private Label? _lblValOrders;
        private Label? _lblTrendOrders;

        private RoundedPanel? _cardApproved;
        private Label? _lblValApproved;
        private Label? _lblTrendApproved;

        private RoundedPanel? _cardUsers;
        private Label? _lblValUsers;
        private DonutChartControl? _donutUsers;

        private RoundedPanel? _cardSubs;
        private Label? _lblValSubs;
        private DonutChartControl? _donutSubs;

        private RoundedPanel? _cardMonthTotal;
        private Label? _lblValMonthTotal;
        private Label? _lblTrendMonthTotal;

        private RoundedPanel? _cardRevenue;
        private Label? _lblValRevenue;
        private Label? _lblTrendRevenue;

        private RoundedPanel? _cardInvoices;
        private Label? _lblValInvoices;
        private Label? _lblSubInvoices;
        private Panel? _badgeInvoices;

        private RoundedPanel? _cardFunds;
        private Label? _lblValFunds;
        private Label? _lblSubFunds;
        private Panel? _badgeFunds;

        private Label? _lblYearSales;
        private Label? _lblYearAct;
        private RoundedSearchBox? _txtSearchAnalyticsOrders;
        private RoundedComboBox? _cmbAnalyticsStatus;
        private ModernButton? _btnRefreshAnalytics;
        private Label? _lblAnalyticsOrderCount;
        private System.Windows.Forms.Timer? _analyticsRefreshTimer;
        private DateTime? _currentStartDate = new DateTime(2022, 8, 1);
        private DateTime? _currentEndDate = new DateTime(2022, 8, 31);
        private readonly System.Globalization.CultureInfo _inCulture = new("en-IN");
        public bool IsSignedOut { get; private set; }

        public void SignOut()
        {
            IsSignedOut = true;
            Close();
        }

        public DashboardForm(User? user = null)
        {
            _currentUser = user ?? new User("admin", "Kristi Kamlykova", "Administrator", "KK");

            Text = "Business Analytics Dashboard — S TOM'S Platform";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1340, 860);
            MinimumSize = new Size(1150, 720);
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;

            ThemeManager.OnThemeChanged += OnThemeUpdated;

            BuildLayout();
            ApplyThemeColors();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ThemeManager.SaveTheme(ThemeManager.CurrentTheme);
            _analyticsRefreshTimer?.Stop();
            _analyticsRefreshTimer?.Dispose();
            ThemeManager.OnThemeChanged -= OnThemeUpdated;
            base.OnFormClosed(e);
        }

        private void OnThemeUpdated(AppTheme theme)
        {
            ApplyThemeColors();
        }

        private void ApplyThemeColors()
        {
            SuspendLayout();
            try
            {
                BackColor = ThemeManager.Background;
                if (_sidebarPanel != null) _sidebarPanel.BackColor = ThemeManager.SidebarBg;
                if (_menuPanel != null) _menuPanel.BackColor = ThemeManager.SidebarBg;
                if (_brandBox != null)
                {
                    _brandBox.BackColor = ThemeManager.SidebarBg;
                    _brandBox.Invalidate();
                }
                if (_headerPanel != null) _headerPanel.BackColor = ThemeManager.Background;
                if (_contentPanel != null) _contentPanel.BackColor = ThemeManager.Background;

                if (_profileContainer != null) _profileContainer.BackColor = ThemeManager.Background;
                if (_avatarBox != null)
                {
                    _avatarBox.BackColor = ThemeManager.Background;
                    _avatarBox.Invalidate();
                }

                if (_lblHeaderTitle != null)
                {
                    _lblHeaderTitle.ForeColor = ThemeManager.TextPrimary;
                    _lblHeaderTitle.BackColor = Color.Transparent;
                }
                if (_lblProfileName != null)
                {
                    _lblProfileName.ForeColor = ThemeManager.TextPrimary;
                    _lblProfileName.BackColor = Color.Transparent;
                }

                if (_datePill != null)
                {
                    _datePill.BackColor = ThemeManager.Background;
                    _datePill.CustomBackgroundColor = ThemeManager.CardBg;
                    _datePill.CustomBorderColor = ThemeManager.CardBorder;
                    _datePill.Invalidate(true);
                }
                if (_lblDate != null)
                {
                    _lblDate.ForeColor = ThemeManager.TextSecondary;
                    _lblDate.BackColor = Color.Transparent;
                }

                if (_themeCards != null)
                {
                    foreach (var card in _themeCards)
                    {
                        if (card == null) continue;
                        card.BackColor = ThemeManager.CardBg;
                        card.CustomBackgroundColor = ThemeManager.CardBg;
                        card.CustomBorderColor = ThemeManager.CardBorder;
                        card.Invalidate(true);
                    }
                }

                if (_themeTitleLabels != null)
                {
                    foreach (var lbl in _themeTitleLabels)
                    {
                        if (lbl == null) continue;
                        lbl.ForeColor = ThemeManager.TextPrimary;
                        lbl.BackColor = Color.Transparent;
                    }
                }

                if (_themeSubLabels != null)
                {
                    foreach (var lbl in _themeSubLabels)
                    {
                        if (lbl == null) continue;
                        lbl.ForeColor = ThemeManager.TextSecondary;
                        lbl.BackColor = Color.Transparent;
                    }
                }

                if (_themeGrids != null)
                {
                    foreach (var grid in _themeGrids)
                    {
                        if (grid == null) continue;
                        grid.BackgroundColor = ThemeManager.CardBg;
                        grid.DefaultCellStyle.BackColor = ThemeManager.CardBg;
                        grid.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
                        grid.DefaultCellStyle.SelectionBackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(45, 50, 75) : Color.FromArgb(230, 235, 255);
                        grid.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
                        grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TableHeaderBg;
                        grid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
                        grid.GridColor = ThemeManager.CardBorder;
                    }
                }

                if (_themeTextBoxes != null)
                {
                    foreach (var tb in _themeTextBoxes)
                    {
                        if (tb == null) continue;
                        tb.BackColor = ThemeManager.CardBg;
                        tb.ForeColor = ThemeManager.TextPrimary;
                    }
                }

                if (_themeComboBoxes != null)
                {
                    foreach (var cb in _themeComboBoxes)
                    {
                        if (cb == null) continue;
                        cb.BackColor = ThemeManager.CardBg;
                        cb.ForeColor = ThemeManager.TextPrimary;
                    }
                }

                // Update sidebar menu button colors
                if (_menuButtons != null)
                {
                    for (int i = 0; i < _menuButtons.Count; i++)
                    {
                        if (_menuButtons[i] == null) continue;
                        bool isActive = i == _activeMenuIndex;
                        _menuButtons[i].ForeColor = isActive ? Color.White : ThemeManager.TextSecondary;
                        _menuButtons[i].BackColor = isActive ? ThemeManager.AccentBlue : ThemeManager.SidebarBg;
                        _menuButtons[i].Font = new Font("Segoe UI", 9.5f, isActive ? FontStyle.Bold : FontStyle.Regular);
                    }
                }

                UpdateAnalyticsTableTheme();

                _txtSearchInvoice?.ApplyTheme(ThemeManager.CurrentTheme);
                _cmbFilterStatus?.ApplyTheme(ThemeManager.CurrentTheme);
                _txtSearchProduct?.ApplyTheme(ThemeManager.CurrentTheme);
                _cmbProductCategory?.ApplyTheme(ThemeManager.CurrentTheme);
                _cmbProductStatus?.ApplyTheme(ThemeManager.CurrentTheme);
                _txtSearchCustomer?.ApplyTheme(ThemeManager.CurrentTheme);
                _cmbCustomerCountry?.ApplyTheme(ThemeManager.CurrentTheme);
                if (_btnRefreshBilling != null)
                {
                    bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
                    _btnRefreshBilling.IsOutlined = false;
                    _btnRefreshBilling.NormalColor = isDark ? Color.FromArgb(40, 48, 72) : Color.FromArgb(55, 65, 81);
                    _btnRefreshBilling.HoverColor = isDark ? Color.FromArgb(55, 65, 95) : Color.FromArgb(75, 85, 99);
                    _btnRefreshBilling.PressedColor = isDark ? Color.FromArgb(70, 82, 120) : Color.FromArgb(31, 41, 55);
                    _btnRefreshBilling.ForeColor = Color.White;
                    _btnRefreshBilling.TextHoverColor = Color.White;
                    _btnRefreshBilling.Invalidate();
                }

                if (_billingGridWrapper != null)
                {
                    bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
                    _billingGridWrapper.CustomBackgroundColor = isDark ? Color.FromArgb(20, 22, 33) : Color.FromArgb(248, 250, 252);
                    _billingGridWrapper.CustomBorderColor = isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240);
                    _billingGridWrapper.Invalidate();
                }
                if (_billingScrollBar != null)
                {
                    _billingScrollBar.Invalidate();
                }
                if (_lblBillingCountBadge != null) _lblBillingCountBadge.Invalidate();
                if (_lblBillingPaidBadge != null) _lblBillingPaidBadge.Invalidate();
                if (_lblBillingDueBadge != null) _lblBillingDueBadge.Invalidate();
                if (_gridInvoices != null)
                {
                    _gridInvoices.Invalidate();
                }

                if (_customersGridWrapper != null)
                {
                    bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
                    _customersGridWrapper.CustomBackgroundColor = isDark ? Color.FromArgb(20, 22, 33) : Color.FromArgb(248, 250, 252);
                    _customersGridWrapper.CustomBorderColor = isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240);
                    _customersGridWrapper.Invalidate();
                }
                if (_customerScrollBar != null)
                {
                    _customerScrollBar.Invalidate();
                }
                if (_gridCustomers != null)
                {
                    _gridCustomers.Invalidate();
                }

                if (_lblSettingsCurrentTheme != null)
                {
                    _lblSettingsCurrentTheme.Text = $"Current Theme:  {ThemeManager.CurrentTheme}";
                    _lblSettingsCurrentTheme.ForeColor = ThemeManager.AccentBlue;
                }

                if (_settingsCompanyPreviewPanel != null)
                {
                    _ = RefreshCompanySettingsPreviewAsync(_settingsCompanyPreviewPanel);
                }

                if (_contentPanel != null) _contentPanel.Invalidate(true);
                Invalidate(true);
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void BuildLayout()
        {
            // 1. Sidebar (Left: 220px)
            BuildSidebar();

            // 2. Header (Top: 75px)
            BuildHeader();

            // 3. Main Content Area (Fill)
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Background,
                AutoScroll = true,
                Padding = new Padding(25, 0, 25, 25)
            };
            _contentPanel.Resize += (s, e) => UpdateLayoutForCurrentPage();
            Controls.Add(_contentPanel);
            _contentPanel.BringToFront();

            // Load Analytics page by default
            LoadAnalyticsPage();
        }

        #region Sidebar
        private void BuildSidebar()
        {
            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = ThemeManager.SidebarBg,
                Padding = new Padding(15, 20, 15, 20)
            };
            Controls.Add(_sidebarPanel);

            // Brand Logo & Name
            _brandBox = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };
            _brandBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Petal logo
                using (var b1 = new SolidBrush(ThemeManager.AccentPurple))
                {
                    g.FillEllipse(b1, 5, 8, 14, 14);
                    g.FillEllipse(b1, 21, 24, 14, 14);
                }
                using (var b2 = new SolidBrush(ThemeManager.AccentPink))
                {
                    g.FillEllipse(b2, 21, 8, 14, 14);
                    g.FillEllipse(b2, 5, 24, 14, 14);
                }

                using var font = new Font("Segoe UI", 13.5f, FontStyle.Bold);
                using var brush = new SolidBrush(ThemeManager.TextPrimary);
                g.DrawString("Business", font, brush, 45, 10);
            };
            _sidebarPanel.Controls.Add(_brandBox);

            // Menu Items Container
            _menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.SidebarBg,
                Padding = new Padding(0, 15, 0, 0)
            };
            _sidebarPanel.Controls.Add(_menuPanel);
            _menuPanel.BringToFront();

            int btnY = 10;
            for (int i = 0; i < _menuItems.Length; i++)
            {
                int menuIndex = i;
                string title = _menuItems[i];
                string icon = _menuIcons[i];
                bool isActive = i == 0; // Analytics is active by default

                var btn = new Button
                {
                    Text = $"   {icon}   {title}",
                    TextAlign = ContentAlignment.MiddleLeft,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, isActive ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isActive ? Color.White : ThemeManager.TextSecondary,
                    BackColor = isActive ? ThemeManager.AccentBlue : ThemeManager.SidebarBg,
                    Size = new Size(190, 42),
                    Location = new Point(0, btnY),
                    Cursor = Cursors.Hand,
                    Tag = menuIndex
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += MenuButton_Click;

                _menuButtons.Add(btn);
                _menuPanel.Controls.Add(btn);
                btnY += 48;
            }
        }

        private void MenuButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int index) return;

            if (index == 6) // Sign Out
            {
                var confirm = MessageBox.Show("Are you sure you want to sign out?", "Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    SignOut();
                }
                return;
            }

            if (index == _activeMenuIndex) return;

            // Update active state
            _activeMenuIndex = index;
            for (int i = 0; i < _menuButtons.Count; i++)
            {
                bool isActive = i == _activeMenuIndex;
                _menuButtons[i].ForeColor = isActive ? Color.White : ThemeManager.TextSecondary;
                _menuButtons[i].BackColor = isActive ? ThemeManager.AccentBlue : ThemeManager.SidebarBg;
                _menuButtons[i].Font = new Font("Segoe UI", 9.5f, isActive ? FontStyle.Bold : FontStyle.Regular);
            }

            // Update header title
            _lblHeaderTitle.Text = _menuItems[index];

            // Clear content and load new page
            ClearContentPanel();

            switch (index)
            {
                case 0: LoadAnalyticsPage(); break;
                case 1: LoadBillingPage(); break;
                case 2: LoadProductsPage(); break;
                case 3: LoadMessagesPage(); break;
                case 4: LoadCustomersPage(); break;
                case 5: LoadSettingsPage(); break;
            }

            ApplyThemeColors();
        }

        public void NavigateToMenu(int menuIndex)
        {
            if (menuIndex >= 0 && menuIndex < _menuButtons.Count)
            {
                MenuButton_Click(_menuButtons[menuIndex], EventArgs.Empty);
            }
        }

        private void ClearContentPanel()
        {
            // Remove theme-tracked controls that belong to the content panel
            _themeCards.Clear();
            _themeTitleLabels.RemoveAll(l => l.FindForm() == this && _contentPanel.Contains(l) || l.Parent != null && IsDescendantOf(l, _contentPanel));
            _themeSubLabels.RemoveAll(l => l.FindForm() == this && _contentPanel.Contains(l) || l.Parent != null && IsDescendantOf(l, _contentPanel));
            _themeGrids.Clear();
            _themeTextBoxes.Clear();
            _themeComboBoxes.Clear();

            // Actually clear from the theme label lists — just reset all
            _themeTitleLabels.Clear();
            _themeSubLabels.Clear();

            // Clear page-specific tracking
            _billingMetricCards.Clear();
            _selectedBillingCardIndex = -1;
            _lblBillingTableTitle = null;
            _lblValTotalSales = null;
            _lblValTotalInvoices = null;
            _lblValPaidInvoices = null;
            _lblValUnpaidInvoices = null;
            _lblValOutstanding = null;
            _lblTrendTotalSales = null;
            _lblTrendTotalInvoices = null;
            _lblTrendPaidInvoices = null;
            _lblTrendUnpaidInvoices = null;
            _lblTrendOutstanding = null;
            _billingToolbarCard = null;
            _billingTableCard = null;
            _gridInvoices = null;
            _txtSearchInvoice = null;
            _lblStatusFilter = null;
            _cmbFilterStatus = null;
            _btnCreateInvoice = null;
            _btnViewInvoices = null;
            _btnManageCustomers = null;
            _btnProfileSettings = null;
            _btnRefreshBilling = null;
            _billingGridWrapper = null;
            _billingScrollBar = null;
            _lblBillingCountBadge = null;
            _lblBillingPaidBadge = null;
            _lblBillingDueBadge = null;
            _lblBillingHint = null;
            _hoveredBillingRowIndex = -1;
            _currentBillingInvoices.Clear();

            _productMetricCards.Clear();
            _selectedProductCardIndex = -1;
            _lblValTotalProducts = null;
            _lblValActiveProducts = null;
            _lblValLowStock = null;
            _lblValTotalStockValue = null;
            _lblTrendTotalProducts = null;
            _lblTrendActiveProducts = null;
            _lblTrendLowStock = null;
            _lblTrendTotalStockValue = null;
            _productsToolbarCard = null;
            _txtSearchProduct = null;
            _lblProductCategoryFilter = null;
            _cmbProductCategory = null;
            _lblProductStatusFilter = null;
            _cmbProductStatus = null;
            _btnProductAdd = null;
            _btnProductScanBarcode = null;
            _btnProductRefresh = null;
            _productsTableCard = null;
            _lblProductTableTitle = null;
            _lblProductHint = null;
            _productsGridWrapper = null;
            _gridProducts = null;
            _productScrollBar = null;
            _hoveredProductRowIndex = -1;
            _allLoadedProducts.Clear();
            _currentFilteredProducts.Clear();

            _customerMetricCards.Clear();
            _selectedCustomerCardIndex = -1;
            _lblValTotalCustomers = null;
            _lblValActiveCustomers = null;
            _lblValTotalCities = null;
            _lblValTotalCountries = null;
            _lblTrendTotalCustomers = null;
            _lblTrendActiveCustomers = null;
            _lblTrendTotalCities = null;
            _lblTrendTotalCountries = null;
            _customersToolbarCard = null;
            _txtSearchCustomer = null;
            _lblCustomerCountryFilter = null;
            _cmbCustomerCountry = null;
            _btnCustomerAdd = null;
            _btnCustomerRefresh = null;
            _customersTableCard = null;
            _lblCustomerTableTitle = null;
            _lblCustomerHint = null;
            _customersGridWrapper = null;
            _gridCustomers = null;
            _customerScrollBar = null;
            _hoveredCustomerRowIndex = -1;
            _allLoadedCustomers.Clear();
            _currentFilteredCustomers.Clear();

            _messagesHeaderCard = null;
            _messagesListCard = null;
            _btnMessagesMarkAll = null;

            _settingsThemeCard = null;
            _settingsCompanyCard = null;
            _settingsCompanyPreviewPanel = null;
            _lblSettingsCurrentTheme = null;
            _settingsInfoCard = null;
            _settingsSessionCard = null;

            _analyticsTopCards.Clear();
            _analyticsRow2Cards.Clear();
            _analyticsSalesCard = null;
            _analyticsActivityCard = null;
            _analyticsTableCard = null;
            _lblAnalyticsTableTitle = null;
            _analyticsGridWrapper = null;
            _analyticsBarChart = null;
            _analyticsSplineChart = null;
            _analyticsGridOrders = null;
            _analyticsScrollBar = null;
            _lblAnalyticsRevTotal = null;
            _lblAnalyticsDeliveredCount = null;
            _lblAnalyticsProcessedCount = null;
            _lblAnalyticsCancelledCount = null;
            _btnAnalyticsAddOrder = null;
            _hoveredOrderRowIndex = -1;

            _contentPanel.Controls.Clear();
        }

        private static bool IsDescendantOf(Control child, Control parent)
        {
            Control? current = child.Parent;
            while (current != null)
            {
                if (current == parent) return true;
                current = current.Parent;
            }
            return false;
        }
        #endregion

        #region Header
        private void BuildHeader()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = ThemeManager.Background,
                Padding = new Padding(25, 0, 25, 0)
            };
            Controls.Add(_headerPanel);

            // Title
            _lblHeaderTitle = new Label
            {
                Text = "Analytics",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(25, 19)
            };
            _headerPanel.Controls.Add(_lblHeaderTitle);

            // Date Range Pill - perfectly aligned at Y=19 with height 36 (Interactive calendar selector)
            _datePill = new RoundedPanel
            {
                BorderRadius = 10,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder,
                Size = new Size(225, 36),
                Location = new Point(_lblHeaderTitle.Right + 20, 19),
                Cursor = Cursors.Hand,
                EnableHoverEffect = true
            };
            _lblDate = new Label
            {
                Text = "01.08.2022 - 31.08.2022  📅",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            _lblDate.MouseEnter += (s, e) =>
            {
                _datePill.CustomBorderColor = ThemeManager.AccentBlue;
                _datePill.Invalidate();
            };
            _lblDate.MouseLeave += (s, e) =>
            {
                _datePill.CustomBorderColor = ThemeManager.CardBorder;
                _datePill.Invalidate();
            };
            _lblDate.Click += (s, e) => ShowDateRangePicker();
            _datePill.Click += (s, e) => ShowDateRangePicker();

            _datePill.Controls.Add(_lblDate);
            _themeSubLabels.Add(_lblDate);
            _headerPanel.Controls.Add(_datePill);

            // Keep Date Pill dynamically next to title if title changes
            _lblHeaderTitle.SizeChanged += (s, e) =>
            {
                _datePill.Location = new Point(_lblHeaderTitle.Right + 20, 19);
            };

            // User Profile Container on Right (Docked Right, Height 75)
            _profileContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 330,
                BackColor = ThemeManager.Background
            };
            _headerPanel.Controls.Add(_profileContainer);

            // 1. Theme Toggle - Centered vertically on pixel 37 (Height: 30, Y: 22)
            _themeToggle = new ThemeToggle
            {
                Size = new Size(64, 30),
                Location = new Point(15, 22)
            };
            _profileContainer.Controls.Add(_themeToggle);

            // 2. User Avatar Circle - Centered vertically on pixel 37 (Height: 36, Y: 19)
            _avatarBox = new Panel
            {
                Size = new Size(36, 36),
                Location = new Point(95, 19),
                BackColor = ThemeManager.Background
            };
            _avatarBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var b = new SolidBrush(Color.FromArgb(234, 150, 100));
                g.FillEllipse(b, 1, 1, 34, 34);

                using var font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_currentUser.AvatarInitials, font, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
            };
            _profileContainer.Controls.Add(_avatarBox);

            // 3. User Profile Name - Centered vertically on pixel 37 (Height: 36, Y: 19, MiddleLeft text)
            _lblProfileName = new Label
            {
                Text = _currentUser.FullName,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(180, 36),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(140, 19)
            };
            _profileContainer.Controls.Add(_lblProfileName);
        }
        #endregion

        // =====================================================
        // PAGE: ANALYTICS (Main Dashboard)
        // =====================================================
        #region Analytics Page
        private void LoadAnalyticsPage()
        {
            int cardWidth = 245;
            int cardHeight = 110;
            int gap = 20;

            _analyticsTopCards.Clear();
            _analyticsRow2Cards.Clear();

            // ROW 1: 4 Top Metric Cards (With Hover Effects & Dynamic Out Labels)
            _cardOrders = CreateMetricCard("Orders", "201", "↑ 8.2% since last month", true, "📦", 0, 0, cardWidth, cardHeight, out _lblValOrders!, out _lblTrendOrders!);
            _cardOrders.OnCardClicked = (c) => NavigateToMenu(1); // Jump to Sales & Billing
            _analyticsTopCards.Add(_cardOrders);
            _contentPanel.Controls.Add(_cardOrders);

            _cardApproved = CreateMetricCard("Approved", "36", "↑ 3.4% since last month", true, "✅", cardWidth + gap, 0, cardWidth, cardHeight, out _lblValApproved!, out _lblTrendApproved!);
            _analyticsTopCards.Add(_cardApproved);
            _contentPanel.Controls.Add(_cardApproved);

            _cardUsers = CreateDonutMetricCard("Users", "4.890", new List<DonutSlice>
            {
                new("New", 63f, Color.FromArgb(245, 158, 11)),
                new("Returning", 25f, Color.FromArgb(234, 88, 12)),
                new("Inactive", 12f, Color.FromArgb(100, 116, 139))
            }, (cardWidth + gap) * 2, 0, cardWidth + 30, cardHeight, out _lblValUsers!, out _donutUsers!);
            _cardUsers.OnCardClicked = (c) => NavigateToMenu(4); // Jump to Customers
            _analyticsTopCards.Add(_cardUsers);
            _contentPanel.Controls.Add(_cardUsers);

            _cardSubs = CreateDonutMetricCard("Subscriptions", "1.201", new List<DonutSlice>
            {
                new("Paid", 70f, ThemeManager.AccentBlue),
                new("Trial", 30f, ThemeManager.AccentCyan)
            }, (cardWidth + gap) * 3 + 30, 0, cardWidth + 30, cardHeight, out _lblValSubs!, out _donutSubs!);
            _cardSubs.OnCardClicked = (c) => NavigateToMenu(2); // Jump to Products
            _analyticsTopCards.Add(_cardSubs);
            _contentPanel.Controls.Add(_cardSubs);

            // ROW 2: Sub-metrics
            int row2Y = cardHeight + gap;
            _cardMonthTotal = CreateMetricCard("Month total", "₹11,64,084", "↓ 0.2% since last month", false, "₹", 0, row2Y, cardWidth, cardHeight, out _lblValMonthTotal!, out _lblTrendMonthTotal!);
            _analyticsRow2Cards.Add(_cardMonthTotal);
            _contentPanel.Controls.Add(_cardMonthTotal);

            _cardRevenue = CreateMetricCard("Revenue", "₹1,352", "↓ 1.2% since last month", false, "💳", cardWidth + gap, row2Y, cardWidth, cardHeight, out _lblValRevenue!, out _lblTrendRevenue!);
            _analyticsRow2Cards.Add(_cardRevenue);
            _contentPanel.Controls.Add(_cardRevenue);

            _cardInvoices = CreateCircleProgressCard("Paid Invoices", "₹1,21,745.00", "Current Financial Year", "+15%", ThemeManager.AccentPurple, (cardWidth + gap) * 2, row2Y, cardWidth + 30, cardHeight, out _lblValInvoices!, out _lblSubInvoices!, out _badgeInvoices!);
            _cardInvoices.Cursor = Cursors.Hand;
            _cardInvoices.OnCardClicked = (c) => NavigateToMenu(1); // Jump to Sales & Billing
            _analyticsRow2Cards.Add(_cardInvoices);
            _contentPanel.Controls.Add(_cardInvoices);

            _cardFunds = CreateCircleProgressCard("Funds received", "₹1,50,256.23", "Current Financial Year", "+59%", ThemeManager.AccentGreen, (cardWidth + gap) * 3 + 30, row2Y, cardWidth + 30, cardHeight, out _lblValFunds!, out _lblSubFunds!, out _badgeFunds!);
            _cardFunds.Cursor = Cursors.Hand;
            _cardFunds.OnCardClicked = (c) => NavigateToMenu(1); // Jump to Sales & Billing
            _analyticsRow2Cards.Add(_cardFunds);
            _contentPanel.Controls.Add(_cardFunds);

            // ROW 3: Large Charts (Sales Dynamics & User Activity)
            int row3Y = row2Y + cardHeight + gap;
            int chartCardWidth = 515;
            int chartCardHeight = 270;

            // Chart 1: Sales Dynamics (Bar Chart with Interactive Year Selector & Tooltips)
            _analyticsSalesCard = new RoundedPanel
            {
                Location = new Point(0, row3Y),
                Size = new Size(chartCardWidth, chartCardHeight),
                BorderRadius = 14,
                BorderWidth = 1,
                EnableHoverEffect = true
            };
            _themeCards.Add(_analyticsSalesCard);

            var lblSalesTitle = new Label
            {
                Text = "Sales dynamics",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(18, 14),
                AutoSize = true
            };
            _analyticsSalesCard.Controls.Add(lblSalesTitle);
            _themeTitleLabels.Add(lblSalesTitle);

            _lblYearSales = new Label
            {
                Text = "2021  ▼",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                BackColor = Color.Transparent,
                Location = new Point(chartCardWidth - 85, 14),
                AutoSize = true,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _lblYearSales.Click += (s, e) => ShowYearSelector(_lblYearSales, year =>
            {
                _lblYearSales.Text = $"{year}  ▼";
                var points = _dbService.GetSalesDynamics(year);
                _analyticsBarChart?.SetData(points);
            });
            _analyticsSalesCard.Controls.Add(_lblYearSales);
            _themeSubLabels.Add(_lblYearSales);

            _analyticsBarChart = new BarChartControl
            {
                Location = new Point(10, 45),
                Size = new Size(chartCardWidth - 20, chartCardHeight - 55),
                DataPoints = _dbService.GetSalesDynamics(2021),
                CurrencySymbol = "₹"
            };
            _analyticsSalesCard.Controls.Add(_analyticsBarChart);
            _contentPanel.Controls.Add(_analyticsSalesCard);

            // Chart 2: Overall User Activity (Spline Chart with Interactive Year Selector & Guidelines)
            _analyticsActivityCard = new RoundedPanel
            {
                Location = new Point(chartCardWidth + gap, row3Y),
                Size = new Size(chartCardWidth + 60, chartCardHeight),
                BorderRadius = 14,
                BorderWidth = 1,
                EnableHoverEffect = true
            };
            _themeCards.Add(_analyticsActivityCard);

            var lblActTitle = new Label
            {
                Text = "Overall User Activity",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(18, 14),
                AutoSize = true
            };
            _analyticsActivityCard.Controls.Add(lblActTitle);
            _themeTitleLabels.Add(lblActTitle);

            _lblYearAct = new Label
            {
                Text = "2021  ▼",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentPurple,
                BackColor = Color.Transparent,
                Location = new Point(chartCardWidth - 25, 14),
                AutoSize = true,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _lblYearAct.Click += (s, e) => ShowYearSelector(_lblYearAct, year =>
            {
                _lblYearAct.Text = $"{year}  ▼";
                var curve = _dbService.GetActivityCurve(year);
                _analyticsSplineChart?.SetData(curve);
            });
            _analyticsActivityCard.Controls.Add(_lblYearAct);
            _themeSubLabels.Add(_lblYearAct);

            _analyticsSplineChart = new SplineChartControl
            {
                Location = new Point(10, 45),
                Size = new Size(chartCardWidth + 40, chartCardHeight - 55),
                NormalizedPoints = _dbService.GetActivityCurve(2021),
                LineColor = Color.FromArgb(190, 80, 240)
            };
            _analyticsActivityCard.Controls.Add(_analyticsSplineChart);
            _contentPanel.Controls.Add(_analyticsActivityCard);

            // ROW 4: Customer Order Table with Working Search, Filter, Sort, Hover, and Details Modal
            int row4Y = row3Y + chartCardHeight + gap;
            int tableCardWidth = chartCardWidth + gap + chartCardWidth + 60;
            int tableCardHeight = 360;

            _analyticsTableCard = new RoundedPanel
            {
                Location = new Point(0, row4Y),
                Size = new Size(tableCardWidth, tableCardHeight),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_analyticsTableCard);

            _lblAnalyticsTableTitle = new Label
            {
                Text = "Customer order",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(18, 14),
                AutoSize = true
            };
            _lblAnalyticsTableTitle.SizeChanged += (s, e) => LayoutTableTopBadges();
            _analyticsTableCard.Controls.Add(_lblAnalyticsTableTitle);
            _themeTitleLabels.Add(_lblAnalyticsTableTitle);

            // Mini Stats Badges next to title (Dynamic Non-Colliding Layout with CSS pill badges)
            _lblAnalyticsOrderCount = new Label
            {
                Text = "10 Orders",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Padding = new Padding(8, 3, 8, 3),
                Cursor = Cursors.Hand,
                AutoSize = true
            };
            AttachPillBadgePaint(_lblAnalyticsOrderCount, () => (
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(37, 43, 67) : Color.FromArgb(241, 245, 249),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(59, 66, 101) : Color.FromArgb(203, 213, 225),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105)
            ));
            _lblAnalyticsOrderCount.Click += (s, e) => { if (_cmbAnalyticsStatus != null) _cmbAnalyticsStatus.SelectedItem = "All"; };
            _lblAnalyticsOrderCount.MouseWheel += (s, e) => _cmbAnalyticsStatus?.HandleWheelScroll(e.Delta);
            _lblAnalyticsOrderCount.SizeChanged += (s, e) => LayoutTableTopBadges();
            _analyticsTableCard.Controls.Add(_lblAnalyticsOrderCount);

            _lblAnalyticsDeliveredCount = new Label
            {
                Text = "● 0 Delivered",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Padding = new Padding(8, 3, 8, 3),
                Cursor = Cursors.Hand,
                AutoSize = true
            };
            AttachPillBadgePaint(_lblAnalyticsDeliveredCount, () => (
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(19, 56, 39) : Color.FromArgb(220, 252, 231),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(16, 185, 129) : Color.FromArgb(134, 239, 172),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(52, 211, 153) : Color.FromArgb(21, 128, 61)
            ));
            _lblAnalyticsDeliveredCount.Click += (s, e) => { if (_cmbAnalyticsStatus != null) _cmbAnalyticsStatus.SelectedItem = "Delivered"; };
            _lblAnalyticsDeliveredCount.MouseWheel += (s, e) => _cmbAnalyticsStatus?.HandleWheelScroll(e.Delta);
            _lblAnalyticsDeliveredCount.SizeChanged += (s, e) => LayoutTableTopBadges();
            _analyticsTableCard.Controls.Add(_lblAnalyticsDeliveredCount);

            _lblAnalyticsProcessedCount = new Label
            {
                Text = "● 0 Processed",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Padding = new Padding(8, 3, 8, 3),
                Cursor = Cursors.Hand,
                AutoSize = true
            };
            AttachPillBadgePaint(_lblAnalyticsProcessedCount, () => (
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(56, 42, 16) : Color.FromArgb(254, 243, 199),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(245, 158, 11) : Color.FromArgb(253, 230, 138),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(251, 191, 36) : Color.FromArgb(180, 83, 9)
            ));
            _lblAnalyticsProcessedCount.Click += (s, e) => { if (_cmbAnalyticsStatus != null) _cmbAnalyticsStatus.SelectedItem = "Processed"; };
            _lblAnalyticsProcessedCount.MouseWheel += (s, e) => _cmbAnalyticsStatus?.HandleWheelScroll(e.Delta);
            _lblAnalyticsProcessedCount.SizeChanged += (s, e) => LayoutTableTopBadges();
            _analyticsTableCard.Controls.Add(_lblAnalyticsProcessedCount);

            _lblAnalyticsCancelledCount = new Label
            {
                Text = "● 0 Cancelled",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Padding = new Padding(8, 3, 8, 3),
                Cursor = Cursors.Hand,
                AutoSize = true
            };
            AttachPillBadgePaint(_lblAnalyticsCancelledCount, () => (
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(59, 26, 30) : Color.FromArgb(254, 226, 226),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(239, 68, 68) : Color.FromArgb(254, 202, 202),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(185, 28, 28)
            ));
            _lblAnalyticsCancelledCount.Click += (s, e) => { if (_cmbAnalyticsStatus != null) _cmbAnalyticsStatus.SelectedItem = "Cancelled"; };
            _lblAnalyticsCancelledCount.MouseWheel += (s, e) => _cmbAnalyticsStatus?.HandleWheelScroll(e.Delta);
            _lblAnalyticsCancelledCount.SizeChanged += (s, e) => LayoutTableTopBadges();
            _analyticsTableCard.Controls.Add(_lblAnalyticsCancelledCount);

            _lblAnalyticsRevTotal = new Label
            {
                Text = "Total: $0",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Padding = new Padding(8, 3, 8, 3),
                AutoSize = true
            };
            AttachPillBadgePaint(_lblAnalyticsRevTotal, () => (
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(238, 242, 255),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(99, 102, 241) : Color.FromArgb(199, 210, 254),
                ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(165, 180, 252) : Color.FromArgb(79, 70, 229)
            ));
            _lblAnalyticsRevTotal.SizeChanged += (s, e) => LayoutTableTopBadges();
            _analyticsTableCard.Controls.Add(_lblAnalyticsRevTotal);

            // Right Toolbar:
            // 1. + Add Order Button (Modern CSS Pill with Linear Gradient & Hover Glow)
            _btnAnalyticsAddOrder = new ModernButton
            {
                Text = "+ Order",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(86, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 15,
                UseGradient = true,
                GradientColorStart = Color.FromArgb(67, 97, 238),
                GradientColorEnd = Color.FromArgb(79, 70, 229),
                HoverGradientColorStart = Color.FromArgb(99, 102, 241),
                HoverGradientColorEnd = Color.FromArgb(129, 140, 248),
                PressedColor = Color.FromArgb(55, 48, 163),
                HasGlowOnHover = true,
                GlowColor = Color.FromArgb(129, 140, 248)
            };
            _btnAnalyticsAddOrder.Click += async (s, e) => await PromptAddCustomerOrderAsync();
            _analyticsTableCard.Controls.Add(_btnAnalyticsAddOrder);

            // 2. Search box (Modern CSS Rounded Pill Search with Lens Icon, Focus Ring & Clear Button)
            _txtSearchAnalyticsOrders = new RoundedSearchBox
            {
                PlaceholderText = "Search customer, city, status, price...",
                Font = new Font("Segoe UI", 9f),
                Size = new Size(210, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 15
            };
            _txtSearchAnalyticsOrders.TextChanged += async (s, e) => await FilterAnalyticsOrdersAsync();
            _txtSearchAnalyticsOrders.SearchTriggered += async (s, e) => await FilterAnalyticsOrdersAsync();
            _analyticsTableCard.Controls.Add(_txtSearchAnalyticsOrders);
            _themeTextBoxes.Add(_txtSearchAnalyticsOrders.InnerTextBox);

            // 3. Status Filter ComboBox (Modern CSS / Bootstrap 5 Rounded Pill Dropdown)
            _cmbAnalyticsStatus = new RoundedComboBox
            {
                Size = new Size(130, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 15
            };
            _cmbAnalyticsStatus.Items.AddRange(new string[] { "All", "Delivered", "Processed", "Cancelled" });
            _cmbAnalyticsStatus.SelectedIndex = 0;
            _cmbAnalyticsStatus.SelectedIndexChanged += async (s, e) => await FilterAnalyticsOrdersAsync();
            _analyticsTableCard.Controls.Add(_cmbAnalyticsStatus);

            // 4. Refresh Button (Modern CSS & Bootstrap 5 Pill with Hover Glow and Outlined Border)
            _btnRefreshAnalytics = new ModernButton
            {
                Text = "🔄 Refresh",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(96, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 15,
                IsOutlined = true,
                BorderOutlineColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(70, 85, 130) : Color.FromArgb(203, 213, 225),
                NormalColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(28, 35, 55) : Color.FromArgb(241, 245, 249),
                HoverColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(45, 58, 92) : Color.FromArgb(226, 232, 240),
                PressedColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(60, 75, 120) : Color.FromArgb(203, 213, 225),
                ForeColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(224, 231, 255) : Color.FromArgb(51, 65, 85),
                TextHoverColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.White : Color.FromArgb(15, 23, 42),
                HasGlowOnHover = true,
                GlowColor = Color.FromArgb(99, 102, 241)
            };
            _btnRefreshAnalytics.Click += async (s, e) => await RefreshAnalyticsDataAsync();
            _analyticsTableCard.Controls.Add(_btnRefreshAnalytics);

            LayoutTableTopToolbar();
            LayoutTableTopBadges();
            UpdateAnalyticsTableTheme();

            // 5. CSS Rounded Table Container (border-radius: 12px, clips sharp DataGridView corners)
            _analyticsGridWrapper = new RoundedPanel
            {
                Location = new Point(18, 50),
                Size = new Size(tableCardWidth - 36, tableCardHeight - 65),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBorderColor = ThemeManager.CardBorder,
                CustomBackgroundColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(20, 22, 33) : Color.FromArgb(248, 250, 252)
            };
            _themeCards.Add(_analyticsGridWrapper);

            _analyticsGridWrapper.Resize += (s, e) =>
            {
                if (_analyticsGridWrapper.Width > 0 && _analyticsGridWrapper.Height > 0)
                {
                    using var p = CreateRoundedRectanglePath(_analyticsGridWrapper.ClientRectangle, 12);
                    _analyticsGridWrapper.Region = new Region(p);
                }
                if (_analyticsGridOrders != null)
                {
                    _analyticsGridOrders.Location = new Point(1, 1);
                    _analyticsGridOrders.Size = new Size(_analyticsGridWrapper.Width - 11, _analyticsGridWrapper.Height - 2);
                }
                if (_analyticsScrollBar != null)
                {
                    _analyticsScrollBar.Location = new Point(_analyticsGridWrapper.Width - 9, 1);
                    _analyticsScrollBar.Size = new Size(8, _analyticsGridWrapper.Height - 2);
                }
            };

            // DataGridView embedded inside rounded wrapper (No native scrollbars, fully clipped corners)
            _analyticsGridOrders = CreateStyledGrid();
            _analyticsGridOrders.Location = new Point(1, 1);
            _analyticsGridOrders.Size = new Size(_analyticsGridWrapper.Width - 11, _analyticsGridWrapper.Height - 2);
            _analyticsGridOrders.RowTemplate.Height = 44;
            _analyticsGridOrders.ColumnHeadersHeight = 42;
            _analyticsGridOrders.CellBorderStyle = DataGridViewCellBorderStyle.None;
            _analyticsGridOrders.EnableHeadersVisualStyles = false;
            _analyticsGridOrders.ScrollBars = ScrollBars.None;

            _analyticsGridOrders.Columns.Add("Profile", "Profile");
            _analyticsGridOrders.Columns.Add("Address", "Address");
            _analyticsGridOrders.Columns.Add("Date", "Date");
            _analyticsGridOrders.Columns.Add("Status", "Status");
            _analyticsGridOrders.Columns.Add("Price", "Price");

            _analyticsGridOrders.Columns[0].Width = 180;
            _analyticsGridOrders.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _analyticsGridOrders.Columns[1].MinimumWidth = 180;
            _analyticsGridOrders.Columns[2].Width = 130;
            _analyticsGridOrders.Columns[3].Width = 145;
            _analyticsGridOrders.Columns[4].Width = 120;

            // Interactive Column Header Click -> Dynamic Sorting with Indicators
            _analyticsGridOrders.ColumnHeaderMouseClick += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.ColumnIndex >= _analyticsGridOrders.Columns.Count) return;
                string colName = _analyticsGridOrders.Columns[e.ColumnIndex].Name;
                if (_analyticsSortColumn == colName)
                {
                    _analyticsSortAscending = !_analyticsSortAscending;
                }
                else
                {
                    _analyticsSortColumn = colName;
                    _analyticsSortAscending = true;
                }
                ApplyAnalyticsOrdersAndSorting(_currentAnalyticsOrders);
                _analyticsGridOrders.Invalidate();
            };

            // Custom Cell Painting: Theme Adaptive, Rounded Wrapper, Gradient Avatars, Glow Pills, Neon Hover Lines
            _analyticsGridOrders.CellPainting += (s, e) =>
            {
                if (e.Graphics == null) return;
                bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

                if (e.RowIndex == -1)
                {
                    // Header Painting with Active Sorted Column Accent
                    e.PaintBackground(e.CellBounds, true);
                    bool isSortedCol = (_analyticsGridOrders.Columns[e.ColumnIndex].Name == _analyticsSortColumn);
                    Color headerBg = isSortedCol 
                        ? Color.FromArgb(0, 122, 255) 
                        : (isDark ? Color.FromArgb(20, 22, 33) : Color.FromArgb(241, 245, 249));
                    using (var b = new SolidBrush(headerBg))
                    {
                        e.Graphics.FillRectangle(b, e.CellBounds);
                    }
                    Color divColor = isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240);
                    using (var divPen = new Pen(divColor, 1f))
                    {
                        e.Graphics.DrawLine(divPen, e.CellBounds.Right - 1, e.CellBounds.Top + 8, e.CellBounds.Right - 1, e.CellBounds.Bottom - 8);
                        e.Graphics.DrawLine(divPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }
                    string headerText = _analyticsGridOrders.Columns[e.ColumnIndex].HeaderText;
                    Color headerTextColor = isSortedCol ? Color.White : (isDark ? ThemeManager.TextSecondary : Color.FromArgb(71, 85, 105));
                    using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(headerTextColor))
                    {
                        e.Graphics.DrawString(headerText, font, textBrush, e.CellBounds.X + 16, e.CellBounds.Y + 13);
                    }

                    // Draw Sort Indicator Polygon (▲ / ▼) on Sorted Column
                    if (isSortedCol)
                    {
                        using var sf = new SolidBrush(Color.White);
                        PointF[] pts;
                        if (_analyticsSortAscending)
                        {
                            pts = new PointF[]
                            {
                                new(e.CellBounds.Right - 22, e.CellBounds.Y + 23),
                                new(e.CellBounds.Right - 16, e.CellBounds.Y + 15),
                                new(e.CellBounds.Right - 10, e.CellBounds.Y + 23)
                            };
                        }
                        else
                        {
                            pts = new PointF[]
                            {
                                new(e.CellBounds.Right - 22, e.CellBounds.Y + 16),
                                new(e.CellBounds.Right - 16, e.CellBounds.Y + 24),
                                new(e.CellBounds.Right - 10, e.CellBounds.Y + 16)
                            };
                        }
                        e.Graphics.FillPolygon(sf, pts);
                    }
                    e.Handled = true;
                    return;
                }

                if (e.RowIndex >= 0)
                {
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                    bool isHovered = (e.RowIndex == _hoveredOrderRowIndex);

                    Color rowBg;
                    if (isDark)
                    {
                        rowBg = isSelected 
                            ? Color.FromArgb(42, 50, 80) 
                            : (isHovered 
                                ? Color.FromArgb(34, 40, 64) 
                                : (e.RowIndex % 2 == 0 ? Color.FromArgb(24, 26, 38) : Color.FromArgb(19, 21, 31)));
                    }
                    else
                    {
                        rowBg = isSelected 
                            ? Color.FromArgb(224, 235, 255) 
                            : (isHovered 
                                ? Color.FromArgb(241, 245, 255) 
                                : (e.RowIndex % 2 == 0 ? Color.FromArgb(255, 255, 255) : Color.FromArgb(248, 250, 252)));
                    }

                    using (var brush = new SolidBrush(rowBg))
                    {
                        e.Graphics.FillRectangle(brush, e.CellBounds);
                    }

                    // Neon Left Accent Bar for Hover/Selected
                    if (e.ColumnIndex == 0 && (isHovered || isSelected))
                    {
                        using var accentBar = new SolidBrush(Color.FromArgb(0, 160, 255));
                        e.Graphics.FillRectangle(accentBar, e.CellBounds.Left, e.CellBounds.Top + 4, 3, e.CellBounds.Height - 8);
                    }

                    // Subtle horizontal border
                    Color lineBorder = isDark ? Color.FromArgb(32, 35, 52) : Color.FromArgb(226, 232, 240);
                    using (var linePen = new Pen(lineBorder, 1f))
                    {
                        e.Graphics.DrawLine(linePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }

                    string cellVal = e.Value?.ToString() ?? "";

                    // Column 0: Profile with 2-Tone Circular Gradient Avatar & Initials
                    if (e.ColumnIndex == 0)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int avatarSize = 28;
                        int avatarX = e.CellBounds.X + 14;
                        int avatarY = e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2;

                        var avRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);
                        Color c1 = GetAvatarStartColor(cellVal);
                        Color c2 = GetAvatarEndColor(cellVal);
                        using (var avBrush = new LinearGradientBrush(avRect, c1, c2, LinearGradientMode.ForwardDiagonal))
                        {
                            e.Graphics.FillEllipse(avBrush, avRect);
                        }
                        using (var ringPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1.2f))
                        {
                            e.Graphics.DrawEllipse(ringPen, avatarX, avatarY, avatarSize - 1, avatarSize - 1);
                        }

                        string initial = cellVal.Length > 0 ? cellVal[0].ToString().ToUpperInvariant() : "?";
                        using (var fInitial = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        {
                            e.Graphics.DrawString(initial, fInitial, Brushes.White, new RectangleF(avatarX, avatarY, avatarSize, avatarSize), sf);
                        }

                        Color nameColor = isDark ? Color.White : Color.FromArgb(15, 23, 42);
                        using (var fName = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                        using (var bName = new SolidBrush(nameColor))
                        {
                            e.Graphics.DrawString(cellVal, fName, bName, avatarX + avatarSize + 10, e.CellBounds.Y + 12);
                        }
                        e.Handled = true;
                    }
                    // Column 1: Address with location indicator
                    else if (e.ColumnIndex == 1)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var fPin = new Font("Segoe UI", 8.5f))
                        using (var bPin = new SolidBrush(Color.FromArgb(239, 68, 68)))
                        {
                            e.Graphics.DrawString("📍", fPin, bPin, e.CellBounds.X + 8, e.CellBounds.Y + 12);
                        }
                        Color addrColor = isDark ? Color.FromArgb(190, 200, 220) : Color.FromArgb(71, 85, 105);
                        using (var fText = new Font("Segoe UI", 9f, FontStyle.Regular))
                        using (var bText = new SolidBrush(addrColor))
                        {
                            e.Graphics.DrawString(cellVal, fText, bText, e.CellBounds.X + 28, e.CellBounds.Y + 13);
                        }
                        e.Handled = true;
                    }
                    // Column 2: Date
                    else if (e.ColumnIndex == 2)
                    {
                        Color dateColor = isDark ? Color.FromArgb(165, 175, 205) : Color.FromArgb(100, 116, 139);
                        using (var fText = new Font("Segoe UI", 9f, FontStyle.Regular))
                        using (var bText = new SolidBrush(dateColor))
                        {
                            e.Graphics.DrawString(cellVal, fText, bText, e.CellBounds.X + 12, e.CellBounds.Y + 13);
                        }
                        e.Handled = true;
                    }
                    // Column 3: Status Glow Pill Badge with Pulsing Status Dot
                    else if (e.ColumnIndex == 3)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Color pillBg, pillBorder, pillText;
                        if (isDark)
                        {
                            if (cellVal == "Delivered")
                            {
                                pillBg = Color.FromArgb(20, 52, 38);
                                pillBorder = Color.FromArgb(16, 185, 129);
                                pillText = Color.FromArgb(52, 211, 153);
                            }
                            else if (cellVal == "Processed")
                            {
                                pillBg = Color.FromArgb(52, 40, 16);
                                pillBorder = Color.FromArgb(245, 158, 11);
                                pillText = Color.FromArgb(251, 191, 36);
                            }
                            else if (cellVal == "Cancelled")
                            {
                                pillBg = Color.FromArgb(52, 20, 26);
                                pillBorder = Color.FromArgb(239, 68, 68);
                                pillText = Color.FromArgb(248, 113, 113);
                            }
                            else
                            {
                                pillBg = Color.FromArgb(35, 38, 54);
                                pillBorder = ThemeManager.AccentBlue;
                                pillText = Color.White;
                            }
                        }
                        else
                        {
                            if (cellVal == "Delivered")
                            {
                                pillBg = Color.FromArgb(220, 252, 231);
                                pillBorder = Color.FromArgb(22, 163, 74);
                                pillText = Color.FromArgb(21, 128, 61);
                            }
                            else if (cellVal == "Processed")
                            {
                                pillBg = Color.FromArgb(254, 243, 199);
                                pillBorder = Color.FromArgb(217, 119, 6);
                                pillText = Color.FromArgb(180, 83, 9);
                            }
                            else if (cellVal == "Cancelled")
                            {
                                pillBg = Color.FromArgb(254, 226, 226);
                                pillBorder = Color.FromArgb(220, 38, 38);
                                pillText = Color.FromArgb(185, 28, 28);
                            }
                            else
                            {
                                pillBg = Color.FromArgb(238, 242, 255);
                                pillBorder = ThemeManager.AccentBlue;
                                pillText = ThemeManager.AccentBlue;
                            }
                        }

                        using var fBadge = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        var textSize = e.Graphics.MeasureString(cellVal, fBadge);
                        int badgeW = (int)textSize.Width + 26;
                        int badgeH = 24;
                        int badgeX = e.CellBounds.X + 12;
                        int badgeY = e.CellBounds.Y + (e.CellBounds.Height - badgeH) / 2;

                        var badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);
                        using (var bp = CreateRoundedRectanglePath(badgeRect, 11))
                        {
                            using var bBrush = new SolidBrush(pillBg);
                            e.Graphics.FillPath(bBrush, bp);
                            using var bPen = new Pen(pillBorder, 1.2f);
                            e.Graphics.DrawPath(bPen, bp);
                        }

                        // Outer glowing aura around dot
                        using (var haloBrush = new SolidBrush(Color.FromArgb(50, pillBorder.R, pillBorder.G, pillBorder.B)))
                        {
                            e.Graphics.FillEllipse(haloBrush, badgeX + 6, badgeY + (badgeH - 10) / 2, 10, 10);
                        }
                        // Core glowing dot
                        using (var dotBrush = new SolidBrush(pillBorder))
                        {
                            e.Graphics.FillEllipse(dotBrush, badgeX + 8, badgeY + (badgeH - 6) / 2, 6, 6);
                        }

                        using (var tBrush = new SolidBrush(pillText))
                        {
                            e.Graphics.DrawString(cellVal, fBadge, tBrush, badgeX + 18, badgeY + 4);
                        }

                        e.Handled = true;
                    }
                    // Column 4: Price
                    else if (e.ColumnIndex == 4)
                    {
                        Color priceColor = isDark ? Color.FromArgb(230, 245, 255) : Color.FromArgb(15, 23, 42);
                        using (var fPrice = new Font("Segoe UI", 10f, FontStyle.Bold))
                        using (var bPrice = new SolidBrush(priceColor))
                        {
                            e.Graphics.DrawString(cellVal, fPrice, bPrice, e.CellBounds.X + 12, e.CellBounds.Y + 12);
                        }
                        e.Handled = true;
                    }
                }
            };

            // Interactive Row Hover Tracking
            _analyticsGridOrders.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex != _hoveredOrderRowIndex)
                {
                    int prev = _hoveredOrderRowIndex;
                    _hoveredOrderRowIndex = e.RowIndex;
                    if (prev >= 0 && prev < _analyticsGridOrders.RowCount) _analyticsGridOrders.InvalidateRow(prev);
                    if (_hoveredOrderRowIndex >= 0 && _hoveredOrderRowIndex < _analyticsGridOrders.RowCount) _analyticsGridOrders.InvalidateRow(_hoveredOrderRowIndex);
                }
            };

            _analyticsGridOrders.MouseLeave += (s, e) =>
            {
                if (_hoveredOrderRowIndex >= 0)
                {
                    int prev = _hoveredOrderRowIndex;
                    _hoveredOrderRowIndex = -1;
                    if (prev >= 0 && prev < _analyticsGridOrders.RowCount) _analyticsGridOrders.InvalidateRow(prev);
                }
            };

            // Double Click Row -> Launch Interactive Order Details Dialog Modal
            _analyticsGridOrders.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _currentAnalyticsOrders.Count)
                {
                    var order = _currentAnalyticsOrders[e.RowIndex];
                    using var dlg = new OrderDetailsDialog(order, async updatedOrder =>
                    {
                        await FilterAnalyticsOrdersAsync();
                    });
                    dlg.ShowDialog(this);
                }
            };

            // Single Click on Status Column (Column 3) -> Quick Status Toggle (Delivered -> Processed -> Cancelled)
            _analyticsGridOrders.CellClick += async (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _currentAnalyticsOrders.Count && e.ColumnIndex == 3)
                {
                    var order = _currentAnalyticsOrders[e.RowIndex];
                    string nextStatus = await _dbService.ToggleOrderStatusAsync(order.ProfileName);
                    order.Status = nextStatus;
                    ApplyAnalyticsOrdersAndSorting(_currentAnalyticsOrders);
                }
            };

            // Add DataGridView and Custom DarkScrollBar inside the rounded wrapper
            _analyticsGridWrapper.Controls.Add(_analyticsGridOrders);

            _analyticsScrollBar = new DarkScrollBar
            {
                Location = new Point(_analyticsGridWrapper.Width - 9, 1),
                Size = new Size(8, _analyticsGridWrapper.Height - 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Visible = false
            };
            _analyticsScrollBar.AttachToGrid(_analyticsGridOrders);
            _analyticsGridWrapper.Controls.Add(_analyticsScrollBar);
            _analyticsScrollBar.BringToFront();

            _analyticsTableCard.Controls.Add(_analyticsGridWrapper);
            _contentPanel.Controls.Add(_analyticsTableCard);



            UpdateAnalyticsLayout();

            // Load live dynamic data from database and backend services
            _ = RefreshAnalyticsDataAsync();

            // Set up background auto-refresh timer (Every 15s)
            _analyticsRefreshTimer?.Stop();
            _analyticsRefreshTimer?.Dispose();
            _analyticsRefreshTimer = new System.Windows.Forms.Timer { Interval = 15000 };
            _analyticsRefreshTimer.Tick += async (s, e) =>
            {
                if (_activeMenuIndex == 0) // Only refresh if actively viewing Analytics page
                {
                    await RefreshAnalyticsDataAsync();
                }
            };
            _analyticsRefreshTimer.Start();
        }

        private void ShowDateRangePicker()
        {
            if (_datePill == null) return;

            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
            Color popBg = isDark ? Color.FromArgb(24, 26, 38) : Color.White;
            Color popBorder = isDark ? Color.FromArgb(45, 50, 75) : Color.FromArgb(226, 232, 240);
            Color textPri = ThemeManager.TextPrimary;
            Color textSec = ThemeManager.TextSecondary;
            Color inputBg = isDark ? Color.FromArgb(32, 35, 52) : Color.FromArgb(245, 247, 250);

            var popup = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                Size = new Size(330, 365),
                BackColor = popBg,
                TopMost = true
            };

            // Calculate position right below _datePill
            Point screenPt = _datePill.PointToScreen(new Point(0, _datePill.Height + 5));
            var screen = Screen.FromControl(_datePill);
            if (screenPt.X + popup.Width > screen.WorkingArea.Right)
                screenPt.X = screen.WorkingArea.Right - popup.Width - 10;
            if (screenPt.Y + popup.Height > screen.WorkingArea.Bottom)
                screenPt.Y = screen.WorkingArea.Bottom - popup.Height - 10;
            popup.Location = screenPt;

            // Border & Card container
            var container = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = popBg,
                CustomBorderColor = popBorder
            };
            popup.Controls.Add(container);

            // Title Header
            var lblTitle = new Label
            {
                Text = "📅  Filter by Date Range",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = textPri,
                BackColor = Color.Transparent,
                Location = new Point(16, 14),
                AutoSize = true
            };
            container.Controls.Add(lblTitle);

            var btnClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = textSec,
                BackColor = Color.Transparent,
                Location = new Point(popup.Width - 36, 12),
                Size = new Size(24, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => popup.Close();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = ThemeManager.AccentRed;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = textSec;
            container.Controls.Add(btnClose);

            // Divider
            var pnlDiv = new Panel
            {
                Location = new Point(16, 42),
                Size = new Size(popup.Width - 32, 1),
                BackColor = popBorder
            };
            container.Controls.Add(pnlDiv);

            // Presets Label
            var lblPresets = new Label
            {
                Text = "QUICK PRESETS",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                BackColor = Color.Transparent,
                Location = new Point(16, 52),
                AutoSize = true
            };
            container.Controls.Add(lblPresets);

            // Date Pickers declared upfront so presets can update them
            var dtpStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Font = new Font("Segoe UI", 9f),
                Location = new Point(16, 224),
                Size = new Size(140, 26),
                Value = _currentStartDate ?? new DateTime(2022, 8, 1)
            };

            var dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Font = new Font("Segoe UI", 9f),
                Location = new Point(168, 224),
                Size = new Size(140, 26),
                Value = _currentEndDate ?? new DateTime(2022, 8, 31)
            };

            void ApplySelectedRange(DateTime? start, DateTime? end)
            {
                _currentStartDate = start;
                _currentEndDate = end;
                if (_lblDate != null)
                {
                    if (start.HasValue && end.HasValue)
                        _lblDate.Text = $"{start.Value:dd.MM.yyyy} - {end.Value:dd.MM.yyyy}  📅";
                    else if (start.HasValue)
                        _lblDate.Text = $"From {start.Value:dd.MM.yyyy}  📅";
                    else if (end.HasValue)
                        _lblDate.Text = $"Until {end.Value:dd.MM.yyyy}  📅";
                    else
                        _lblDate.Text = "All Time  📅";
                }
                popup.Close();
                _ = RefreshAnalyticsDataAsync();
            }

            ModernButton CreatePresetBtn(string text, int x, int y, int w, Action onClick)
            {
                var btn = new ModernButton
                {
                    Text = text,
                    Font = new Font("Segoe UI", 8f),
                    Location = new Point(x, y),
                    Size = new Size(w, 28),
                    NormalColor = inputBg,
                    HoverColor = isDark ? Color.FromArgb(45, 52, 78) : Color.FromArgb(230, 238, 248),
                    ForeColor = textPri,
                    BorderRadius = 6
                };
                btn.Click += (s, e) => onClick();
                return btn;
            }

            // Quick Preset Buttons
            // Row 1: August 2022 (Default demo data) and All Time
            container.Controls.Add(CreatePresetBtn("August 2022 (Default)", 16, 72, 160, () =>
            {
                ApplySelectedRange(new DateTime(2022, 8, 1), new DateTime(2022, 8, 31));
            }));

            container.Controls.Add(CreatePresetBtn("All Time", 182, 72, 126, () =>
            {
                ApplySelectedRange(null, null);
            }));

            // Row 2: Today and Last 7 Days
            container.Controls.Add(CreatePresetBtn("Today", 16, 106, 140, () =>
            {
                ApplySelectedRange(DateTime.Today, DateTime.Today);
            }));

            container.Controls.Add(CreatePresetBtn("Last 7 Days", 162, 106, 146, () =>
            {
                ApplySelectedRange(DateTime.Today.AddDays(-7), DateTime.Today);
            }));

            // Row 3: This Month and Last 30 Days
            container.Controls.Add(CreatePresetBtn("This Month", 16, 140, 140, () =>
            {
                var now = DateTime.Today;
                var start = new DateTime(now.Year, now.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                ApplySelectedRange(start, end);
            }));

            container.Controls.Add(CreatePresetBtn("Last 30 Days", 162, 140, 146, () =>
            {
                ApplySelectedRange(DateTime.Today.AddDays(-30), DateTime.Today);
            }));

            // Custom Range Divider
            var pnlDiv2 = new Panel
            {
                Location = new Point(16, 180),
                Size = new Size(popup.Width - 32, 1),
                BackColor = popBorder
            };
            container.Controls.Add(pnlDiv2);

            // Custom Range Labels & Pickers
            var lblCustom = new Label
            {
                Text = "CUSTOM RANGE",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                BackColor = Color.Transparent,
                Location = new Point(16, 188),
                AutoSize = true
            };
            container.Controls.Add(lblCustom);

            var lblFrom = new Label
            {
                Text = "From:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textSec,
                BackColor = Color.Transparent,
                Location = new Point(16, 206),
                AutoSize = true
            };
            container.Controls.Add(lblFrom);
            container.Controls.Add(dtpStart);

            var lblTo = new Label
            {
                Text = "To:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textSec,
                BackColor = Color.Transparent,
                Location = new Point(168, 206),
                AutoSize = true
            };
            container.Controls.Add(lblTo);
            container.Controls.Add(dtpEnd);

            // Apply Button
            var btnApply = new ModernButton
            {
                Text = "✓ Apply Range",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(16, 268),
                Size = new Size(185, 34),
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(90, 130, 255),
                ForeColor = Color.White,
                BorderRadius = 6
            };
            btnApply.Click += (s, e) =>
            {
                if (dtpStart.Value.Date > dtpEnd.Value.Date)
                {
                    MessageBox.Show(popup, "Start date cannot be after end date.", "Invalid Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ApplySelectedRange(dtpStart.Value.Date, dtpEnd.Value.Date);
            };
            container.Controls.Add(btnApply);

            // Clear Button
            var btnReset = new ModernButton
            {
                Text = "Clear",
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(208, 268),
                Size = new Size(100, 34),
                NormalColor = inputBg,
                HoverColor = isDark ? Color.FromArgb(50, 55, 80) : Color.FromArgb(226, 232, 240),
                ForeColor = textSec,
                BorderRadius = 6
            };
            btnReset.Click += (s, e) =>
            {
                ApplySelectedRange(null, null);
            };
            container.Controls.Add(btnReset);

            // Close on deactivate when user clicks outside
            popup.Deactivate += (s, e) =>
            {
                try { popup.Close(); } catch { }
            };

            popup.Show(this);
        }

        private void ShowYearSelector(Control anchor, Action<int> onYearSelected)
        {
            var menu = new ContextMenuStrip
            {
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                ShowImageMargin = false
            };
            int[] years = { 2021, 2022, 2023, 2024, 2025, 2026 };
            foreach (int y in years)
            {
                int capturedYear = y;
                var item = new ToolStripMenuItem($"{capturedYear}")
                {
                    ForeColor = ThemeManager.TextPrimary,
                    BackColor = ThemeManager.CardBg
                };
                item.Click += (s, e) => onYearSelected(capturedYear);
                menu.Items.Add(item);
            }
            menu.Show(anchor, new Point(0, anchor.Height));
        }

        private async Task FilterAnalyticsOrdersAsync()
        {
            if (_analyticsGridOrders == null) return;
            string search = _txtSearchAnalyticsOrders?.Text?.Trim() ?? "";
            string status = _cmbAnalyticsStatus?.SelectedItem?.ToString() ?? "All";
            var orders = await _dbService.GetOrdersAsync(search, status, _currentStartDate, _currentEndDate);
            ApplyAnalyticsOrdersAndSorting(orders);
        }

        private void ApplyAnalyticsOrdersAndSorting(List<CustomerOrder> orders)
        {
            if (_analyticsGridOrders == null) return;

            _currentAnalyticsOrders = orders ?? new List<CustomerOrder>();

            // Dynamic Column Sorting
            IEnumerable<CustomerOrder> sorted = _currentAnalyticsOrders;
            if (_analyticsSortColumn == "Profile")
            {
                sorted = _analyticsSortAscending ? sorted.OrderBy(o => o.ProfileName) : sorted.OrderByDescending(o => o.ProfileName);
            }
            else if (_analyticsSortColumn == "Address")
            {
                sorted = _analyticsSortAscending ? sorted.OrderBy(o => o.Address) : sorted.OrderByDescending(o => o.Address);
            }
            else if (_analyticsSortColumn == "Date")
            {
                sorted = _analyticsSortAscending 
                    ? sorted.OrderBy(o => DateTime.TryParseExact(o.OrderDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var dt) ? dt : DateTime.MinValue)
                    : sorted.OrderByDescending(o => DateTime.TryParseExact(o.OrderDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var dt) ? dt : DateTime.MinValue);
            }
            else if (_analyticsSortColumn == "Status")
            {
                sorted = _analyticsSortAscending ? sorted.OrderBy(o => o.Status) : sorted.OrderByDescending(o => o.Status);
            }
            else if (_analyticsSortColumn == "Price")
            {
                sorted = _analyticsSortAscending 
                    ? sorted.OrderBy(o => decimal.TryParse(o.Price.Replace("₹", "").Replace("$", "").Replace(",", "").Trim(), out var p) ? p : 0)
                    : sorted.OrderByDescending(o => decimal.TryParse(o.Price.Replace("₹", "").Replace("$", "").Replace(",", "").Trim(), out var p) ? p : 0);
            }

            _currentAnalyticsOrders = sorted.ToList();

            _analyticsGridOrders.Rows.Clear();
            foreach (var o in _currentAnalyticsOrders)
            {
                string displayPrice = o.Price;
                if (!displayPrice.StartsWith("₹")) displayPrice = "₹" + displayPrice.Replace("$", "").Trim();
                _analyticsGridOrders.Rows.Add(o.ProfileName, o.Address, o.OrderDate, o.Status, displayPrice);
            }

            // Update Header Stats Badges
            if (_lblAnalyticsOrderCount != null)
            {
                _lblAnalyticsOrderCount.Text = $"{_currentAnalyticsOrders.Count} Orders";
            }

            if (_lblAnalyticsRevTotal != null)
            {
                decimal totalRev = _currentAnalyticsOrders.Sum(o => decimal.TryParse(o.Price.Replace("₹", "").Replace("$", "").Replace(",", "").Trim(), out var p) ? p : 0);
                _lblAnalyticsRevTotal.Text = $"Total: ₹{totalRev.ToString("N0", _inCulture)}";
            }

            if (_lblAnalyticsDeliveredCount != null)
            {
                int delCount = _currentAnalyticsOrders.Count(o => o.Status == "Delivered");
                _lblAnalyticsDeliveredCount.Text = $"● {delCount} Delivered";
            }

            if (_lblAnalyticsProcessedCount != null)
            {
                int procCount = _currentAnalyticsOrders.Count(o => o.Status == "Processed");
                _lblAnalyticsProcessedCount.Text = $"● {procCount} Processed";
            }

            if (_lblAnalyticsCancelledCount != null)
            {
                int canCount = _currentAnalyticsOrders.Count(o => o.Status == "Cancelled");
                _lblAnalyticsCancelledCount.Text = $"● {canCount} Cancelled";
            }

            LayoutTableTopBadges();
            _analyticsTableCard?.Invalidate(true);
            _analyticsScrollBar?.SyncWithGrid();
        }

        private void AttachPillBadgePaint(Label lbl, Func<(Color Bg, Color Border, Color Text)> colorSelector)
        {
            lbl.AutoSize = true;
            lbl.BackColor = Color.Transparent;
            lbl.ForeColor = Color.Transparent;

            lbl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var (bg, border, text) = colorSelector();
                var rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);
                if (rect.Width <= 2 || rect.Height <= 2) return;

                int radius = Math.Min(8, rect.Height / 2);
                using var path = CreateRoundedRectanglePath(rect, radius);
                using var bBrush = new SolidBrush(bg);
                e.Graphics.FillPath(bBrush, path);

                if (border != Color.Transparent)
                {
                    using var bPen = new Pen(border, 1f);
                    e.Graphics.DrawPath(bPen, path);
                }

                using var tBrush = new SolidBrush(text);
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(lbl.Text, lbl.Font, tBrush, new RectangleF(0, 0, lbl.Width, lbl.Height), sf);
            };
        }

        private void LayoutTableTopBadges()
        {
            if (_analyticsTableCard == null) return;
            int startX = _lblAnalyticsTableTitle != null 
                ? Math.Max(180, _lblAnalyticsTableTitle.Right + 14) 
                : 180;

            int toolbarLeft = _analyticsTableCard.Width;
            if (_btnAnalyticsAddOrder != null && _btnAnalyticsAddOrder.Visible)
            {
                toolbarLeft = _btnAnalyticsAddOrder.Left - 10;
            }
            else if (_txtSearchAnalyticsOrders != null && _txtSearchAnalyticsOrders.Visible)
            {
                toolbarLeft = _txtSearchAnalyticsOrders.Left - 10;
            }

            Label?[] badges = {
                _lblAnalyticsOrderCount,
                _lblAnalyticsDeliveredCount,
                _lblAnalyticsProcessedCount,
                _lblAnalyticsCancelledCount,
                _lblAnalyticsRevTotal
            };

            int currentX = startX;
            foreach (var b in badges)
            {
                if (b == null) continue;
                if (currentX + b.Width > toolbarLeft)
                {
                    b.Visible = false;
                }
                else
                {
                    b.Visible = true;
                    b.Location = new Point(currentX, 16);
                    currentX += b.Width + 8;
                }
            }
        }

        private void LayoutBillingTopBadges()
        {
            // Mini badges removed from table bar per user request
            if (_lblBillingCountBadge != null) _lblBillingCountBadge.Visible = false;
            if (_lblBillingPaidBadge != null) _lblBillingPaidBadge.Visible = false;
            if (_lblBillingDueBadge != null) _lblBillingDueBadge.Visible = false;
        }

        private void LayoutTableTopToolbar()
        {
            if (_analyticsTableCard == null) return;
            int rightEdge = _analyticsTableCard.Width - 18;

            if (_btnRefreshAnalytics != null)
            {
                _btnRefreshAnalytics.Size = new Size(96, 30);
                _btnRefreshAnalytics.Location = new Point(rightEdge - _btnRefreshAnalytics.Width, 13);
                rightEdge -= _btnRefreshAnalytics.Width + 8;
            }

            if (_cmbAnalyticsStatus != null)
            {
                _cmbAnalyticsStatus.Size = new Size(130, 30);
                _cmbAnalyticsStatus.Location = new Point(rightEdge - _cmbAnalyticsStatus.Width, 13);
                rightEdge -= _cmbAnalyticsStatus.Width + 8;
            }

            if (_txtSearchAnalyticsOrders != null)
            {
                int searchW = Math.Max(140, Math.Min(220, rightEdge - 280));
                _txtSearchAnalyticsOrders.Size = new Size(searchW, 30);
                _txtSearchAnalyticsOrders.Location = new Point(rightEdge - searchW, 13);
                rightEdge -= searchW + 8;
            }

            if (_btnAnalyticsAddOrder != null)
            {
                _btnAnalyticsAddOrder.Size = new Size(86, 30);
                _btnAnalyticsAddOrder.Location = new Point(rightEdge - _btnAnalyticsAddOrder.Width, 13);
            }
        }

        private void UpdateAnalyticsTableTheme()
        {
            if (_analyticsTableCard == null) return;
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

            _analyticsTableCard.CustomBackgroundColor = ThemeManager.CardBg;
            _analyticsTableCard.CustomBorderColor = ThemeManager.CardBorder;

            if (_analyticsGridWrapper != null)
            {
                _analyticsGridWrapper.CustomBackgroundColor = isDark ? Color.FromArgb(20, 22, 33) : Color.FromArgb(248, 250, 252);
                _analyticsGridWrapper.CustomBorderColor = isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240);
                _analyticsGridWrapper.Invalidate();
            }

            if (_txtSearchAnalyticsOrders != null)
            {
                _txtSearchAnalyticsOrders.ApplyTheme(ThemeManager.CurrentTheme);
            }

            if (_cmbAnalyticsStatus != null)
            {
                _cmbAnalyticsStatus.ApplyTheme(ThemeManager.CurrentTheme);
            }

            if (_btnRefreshAnalytics != null)
            {
                _btnRefreshAnalytics.BorderOutlineColor = isDark ? Color.FromArgb(70, 85, 130) : Color.FromArgb(203, 213, 225);
                _btnRefreshAnalytics.NormalColor = isDark ? Color.FromArgb(28, 35, 55) : Color.FromArgb(241, 245, 249);
                _btnRefreshAnalytics.HoverColor = isDark ? Color.FromArgb(45, 58, 92) : Color.FromArgb(226, 232, 240);
                _btnRefreshAnalytics.PressedColor = isDark ? Color.FromArgb(60, 75, 120) : Color.FromArgb(203, 213, 225);
                _btnRefreshAnalytics.ForeColor = isDark ? Color.FromArgb(224, 231, 255) : Color.FromArgb(51, 65, 85);
                _btnRefreshAnalytics.TextHoverColor = isDark ? Color.White : Color.FromArgb(15, 23, 42);
                _btnRefreshAnalytics.Invalidate();
            }

            if (_analyticsGridOrders != null)
            {
                _analyticsGridOrders.BackgroundColor = isDark ? Color.FromArgb(20, 22, 33) : Color.FromArgb(248, 250, 252);
                _analyticsGridOrders.Invalidate();
            }

            _analyticsScrollBar?.ApplyTheme(ThemeManager.CurrentTheme);
            _analyticsTableCard.Invalidate(true);
        }

        private bool _isAddOrderDialogOpen = false;

        private async Task PromptAddCustomerOrderAsync()
        {
            if (_isAddOrderDialogOpen) return;
            _isAddOrderDialogOpen = true;

            try
            {
                DateTime? defaultDate = _currentEndDate ?? DateTime.Today;
                using var dlg = new AddCustomerOrder(defaultDate);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.CreatedOrder != null)
                {
                    // 1. If the new order's date is outside current filter, reset to All Time so it's guaranteed to be visible!
                    if (DateTime.TryParseExact(dlg.CreatedOrder.OrderDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var orderDt))
                    {
                        if ((_currentStartDate.HasValue && orderDt.Date < _currentStartDate.Value.Date) ||
                            (_currentEndDate.HasValue && orderDt.Date > _currentEndDate.Value.Date))
                        {
                            _currentStartDate = null;
                            _currentEndDate = null;
                            if (_lblDate != null)
                            {
                                _lblDate.Text = "All Time  📅";
                            }
                        }
                    }

                    // 2. Clear any search filter that would hide the new order
                    if (_txtSearchAnalyticsOrders != null && !string.IsNullOrEmpty(_txtSearchAnalyticsOrders.Text))
                    {
                        _txtSearchAnalyticsOrders.Text = string.Empty;
                    }

                    // 3. Reset status dropdown to 'All'
                    if (_cmbAnalyticsStatus != null && _cmbAnalyticsStatus.SelectedIndex != 0)
                    {
                        _cmbAnalyticsStatus.SelectedIndex = 0;
                    }

                    // 4. Default sorting to Date descending so newest order is at Row 0
                    _analyticsSortColumn = "Date";
                    _analyticsSortAscending = false;

                    // 5. Refresh analytics table & cards
                    await FilterAnalyticsOrdersAsync();
                    await RefreshAnalyticsDataAsync();

                    // 6. Select and scroll to Row 0 so user sees the newly added order immediately
                    if (_analyticsGridOrders != null && _analyticsGridOrders.Rows.Count > 0)
                    {
                        _analyticsGridOrders.ClearSelection();
                        _analyticsGridOrders.Rows[0].Selected = true;
                        _analyticsGridOrders.FirstDisplayedScrollingRowIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to display order dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isAddOrderDialogOpen = false;
            }
        }


        private async Task RefreshAnalyticsDataAsync()
        {
            try
            {
                var summary = _dbService.GetAnalyticsSummary();
                var billingMetrics = await _billingService.GetSalesDashboardMetricsAsync();
                var orders = await _dbService.GetOrdersAsync(_txtSearchAnalyticsOrders?.Text, _cmbAnalyticsStatus?.SelectedItem?.ToString(), _currentStartDate, _currentEndDate);

                // 1. Orders Card
                if (_lblValOrders != null)
                {
                    int totalOrderCount = summary.TotalOrders;
                    _lblValOrders.Text = totalOrderCount.ToString();
                    if (_lblTrendOrders != null) _lblTrendOrders.Text = summary.OrdersTrend;
                    if (_cardOrders != null)
                    {
                        _cardOrders.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Customer Orders Performance",
                            Icon = "📦",
                            CurrentValue = $"{totalOrderCount} Orders",
                            PreviousValue = "185 Orders",
                            GrowthText = summary.OrdersTrend,
                            IsPositiveGrowth = summary.OrdersTrendPositive,
                            Category = "Sales & Fulfillment",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Delivered Orders", $"{orders.Count(o => o.Status == "Delivered")}"),
                                ("Processed in Transit", $"{orders.Count(o => o.Status == "Processed")}"),
                                ("Cancelled Orders", $"{orders.Count(o => o.Status == "Cancelled")}"),
                                ("Active Invoices in DB", $"{billingMetrics.TotalInvoices}")
                            },
                            FooterTip = "Click card to jump to Sales & Billing invoices"
                        };
                    }
                }

                // 2. Approved Card
                if (_lblValApproved != null)
                {
                    _lblValApproved.Text = summary.ApprovedOrders.ToString();
                    if (_lblTrendApproved != null) _lblTrendApproved.Text = summary.ApprovedTrend;
                    if (_cardApproved != null)
                    {
                        _cardApproved.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Order Approval SLA",
                            Icon = "✅",
                            CurrentValue = $"{summary.ApprovedOrders} Approved",
                            PreviousValue = "34 Approved",
                            GrowthText = summary.ApprovedTrend,
                            IsPositiveGrowth = summary.ApprovedTrendPositive,
                            Category = "Quality & Verification",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Approval Compliance", "94.7% within SLA"),
                                ("Auto-Approved", "32 orders"),
                                ("Manual Review", "4 orders"),
                                ("Avg Verification Time", "14 mins")
                            }
                        };
                    }
                }

                // 3. Users Card
                if (_lblValUsers != null)
                {
                    _lblValUsers.Text = summary.TotalUsers;
                    if (_cardUsers != null)
                    {
                        _cardUsers.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Active Platform Users",
                            Icon = "👥",
                            CurrentValue = $"{summary.TotalUsers} Total Users",
                            PreviousValue = "4,620 Users",
                            GrowthText = "+5.8% quarterly growth",
                            IsPositiveGrowth = true,
                            Category = "Community & Reach",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("New Users (30 Days)", "3,080 users (63%)"),
                                ("Returning Users", "1,223 users (25%)"),
                                ("Inactive Users", "587 users (12%)"),
                                ("Retention Rate", "78.4% monthly")
                            },
                            FooterTip = "Click to open Customers directory"
                        };
                    }
                }

                // 4. Subscriptions Card
                if (_lblValSubs != null)
                {
                    _lblValSubs.Text = summary.ActiveSubscriptions;
                    if (_cardSubs != null)
                    {
                        _cardSubs.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Subscriptions & Licenses",
                            Icon = "💎",
                            CurrentValue = $"{summary.ActiveSubscriptions} Active",
                            PreviousValue = "1,140 Subscriptions",
                            GrowthText = "+5.3% monthly net gain",
                            IsPositiveGrowth = true,
                            Category = "Recurring Revenue",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Paid Subscriptions", "841 licenses (70%)"),
                                ("Trial Accounts", "360 users (30%)"),
                                ("Conversion Rate", "24.6% trial to paid"),
                                ("Avg Contract Value", "₹9,800.00 / mo")
                            },
                            FooterTip = "Click to view Products & catalog"
                        };
                    }
                }

                // 5. Month total Card
                if (_lblValMonthTotal != null)
                {
                    decimal mTotal = billingMetrics.TotalSales > 0 ? billingMetrics.TotalSales : summary.MonthTotal;
                    _lblValMonthTotal.Text = $"₹{mTotal.ToString("N0", _inCulture)}";
                    if (_lblTrendMonthTotal != null) _lblTrendMonthTotal.Text = summary.MonthTotalTrend;
                    if (_cardMonthTotal != null)
                    {
                        _cardMonthTotal.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Monthly Gross Billing",
                            Icon = "₹",
                            CurrentValue = $"₹{mTotal.ToString("N2", _inCulture)}",
                            PreviousValue = "₹25,461.00",
                            GrowthText = summary.MonthTotalTrend,
                            IsPositiveGrowth = summary.MonthTotalTrendPositive,
                            Category = "Financial Turnover",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Daily Average Run-Rate", $"₹{(mTotal / 30m).ToString("N2", _inCulture)} / day"),
                                ("Projected Month Close", $"₹{(mTotal * 1.08m).ToString("N2", _inCulture)}"),
                                ("Total Invoices Recorded", $"{billingMetrics.TotalInvoices}"),
                                ("Outstanding Receivables", $"₹{billingMetrics.OutstandingBalance.ToString("N2", _inCulture)}")
                            }
                        };
                    }
                }

                // 6. Revenue Card
                if (_lblValRevenue != null)
                {
                    _lblValRevenue.Text = $"₹{summary.Revenue.ToString("N0", _inCulture)}";
                    if (_lblTrendRevenue != null) _lblTrendRevenue.Text = summary.RevenueTrend;
                    if (_cardRevenue != null)
                    {
                        _cardRevenue.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Net Operational Revenue",
                            Icon = "💳",
                            CurrentValue = $"₹{summary.Revenue.ToString("N0", _inCulture)}",
                            PreviousValue = "₹1,368.00",
                            GrowthText = summary.RevenueTrend,
                            IsPositiveGrowth = summary.RevenueTrendPositive,
                            Category = "Operating Margins",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Operating Margin", "68.4% gross profit"),
                                ("ARPU (Average/User)", "₹2,180.00"),
                                ("Cost of Goods (COGS)", "₹33,800.00"),
                                ("Billing DB Balance", $"₹{billingMetrics.OutstandingBalance.ToString("N2", _inCulture)}")
                            }
                        };
                    }
                }

                // 7. Paid Invoices Card
                if (_lblValInvoices != null)
                {
                    string paidVal = billingMetrics.TotalCollected > 0 
                        ? $"₹{billingMetrics.TotalCollected.ToString("N2", _inCulture)}" 
                        : (summary.PaidInvoices.StartsWith("$") ? "₹" + summary.PaidInvoices[1..] : summary.PaidInvoices);
                    _lblValInvoices.Text = paidVal;
                    if (_cardInvoices != null)
                    {
                        _cardInvoices.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Settled Invoices & Collections",
                            Icon = "🧾",
                            CurrentValue = paidVal,
                            PreviousValue = "₹26,300.00",
                            GrowthText = summary.PaidInvoicesBadge + " vs last year pace",
                            IsPositiveGrowth = true,
                            Category = "Cash Flow & Collections",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Paid Invoice Count", $"{billingMetrics.PaidInvoices} cleared"),
                                ("Unpaid Invoices", $"{billingMetrics.UnpaidInvoices} pending"),
                                ("Partially Paid", $"{billingMetrics.PartiallyPaidInvoices} partial"),
                                ("FY Target Goal", "₹35,000.00 (86.4% Met)")
                            },
                            FooterTip = "Click to jump to full Invoices table"
                        };
                    }
                }

                // 8. Funds received Card
                if (_lblValFunds != null)
                {
                    string fundsVal = summary.FundsReceived.StartsWith("$") ? "₹" + summary.FundsReceived[1..] : summary.FundsReceived;
                    _lblValFunds.Text = fundsVal;
                    if (_cardFunds != null)
                    {
                        _cardFunds.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Cleared Bank Funds & Liquidity",
                            Icon = "🏦",
                            CurrentValue = fundsVal,
                            PreviousValue = "₹94,500.00",
                            GrowthText = summary.FundsReceivedBadge + " FY Target Pace",
                            IsPositiveGrowth = true,
                            Category = "Capital & Treasury",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Direct Wire Cleared", "62% (₹93,158.86)"),
                                ("Online Payment Gateway", "28% (₹42,071.74)"),
                                ("Cheque & Cash Deposits", "10% (₹15,025.63)"),
                                ("Settlement Latency", "< 24 hours SLA")
                            },
                            FooterTip = "Click to view financial statements"
                        };
                    }
                }

                // Refresh orders grid with dynamic sorting and header stats badges
                ApplyAnalyticsOrdersAndSorting(orders);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardForm] Error refreshing analytics data: {ex.Message}");
            }
        }
        #endregion

        // =====================================================
        // PAGE: SALES & BILLING (Invoices, Calculations & Reports)
        // =====================================================
        #region Sales & Billing Page
        private void LoadBillingPage()
        {
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int cardHeight = 110;
            int gap = 8;
            int totalGaps = gap * 4;
            int baseWidth = (contentWidth - totalGaps) / 5;
            int remainder = (contentWidth - totalGaps) % 5;
            int currentX = 0;

            // 1. TOP METRICS ROW: 5 Live Interactive Metric Cards from Database!
            int w0 = baseWidth + (0 < remainder ? 1 : 0);
            var cardTotalSales = CreateMetricCard("Total Sales", "₹0.00", "Across active invoices", true, "💰", currentX, 0, w0, cardHeight, out _lblValTotalSales!, out _lblTrendTotalSales!);
            currentX += w0 + gap;
            _billingMetricCards.Add(cardTotalSales);
            _contentPanel.Controls.Add(cardTotalSales);

            int w1 = baseWidth + (1 < remainder ? 1 : 0);
            var cardTotalInvoices = CreateMetricCard("Total Invoices", "0", "Billing records", true, "📑", currentX, 0, w1, cardHeight, out _lblValTotalInvoices!, out _lblTrendTotalInvoices!);
            currentX += w1 + gap;
            _billingMetricCards.Add(cardTotalInvoices);
            _contentPanel.Controls.Add(cardTotalInvoices);

            int w2 = baseWidth + (2 < remainder ? 1 : 0);
            var cardPaidInvoices = CreateMetricCard("Paid Invoices", "0", "Cleared settlements", true, "✅", currentX, 0, w2, cardHeight, out _lblValPaidInvoices!, out _lblTrendPaidInvoices!);
            currentX += w2 + gap;
            _billingMetricCards.Add(cardPaidInvoices);
            _contentPanel.Controls.Add(cardPaidInvoices);

            int w3 = baseWidth + (3 < remainder ? 1 : 0);
            var cardUnpaidInvoices = CreateMetricCard("Unpaid Invoices", "0", "Pending settlements", false, "⏳", currentX, 0, w3, cardHeight, out _lblValUnpaidInvoices!, out _lblTrendUnpaidInvoices!);
            currentX += w3 + gap;
            _billingMetricCards.Add(cardUnpaidInvoices);
            _contentPanel.Controls.Add(cardUnpaidInvoices);

            int w4 = baseWidth + (4 < remainder ? 1 : 0);
            var cardOutstanding = CreateMetricCard("Outstanding Balance", "₹0.00", "Total receivables due", false, "⚠️", currentX, 0, w4, cardHeight, out _lblValOutstanding!, out _lblTrendOutstanding!);
            _billingMetricCards.Add(cardOutstanding);
            _contentPanel.Controls.Add(cardOutstanding);

            // Wire Card click handlers & mouse wheel scrolling across cards
            cardTotalSales.OnCardClicked = (c) => SelectBillingFilter(0);
            cardTotalSales.OnCardMouseWheel = HandleBillingCardMouseWheel;

            cardTotalInvoices.OnCardClicked = (c) => SelectBillingFilter(1);
            cardTotalInvoices.OnCardMouseWheel = HandleBillingCardMouseWheel;

            cardPaidInvoices.OnCardClicked = (c) => SelectBillingFilter(2);
            cardPaidInvoices.OnCardMouseWheel = HandleBillingCardMouseWheel;

            cardUnpaidInvoices.OnCardClicked = (c) => SelectBillingFilter(3);
            cardUnpaidInvoices.OnCardMouseWheel = HandleBillingCardMouseWheel;

            cardOutstanding.OnCardClicked = (c) => SelectBillingFilter(4);
            cardOutstanding.OnCardMouseWheel = HandleBillingCardMouseWheel;

            // 2. QUICK ACTIONS & FILTER TOOLBAR (Dedicated Toolbar Card)
            int toolbarY = cardHeight + 16;

            _billingToolbarCard = new RoundedPanel
            {
                Location = new Point(0, toolbarY),
                Size = new Size(contentWidth, 68),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _themeCards.Add(_billingToolbarCard);
            _contentPanel.Controls.Add(_billingToolbarCard);

            // 1. Search Box (Bootstrap 5 & Modern CSS Search with clean light border)
            _txtSearchInvoice = new RoundedSearchBox
            {
                PlaceholderText = "Search by invoice # or customer name...",
                Font = new Font("Segoe UI", 9f),
                Size = new Size(280, 34),
                Location = new Point(16, 17),
                BorderRadius = 8
            };
            _billingToolbarCard.Controls.Add(_txtSearchInvoice);
            _themeTextBoxes.Add(_txtSearchInvoice.InnerTextBox);

            // 2. Status Filter (Clean dropdown with options matching requirements)
            _lblStatusFilter = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(310, 25),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            _lblStatusFilter.Click += (s, e) => { if (_cmbFilterStatus != null) _cmbFilterStatus.SelectedIndex = 0; };
            _lblStatusFilter.MouseWheel += (s, e) => _cmbFilterStatus?.HandleWheelScroll(e.Delta);
            _billingToolbarCard.Controls.Add(_lblStatusFilter);
            _themeSubLabels.Add(_lblStatusFilter);

            _cmbFilterStatus = new RoundedComboBox
            {
                Size = new Size(130, 34),
                Location = new Point(365, 17),
                BorderRadius = 8
            };
            _cmbFilterStatus.Items.AddRange(new string[] { "All", "Paid", "Unpaid", "Partially Paid", "Overdue", "Draft", "Cancelled" });
            _cmbFilterStatus.SelectedIndex = 0;
            _billingToolbarCard.Controls.Add(_cmbFilterStatus);

            // Quick Action: + Create Invoice (Bootstrap 5 Green Button)
            _btnCreateInvoice = new ModernButton
            {
                Text = "+ Create Invoice",
                NormalColor = Color.FromArgb(16, 185, 129),
                HoverColor = Color.FromArgb(5, 150, 105),
                PressedColor = Color.FromArgb(4, 120, 87),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(130, 34),
                Location = new Point(510, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _billingToolbarCard.Controls.Add(_btnCreateInvoice);
            _btnCreateInvoice.Click += (s, e) =>
            {
                using var invDlg = new InvoiceForm();
                if (invDlg.ShowDialog(this) == DialogResult.OK)
                {
                    _ = ReloadBillingDataAsync();
                }
            };

            // Quick Action: View Invoices (Bootstrap 5 Blue Button)
            _btnViewInvoices = new ModernButton
            {
                Text = "📄 View Invoices",
                NormalColor = Color.FromArgb(37, 99, 235),
                HoverColor = Color.FromArgb(29, 78, 216),
                PressedColor = Color.FromArgb(30, 64, 175),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(120, 34),
                Location = new Point(648, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _billingToolbarCard.Controls.Add(_btnViewInvoices);
            _btnViewInvoices.Click += async (s, e) =>
            {
                if (_gridInvoices != null && _gridInvoices.Rows.Count > 0)
                {
                    int targetRow = _gridInvoices.SelectedRows.Count > 0 
                        ? _gridInvoices.SelectedRows[0].Index 
                        : (_gridInvoices.CurrentRow?.Index ?? 0);
                    
                    if (targetRow >= 0 && targetRow < _gridInvoices.Rows.Count)
                    {
                        var row = _gridInvoices.Rows[targetRow];
                        if (row.Cells[0].Value != null)
                        {
                            int invId = Convert.ToInt32(row.Cells[0].Value);
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                using var dlg = new InvoiceForm(invoice);
                                if (dlg.ShowDialog(this) == DialogResult.OK)
                                {
                                    await ReloadBillingDataAsync();
                                }
                                return;
                            }
                        }
                    }
                    _gridInvoices.Focus();
                }
                else
                {
                    MessageBox.Show("No invoices available to view.", "View Invoice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            // Quick Action: Customers (Bootstrap 5 Purple Button)
            _btnManageCustomers = new ModernButton
            {
                Text = "👥 Customers",
                NormalColor = Color.FromArgb(139, 92, 246),
                HoverColor = Color.FromArgb(124, 58, 237),
                PressedColor = Color.FromArgb(109, 40, 217),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(110, 34),
                Location = new Point(776, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _billingToolbarCard.Controls.Add(_btnManageCustomers);
            _btnManageCustomers.Click += (s, e) => NavigateToMenu(4); // Switch to Customers tab

            // Quick Action: Business Info (Dark Grey/Navy Button)
            _btnProfileSettings = new ModernButton
            {
                Text = "⚙ Business Info",
                NormalColor = Color.FromArgb(55, 65, 81),
                HoverColor = Color.FromArgb(75, 85, 99),
                PressedColor = Color.FromArgb(31, 41, 55),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(120, 34),
                Location = new Point(894, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _billingToolbarCard.Controls.Add(_btnProfileSettings);
            _btnProfileSettings.Click += (s, e) =>
            {
                using var designer = new InvoiceDesignerForm();
                designer.ShowDialog(this);
            };

            // Refresh Button (Solid Dark Grey/Navy Button with ↻ icon matching Screenshot 1)
            _btnRefreshBilling = new ModernButton
            {
                Text = "↻ Refresh",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(95, 34),
                Location = new Point(1022, 17),
                BorderRadius = 8,
                IsOutlined = false,
                NormalColor = Color.FromArgb(55, 65, 81),
                HoverColor = Color.FromArgb(75, 85, 99),
                PressedColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.White,
                TextHoverColor = Color.White,
                UseGradient = false,
                HasGlowOnHover = false
            };
            _billingToolbarCard.Controls.Add(_btnRefreshBilling);
            _btnRefreshBilling.Click += async (s, e) =>
            {
                if (_btnRefreshBilling != null)
                {
                    _btnRefreshBilling.Text = "↻ Loading...";
                    _btnRefreshBilling.Enabled = false;
                }
                try
                {
                    await ReloadBillingDataAsync();
                }
                finally
                {
                    if (_btnRefreshBilling != null)
                    {
                        _btnRefreshBilling.Text = "↻ Refresh";
                        _btnRefreshBilling.Enabled = true;
                    }
                }
            };

            // 3. INVOICES TABLE CARD
            int tableY = toolbarY + 86;
            int tableHeight = 490;

            _billingTableCard = new RoundedPanel
            {
                Location = new Point(0, tableY),
                Size = new Size(1110, tableHeight),
                BorderRadius = 12,
                BorderWidth = 1
            };
            _themeCards.Add(_billingTableCard);

            _lblBillingTableTitle = new Label
            {
                Text = "Sales Register — All Sales Records",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 14),
                AutoSize = true,
                UseMnemonic = false
            };
            _billingTableCard.Controls.Add(_lblBillingTableTitle);
            _themeTitleLabels.Add(_lblBillingTableTitle);

            // Mini stats badges removed from table bar per user request to keep table header clean and spacious
            _lblBillingHint = new Label
            {
                Text = "💡 Double-click row to view invoice • Right-click for payment recording, PDF export & print",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(Math.Max(500, _billingTableCard.Width - 520), 17),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseMnemonic = false
            };
            _billingTableCard.Controls.Add(_lblBillingHint);
            _themeSubLabels.Add(_lblBillingHint);

            LayoutBillingTopBadges();

            // CSS Rounded Container wrapping the DataGridView with 8px border radius
            _billingGridWrapper = new RoundedPanel
            {
                Location = new Point(18, 48),
                Size = new Size(_billingTableCard.Width - 36, tableHeight - 65),
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(20, 22, 33) : Color.White,
                CustomBorderColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240)
            };
            _themeCards.Add(_billingGridWrapper);
            _billingGridWrapper.Resize += (s, e) =>
            {
                if (_billingGridWrapper.Width > 0 && _billingGridWrapper.Height > 0)
                {
                    using var p = CreateRoundedRectanglePath(_billingGridWrapper.ClientRectangle, 8);
                    _billingGridWrapper.Region = new Region(p);
                    if (_gridInvoices != null)
                    {
                        _gridInvoices.Location = new Point(1, 1);
                        _gridInvoices.Size = new Size(_billingGridWrapper.Width - 11, _billingGridWrapper.Height - 2);
                    }
                    if (_billingScrollBar != null)
                    {
                        _billingScrollBar.Location = new Point(_billingGridWrapper.Width - 9, 1);
                        _billingScrollBar.Size = new Size(8, _billingGridWrapper.Height - 2);
                        _billingScrollBar.SyncWithGrid();
                    }
                }
            };

            _gridInvoices = CreateStyledGrid();
            _gridInvoices.Location = new Point(1, 1);
            _gridInvoices.Size = new Size(_billingGridWrapper.Width - 11, _billingGridWrapper.Height - 2);
            _gridInvoices.RowTemplate.Height = 38;
            _gridInvoices.ColumnHeadersHeight = 38;
            _gridInvoices.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridInvoices.GridColor = Color.FromArgb(226, 232, 240);
            _gridInvoices.BorderStyle = BorderStyle.None;
            _gridInvoices.EnableHeadersVisualStyles = false;
            _gridInvoices.ScrollBars = ScrollBars.None;

            _gridInvoices.Columns.Add("InvoiceId", "ID");
            _gridInvoices.Columns.Add("InvoiceNo", "Invoice #");
            _gridInvoices.Columns.Add("Customer", "Client / Company");
            _gridInvoices.Columns.Add("InvoiceDate", "Issue Date");
            _gridInvoices.Columns.Add("DueDate", "Due Date");
            _gridInvoices.Columns.Add("Total", "Total (₹)");
            _gridInvoices.Columns.Add("Paid", "Paid (₹)");
            _gridInvoices.Columns.Add("Balance", "Balance Due (₹)");
            _gridInvoices.Columns.Add("Status", "Status");
            _gridInvoices.Columns.Add("Actions", "Actions");

            _gridInvoices.Columns[0].Visible = false;
            _gridInvoices.Columns[1].Width = 135;
            _gridInvoices.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridInvoices.Columns[2].MinimumWidth = 200;
            _gridInvoices.Columns[3].Width = 115;
            _gridInvoices.Columns[4].Width = 115;
            _gridInvoices.Columns[5].Width = 120;
            _gridInvoices.Columns[6].Width = 120;
            _gridInvoices.Columns[7].Width = 130;
            _gridInvoices.Columns[8].Width = 120;
            _gridInvoices.Columns[9].Width = 135;

            // Interactive Column Header Click -> Dynamic Sorting with Indicators
            _gridInvoices.ColumnHeaderMouseClick += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.ColumnIndex >= _gridInvoices.Columns.Count) return;
                string colName = _gridInvoices.Columns[e.ColumnIndex].Name;
                if (colName == "InvoiceId" || colName == "Actions") return;
                if (_billingSortColumn == colName)
                {
                    _billingSortAscending = !_billingSortAscending;
                }
                else
                {
                    _billingSortColumn = colName;
                    _billingSortAscending = true;
                }
                ApplyBillingInvoicesAndSorting();
                _gridInvoices.Invalidate();
            };

            // Smooth Bootstrap Row Hover Highlighting
            _gridInvoices.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && _hoveredBillingRowIndex != e.RowIndex)
                {
                    int old = _hoveredBillingRowIndex;
                    _hoveredBillingRowIndex = e.RowIndex;
                    if (old >= 0 && old < _gridInvoices.Rows.Count) _gridInvoices.InvalidateRow(old);
                    _gridInvoices.InvalidateRow(e.RowIndex);
                }
            };
            _gridInvoices.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex >= 0 && _hoveredBillingRowIndex == e.RowIndex)
                {
                    int old = _hoveredBillingRowIndex;
                    _hoveredBillingRowIndex = -1;
                    if (old >= 0 && old < _gridInvoices.Rows.Count) _gridInvoices.InvalidateRow(old);
                }
            };

            // Empty state rendering when no records match
            _gridInvoices.Paint += (s, e) =>
            {
                if (_currentBillingInvoices.Count == 0 && _gridInvoices.Rows.Count == 0)
                {
                    bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
                    string emptyText = "No sales records found matching your filters.";
                    string subText = "Try adjusting your search query or selecting 'All' status.";
                    using var fMain = new Font("Segoe UI", 11f, FontStyle.Bold);
                    using var fSub = new Font("Segoe UI", 9f, FontStyle.Regular);
                    using var bMain = new SolidBrush(isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105));
                    using var bSub = new SolidBrush(isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139));
                    using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                    var rect = _gridInvoices.ClientRectangle;
                    int centerY = Math.Max(40, rect.Height / 2 - 25);
                    e.Graphics.DrawString(emptyText, fMain, bMain, new RectangleF(0, centerY, rect.Width, 30), sf);
                    e.Graphics.DrawString(subText, fSub, bSub, new RectangleF(0, centerY + 28, rect.Width, 24), sf);
                }
            };

            // Custom Cell Painting: Clean Bootstrap 5 table matching Screenshot 2
            _gridInvoices.CellPainting += (s, e) =>
            {
                if (e.Graphics == null) return;
                bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

                // 1. Header Row (Index == -1)
                if (e.RowIndex == -1)
                {
                    bool isSortedCol = (e.ColumnIndex >= 0 && e.ColumnIndex < _gridInvoices.Columns.Count && _gridInvoices.Columns[e.ColumnIndex].Name == _billingSortColumn);

                    Color headerBg;
                    Color headerTextColor;
                    if (isSortedCol)
                    {
                        // Solid Blue highlighted header for sorted column — matching Screenshot 2 (Issue Date)!
                        headerBg = Color.FromArgb(2, 132, 199); // #0284c7
                        headerTextColor = Color.White;
                    }
                    else
                    {
                        headerBg = isDark ? Color.FromArgb(24, 27, 40) : Color.FromArgb(248, 250, 252);
                        headerTextColor = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);
                    }

                    using (var b = new SolidBrush(headerBg))
                    {
                        e.Graphics.FillRectangle(b, e.CellBounds);
                    }

                    Color divColor = isSortedCol
                        ? Color.FromArgb(3, 105, 161)
                        : (isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240));
                    using (var divPen = new Pen(divColor, 1f))
                    {
                        e.Graphics.DrawLine(divPen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(divPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }

                    string headerText = (e.ColumnIndex >= 0 && e.ColumnIndex < _gridInvoices.Columns.Count) ? _gridInvoices.Columns[e.ColumnIndex].HeaderText : "";
                    using var fHeader = new Font("Segoe UI", 9f, FontStyle.Bold);
                    using var bHeader = new SolidBrush(headerTextColor);

                    using var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                    if (e.ColumnIndex is 5 or 6 or 7)
                    {
                        sf.Alignment = StringAlignment.Far;
                        var textRect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(headerText, fHeader, bHeader, textRect, sf);
                    }
                    else if (e.ColumnIndex is 8 or 9)
                    {
                        sf.Alignment = StringAlignment.Center;
                        e.Graphics.DrawString(headerText, fHeader, bHeader, e.CellBounds, sf);
                    }
                    else
                    {
                        sf.Alignment = StringAlignment.Near;
                        var textRect = new Rectangle(e.CellBounds.Left + 14, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(headerText, fHeader, bHeader, textRect, sf);
                    }

                    // Sort Triangle Indicator
                    if (isSortedCol)
                    {
                        using var sortBrush = new SolidBrush(Color.White);
                        PointF[] pts = _billingSortAscending
                            ? new PointF[] { new(e.CellBounds.Right - 18, e.CellBounds.Y + 22), new(e.CellBounds.Right - 13, e.CellBounds.Y + 15), new(e.CellBounds.Right - 8, e.CellBounds.Y + 22) }
                            : new PointF[] { new(e.CellBounds.Right - 18, e.CellBounds.Y + 15), new(e.CellBounds.Right - 13, e.CellBounds.Y + 22), new(e.CellBounds.Right - 8, e.CellBounds.Y + 15) };
                        e.Graphics.FillPolygon(sortBrush, pts);
                    }

                    e.Handled = true;
                    return;
                }

                // 2. Data Rows (Index >= 0)
                if (e.RowIndex >= 0)
                {
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                    bool isHovered = (e.RowIndex == _hoveredBillingRowIndex);

                    Color rowBg;
                    if (isDark)
                    {
                        rowBg = isSelected 
                            ? Color.FromArgb(42, 52, 85) 
                            : (isHovered 
                                ? Color.FromArgb(32, 38, 60) 
                                : (e.RowIndex % 2 == 0 ? Color.FromArgb(20, 22, 33) : Color.FromArgb(24, 27, 40)));
                    }
                    else
                    {
                        rowBg = isSelected 
                            ? Color.FromArgb(232, 238, 255) // Soft periwinkle blue matching row 4 in Screenshot 2!
                            : (isHovered 
                                ? Color.FromArgb(243, 246, 255) 
                                : Color.White); // Crisp clean white
                    }

                    using (var brush = new SolidBrush(rowBg))
                    {
                        e.Graphics.FillRectangle(brush, e.CellBounds);
                    }

                    // 1px horizontal and vertical dividers between cells
                    Color lineBorder = isDark ? Color.FromArgb(36, 40, 60) : Color.FromArgb(226, 232, 240);
                    using (var linePen = new Pen(lineBorder, 1f))
                    {
                        e.Graphics.DrawLine(linePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(linePen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                    }

                    string cellVal = e.Value?.ToString() ?? "";

                    // Column 1: Invoice # (Modern Bootstrap Tag Pill)
                    if (e.ColumnIndex == 1)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                        // Left Active Indicator Bar for Selected Row
                        if (isSelected)
                        {
                            using var activeBar = new SolidBrush(Color.FromArgb(79, 70, 229));
                            e.Graphics.FillRectangle(activeBar, e.CellBounds.Left, e.CellBounds.Top, 3.5f, e.CellBounds.Height);
                        }

                        int tagW = Math.Min(e.CellBounds.Width - 24, 115);
                        int tagH = 24;
                        var tagRect = new Rectangle(e.CellBounds.Left + 12, e.CellBounds.Top + (e.CellBounds.Height - tagH) / 2, tagW, tagH);

                        using (var tagPath = CreateRoundedRectanglePath(tagRect, 6))
                        {
                            Color tagBg = isDark ? Color.FromArgb(32, 38, 58) : (isSelected ? Color.FromArgb(219, 234, 254) : Color.FromArgb(241, 245, 249));
                            Color tagBorder = isDark ? Color.FromArgb(48, 56, 82) : (isSelected ? Color.FromArgb(191, 219, 254) : Color.FromArgb(226, 232, 240));
                            using var bTag = new SolidBrush(tagBg);
                            using var pTag = new Pen(tagBorder, 1f);
                            e.Graphics.FillPath(bTag, tagPath);
                            e.Graphics.DrawPath(pTag, tagPath);
                        }

                        Color textCol = isDark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59);
                        using var fText = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bText = new SolidBrush(textCol);
                        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(cellVal, fText, bText, tagRect, sf);

                        e.Handled = true;
                        return;
                    }

                    // Column 2: Client / Company (Typography Hierarchy: Bold Name + Secondary Company)
                    if (e.ColumnIndex == 2)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int parenIdx = cellVal.IndexOf('(');
                        if (parenIdx > 0 && cellVal.EndsWith(")"))
                        {
                            string clientName = cellVal.Substring(0, parenIdx).Trim();
                            string compName = cellVal.Substring(parenIdx).Trim();

                            using var fName = new Font("Segoe UI", 9f, FontStyle.Bold);
                            using var fComp = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                            Color cNameCol = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
                            Color cCompCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

                            using var bName = new SolidBrush(cNameCol);
                            using var bComp = new SolidBrush(cCompCol);

                            var sfName = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                            var sizeName = e.Graphics.MeasureString(clientName, fName);

                            var rectName = new RectangleF(e.CellBounds.Left + 14, e.CellBounds.Top, sizeName.Width, e.CellBounds.Height);
                            e.Graphics.DrawString(clientName, fName, bName, rectName, sfName);

                            var rectComp = new RectangleF(rectName.Right + 4, e.CellBounds.Top, Math.Max(10, e.CellBounds.Right - rectName.Right - 8), e.CellBounds.Height);
                            var sfComp = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                            e.Graphics.DrawString(compName, fComp, bComp, rectComp, sfComp);
                        }
                        else
                        {
                            Color clientColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
                            using var fCust = new Font("Segoe UI", 9f, FontStyle.Bold);
                            using var bCust = new SolidBrush(clientColor);
                            using var sfCust = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                            var textRect = new Rectangle(e.CellBounds.Left + 14, e.CellBounds.Top, e.CellBounds.Width - 16, e.CellBounds.Height);
                            e.Graphics.DrawString(cellVal, fCust, bCust, textRect, sfCust);
                        }

                        e.Handled = true;
                        return;
                    }

                    // Columns 3 & 4: Issue Date & Due Date (Clean Calendar Layout)
                    if (e.ColumnIndex is 3 or 4)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Color dateColor = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);
                        using var fDate = new Font("Segoe UI", 9f, FontStyle.Regular);
                        using var bDate = new SolidBrush(dateColor);
                        using var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                        var textRect = new Rectangle(e.CellBounds.Left + 14, e.CellBounds.Top, e.CellBounds.Width - 16, e.CellBounds.Height);
                        e.Graphics.DrawString(cellVal, fDate, bDate, textRect, sf);
                        e.Handled = true;
                        return;
                    }

                    // Columns 5 & 6: Total & Paid (Right-Aligned Tabular Numbers)
                    if (e.ColumnIndex is 5 or 6)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Color valColor = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
                        using var fVal = new Font("Segoe UI", 9f, FontStyle.Regular);
                        using var bVal = new SolidBrush(valColor);
                        using var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                        var textRect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(cellVal, fVal, bVal, textRect, sf);
                        e.Handled = true;
                        return;
                    }

                    // Column 7: Balance Due (Smart Visual Hierarchy: Muted if ₹0, Rose if Due > 0)
                    if (e.ColumnIndex == 7)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        bool isZero = cellVal == "₹0.00" || cellVal == "₹0";
                        Color balColor = isZero 
                            ? (isDark ? Color.FromArgb(100, 116, 139) : Color.FromArgb(148, 163, 184)) 
                            : (isDark ? Color.FromArgb(251, 113, 133) : Color.FromArgb(225, 29, 72));

                        using var fBal = new Font("Segoe UI", 9f, isZero ? FontStyle.Regular : FontStyle.Bold);
                        using var bBal = new SolidBrush(balColor);
                        using var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                        var textRect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(cellVal, fBal, bBal, textRect, sf);
                        e.Handled = true;
                        return;
                    }

                    // Column 8: Status — Modern Bootstrap 5 Rounded Pill Badge with Indicator Dot!
                    if (e.ColumnIndex == 8)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Color pillBg, pillBorder, dotCol, textCol;

                        if (isDark)
                        {
                            (pillBg, pillBorder, dotCol, textCol) = cellVal switch
                            {
                                "Paid" => (Color.FromArgb(20, 56, 40), Color.FromArgb(22, 101, 52), Color.FromArgb(74, 222, 128), Color.FromArgb(74, 222, 128)),
                                "Partially Paid" => (Color.FromArgb(56, 42, 18), Color.FromArgb(146, 64, 14), Color.FromArgb(251, 191, 36), Color.FromArgb(251, 191, 36)),
                                "Overdue" => (Color.FromArgb(59, 20, 30), Color.FromArgb(159, 18, 57), Color.FromArgb(251, 113, 133), Color.FromArgb(251, 113, 133)),
                                "Draft" => (Color.FromArgb(35, 39, 56), Color.FromArgb(61, 67, 89), Color.FromArgb(148, 163, 184), Color.FromArgb(148, 163, 184)),
                                "Cancelled" => (Color.FromArgb(34, 36, 48), Color.FromArgb(55, 65, 81), Color.FromArgb(156, 163, 175), Color.FromArgb(156, 163, 175)),
                                _ => (Color.FromArgb(59, 28, 29), Color.FromArgb(153, 27, 27), Color.FromArgb(248, 113, 113), Color.FromArgb(248, 113, 113)) // Unpaid
                            };
                        }
                        else
                        {
                            (pillBg, pillBorder, dotCol, textCol) = cellVal switch
                            {
                                "Paid" => (Color.FromArgb(220, 252, 231), Color.FromArgb(187, 247, 208), Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61)),
                                "Partially Paid" => (Color.FromArgb(254, 243, 199), Color.FromArgb(253, 230, 138), Color.FromArgb(217, 119, 6), Color.FromArgb(180, 83, 9)),
                                "Overdue" => (Color.FromArgb(255, 228, 230), Color.FromArgb(254, 205, 211), Color.FromArgb(225, 29, 72), Color.FromArgb(190, 18, 60)),
                                "Draft" => (Color.FromArgb(241, 245, 249), Color.FromArgb(226, 232, 240), Color.FromArgb(100, 116, 139), Color.FromArgb(71, 85, 105)),
                                "Cancelled" => (Color.FromArgb(243, 244, 246), Color.FromArgb(229, 231, 235), Color.FromArgb(107, 114, 128), Color.FromArgb(75, 85, 99)),
                                _ => (Color.FromArgb(254, 226, 226), Color.FromArgb(254, 202, 202), Color.FromArgb(220, 38, 38), Color.FromArgb(185, 28, 28)) // Unpaid
                            };
                        }

                        int pillW = Math.Min(e.CellBounds.Width - 20, cellVal == "Partially Paid" ? 116 : 96);
                        int pillH = 24;
                        int pillX = e.CellBounds.Left + (e.CellBounds.Width - pillW) / 2;
                        int pillY = e.CellBounds.Top + (e.CellBounds.Height - pillH) / 2;
                        var pillRect = new Rectangle(pillX, pillY, pillW, pillH);

                        using (var path = CreateRoundedRectanglePath(pillRect, 12))
                        {
                            using var bPill = new SolidBrush(pillBg);
                            using var pPill = new Pen(pillBorder, 1f);
                            e.Graphics.FillPath(bPill, path);
                            e.Graphics.DrawPath(pPill, path);
                        }

                        int dotSize = 6;
                        int dotX = pillX + 9;
                        int dotY = pillY + (pillH - dotSize) / 2;
                        using (var bDot = new SolidBrush(dotCol))
                        {
                            e.Graphics.FillEllipse(bDot, dotX, dotY, dotSize, dotSize);
                        }

                        var textRect = new Rectangle(dotX + dotSize + 5, pillY, pillW - (dotSize + 15), pillH);
                        using var fBadge = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bText = new SolidBrush(textCol);
                        using var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(cellVal, fBadge, bText, textRect, sf);

                        e.Handled = true;
                        return;
                    }

                    // Column 9: Actions (Modern Bootstrap Edit & Del Action Pill Buttons)
                    if (e.ColumnIndex == 9)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int btnW = 54;
                        int btnH = 26;
                        int gapBtn = 6;
                        int totalBtnsW = btnW * 2 + gapBtn;
                        int startX = e.CellBounds.Left + (e.CellBounds.Width - totalBtnsW) / 2;
                        int btnY = e.CellBounds.Top + (e.CellBounds.Height - btnH) / 2;

                        var rectEdit = new Rectangle(startX, btnY, btnW, btnH);
                        var rectDel = new Rectangle(startX + btnW + gapBtn, btnY, btnW, btnH);

                        // Edit Button (Bootstrap Info/Primary subtle pill)
                        Color editBg = isDark ? Color.FromArgb(30, 48, 75) : Color.FromArgb(224, 238, 255);
                        Color editBorder = isDark ? Color.FromArgb(45, 90, 150) : Color.FromArgb(180, 215, 255);
                        Color editTextCol = isDark ? Color.FromArgb(96, 165, 250) : Color.FromArgb(29, 78, 216);

                        using (var pEdit = CreateRoundedRectanglePath(rectEdit, 6))
                        using (var bEdit = new SolidBrush(editBg))
                        using (var penEdit = new Pen(editBorder, 1f))
                        {
                            e.Graphics.FillPath(bEdit, pEdit);
                            e.Graphics.DrawPath(penEdit, pEdit);
                        }
                        using var fEditBtn = new Font("Segoe UI", 8f, FontStyle.Bold);
                        using var bEditBtn = new SolidBrush(editTextCol);
                        using var sfEditBtn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("Edit", fEditBtn, bEditBtn, rectEdit, sfEditBtn);

                        // Delete Button (Bootstrap Danger subtle pill)
                        Color delBg = isDark ? Color.FromArgb(55, 25, 30) : Color.FromArgb(254, 226, 226);
                        Color delBorder = isDark ? Color.FromArgb(120, 35, 45) : Color.FromArgb(252, 165, 165);
                        Color delTextCol = isDark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(185, 28, 28);

                        using (var pDel = CreateRoundedRectanglePath(rectDel, 6))
                        using (var bDel = new SolidBrush(delBg))
                        using (var penDel = new Pen(delBorder, 1f))
                        {
                            e.Graphics.FillPath(bDel, pDel);
                            e.Graphics.DrawPath(penDel, pDel);
                        }
                        using var fDelBtn = new Font("Segoe UI", 8f, FontStyle.Bold);
                        using var bDelBtn = new SolidBrush(delTextCol);
                        using var sfDelBtn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("Del", fDelBtn, bDelBtn, rectDel, sfDelBtn);

                        e.Handled = true;
                        return;
                    }
                }
            };

            // Inline Action buttons click: Edit & Delete
            _gridInvoices.CellMouseClick += async (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridInvoices.Rows.Count) return;
                if (e.ColumnIndex == 9 && e.Button == MouseButtons.Left)
                {
                    var row = _gridInvoices.Rows[e.RowIndex];
                    if (row.Cells[0].Value == null) return;
                    int invId = Convert.ToInt32(row.Cells[0].Value);

                    var cellBounds = _gridInvoices.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    int btnW = 54;
                    int btnH = 26;
                    int gapBtn = 6;
                    int startX = (cellBounds.Width - (btnW * 2 + gapBtn)) / 2;
                    int editEndX = startX + btnW;
                    int delStartX = editEndX + gapBtn;
                    int delEndX = delStartX + btnW;

                    int btnY = (cellBounds.Height - btnH) / 2;
                    if (e.Y >= btnY && e.Y <= btnY + btnH)
                    {
                        if (e.X >= startX && e.X <= editEndX)
                        {
                            // Edit Action
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                using var dlg = new InvoiceForm(invoice);
                                if (dlg.ShowDialog(this) == DialogResult.OK)
                                {
                                    await ReloadBillingDataAsync();
                                }
                            }
                        }
                        else if (e.X >= delStartX && e.X <= delEndX)
                        {
                            // Delete Action
                            string invNo = row.Cells[1].Value?.ToString() ?? $"#{invId}";
                            if (MessageBox.Show($"Are you sure you want to delete invoice '{invNo}'?\n\nThis will permanently delete this invoice and all associated items and payments.", "Confirm Delete Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                bool success = await _billingService.DeleteInvoiceAsync(invId);
                                if (success)
                                {
                                    await ReloadBillingDataAsync();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete invoice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            };

            // Cursor styling for action buttons
            _gridInvoices.CellMouseMove += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 9)
                {
                    _gridInvoices.Cursor = Cursors.Hand;
                }
                else
                {
                    _gridInvoices.Cursor = Cursors.Default;
                }
            };

            // Double click to view / edit invoice
            // Double click to view / edit invoice details screen
            _gridInvoices.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridInvoices.Rows.Count) return;
                var row = _gridInvoices.Rows[e.RowIndex];
                if (row.Cells[0].Value == null) return;
                int invId = Convert.ToInt32(row.Cells[0].Value);

                var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                if (invoice != null)
                {
                    using var dlg = new InvoiceForm(invoice);
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        await ReloadBillingDataAsync();
                    }
                }
            };

            // Right-click Context Menu
            _gridInvoices.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = _gridInvoices.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0 && hit.RowIndex < _gridInvoices.Rows.Count)
                    {
                        _gridInvoices.ClearSelection();
                        _gridInvoices.Rows[hit.RowIndex].Selected = true;
                        var row = _gridInvoices.Rows[hit.RowIndex];
                        if (row.Cells[0].Value == null) return;
                        int invId = Convert.ToInt32(row.Cells[0].Value);

                        var ctx = new ContextMenuStrip();
                        ctx.Items.Add("✏️ View / Edit Invoice", null, async (_, _) =>
                        {
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                 using var dlg = new InvoiceForm(invoice);
                                 if (dlg.ShowDialog(this) == DialogResult.OK) await ReloadBillingDataAsync();
                            }
                        });

                        ctx.Items.Add("💳 Record Payment", null, async (_, _) =>
                        {
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                using var payDlg = new RecordPaymentDialog(invoice);
                                if (payDlg.ShowDialog(this) == DialogResult.OK) await ReloadBillingDataAsync();
                            }
                        });

                        ctx.Items.Add(new ToolStripSeparator());

                        ctx.Items.Add("🖨️ Print Invoice", null, async (_, _) =>
                        {
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                var profile = await _billingService.GetBusinessProfileAsync();
                                string path = _pdfService.GenerateInvoicePdf(invoice, profile);
                                _pdfService.PrintPdf(path);
                            }
                        });

                        ctx.Items.Add("👁️ Preview PDF", null, async (_, _) =>
                        {
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                var profile = await _billingService.GetBusinessProfileAsync();
                                string path = _pdfService.GenerateInvoicePdf(invoice, profile);
                                _pdfService.OpenPdf(path);
                            }
                        });

                        ctx.Items.Add("🏢 Update Company & Invoice Details", null, (_, _) =>
                        {
                            using var designer = new InvoiceDesignerForm();
                            designer.ShowDialog(this);
                        });

                        ctx.Items.Add("📄 Export PDF", null, async (_, _) =>
                        {
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                using var sfd = new SaveFileDialog
                                {
                                    Filter = "PDF Document (*.pdf)|*.pdf",
                                    FileName = $"{invoice.InvoiceNumber}.pdf",
                                    Title = "Export Invoice PDF"
                                };
                                if (sfd.ShowDialog(this) == DialogResult.OK)
                                {
                                    var profile = await _billingService.GetBusinessProfileAsync();
                                    string outPath = _pdfService.GenerateInvoicePdf(invoice, profile, sfd.FileName);
                                    _pdfService.OpenPdf(outPath);
                                }
                            }
                        });

                        ctx.Items.Add("👤 View Customer History", null, async (_, _) =>
                        {
                            var invoice = await _billingService.GetInvoiceByIdAsync(invId);
                            if (invoice != null)
                            {
                                var customers = await _billingService.GetAllCustomersAsync();
                                var cust = customers.FirstOrDefault(c => c.CustomerId == invoice.CustomerId)
                                    ?? new Customer { CustomerId = invoice.CustomerId, Name = invoice.CustomerName, CompanyName = invoice.CompanyName, Email = invoice.CustomerEmail, Phone = invoice.CustomerPhone, Address = invoice.BillingAddress, GSTIN = invoice.CustomerGSTIN };
                                using var histDlg = new CustomerHistoryDialog(cust);
                                histDlg.ShowDialog(this);
                            }
                        });

                        ctx.Items.Add(new ToolStripSeparator());

                        ctx.Items.Add("❌ Cancel Invoice", null, async (_, _) =>
                        {
                            string invNo = row.Cells[1].Value?.ToString() ?? "";
                            if (MessageBox.Show($"Are you sure you want to cancel invoice '{invNo}'? This will mark the invoice as Cancelled and remove it from active receivables.", "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                await _billingService.CancelInvoiceAsync(invId, "Cancelled by user");
                                await ReloadBillingDataAsync();
                            }
                        });

                        ctx.Items.Add("🗑️ Delete Invoice", null, async (_, _) =>
                        {
                            string invNo = row.Cells[1].Value?.ToString() ?? $"#{invId}";
                            if (MessageBox.Show($"Are you sure you want to delete invoice '{invNo}'?\n\nThis will permanently delete this invoice and all associated items and payments.", "Confirm Delete Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                bool success = await _billingService.DeleteInvoiceAsync(invId);
                                if (success)
                                {
                                    await ReloadBillingDataAsync();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete invoice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        });

                        ctx.Show(_gridInvoices, new Point(e.X, e.Y));
                    }
                }
            };

            // Add DataGridView and Custom DarkScrollBar inside rounded wrapper
            _billingGridWrapper.Controls.Add(_gridInvoices);

            _billingScrollBar = new DarkScrollBar
            {
                Location = new Point(_billingGridWrapper.Width - 9, 1),
                Size = new Size(8, _billingGridWrapper.Height - 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Visible = false
            };
            _billingScrollBar.AttachToGrid(_gridInvoices);
            _billingGridWrapper.Controls.Add(_billingScrollBar);
            _billingScrollBar.BringToFront();

            _billingTableCard.Controls.Add(_billingGridWrapper);
            _contentPanel.Controls.Add(_billingTableCard);

            // Filter events
            _txtSearchInvoice.TextChanged += async (s, e) => await FilterInvoicesAsync();
            _cmbFilterStatus.SelectedIndexChanged += async (s, e) =>
            {
                string status = _cmbFilterStatus.SelectedItem?.ToString() ?? "All";
                _selectedBillingCardIndex = status switch
                {
                    "Paid" => 2,
                    "Unpaid" => 3,
                    "All" => 0,
                    _ => -1
                };
                UpdateBillingCardHighlight(_selectedBillingCardIndex);
                await FilterInvoicesAsync();
            };

            // Apply responsive layout to fill available width and height
            UpdateBillingLayout();

            // Initial load
            _ = ReloadBillingDataAsync();
        }

        private void SelectBillingFilter(int cardIndex)
        {
            // Toggle back to All (card 0) if clicking the active card again
            if (_selectedBillingCardIndex == cardIndex && cardIndex != 0)
            {
                cardIndex = 0;
            }

            _selectedBillingCardIndex = cardIndex;
            UpdateBillingCardHighlight(cardIndex);

            // Synchronize status ComboBox
            if (_cmbFilterStatus != null)
            {
                switch (cardIndex)
                {
                    case 0:
                    case 1:
                        _cmbFilterStatus.SelectedItem = "All";
                        break;
                    case 2:
                        _cmbFilterStatus.SelectedItem = "Paid";
                        break;
                    case 3:
                        _cmbFilterStatus.SelectedItem = "Unpaid";
                        break;
                    case 4:
                        _cmbFilterStatus.SelectedItem = "All";
                        break;
                }
            }

            _ = FilterInvoicesAsync();
        }

        private void UpdateBillingCardHighlight(int selectedIndex)
        {
            for (int i = 0; i < _billingMetricCards.Count; i++)
            {
                var card = _billingMetricCards[i];
                if (i == selectedIndex)
                {
                    card.BorderWidth = 2;
                    card.CustomBorderColor = i switch
                    {
                        2 => Color.FromArgb(16, 185, 129),   // Vibrant Green for Paid
                        3 => Color.FromArgb(245, 158, 11),   // Vibrant Amber for Unpaid
                        4 => Color.FromArgb(239, 68, 68),    // Vibrant Red/Coral for Outstanding
                        _ => ThemeManager.AccentBlue         // Vibrant Blue for Total Sales / Invoices
                    };
                }
                else
                {
                    card.BorderWidth = 1;
                    card.CustomBorderColor = Color.Empty;
                }
                card.Invalidate();
            }
        }

        private void HandleBillingCardMouseWheel(MouseEventArgs e)
        {
            if (_billingMetricCards.Count == 0) return;
            int current = _selectedBillingCardIndex < 0 ? 0 : _selectedBillingCardIndex;
            int next = (e.Delta < 0)
                ? (current + 1) % _billingMetricCards.Count
                : (current - 1 + _billingMetricCards.Count) % _billingMetricCards.Count;
            SelectBillingFilter(next);
        }

        private async Task ReloadBillingDataAsync()
        {
            try
            {
                var metrics = await _billingService.GetSalesDashboardMetricsAsync();

                if (_lblValTotalSales != null) _lblValTotalSales.Text = $"₹{metrics.TotalSales:N2}";
                if (_lblValTotalInvoices != null) _lblValTotalInvoices.Text = $"{metrics.TotalInvoices}";
                if (_lblValPaidInvoices != null) _lblValPaidInvoices.Text = $"{metrics.PaidInvoices} Paid";
                if (_lblValUnpaidInvoices != null) _lblValUnpaidInvoices.Text = $"{metrics.UnpaidInvoices + metrics.OverdueInvoices} Due";
                if (_lblValOutstanding != null) _lblValOutstanding.Text = $"₹{metrics.OutstandingBalance:N2}";

                if (_lblTrendTotalSales != null) _lblTrendTotalSales.Text = $"{metrics.TotalInvoices} active invoices";
                if (_lblTrendTotalInvoices != null) _lblTrendTotalInvoices.Text = $"{metrics.PaidInvoices} paid • {metrics.UnpaidInvoices} due";
                if (_lblTrendPaidInvoices != null) _lblTrendPaidInvoices.Text = $"₹{metrics.TotalCollected:N2} collected";
                if (_lblTrendUnpaidInvoices != null) _lblTrendUnpaidInvoices.Text = $"{metrics.OverdueInvoices} overdue";
                if (_lblTrendOutstanding != null) _lblTrendOutstanding.Text = "Total receivables due";

                if (_lblBillingCountBadge != null) _lblBillingCountBadge.Text = $"{metrics.TotalInvoices} Invoices";
                if (_lblBillingPaidBadge != null) _lblBillingPaidBadge.Text = $"● {metrics.PaidInvoices} Paid";
                if (_lblBillingDueBadge != null) _lblBillingDueBadge.Text = $"● {metrics.UnpaidInvoices + metrics.OverdueInvoices} Due";
                LayoutBillingTopBadges();

                // Attach rich interactive MetricDetailInfo popups to all 5 cards
                if (_billingMetricCards.Count == 5)
                {
                    _billingMetricCards[0].MetricInfo = new MetricDetailInfo
                    {
                        Title = "Sales & Invoiced Revenue",
                        Icon = "💰",
                        CurrentValue = $"₹{metrics.TotalSales:N2}",
                        PreviousValue = "Across all billing records",
                        GrowthText = $"{metrics.TotalInvoices} Invoices Generated",
                        IsPositiveGrowth = true,
                        Category = "Revenue Analysis",
                        LastUpdated = DateTime.Now,
                        Stats = new List<(string Label, string Value)>
                        {
                            ("Total Invoices", $"{metrics.TotalInvoices} records"),
                            ("Total Collected", $"₹{metrics.TotalCollected:N2}"),
                            ("Outstanding Balance", $"₹{metrics.OutstandingBalance:N2}"),
                            ("Avg Invoice Size", metrics.TotalInvoices > 0 ? $"₹{(metrics.TotalSales / metrics.TotalInvoices):N2}" : "₹0.00")
                        },
                        FooterTip = "Click card to display All Invoices"
                    };

                    _billingMetricCards[1].MetricInfo = new MetricDetailInfo
                    {
                        Title = "Invoice Register Portfolio",
                        Icon = "📑",
                        CurrentValue = $"{metrics.TotalInvoices} Invoices",
                        PreviousValue = $"{metrics.PaidInvoices} Settled",
                        GrowthText = $"{metrics.PaidInvoices} fully settled",
                        IsPositiveGrowth = true,
                        Category = "Billing Operations",
                        LastUpdated = DateTime.Now,
                        Stats = new List<(string Label, string Value)>
                        {
                            ("Paid / Cleared", $"{metrics.PaidInvoices} invoices"),
                            ("Unpaid / Due", $"{metrics.UnpaidInvoices} invoices"),
                            ("Partially Paid", $"{metrics.PartiallyPaidInvoices} invoices"),
                            ("Overdue Invoices", $"{metrics.OverdueInvoices} invoices")
                        },
                        FooterTip = "Click card to display All Invoices"
                    };

                    _billingMetricCards[2].MetricInfo = new MetricDetailInfo
                    {
                        Title = "Cleared & Settled Invoices",
                        Icon = "✅",
                        CurrentValue = $"{metrics.PaidInvoices} Settled",
                        PreviousValue = "Cleared records",
                        GrowthText = $"₹{metrics.TotalCollected:N2} collected",
                        IsPositiveGrowth = true,
                        Category = "Cash Collection",
                        LastUpdated = DateTime.Now,
                        Stats = new List<(string Label, string Value)>
                        {
                            ("Settled Count", $"{metrics.PaidInvoices} invoices"),
                            ("Settled Amount", $"₹{metrics.TotalCollected:N2}"),
                            ("Collection Rate", metrics.TotalSales > 0 ? $"{(metrics.TotalCollected / metrics.TotalSales * 100):N1}%" : "100%"),
                            ("Settlement Status", "100% Cleared (₹0 Due)")
                        },
                        FooterTip = "Click card to filter table by Paid Invoices"
                    };

                    _billingMetricCards[3].MetricInfo = new MetricDetailInfo
                    {
                        Title = "Pending Settlements & Due",
                        Icon = "⏳",
                        CurrentValue = $"{metrics.UnpaidInvoices + metrics.OverdueInvoices} Due",
                        PreviousValue = $"{metrics.UnpaidInvoices} Pending",
                        GrowthText = $"{metrics.OverdueInvoices} Overdue",
                        IsPositiveGrowth = false,
                        Category = "Accounts Receivable",
                        LastUpdated = DateTime.Now,
                        Stats = new List<(string Label, string Value)>
                        {
                            ("Pending Count", $"{metrics.UnpaidInvoices} invoices"),
                            ("Partially Paid", $"{metrics.PartiallyPaidInvoices} invoices"),
                            ("Overdue Count", $"{metrics.OverdueInvoices} invoices"),
                            ("Total Pending", $"{metrics.UnpaidInvoices + metrics.PartiallyPaidInvoices + metrics.OverdueInvoices} invoices")
                        },
                        FooterTip = "Click card to filter table by Unpaid Invoices"
                    };

                    _billingMetricCards[4].MetricInfo = new MetricDetailInfo
                    {
                        Title = "Total Outstanding Receivables",
                        Icon = "⚠️",
                        CurrentValue = $"₹{metrics.OutstandingBalance:N2}",
                        PreviousValue = "Awaiting payment",
                        GrowthText = $"{metrics.OverdueInvoices} Overdue invoices",
                        IsPositiveGrowth = false,
                        Category = "Credit & Risk Management",
                        LastUpdated = DateTime.Now,
                        Stats = new List<(string Label, string Value)>
                        {
                            ("Total Due", $"₹{metrics.OutstandingBalance:N2}"),
                            ("Overdue Records", $"{metrics.OverdueInvoices} records"),
                            ("Total Portfolio Invoices", $"{metrics.TotalInvoices} records"),
                            ("Recovery Action", "Send Payment Reminders")
                        },
                        FooterTip = "Click card to show all invoices with Balance Due > 0"
                    };
                }

                UpdateBillingCardHighlight(_selectedBillingCardIndex);
                await FilterInvoicesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load billing metrics: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task FilterInvoicesAsync()
        {
            if (_gridInvoices == null) return;

            string search = _txtSearchInvoice?.Text?.Trim() ?? "";
            string status = _cmbFilterStatus?.SelectedItem?.ToString() ?? "All";

            bool isOutstandingFilter = (_selectedBillingCardIndex == 4);
            string? queryStatus = isOutstandingFilter ? null : (status == "All" ? null : status);

            List<Invoice> invoices;
            if (queryStatus == "Unpaid")
            {
                var allInvoices = await _billingService.GetInvoicesAsync(
                    search: string.IsNullOrWhiteSpace(search) ? null : search
                );
                invoices = allInvoices.Where(inv => inv.Status.Equals("Unpaid", StringComparison.OrdinalIgnoreCase) || inv.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                invoices = await _billingService.GetInvoicesAsync(
                    search: string.IsNullOrWhiteSpace(search) ? null : search,
                    status: queryStatus
                );
            }

            if (isOutstandingFilter)
            {
                invoices = invoices.Where(inv => inv.BalanceDue > 0).ToList();
            }

            if (_lblBillingTableTitle != null)
            {
                string filterTitle = _selectedBillingCardIndex switch
                {
                    2 => "Paid Records",
                    3 => "Unpaid Records",
                    4 => "Records with Outstanding Balance",
                    _ => (!string.IsNullOrEmpty(status) && status != "All") ? $"{status} Records" : "All Sales Records"
                };
                _lblBillingTableTitle.Text = $"Sales Register — {filterTitle} ({invoices.Count})";
            }

            _currentBillingInvoices = invoices;
            ApplyBillingInvoicesAndSorting();
        }

        private void ApplyBillingInvoicesAndSorting()
        {
            if (_gridInvoices == null) return;

            IEnumerable<Invoice> sorted = _billingSortColumn switch
            {
                "InvoiceNo" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.InvoiceNumber) : _currentBillingInvoices.OrderByDescending(i => i.InvoiceNumber),
                "Customer" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.CustomerName) : _currentBillingInvoices.OrderByDescending(i => i.CustomerName),
                "InvoiceDate" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.InvoiceDate) : _currentBillingInvoices.OrderByDescending(i => i.InvoiceDate),
                "DueDate" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.DueDate) : _currentBillingInvoices.OrderByDescending(i => i.DueDate),
                "Total" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.GrandTotal) : _currentBillingInvoices.OrderByDescending(i => i.GrandTotal),
                "Paid" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.AmountPaid) : _currentBillingInvoices.OrderByDescending(i => i.AmountPaid),
                "Balance" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.BalanceDue) : _currentBillingInvoices.OrderByDescending(i => i.BalanceDue),
                "Status" => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.Status) : _currentBillingInvoices.OrderByDescending(i => i.Status),
                _ => _billingSortAscending ? _currentBillingInvoices.OrderBy(i => i.InvoiceDate) : _currentBillingInvoices.OrderByDescending(i => i.InvoiceDate)
            };

            _gridInvoices.Rows.Clear();
            foreach (var inv in sorted)
            {
                string clientDisplay = string.IsNullOrWhiteSpace(inv.CompanyName) 
                    ? inv.CustomerName 
                    : $"{inv.CustomerName} ({inv.CompanyName})";

                _gridInvoices.Rows.Add(
                    inv.InvoiceId,
                    inv.InvoiceNumber,
                    clientDisplay,
                    inv.InvoiceDate.ToString("dd MMM yyyy"),
                    inv.DueDate.ToString("dd MMM yyyy"),
                    $"₹{inv.GrandTotal:N2}",
                    $"₹{inv.AmountPaid:N2}",
                    $"₹{inv.BalanceDue:N2}",
                    inv.Status,
                    ""
                );
            }

            if (_billingScrollBar != null)
            {
                _billingScrollBar.SyncWithGrid();
            }
        }
        #endregion

        // =====================================================
        // PAGE: PRODUCTS (Add / Edit / Delete / Search)
        // =====================================================
        #region Products Page
        private void LoadProductsPage()
        {
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int cardHeight = 110;
            int gap = 10;
            int totalGaps = gap * 3;
            int baseWidth = (contentWidth - totalGaps) / 4;
            int remainder = (contentWidth - totalGaps) % 4;
            int currentX = 0;

            // 1. TOP METRICS ROW: 4 Live Interactive KPI Cards (Bootstrap 5 Card Deck)
            int w0 = baseWidth + (0 < remainder ? 1 : 0);
            var cardTotal = CreateMetricCard("Total Products", "0 Items", "Catalog items registered", true, "📦", currentX, 0, w0, cardHeight, out _lblValTotalProducts!, out _lblTrendTotalProducts!);
            currentX += w0 + gap;
            _productMetricCards.Add(cardTotal);
            _contentPanel.Controls.Add(cardTotal);

            int w1 = baseWidth + (1 < remainder ? 1 : 0);
            var cardActive = CreateMetricCard("Active Products", "0 Active", "In stock & available", true, "✅", currentX, 0, w1, cardHeight, out _lblValActiveProducts!, out _lblTrendActiveProducts!);
            currentX += w1 + gap;
            _productMetricCards.Add(cardActive);
            _contentPanel.Controls.Add(cardActive);

            int w2 = baseWidth + (2 < remainder ? 1 : 0);
            var cardLowStock = CreateMetricCard("Low Stock Alert", "0 Items", "Under 50 units remaining", false, "⚠️", currentX, 0, w2, cardHeight, out _lblValLowStock!, out _lblTrendLowStock!);
            currentX += w2 + gap;
            _productMetricCards.Add(cardLowStock);
            _contentPanel.Controls.Add(cardLowStock);

            int w3 = baseWidth + (3 < remainder ? 1 : 0);
            var cardValue = CreateMetricCard("Total Inventory Value", "₹0.00", "Total stock valuation", true, "💎", currentX, 0, w3, cardHeight, out _lblValTotalStockValue!, out _lblTrendTotalStockValue!);
            _productMetricCards.Add(cardValue);
            _contentPanel.Controls.Add(cardValue);

            // Card click filter actions
            cardTotal.OnCardClicked = (c) => SelectProductFilter(0);
            cardActive.OnCardClicked = (c) => SelectProductFilter(1);
            cardLowStock.OnCardClicked = (c) => SelectProductFilter(2);
            cardValue.OnCardClicked = (c) => SelectProductFilter(3);

            // 2. QUICK ACTIONS & FILTER TOOLBAR (Bootstrap Toolbar Card)
            int toolbarY = cardHeight + 14;
            _productsToolbarCard = new RoundedPanel
            {
                Location = new Point(0, toolbarY),
                Size = new Size(contentWidth, 68),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _themeCards.Add(_productsToolbarCard);
            _contentPanel.Controls.Add(_productsToolbarCard);

            // Modern Rounded Search Box with icon and clear button
            _txtSearchProduct = new RoundedSearchBox
            {
                PlaceholderText = "Search by product name, category, or ID...",
                Font = new Font("Segoe UI", 9f),
                Size = new Size(270, 34),
                Location = new Point(16, 17),
                BorderRadius = 8
            };
            _txtSearchProduct.ApplyTheme(ThemeManager.CurrentTheme);
            _productsToolbarCard.Controls.Add(_txtSearchProduct);

            // Category Filter Dropdown
            _lblProductCategoryFilter = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(300, 25),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            _lblProductCategoryFilter.Click += (s, e) => { if (_cmbProductCategory != null) _cmbProductCategory.SelectedIndex = 0; };
            _productsToolbarCard.Controls.Add(_lblProductCategoryFilter);
            _themeSubLabels.Add(_lblProductCategoryFilter);

            _cmbProductCategory = new RoundedComboBox
            {
                Size = new Size(160, 34),
                Location = new Point(365, 17),
                BorderRadius = 8
            };
            _cmbProductCategory.Items.Add("All Categories");
            _cmbProductCategory.SelectedIndex = 0;
            _cmbProductCategory.ApplyTheme(ThemeManager.CurrentTheme);
            _productsToolbarCard.Controls.Add(_cmbProductCategory);

            // Status Filter Dropdown
            _lblProductStatusFilter = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(540, 25),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            _lblProductStatusFilter.Click += (s, e) => { if (_cmbProductStatus != null) _cmbProductStatus.SelectedIndex = 0; };
            _productsToolbarCard.Controls.Add(_lblProductStatusFilter);
            _themeSubLabels.Add(_lblProductStatusFilter);

            _cmbProductStatus = new RoundedComboBox
            {
                Size = new Size(140, 34),
                Location = new Point(590, 17),
                BorderRadius = 8
            };
            _cmbProductStatus.Items.AddRange(new string[] { "All", "Active", "Inactive", "Low Stock (< 50)" });
            _cmbProductStatus.SelectedIndex = 0;
            _cmbProductStatus.ApplyTheme(ThemeManager.CurrentTheme);
            _productsToolbarCard.Controls.Add(_cmbProductStatus);

            // + Add Product (Bootstrap 5 Green Button)
            _btnProductAdd = new ModernButton
            {
                Text = "+ Add Product",
                NormalColor = Color.FromArgb(16, 185, 129),
                HoverColor = Color.FromArgb(5, 150, 105),
                PressedColor = Color.FromArgb(4, 120, 87),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(120, 34),
                Location = new Point(contentWidth - 380, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _productsToolbarCard.Controls.Add(_btnProductAdd);
            _btnProductAdd.Click += (s, e) => ShowProductDialog(null);

            // 📷 Scan Barcode Button (Modern Indigo Button)
            _btnProductScanBarcode = new ModernButton
            {
                Text = "📷 Scan Barcode",
                NormalColor = Color.FromArgb(99, 102, 241),
                HoverColor = Color.FromArgb(79, 70, 229),
                PressedColor = Color.FromArgb(67, 56, 202),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(135, 34),
                Location = new Point(contentWidth - 245, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _productsToolbarCard.Controls.Add(_btnProductScanBarcode);
            _btnProductScanBarcode.Click += (s, e) => ShowProductBarcodeScanner();

            // Refresh Button (Bootstrap 5 Blue Button)
            _btnProductRefresh = new ModernButton
            {
                Text = "↻ Refresh",
                NormalColor = Color.FromArgb(37, 99, 235),
                HoverColor = Color.FromArgb(29, 78, 216),
                PressedColor = Color.FromArgb(30, 64, 175),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(95, 34),
                Location = new Point(contentWidth - 100, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _productsToolbarCard.Controls.Add(_btnProductRefresh);
            _btnProductRefresh.Click += async (s, e) => await LoadProductData();

            // 3. TABLE CARD & CONTAINER
            int tableY = toolbarY + 68 + 14;
            int tableHeight = Math.Max(420, _contentPanel.ClientSize.Height - tableY - 20);

            _productsTableCard = new RoundedPanel
            {
                Location = new Point(0, tableY),
                Size = new Size(contentWidth, tableHeight),
                BorderRadius = 14,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _themeCards.Add(_productsTableCard);
            _contentPanel.Controls.Add(_productsTableCard);

            // Card Header: Title & Hint
            _lblProductTableTitle = new Label
            {
                Text = "Product Inventory Register",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(16, 14),
                AutoSize = true
            };
            _productsTableCard.Controls.Add(_lblProductTableTitle);
            _themeTitleLabels.Add(_lblProductTableTitle);

            _lblProductHint = new Label
            {
                Text = "💡 Click headers to sort | Quick action buttons for Edit / Delete",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(contentWidth - 420, 16),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _productsTableCard.Controls.Add(_lblProductHint);
            _themeSubLabels.Add(_lblProductHint);

            // Rounded Table Wrapper for crisp border clipping
            _productsGridWrapper = new RoundedPanel
            {
                Location = new Point(16, 44),
                Size = new Size(contentWidth - 32, tableHeight - 60),
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _themeCards.Add(_productsGridWrapper);
            _productsTableCard.Controls.Add(_productsGridWrapper);

            // Bootstrap 5 Styled DataGridView
            _gridProducts = CreateStyledGrid();
            _gridProducts.Location = new Point(1, 1);
            _gridProducts.Size = new Size(_productsGridWrapper.Width - 11, _productsGridWrapper.Height - 2);
            _gridProducts.RowTemplate.Height = 40;
            _gridProducts.ColumnHeadersHeight = 38;
            _gridProducts.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridProducts.GridColor = Color.FromArgb(226, 232, 240);
            _gridProducts.BorderStyle = BorderStyle.None;
            _gridProducts.EnableHeadersVisualStyles = false;
            _gridProducts.ScrollBars = ScrollBars.None;

            _gridProducts.Columns.Add("ID", "ID");
            _gridProducts.Columns.Add("Name", "Product Name");
            _gridProducts.Columns.Add("Category", "Category");
            _gridProducts.Columns.Add("Price", "Price (₹)");
            _gridProducts.Columns.Add("Stock", "Stock Quantity");
            _gridProducts.Columns.Add("Status", "Status");
            _gridProducts.Columns.Add("Actions", "Actions");

            _gridProducts.Columns[0].Width = 70;
            _gridProducts.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridProducts.Columns[1].MinimumWidth = 220;
            _gridProducts.Columns[2].Width = 160;
            _gridProducts.Columns[3].Width = 120;
            _gridProducts.Columns[4].Width = 140;
            _gridProducts.Columns[5].Width = 120;
            _gridProducts.Columns[6].Width = 150;

            // Sorting by Column Header Click
            _gridProducts.ColumnHeaderMouseClick += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.ColumnIndex >= _gridProducts.Columns.Count) return;
                string colName = _gridProducts.Columns[e.ColumnIndex].Name;
                if (colName == "Actions") return;

                if (_productSortColumn == colName)
                {
                    _productSortAscending = !_productSortAscending;
                }
                else
                {
                    _productSortColumn = colName;
                    _productSortAscending = true;
                }
                ApplyProductSorting();
                _gridProducts.Invalidate();
            };

            // Smooth Bootstrap Row Hover Highlighting
            _gridProducts.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && _hoveredProductRowIndex != e.RowIndex)
                {
                    int old = _hoveredProductRowIndex;
                    _hoveredProductRowIndex = e.RowIndex;
                    if (old >= 0 && old < _gridProducts.Rows.Count) _gridProducts.InvalidateRow(old);
                    _gridProducts.InvalidateRow(e.RowIndex);
                }
            };

            _gridProducts.CellMouseLeave += (s, e) =>
            {
                if (_hoveredProductRowIndex >= 0 && _hoveredProductRowIndex < _gridProducts.Rows.Count)
                {
                    int old = _hoveredProductRowIndex;
                    _hoveredProductRowIndex = -1;
                    _gridProducts.InvalidateRow(old);
                }
                _gridProducts.Cursor = Cursors.Default;
            };

            // Custom Cell Painting: Bootstrap 5 Table Styling
            _gridProducts.CellPainting += (s, e) =>
            {
                if (e.Graphics == null) return;
                bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

                // 1. Column Headers
                if (e.RowIndex == -1)
                {
                    bool isSortedCol = (e.ColumnIndex >= 0 && e.ColumnIndex < _gridProducts.Columns.Count && _gridProducts.Columns[e.ColumnIndex].Name == _productSortColumn);

                    Color headerBg = isSortedCol 
                        ? Color.FromArgb(2, 132, 199) 
                        : (isDark ? Color.FromArgb(24, 27, 40) : Color.FromArgb(248, 250, 252));
                    Color headerTextColor = isSortedCol 
                        ? Color.White 
                        : (isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105));

                    using (var b = new SolidBrush(headerBg))
                    {
                        e.Graphics.FillRectangle(b, e.CellBounds);
                    }

                    Color divColor = isSortedCol
                        ? Color.FromArgb(3, 105, 161)
                        : (isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240));
                    using (var divPen = new Pen(divColor, 1f))
                    {
                        e.Graphics.DrawLine(divPen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(divPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }

                    string headerText = (e.ColumnIndex >= 0 && e.ColumnIndex < _gridProducts.Columns.Count) ? _gridProducts.Columns[e.ColumnIndex].HeaderText : "";
                    using var fHeader = new Font("Segoe UI", 9f, FontStyle.Bold);
                    using var bHeader = new SolidBrush(headerTextColor);
                    using var sf = new StringFormat { LineAlignment = StringAlignment.Center };

                    if (e.ColumnIndex == 0 || e.ColumnIndex == 4 || e.ColumnIndex == 5 || e.ColumnIndex == 6)
                    {
                        sf.Alignment = StringAlignment.Center;
                        e.Graphics.DrawString(headerText, fHeader, bHeader, e.CellBounds, sf);
                    }
                    else if (e.ColumnIndex == 3)
                    {
                        sf.Alignment = StringAlignment.Far;
                        var textRect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(headerText, fHeader, bHeader, textRect, sf);
                    }
                    else
                    {
                        sf.Alignment = StringAlignment.Near;
                        var textRect = new Rectangle(e.CellBounds.Left + 14, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(headerText, fHeader, bHeader, textRect, sf);
                    }

                    // Sort arrow on sorted column
                    if (isSortedCol)
                    {
                        using var sortBrush = new SolidBrush(Color.White);
                        PointF[] pts = _productSortAscending
                            ? new PointF[] { new(e.CellBounds.Right - 18, e.CellBounds.Y + 22), new(e.CellBounds.Right - 13, e.CellBounds.Y + 15), new(e.CellBounds.Right - 8, e.CellBounds.Y + 22) }
                            : new PointF[] { new(e.CellBounds.Right - 18, e.CellBounds.Y + 15), new(e.CellBounds.Right - 13, e.CellBounds.Y + 22), new(e.CellBounds.Right - 8, e.CellBounds.Y + 15) };
                        e.Graphics.FillPolygon(sortBrush, pts);
                    }

                    e.Handled = true;
                    return;
                }

                // 2. Data Rows
                if (e.RowIndex >= 0)
                {
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                    bool isHovered = (e.RowIndex == _hoveredProductRowIndex);

                    Color rowBg;
                    if (isDark)
                    {
                        rowBg = isSelected
                            ? Color.FromArgb(42, 52, 85)
                            : (isHovered
                                ? Color.FromArgb(32, 38, 60)
                                : (e.RowIndex % 2 == 0 ? Color.FromArgb(20, 22, 33) : Color.FromArgb(24, 27, 40)));
                    }
                    else
                    {
                        rowBg = isSelected
                            ? Color.FromArgb(232, 238, 255)
                            : (isHovered
                                ? Color.FromArgb(243, 246, 255)
                                : (e.RowIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 252)));
                    }

                    using (var brush = new SolidBrush(rowBg))
                    {
                        e.Graphics.FillRectangle(brush, e.CellBounds);
                    }

                    // Neon Left Accent Bar on Hover/Selected (Column 0)
                    if (e.ColumnIndex == 0 && (isHovered || isSelected))
                    {
                        using var activeBar = new SolidBrush(Color.FromArgb(14, 165, 233));
                        e.Graphics.FillRectangle(activeBar, e.CellBounds.Left, e.CellBounds.Top, 3.5f, e.CellBounds.Height);
                    }

                    // 1px dividers
                    Color lineBorder = isDark ? Color.FromArgb(36, 40, 60) : Color.FromArgb(226, 232, 240);
                    using (var linePen = new Pen(lineBorder, 1f))
                    {
                        e.Graphics.DrawLine(linePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(linePen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                    }

                    string cellVal = e.Value?.ToString() ?? "";

                    // Column 0: Centered Modern ID Tag
                    if (e.ColumnIndex == 0)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int idW = 46;
                        int idH = 22;
                        var idRect = new Rectangle(e.CellBounds.Left + (e.CellBounds.Width - idW) / 2, e.CellBounds.Top + (e.CellBounds.Height - idH) / 2, idW, idH);
                        using (var path = CreateRoundedRectanglePath(idRect, 6))
                        {
                            Color idBg = isDark ? Color.FromArgb(32, 38, 58) : Color.FromArgb(241, 245, 249);
                            Color idBorder = isDark ? Color.FromArgb(48, 56, 82) : Color.FromArgb(226, 232, 240);
                            using var bId = new SolidBrush(idBg);
                            using var pId = new Pen(idBorder, 1f);
                            e.Graphics.FillPath(bId, path);
                            e.Graphics.DrawPath(pId, path);
                        }
                        Color idTextCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
                        using var fId = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bIdText = new SolidBrush(idTextCol);
                        using var sfId = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString($"#{cellVal}", fId, bIdText, idRect, sfId);
                        e.Handled = true;
                        return;
                    }

                    // Column 1: Colorful Initial Avatar + Product Name
                    if (e.ColumnIndex == 1)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int avatarSize = 26;
                        int avatarX = e.CellBounds.Left + 12;
                        int avatarY = e.CellBounds.Top + (e.CellBounds.Height - avatarSize) / 2;
                        var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

                        int hash = Math.Abs(cellVal.GetHashCode());
                        Color gradStart = (hash % 5) switch
                        {
                            0 => Color.FromArgb(99, 102, 241),
                            1 => Color.FromArgb(14, 165, 233),
                            2 => Color.FromArgb(16, 185, 129),
                            3 => Color.FromArgb(245, 158, 11),
                            _ => Color.FromArgb(236, 72, 153)
                        };
                        Color gradEnd = Color.FromArgb(Math.Min(255, gradStart.R + 30), Math.Min(255, gradStart.G + 30), Math.Min(255, gradStart.B + 30));

                        using (var avBrush = new LinearGradientBrush(avatarRect, gradStart, gradEnd, 45f))
                        {
                            e.Graphics.FillEllipse(avBrush, avatarRect);
                        }

                        string initial = string.IsNullOrEmpty(cellVal) ? "P" : cellVal.Substring(0, 1).ToUpperInvariant();
                        using (var fInit = new Font("Segoe UI", 9f, FontStyle.Bold))
                        using (var bInit = new SolidBrush(Color.White))
                        using (var sfInit = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        {
                            e.Graphics.DrawString(initial, fInit, bInit, avatarRect, sfInit);
                        }

                        Color nameCol = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
                        using var fName = new Font("Segoe UI", 9f, FontStyle.Bold);
                        using var bName = new SolidBrush(nameCol);
                        using var sfName = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                        var nameRect = new Rectangle(avatarRect.Right + 10, e.CellBounds.Top, e.CellBounds.Right - avatarRect.Right - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(cellVal, fName, bName, nameRect, sfName);

                        e.Handled = true;
                        return;
                    }

                    // Column 2: Bootstrap Soft Pastel Category Pill
                    if (e.ColumnIndex == 2)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Color catBg, catBorder, catTextCol;
                        if (isDark)
                        {
                            (catBg, catBorder, catTextCol) = cellVal switch
                            {
                                "AI & ML" => (Color.FromArgb(40, 36, 75), Color.FromArgb(79, 70, 229), Color.FromArgb(165, 180, 252)),
                                "Cloud Services" => (Color.FromArgb(20, 45, 70), Color.FromArgb(14, 165, 233), Color.FromArgb(125, 211, 252)),
                                "Developer Tools" => (Color.FromArgb(55, 42, 20), Color.FromArgb(217, 119, 6), Color.FromArgb(253, 230, 138)),
                                "Software" => (Color.FromArgb(20, 50, 38), Color.FromArgb(16, 185, 129), Color.FromArgb(110, 231, 183)),
                                "Hardware & IoT" => (Color.FromArgb(55, 35, 20), Color.FromArgb(234, 88, 12), Color.FromArgb(254, 215, 170)),
                                "Security" => (Color.FromArgb(55, 20, 30), Color.FromArgb(225, 29, 72), Color.FromArgb(254, 205, 211)),
                                _ => (Color.FromArgb(35, 39, 56), Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225))
                            };
                        }
                        else
                        {
                            (catBg, catBorder, catTextCol) = cellVal switch
                            {
                                "AI & ML" => (Color.FromArgb(238, 242, 255), Color.FromArgb(199, 210, 254), Color.FromArgb(67, 56, 202)),
                                "Cloud Services" => (Color.FromArgb(224, 242, 254), Color.FromArgb(186, 230, 253), Color.FromArgb(3, 105, 161)),
                                "Developer Tools" => (Color.FromArgb(254, 243, 199), Color.FromArgb(253, 230, 138), Color.FromArgb(180, 83, 9)),
                                "Software" => (Color.FromArgb(209, 250, 229), Color.FromArgb(167, 243, 208), Color.FromArgb(4, 120, 87)),
                                "Hardware & IoT" => (Color.FromArgb(255, 237, 213), Color.FromArgb(254, 215, 170), Color.FromArgb(194, 65, 12)),
                                "Security" => (Color.FromArgb(255, 228, 230), Color.FromArgb(254, 205, 211), Color.FromArgb(190, 18, 60)),
                                _ => (Color.FromArgb(241, 245, 249), Color.FromArgb(226, 232, 240), Color.FromArgb(71, 85, 105))
                            };
                        }

                        int catW = Math.Min(e.CellBounds.Width - 20, 135);
                        int catH = 24;
                        int catX = e.CellBounds.Left + 10;
                        int catY = e.CellBounds.Top + (e.CellBounds.Height - catH) / 2;
                        var catRect = new Rectangle(catX, catY, catW, catH);
                        using (var path = CreateRoundedRectanglePath(catRect, 12))
                        {
                            using var bCat = new SolidBrush(catBg);
                            using var pCat = new Pen(catBorder, 1f);
                            e.Graphics.FillPath(bCat, path);
                            e.Graphics.DrawPath(pCat, path);
                        }
                        using var fCat = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bCatText = new SolidBrush(catTextCol);
                        using var sfCat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                        e.Graphics.DrawString(cellVal, fCat, bCatText, catRect, sfCat);
                        e.Handled = true;
                        return;
                    }

                    // Column 3: Tabular Right-Aligned Price in Indian Rupees (₹)
                    if (e.ColumnIndex == 3)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        string cleanVal = cellVal.TrimStart('$', '₹', ' ');
                        string priceDisplay = decimal.TryParse(cleanVal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal pVal)
                            ? $"₹{pVal:N2}"
                            : (cellVal.StartsWith("₹") ? cellVal : $"₹{cellVal}");
                        Color priceCol = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
                        using var fPrice = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        using var bPrice = new SolidBrush(priceCol);
                        using var sfPrice = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                        var rect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(priceDisplay, fPrice, bPrice, rect, sfPrice);
                        e.Handled = true;
                        return;
                    }

                    // Column 4: Stock Quantity with Bootstrap Badge & Indicator Dot (No unicode emojis to prevent tofu squares)
                    if (e.ColumnIndex == 4)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int stockVal = int.TryParse(cellVal, out int sv) ? sv : 0;
                        bool isLow = stockVal < 50;
                        bool isMid = stockVal >= 50 && stockVal <= 100;

                        Color stockBg, stockBorder, stockTextCol, dotCol;
                        string stockBadgeText;
                        if (isLow)
                        {
                            stockBg = isDark ? Color.FromArgb(59, 20, 30) : Color.FromArgb(254, 226, 226);
                            stockBorder = isDark ? Color.FromArgb(159, 18, 57) : Color.FromArgb(252, 165, 165);
                            stockTextCol = isDark ? Color.FromArgb(251, 113, 133) : Color.FromArgb(220, 38, 38);
                            dotCol = isDark ? Color.FromArgb(251, 113, 133) : Color.FromArgb(220, 38, 38);
                            stockBadgeText = $"Low: {stockVal}";
                        }
                        else if (isMid)
                        {
                            stockBg = isDark ? Color.FromArgb(56, 42, 18) : Color.FromArgb(254, 243, 199);
                            stockBorder = isDark ? Color.FromArgb(146, 64, 14) : Color.FromArgb(253, 230, 138);
                            stockTextCol = isDark ? Color.FromArgb(251, 191, 36) : Color.FromArgb(180, 83, 9);
                            dotCol = isDark ? Color.FromArgb(251, 191, 36) : Color.FromArgb(217, 119, 6);
                            stockBadgeText = $"{stockVal} units";
                        }
                        else
                        {
                            stockBg = isDark ? Color.FromArgb(20, 56, 40) : Color.FromArgb(220, 252, 231);
                            stockBorder = isDark ? Color.FromArgb(22, 101, 52) : Color.FromArgb(187, 247, 208);
                            stockTextCol = isDark ? Color.FromArgb(74, 222, 128) : Color.FromArgb(22, 163, 74);
                            dotCol = isDark ? Color.FromArgb(74, 222, 128) : Color.FromArgb(22, 163, 74);
                            stockBadgeText = $"{stockVal} in stock";
                        }

                        int stkW = Math.Min(e.CellBounds.Width - 20, 120);
                        int stkH = 24;
                        int stkX = e.CellBounds.Left + (e.CellBounds.Width - stkW) / 2;
                        int stkY = e.CellBounds.Top + (e.CellBounds.Height - stkH) / 2;
                        var stkRect = new Rectangle(stkX, stkY, stkW, stkH);
                        using (var path = CreateRoundedRectanglePath(stkRect, 12))
                        {
                            using var bStk = new SolidBrush(stockBg);
                            using var pStk = new Pen(stockBorder, 1f);
                            e.Graphics.FillPath(bStk, path);
                            e.Graphics.DrawPath(pStk, path);
                        }

                        // Crisp GDI+ circle indicator dot
                        int dotSize = 6;
                        int dotX = stkX + 10;
                        int dotY = stkY + (stkH - dotSize) / 2;
                        using (var bDot = new SolidBrush(dotCol))
                        {
                            e.Graphics.FillEllipse(bDot, dotX, dotY, dotSize, dotSize);
                        }

                        var textRect = new Rectangle(dotX + dotSize + 6, stkY, stkW - (dotSize + 16), stkH);
                        using var fStk = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bStkText = new SolidBrush(stockTextCol);
                        using var sfStk = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(stockBadgeText, fStk, bStkText, textRect, sfStk);
                        e.Handled = true;
                        return;
                    }

                    // Column 5: Status — Bootstrap 5 .badge rounded-pill with Glowing Dot
                    if (e.ColumnIndex == 5)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Color pillBg, pillBorder, dotCol, textCol;
                        if (isDark)
                        {
                            (pillBg, pillBorder, dotCol, textCol) = cellVal == "Active"
                                 ? (Color.FromArgb(20, 56, 40), Color.FromArgb(22, 101, 52), Color.FromArgb(74, 222, 128), Color.FromArgb(74, 222, 128))
                                 : (Color.FromArgb(59, 28, 29), Color.FromArgb(153, 27, 27), Color.FromArgb(248, 113, 113), Color.FromArgb(248, 113, 113));
                        }
                        else
                        {
                            (pillBg, pillBorder, dotCol, textCol) = cellVal == "Active"
                                ? (Color.FromArgb(220, 252, 231), Color.FromArgb(187, 247, 208), Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61))
                                : (Color.FromArgb(254, 226, 226), Color.FromArgb(254, 202, 202), Color.FromArgb(220, 38, 38), Color.FromArgb(185, 28, 28));
                        }

                        int pillW = 86;
                        int pillH = 24;
                        int pillX = e.CellBounds.Left + (e.CellBounds.Width - pillW) / 2;
                        int pillY = e.CellBounds.Top + (e.CellBounds.Height - pillH) / 2;
                        var pillRect = new Rectangle(pillX, pillY, pillW, pillH);

                        using (var path = CreateRoundedRectanglePath(pillRect, 12))
                        {
                            using var bPill = new SolidBrush(pillBg);
                            using var pPill = new Pen(pillBorder, 1f);
                            e.Graphics.FillPath(bPill, path);
                            e.Graphics.DrawPath(pPill, path);
                        }

                        int dotSize = 6;
                        int dotX = pillX + 10;
                        int dotY = pillY + (pillH - dotSize) / 2;
                        using (var bDot = new SolidBrush(dotCol))
                        {
                            e.Graphics.FillEllipse(bDot, dotX, dotY, dotSize, dotSize);
                        }

                        var textRect = new Rectangle(dotX + dotSize + 6, pillY, pillW - (dotSize + 16), pillH);
                        using var fBadge = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bText = new SolidBrush(textCol);
                        using var sfPill = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(cellVal, fBadge, bText, textRect, sfPill);
                        e.Handled = true;
                        return;
                    }

                    // Column 6: Actions — 2 Dedicated Bootstrap-Styled Action Buttons (Edit & Delete without tofu emojis)
                    if (e.ColumnIndex == 6)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int btnW = 58;
                        int btnH = 26;
                        int gapBtn = 6;
                        int startX = e.CellBounds.Left + (e.CellBounds.Width - (btnW * 2 + gapBtn)) / 2;
                        int btnY = e.CellBounds.Top + (e.CellBounds.Height - btnH) / 2;

                        var rectEdit = new Rectangle(startX, btnY, btnW, btnH);
                        var rectDel = new Rectangle(startX + btnW + gapBtn, btnY, btnW, btnH);

                        // Edit Button (Bootstrap Info/Primary subtle pill)
                        Color editBg = isDark ? Color.FromArgb(30, 48, 75) : Color.FromArgb(224, 238, 255);
                        Color editBorder = isDark ? Color.FromArgb(45, 90, 150) : Color.FromArgb(180, 215, 255);
                        Color editTextCol = isDark ? Color.FromArgb(96, 165, 250) : Color.FromArgb(29, 78, 216);

                        using (var pEdit = CreateRoundedRectanglePath(rectEdit, 6))
                        using (var bEdit = new SolidBrush(editBg))
                        using (var penEdit = new Pen(editBorder, 1f))
                        {
                            e.Graphics.FillPath(bEdit, pEdit);
                            e.Graphics.DrawPath(penEdit, pEdit);
                        }
                        using var fEditBtn = new Font("Segoe UI", 8f, FontStyle.Bold);
                        using var bEditBtn = new SolidBrush(editTextCol);
                        using var sfEditBtn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("Edit", fEditBtn, bEditBtn, rectEdit, sfEditBtn);

                        // Delete Button (Bootstrap Danger subtle pill)
                        Color delBg = isDark ? Color.FromArgb(55, 25, 30) : Color.FromArgb(254, 226, 226);
                        Color delBorder = isDark ? Color.FromArgb(120, 35, 45) : Color.FromArgb(252, 165, 165);
                        Color delTextCol = isDark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(185, 28, 28);

                        using (var pDel = CreateRoundedRectanglePath(rectDel, 6))
                        using (var bDel = new SolidBrush(delBg))
                        using (var penDel = new Pen(delBorder, 1f))
                        {
                            e.Graphics.FillPath(bDel, pDel);
                            e.Graphics.DrawPath(penDel, pDel);
                        }
                        using var fDelBtn = new Font("Segoe UI", 8f, FontStyle.Bold);
                        using var bDelBtn = new SolidBrush(delTextCol);
                        using var sfDelBtn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("Del", fDelBtn, bDelBtn, rectDel, sfDelBtn);

                        e.Handled = true;
                        return;
                    }
                }
            };

            // Cell click on Column 6 for Edit / Delete actions
            _gridProducts.CellMouseClick += async (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridProducts.Rows.Count) return;
                if (e.ColumnIndex == 6)
                {
                    var row = _gridProducts.Rows[e.RowIndex];
                    if (row.Cells[0].Value == null) return;
                    int id = Convert.ToInt32(row.Cells[0].Value);
                    var product = _allLoadedProducts.FirstOrDefault(p => p.ProductId == id);
                    if (product == null) return;

                    var cellBounds = _gridProducts.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    int btnW = 58;
                    int btnH = 26;
                    int gapBtn = 6;
                    int startX = (cellBounds.Width - (btnW * 2 + gapBtn)) / 2;
                    int editEndX = startX + btnW;
                    int delStartX = editEndX + gapBtn;
                    int delEndX = delStartX + btnW;

                    int btnY = (cellBounds.Height - btnH) / 2;
                    if (e.Y >= btnY && e.Y <= btnY + btnH)
                    {
                        if (e.X >= startX && e.X <= editEndX)
                        {
                            ShowProductDialog(product);
                        }
                        else if (e.X >= delStartX && e.X <= delEndX)
                        {
                            if (MessageBox.Show($"Are you sure you want to delete product \"{product.Name}\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                await _dbService.DeleteProductAsync(product.ProductId);
                                await LoadProductData();
                            }
                        }
                    }
                }
            };

            // Cursor styling for action buttons
            _gridProducts.CellMouseMove += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 6)
                {
                    _gridProducts.Cursor = Cursors.Hand;
                }
                else
                {
                    _gridProducts.Cursor = Cursors.Default;
                }
            };

            // Attach Custom DarkScrollBar
            _productsGridWrapper.Controls.Add(_gridProducts);

            _productScrollBar = new DarkScrollBar
            {
                Location = new Point(_productsGridWrapper.Width - 9, 1),
                Size = new Size(8, _productsGridWrapper.Height - 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Visible = false
            };
            _productScrollBar.AttachToGrid(_gridProducts);
            _productsGridWrapper.Controls.Add(_productScrollBar);
            _productScrollBar.BringToFront();

            // Wire Search and Filter events
            _txtSearchProduct.TextChanged += (s, e) => FilterProducts();

            _cmbProductCategory.SelectedIndexChanged += (s, e) => FilterProducts();

            _cmbProductStatus.SelectedIndexChanged += (s, e) =>
            {
                string status = _cmbProductStatus.SelectedItem?.ToString() ?? "All";
                _selectedProductCardIndex = status switch
                {
                    "Active" => 1,
                    "Low Stock (< 50)" => 2,
                    "All" => 0,
                    _ => -1
                };
                UpdateProductCardHighlight(_selectedProductCardIndex);
                FilterProducts();
            };

            // Double-click row to edit
            _gridProducts.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridProducts.Rows.Count) return;
                var row = _gridProducts.Rows[e.RowIndex];
                if (row.Cells[0].Value == null) return;
                int id = Convert.ToInt32(row.Cells[0].Value);
                var product = _allLoadedProducts.FirstOrDefault(p => p.ProductId == id);
                if (product != null) ShowProductDialog(product);
            };

            // Right-click context menu
            _gridProducts.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = _gridProducts.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        _gridProducts.ClearSelection();
                        _gridProducts.Rows[hit.RowIndex].Selected = true;

                        var row = _gridProducts.Rows[hit.RowIndex];
                        if (row.Cells[0].Value == null) return;
                        int id = Convert.ToInt32(row.Cells[0].Value);
                        var product = _allLoadedProducts.FirstOrDefault(p => p.ProductId == id);
                        if (product == null) return;

                        var ctx = new ContextMenuStrip();
                        ctx.Items.Add("✏️ Edit Product", null, (_, _) => ShowProductDialog(product));
                        ctx.Items.Add("🗑️ Delete Product", null, async (_, _) =>
                        {
                            if (MessageBox.Show($"Are you sure you want to delete \"{product.Name}\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                await _dbService.DeleteProductAsync(product.ProductId);
                                await LoadProductData();
                            }
                        });
                        ctx.Show(_gridProducts, new Point(e.X, e.Y));
                    }
                }
            };

            UpdateProductsLayout();

            // Load initial data
            _ = LoadProductData();
        }

        private async Task LoadProductData()
        {
            var products = await _dbService.GetProductsAsync();
            _allLoadedProducts = products;

            // Populate category dropdown
            if (_cmbProductCategory != null)
            {
                string curSelected = _cmbProductCategory.SelectedItem?.ToString() ?? "All Categories";
                var categories = products
                    .Select(p => p.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                _cmbProductCategory.Items.Clear();
                _cmbProductCategory.Items.Add("All Categories");
                foreach (var cat in categories)
                {
                    _cmbProductCategory.Items.Add(cat);
                }
                _cmbProductCategory.SelectedItem = categories.Contains(curSelected) ? curSelected : "All Categories";
            }

            // Update KPI cards
            UpdateProductKPIs(products);

            // Filter & populate grid
            FilterProducts();
        }

        private void UpdateProductKPIs(List<Product> products)
        {
            if (_lblValTotalProducts == null || _lblValActiveProducts == null || _lblValLowStock == null || _lblValTotalStockValue == null) return;

            int total = products.Count;
            int active = products.Count(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase));
            int lowStock = products.Count(p => p.StockQuantity < 50);
            decimal totalVal = products.Sum(p => p.Price * p.StockQuantity);

            _lblValTotalProducts.Text = $"{total} Items";
            _lblValActiveProducts.Text = $"{active} Active";
            _lblValLowStock.Text = $"{lowStock} Items";
            _lblValTotalStockValue.Text = $"₹{totalVal:N2}";

            if (_lblTrendTotalProducts != null) _lblTrendTotalProducts.Text = $"{active} active in catalog";
            if (_lblTrendActiveProducts != null) _lblTrendActiveProducts.Text = "Ready for customer orders";
            if (_lblTrendLowStock != null) _lblTrendLowStock.Text = lowStock > 0 ? "⚠️ Reorder recommended" : "✓ Stock healthy";
            if (_lblTrendTotalStockValue != null) _lblTrendTotalStockValue.Text = $"Avg ₹{(total > 0 ? totalVal / total : 0):N2} per item";
        }

        private void FilterProducts()
        {
            if (_allLoadedProducts == null) return;

            string search = _txtSearchProduct?.Text?.Trim().ToLowerInvariant() ?? "";
            string category = _cmbProductCategory?.SelectedItem?.ToString() ?? "All Categories";
            string status = _cmbProductStatus?.SelectedItem?.ToString() ?? "All";

            var filtered = _allLoadedProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(p =>
                    p.ProductId.ToString().Contains(search) ||
                    p.Name.ToLowerInvariant().Contains(search) ||
                    p.Category.ToLowerInvariant().Contains(search) ||
                    (!string.IsNullOrEmpty(p.Barcode) && p.Barcode.ToLowerInvariant().Contains(search)) ||
                    p.Price.ToString().Contains(search) ||
                    p.StockQuantity.ToString().Contains(search));
            }

            if (category != "All Categories" && !string.IsNullOrWhiteSpace(category))
            {
                filtered = filtered.Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
            }

            if (status == "Active" || status == "Inactive")
            {
                filtered = filtered.Where(p => string.Equals(p.Status, status, StringComparison.OrdinalIgnoreCase));
            }
            else if (status.Contains("Low Stock"))
            {
                filtered = filtered.Where(p => p.StockQuantity < 50);
            }

            _currentFilteredProducts = filtered.ToList();

            if (_lblProductTableTitle != null)
            {
                string suffix = (status != "All" && !string.IsNullOrEmpty(status)) ? $" — {status}" : "";
                if (category != "All Categories" && !string.IsNullOrEmpty(category)) suffix += $" ({category})";
                _lblProductTableTitle.Text = $"Product Inventory Register{suffix} ({_currentFilteredProducts.Count})";
            }

            ApplyProductSorting();
        }

        private void ApplyProductSorting()
        {
            if (_gridProducts == null) return;

            IEnumerable<Product> sorted = _productSortColumn switch
            {
                "ID" => _productSortAscending ? _currentFilteredProducts.OrderBy(p => p.ProductId) : _currentFilteredProducts.OrderByDescending(p => p.ProductId),
                "Name" => _productSortAscending ? _currentFilteredProducts.OrderBy(p => p.Name) : _currentFilteredProducts.OrderByDescending(p => p.Name),
                "Category" => _productSortAscending ? _currentFilteredProducts.OrderBy(p => p.Category) : _currentFilteredProducts.OrderByDescending(p => p.Category),
                "Price" => _productSortAscending ? _currentFilteredProducts.OrderBy(p => p.Price) : _currentFilteredProducts.OrderByDescending(p => p.Price),
                "Stock" => _productSortAscending ? _currentFilteredProducts.OrderBy(p => p.StockQuantity) : _currentFilteredProducts.OrderByDescending(p => p.StockQuantity),
                "Status" => _productSortAscending ? _currentFilteredProducts.OrderBy(p => p.Status) : _currentFilteredProducts.OrderByDescending(p => p.Status),
                _ => _productSortAscending ? _currentFilteredProducts.OrderBy(p => p.ProductId) : _currentFilteredProducts.OrderByDescending(p => p.ProductId)
            };

            _gridProducts.Rows.Clear();
            foreach (var p in sorted)
            {
                int rIdx = _gridProducts.Rows.Add(p.ProductId, p.Name, p.Category, p.Price.ToString("F2"), p.StockQuantity, p.Status, "");
                if (rIdx >= 0 && rIdx < _gridProducts.Rows.Count)
                {
                    _gridProducts.Rows[rIdx].Cells[1].ToolTipText = $"Barcode: {p.Barcode}\nCategory: {p.Category}\nStock: {p.StockQuantity} units";
                }
            }

            if (_productScrollBar != null)
            {
                _productScrollBar.SyncWithGrid();
            }
        }

        private void SelectProductFilter(int index)
        {
            _selectedProductCardIndex = index;
            UpdateProductCardHighlight(index);

            if (_cmbProductStatus != null)
            {
                switch (index)
                {
                    case 0: // Total
                    case 3: // Value
                        _cmbProductStatus.SelectedIndex = 0; // All
                        if (_cmbProductCategory != null) _cmbProductCategory.SelectedIndex = 0;
                        break;
                    case 1: // Active
                        _cmbProductStatus.SelectedIndex = 1; // Active
                        break;
                    case 2: // Low Stock
                        _cmbProductStatus.SelectedIndex = 3; // Low Stock (< 50)
                        break;
                }
            }
            FilterProducts();
        }

        private void UpdateProductCardHighlight(int selectedIndex)
        {
            for (int i = 0; i < _productMetricCards.Count; i++)
            {
                var card = _productMetricCards[i];
                if (card == null) continue;
                bool isSel = (i == selectedIndex);
                card.BorderWidth = isSel ? 2 : 1;
                card.CustomBorderColor = isSel ? (i switch
                {
                    1 => Color.FromArgb(16, 185, 129),
                    2 => Color.FromArgb(239, 68, 68),
                    3 => Color.FromArgb(14, 165, 233),
                    _ => ThemeManager.AccentBlue
                }) : ThemeManager.CardBorder;
                card.Invalidate();
            }
        }

        private void ShowProductDialog(Product? existing, string? initialBarcode = null)
        {
            bool isEdit = existing != null;
            var dlg = new Form
            {
                Text = isEdit ? "Edit Product — S TOM'S" : "Add Product — S TOM'S",
                Size = new Size(465, 485),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeManager.Background
            };

            int y = 16;
            var lblName = CreateDialogLabel("Product Name *:", y, dlg);
            var txtName = CreateDialogTextBox(existing?.Name ?? "", y + 20, dlg); y += 58;

            var lblBarcode = CreateDialogLabel("Barcode / EAN (Scan or Type):", y, dlg);
            string barVal = existing?.Barcode ?? initialBarcode ?? "";
            var txtBarcode = CreateDialogTextBox(barVal, y + 20, dlg);
            txtBarcode.Font = new Font("Consolas", 10f);
            y += 58;

            var lblCat = CreateDialogLabel("Category *:", y, dlg);
            var txtCat = CreateDialogTextBox(existing?.Category ?? "", y + 20, dlg); y += 58;

            var lblPrice = CreateDialogLabel("Price (₹) *:", y, dlg);
            var txtPrice = CreateDialogTextBox(existing?.Price.ToString("F2") ?? "0.00", y + 20, dlg); y += 58;

            var lblStock = CreateDialogLabel("Stock Quantity *:", y, dlg);
            var txtStock = CreateDialogTextBox(existing?.StockQuantity.ToString() ?? "0", y + 20, dlg); y += 58;

            var lblStatus = CreateDialogLabel("Status:", y, dlg);
            var cmbStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Active", "Inactive" },
                Font = new Font("Segoe UI", 10f),
                Location = new Point(25, y + 20),
                Size = new Size(395, 30),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary
            };
            cmbStatus.SelectedItem = existing?.Status ?? "Active";
            dlg.Controls.Add(cmbStatus); y += 62;

            var btnSave = new ModernButton
            {
                Text = isEdit ? "💾 Update" : "✅ Add Product",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(180, 38),
                Location = new Point(25, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BorderRadius = 10
            };
            dlg.Controls.Add(btnSave);

            var btnCancel = new ModernButton
            {
                Text = "Cancel",
                NormalColor = Color.FromArgb(80, 85, 100),
                HoverColor = Color.FromArgb(100, 105, 120),
                Size = new Size(120, 38),
                Location = new Point(220, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BorderRadius = 10
            };
            dlg.Controls.Add(btnCancel);
            btnCancel.Click += (s, e) => dlg.Close();

            btnSave.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Product name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    MessageBox.Show("Invalid price value.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtStock.Text, out int stock))
                {
                    MessageBox.Show("Invalid stock quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string barcodeVal = txtBarcode.Text.Trim();
                var product = new Product(
                    existing?.ProductId ?? 0,
                    txtName.Text.Trim(),
                    txtCat.Text.Trim(),
                    price,
                    stock,
                    cmbStatus.SelectedItem?.ToString() ?? "Active",
                    barcodeVal
                );

                if (isEdit)
                    await _dbService.UpdateProductAsync(product);
                else
                    await _dbService.AddProductAsync(product);

                dlg.Close();
                await LoadProductData();
            };

            dlg.ShowDialog(this);
        }

        private void ShowProductBarcodeScanner()
        {
            using var scannerDlg = new BarcodeScannerDialog(
                onProductScanned: (product) =>
                {
                    HighlightProductInGrid(product);
                },
                mode: BarcodeScannerMode.InventoryLookup,
                onRegisterNewRequested: (barcode) =>
                {
                    BeginInvoke(new Action(() => ShowProductDialog(null, initialBarcode: barcode)));
                },
                onEditProductRequested: (product) =>
                {
                    BeginInvoke(new Action(() => ShowProductDialog(product)));
                }
            );
            scannerDlg.ShowDialog(this);
        }

        private void HighlightProductInGrid(Product product)
        {
            if (_gridProducts == null) return;

            // Set search text to product name to filter to it
            if (_txtSearchProduct != null)
            {
                _txtSearchProduct.Text = product.Name;
            }
            FilterProducts();

            // Find matching row in grid, select it and scroll into view
            for (int i = 0; i < _gridProducts.Rows.Count; i++)
            {
                var row = _gridProducts.Rows[i];
                if (row.Cells["ID"].Value?.ToString() == product.ProductId.ToString())
                {
                    row.Selected = true;
                    _gridProducts.CurrentCell = row.Cells[1];
                    _gridProducts.FirstDisplayedScrollingRowIndex = i;
                    break;
                }
            }

            if (_lblProductHint != null)
            {
                _lblProductHint.Text = $"✓ Scanned: {product.Name} | Barcode: {product.Barcode} | Stock: {product.StockQuantity}";
                _lblProductHint.ForeColor = ThemeManager.AccentGreen;
            }
        }
        #endregion

        // =====================================================
        // PAGE: MESSAGES (Inbox with Read/Unread)
        // =====================================================
        #region Messages Page
        private void LoadMessagesPage()
        {
            // Title Bar
            _messagesHeaderCard = new RoundedPanel
            {
                Location = new Point(0, 0),
                Size = new Size(1060, 70),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_messagesHeaderCard);

            var lblTitle = new Label
            {
                Text = "✉️  Messages & Notifications",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 20),
                AutoSize = true
            };
            _messagesHeaderCard.Controls.Add(lblTitle);
            _themeTitleLabels.Add(lblTitle);

            _btnMessagesMarkAll = new ModernButton
            {
                Text = "✓ Mark All Read",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(150, 34),
                Location = new Point(880, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 10
            };
            _messagesHeaderCard.Controls.Add(_btnMessagesMarkAll);
            _contentPanel.Controls.Add(_messagesHeaderCard);

            // Message list container
            _messagesListCard = new RoundedPanel
            {
                Location = new Point(0, 85),
                Size = new Size(1060, 550),
                BorderRadius = 14,
                BorderWidth = 1,
                AutoScroll = true
            };
            _themeCards.Add(_messagesListCard);
            _contentPanel.Controls.Add(_messagesListCard);

            // Load messages
            LoadMessageItems(_messagesListCard);

            UpdateMessagesLayout();

            _btnMessagesMarkAll.Click += async (s, e) =>
            {
                await _dbService.MarkAllMessagesReadAsync();
                _messagesListCard.Controls.Clear();
                LoadMessageItems(_messagesListCard);
                UpdateMessagesLayout();
                ApplyThemeColors();
            };
        }

        private async void LoadMessageItems(RoundedPanel container)
        {
            var messages = await _dbService.GetMessagesAsync();
            int y = 10;

            foreach (var msg in messages)
            {
                var msgPanel = new RoundedPanel
                {
                    Location = new Point(12, y),
                    Size = new Size(1010, 100),
                    BorderRadius = 12,
                    BorderWidth = 1,
                    CustomBackgroundColor = msg.IsRead ? ThemeManager.CardBg : (ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(30, 35, 55) : Color.FromArgb(230, 238, 255)),
                    CustomBorderColor = msg.IsRead ? ThemeManager.CardBorder : ThemeManager.AccentBlue,
                    Cursor = Cursors.Hand
                };
                _themeCards.Add(msgPanel);

                // Unread indicator dot
                if (!msg.IsRead)
                {
                    var dot = new Panel
                    {
                        Size = new Size(10, 10),
                        Location = new Point(15, 12),
                        BackColor = Color.Transparent
                    };
                    dot.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using var b = new SolidBrush(ThemeManager.AccentBlue);
                        e.Graphics.FillEllipse(b, 0, 0, 9, 9);
                    };
                    msgPanel.Controls.Add(dot);
                }

                var lblSender = new Label
                {
                    Text = msg.Sender,
                    Font = new Font("Segoe UI", 10f, msg.IsRead ? FontStyle.Regular : FontStyle.Bold),
                    ForeColor = ThemeManager.TextPrimary,
                    Location = new Point(32, 10),
                    AutoSize = true
                };
                msgPanel.Controls.Add(lblSender);
                _themeTitleLabels.Add(lblSender);

                var lblTime = new Label
                {
                    Text = msg.SentDate.ToString("MMM dd, HH:mm"),
                    Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                    ForeColor = ThemeManager.TextSecondary,
                    Location = new Point(900, 12),
                    AutoSize = true
                };
                msgPanel.Controls.Add(lblTime);
                _themeSubLabels.Add(lblTime);

                var lblSubject = new Label
                {
                    Text = msg.Subject,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = ThemeManager.TextPrimary,
                    Location = new Point(32, 34),
                    AutoSize = true
                };
                msgPanel.Controls.Add(lblSubject);
                _themeTitleLabels.Add(lblSubject);

                var lblContent = new Label
                {
                    Text = msg.Content.Length > 100 ? msg.Content[..100] + "..." : msg.Content,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = ThemeManager.TextSecondary,
                    Location = new Point(32, 60),
                    Size = new Size(850, 30),
                    AutoEllipsis = true
                };
                msgPanel.Controls.Add(lblContent);
                _themeSubLabels.Add(lblContent);

                // Click to mark read + show details
                int capturedId = msg.MessageId;
                string capturedContent = msg.Content;
                string capturedSubject = msg.Subject;
                string capturedSender = msg.Sender;
                msgPanel.Click += async (s, e) =>
                {
                    await _dbService.MarkMessageReadAsync(capturedId);
                    MessageBox.Show($"From: {capturedSender}\n\n{capturedContent}", capturedSubject, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload messages page
                    ClearContentPanel();
                    LoadMessagesPage();
                    ApplyThemeColors();
                };

                container.Controls.Add(msgPanel);
                y += 115;
            }

            if (messages.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "📭  No messages found.",
                    Font = new Font("Segoe UI", 12f, FontStyle.Regular),
                    ForeColor = ThemeManager.TextSecondary,
                    Location = new Point(30, 30),
                    AutoSize = true
                };
                container.Controls.Add(lblEmpty);
                _themeSubLabels.Add(lblEmpty);
            }
        }
        #endregion

        // =====================================================
        // PAGE: CUSTOMERS (Add / Edit / Delete / Search)
        // =====================================================
        #region Customers Page
        private void LoadCustomersPage()
        {
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int cardHeight = 110;
            int gap = 10;
            int totalGaps = gap * 3;
            int baseWidth = (contentWidth - totalGaps) / 4;
            int remainder = (contentWidth - totalGaps) % 4;
            int currentX = 0;

            // 1. TOP METRICS ROW: 4 Live Interactive KPI Cards (Bootstrap 5 Card Deck)
            int w0 = baseWidth + (0 < remainder ? 1 : 0);
            var cardTotal = CreateMetricCard("Total Customers", "0 Registered", "Enterprise accounts", true, "👥", currentX, 0, w0, cardHeight, out _lblValTotalCustomers!, out _lblTrendTotalCustomers!);
            currentX += w0 + gap;
            _customerMetricCards.Add(cardTotal);
            _contentPanel.Controls.Add(cardTotal);

            int w1 = baseWidth + (1 < remainder ? 1 : 0);
            var cardActive = CreateMetricCard("Active Accounts", "0 Active", "100% operational status", true, "⭐", currentX, 0, w1, cardHeight, out _lblValActiveCustomers!, out _lblTrendActiveCustomers!);
            currentX += w1 + gap;
            _customerMetricCards.Add(cardActive);
            _contentPanel.Controls.Add(cardActive);

            int w2 = baseWidth + (2 < remainder ? 1 : 0);
            var cardCities = CreateMetricCard("Cities Covered", "0 Cities", "Metropolitan coverage", true, "🏙️", currentX, 0, w2, cardHeight, out _lblValTotalCities!, out _lblTrendTotalCities!);
            currentX += w2 + gap;
            _customerMetricCards.Add(cardCities);
            _contentPanel.Controls.Add(cardCities);

            int w3 = baseWidth + (3 < remainder ? 1 : 0);
            var cardCountries = CreateMetricCard("Global Countries", "0 Countries", "International presence", true, "🌐", currentX, 0, w3, cardHeight, out _lblValTotalCountries!, out _lblTrendTotalCountries!);
            _customerMetricCards.Add(cardCountries);
            _contentPanel.Controls.Add(cardCountries);

            cardTotal.OnCardClicked = (c) => SelectCustomerFilter(0);
            cardActive.OnCardClicked = (c) => SelectCustomerFilter(1);
            cardCities.OnCardClicked = (c) => SelectCustomerFilter(2);
            cardCountries.OnCardClicked = (c) => SelectCustomerFilter(3);

            // 2. QUICK ACTIONS & FILTER TOOLBAR (Bootstrap Toolbar Card)
            int toolbarY = cardHeight + 14;
            _customersToolbarCard = new RoundedPanel
            {
                Location = new Point(0, toolbarY),
                Size = new Size(contentWidth, 68),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _themeCards.Add(_customersToolbarCard);
            _contentPanel.Controls.Add(_customersToolbarCard);

            // Modern Rounded Search Box with icon and clear button
            _txtSearchCustomer = new RoundedSearchBox
            {
                PlaceholderText = "Search by customer name, email, city, country, or ID...",
                Font = new Font("Segoe UI", 9f),
                Size = new Size(330, 34),
                Location = new Point(16, 17),
                BorderRadius = 8
            };
            _txtSearchCustomer.ApplyTheme(ThemeManager.CurrentTheme);
            _customersToolbarCard.Controls.Add(_txtSearchCustomer);

            // Country Filter Dropdown
            _lblCustomerCountryFilter = new Label
            {
                Text = "Country:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(365, 25),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            _lblCustomerCountryFilter.Click += (s, e) => { if (_cmbCustomerCountry != null) _cmbCustomerCountry.SelectedIndex = 0; };
            _customersToolbarCard.Controls.Add(_lblCustomerCountryFilter);
            _themeSubLabels.Add(_lblCustomerCountryFilter);

            _cmbCustomerCountry = new RoundedComboBox
            {
                Size = new Size(160, 34),
                Location = new Point(428, 17),
                BorderRadius = 8
            };
            _cmbCustomerCountry.Items.Add("All Countries");
            _cmbCustomerCountry.SelectedIndex = 0;
            _cmbCustomerCountry.ApplyTheme(ThemeManager.CurrentTheme);
            _customersToolbarCard.Controls.Add(_cmbCustomerCountry);

            // + Add Customer (Bootstrap 5 Green Button)
            _btnCustomerAdd = new ModernButton
            {
                Text = "+ Add Customer",
                NormalColor = Color.FromArgb(16, 185, 129),
                HoverColor = Color.FromArgb(5, 150, 105),
                PressedColor = Color.FromArgb(4, 120, 87),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(140, 34),
                Location = new Point(contentWidth - 250, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _customersToolbarCard.Controls.Add(_btnCustomerAdd);
            _btnCustomerAdd.Click += (s, e) => ShowCustomerDialog(null);

            // Refresh Button (Bootstrap 5 Blue Button)
            _btnCustomerRefresh = new ModernButton
            {
                Text = "↻ Refresh",
                NormalColor = Color.FromArgb(37, 99, 235),
                HoverColor = Color.FromArgb(29, 78, 216),
                PressedColor = Color.FromArgb(30, 64, 175),
                UseGradient = false,
                HasGlowOnHover = false,
                Size = new Size(95, 34),
                Location = new Point(contentWidth - 100, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            _customersToolbarCard.Controls.Add(_btnCustomerRefresh);
            _btnCustomerRefresh.Click += async (s, e) => await LoadCustomerData();

            // 3. TABLE CARD & CONTAINER
            int tableY = toolbarY + 68 + 14;
            int tableHeight = Math.Max(420, _contentPanel.ClientSize.Height - tableY - 20);

            _customersTableCard = new RoundedPanel
            {
                Location = new Point(0, tableY),
                Size = new Size(contentWidth, tableHeight),
                BorderRadius = 14,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _themeCards.Add(_customersTableCard);
            _contentPanel.Controls.Add(_customersTableCard);

            // Card Header: Title & Hint
            _lblCustomerTableTitle = new Label
            {
                Text = "👥  Customer Directory Register",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 14),
                AutoSize = true
            };
            _customersTableCard.Controls.Add(_lblCustomerTableTitle);
            _themeTitleLabels.Add(_lblCustomerTableTitle);

            _lblCustomerHint = new Label
            {
                Text = "Showing registered customers • Click column to sort • Edit/Del buttons active",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(Math.Max(280, contentWidth - 460), 16),
                AutoSize = true
            };
            _customersTableCard.Controls.Add(_lblCustomerHint);
            _themeSubLabels.Add(_lblCustomerHint);

            // Rounded Table Wrapper for crisp rounded corner border clipping
            _customersGridWrapper = new RoundedPanel
            {
                Location = new Point(16, 44),
                Size = new Size(contentWidth - 32, tableHeight - 60),
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            _themeCards.Add(_customersGridWrapper);
            _customersTableCard.Controls.Add(_customersGridWrapper);

            // Bootstrap 5 Styled DataGridView
            _gridCustomers = CreateStyledGrid();
            _gridCustomers.Location = new Point(1, 1);
            _gridCustomers.Size = new Size(_customersGridWrapper.Width - 11, _customersGridWrapper.Height - 2);
            _gridCustomers.RowTemplate.Height = 40;
            _gridCustomers.ColumnHeadersHeight = 38;
            _gridCustomers.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridCustomers.GridColor = Color.FromArgb(226, 232, 240);
            _gridCustomers.BorderStyle = BorderStyle.None;
            _gridCustomers.EnableHeadersVisualStyles = false;
            _gridCustomers.ScrollBars = ScrollBars.None;

            _gridCustomers.Columns.Add("ID", "ID");
            _gridCustomers.Columns.Add("Name", "Customer Name");
            _gridCustomers.Columns.Add("Email", "Email");
            _gridCustomers.Columns.Add("Phone", "Phone");
            _gridCustomers.Columns.Add("City", "City");
            _gridCustomers.Columns.Add("Country", "Country");
            _gridCustomers.Columns.Add("Actions", "Actions");

            _gridCustomers.Columns[0].Width = 70;
            _gridCustomers.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridCustomers.Columns[1].MinimumWidth = 200;
            _gridCustomers.Columns[2].Width = 230;
            _gridCustomers.Columns[3].Width = 150;
            _gridCustomers.Columns[4].Width = 140;
            _gridCustomers.Columns[5].Width = 140;
            _gridCustomers.Columns[6].Width = 150;

            // Sorting by Column Header Click
            _gridCustomers.ColumnHeaderMouseClick += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.ColumnIndex >= _gridCustomers.Columns.Count) return;
                string colName = _gridCustomers.Columns[e.ColumnIndex].Name;
                if (colName == "Actions") return;

                if (_customerSortColumn == colName)
                {
                    _customerSortAscending = !_customerSortAscending;
                }
                else
                {
                    _customerSortColumn = colName;
                    _customerSortAscending = true;
                }
                ApplyCustomerSorting();
                _gridCustomers.Invalidate();
            };

            // Smooth Bootstrap Row Hover Highlighting
            _gridCustomers.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && _hoveredCustomerRowIndex != e.RowIndex)
                {
                    int old = _hoveredCustomerRowIndex;
                    _hoveredCustomerRowIndex = e.RowIndex;
                    if (old >= 0 && old < _gridCustomers.Rows.Count) _gridCustomers.InvalidateRow(old);
                    _gridCustomers.InvalidateRow(e.RowIndex);
                }
            };

            _gridCustomers.CellMouseLeave += (s, e) =>
            {
                if (_hoveredCustomerRowIndex >= 0 && _hoveredCustomerRowIndex < _gridCustomers.Rows.Count)
                {
                    int old = _hoveredCustomerRowIndex;
                    _hoveredCustomerRowIndex = -1;
                    _gridCustomers.InvalidateRow(old);
                }
                _gridCustomers.Cursor = Cursors.Default;
            };

            // Custom Cell Painting: Bootstrap 5 Table Styling
            _gridCustomers.CellPainting += (s, e) =>
            {
                if (e.Graphics == null) return;
                bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

                // 1. Column Headers
                if (e.RowIndex == -1)
                {
                    bool isSortedCol = (e.ColumnIndex >= 0 && e.ColumnIndex < _gridCustomers.Columns.Count && _gridCustomers.Columns[e.ColumnIndex].Name == _customerSortColumn);

                    Color headerBg = isSortedCol
                        ? Color.FromArgb(2, 132, 199)
                        : (isDark ? Color.FromArgb(24, 27, 40) : Color.FromArgb(248, 250, 252));
                    Color headerTextColor = isSortedCol
                        ? Color.White
                        : (isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105));

                    using (var b = new SolidBrush(headerBg))
                    {
                        e.Graphics.FillRectangle(b, e.CellBounds);
                    }

                    Color divColor = isSortedCol
                        ? Color.FromArgb(3, 105, 161)
                        : (isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240));
                    using (var divPen = new Pen(divColor, 1f))
                    {
                        e.Graphics.DrawLine(divPen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(divPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }

                    string headerText = (e.ColumnIndex >= 0 && e.ColumnIndex < _gridCustomers.Columns.Count) ? _gridCustomers.Columns[e.ColumnIndex].HeaderText : "";
                    using var fHeader = new Font("Segoe UI", 9f, FontStyle.Bold);
                    using var bHeader = new SolidBrush(headerTextColor);
                    using var sf = new StringFormat { LineAlignment = StringAlignment.Center };

                    if (e.ColumnIndex == 0 || e.ColumnIndex == 4 || e.ColumnIndex == 5 || e.ColumnIndex == 6)
                    {
                        sf.Alignment = StringAlignment.Center;
                        e.Graphics.DrawString(headerText, fHeader, bHeader, e.CellBounds, sf);
                    }
                    else
                    {
                        sf.Alignment = StringAlignment.Near;
                        var textRect = new Rectangle(e.CellBounds.Left + 14, e.CellBounds.Top, e.CellBounds.Width - 14, e.CellBounds.Height);
                        e.Graphics.DrawString(headerText, fHeader, bHeader, textRect, sf);
                    }

                    // Sort arrow on sorted column
                    if (isSortedCol)
                    {
                        using var sortBrush = new SolidBrush(Color.White);
                        PointF[] pts = _customerSortAscending
                            ? new PointF[] { new(e.CellBounds.Right - 18, e.CellBounds.Y + 22), new(e.CellBounds.Right - 13, e.CellBounds.Y + 15), new(e.CellBounds.Right - 8, e.CellBounds.Y + 22) }
                            : new PointF[] { new(e.CellBounds.Right - 18, e.CellBounds.Y + 15), new(e.CellBounds.Right - 13, e.CellBounds.Y + 22), new(e.CellBounds.Right - 8, e.CellBounds.Y + 15) };
                        e.Graphics.FillPolygon(sortBrush, pts);
                    }

                    e.Handled = true;
                    return;
                }

                // 2. Data Rows
                if (e.RowIndex >= 0)
                {
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                    bool isHovered = (e.RowIndex == _hoveredCustomerRowIndex);

                    Color rowBg;
                    if (isDark)
                    {
                        rowBg = isSelected
                            ? Color.FromArgb(42, 52, 85)
                            : (isHovered
                                ? Color.FromArgb(32, 38, 60)
                                : (e.RowIndex % 2 == 0 ? Color.FromArgb(20, 22, 33) : Color.FromArgb(24, 27, 40)));
                    }
                    else
                    {
                        rowBg = isSelected
                            ? Color.FromArgb(232, 238, 255)
                            : (isHovered
                                ? Color.FromArgb(243, 246, 255)
                                : (e.RowIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 252)));
                    }

                    using (var brush = new SolidBrush(rowBg))
                    {
                        e.Graphics.FillRectangle(brush, e.CellBounds);
                    }

                    // Neon Left Accent Bar on Hover/Selected (Column 0)
                    if (e.ColumnIndex == 0 && (isHovered || isSelected))
                    {
                        using var activeBar = new SolidBrush(Color.FromArgb(14, 165, 233));
                        e.Graphics.FillRectangle(activeBar, e.CellBounds.Left, e.CellBounds.Top, 3.5f, e.CellBounds.Height);
                    }

                    // 1px dividers
                    Color lineBorder = isDark ? Color.FromArgb(36, 40, 60) : Color.FromArgb(226, 232, 240);
                    using (var linePen = new Pen(lineBorder, 1f))
                    {
                        e.Graphics.DrawLine(linePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(linePen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                    }

                    string cellVal = e.Value?.ToString() ?? "";

                    // Column 0: Centered Modern ID Tag
                    if (e.ColumnIndex == 0)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int idW = 46;
                        int idH = 22;
                        var idRect = new Rectangle(e.CellBounds.Left + (e.CellBounds.Width - idW) / 2, e.CellBounds.Top + (e.CellBounds.Height - idH) / 2, idW, idH);
                        using (var path = CreateRoundedRectanglePath(idRect, 6))
                        {
                            Color idBg = isDark ? Color.FromArgb(32, 38, 58) : Color.FromArgb(241, 245, 249);
                            Color idBorder = isDark ? Color.FromArgb(48, 56, 82) : Color.FromArgb(226, 232, 240);
                            using var bId = new SolidBrush(idBg);
                            using var pId = new Pen(idBorder, 1f);
                            e.Graphics.FillPath(bId, path);
                            e.Graphics.DrawPath(pId, path);
                        }
                        Color idTextCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
                        using var fId = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bIdText = new SolidBrush(idTextCol);
                        using var sfId = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString($"#{cellVal}", fId, bIdText, idRect, sfId);
                        e.Handled = true;
                        return;
                    }

                    // Column 1: Colorful Initial Avatar + Customer Name
                    if (e.ColumnIndex == 1)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int avatarSize = 26;
                        int avatarX = e.CellBounds.Left + 12;
                        int avatarY = e.CellBounds.Top + (e.CellBounds.Height - avatarSize) / 2;
                        var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

                        Color gradStart = GetAvatarStartColor(cellVal);
                        Color gradEnd = GetAvatarEndColor(cellVal);

                        using (var avBrush = new LinearGradientBrush(avatarRect, gradStart, gradEnd, 45f))
                        {
                            e.Graphics.FillEllipse(avBrush, avatarRect);
                        }

                        string initial = string.IsNullOrEmpty(cellVal) ? "C" : cellVal.Substring(0, 1).ToUpperInvariant();
                        using (var fInit = new Font("Segoe UI", 9f, FontStyle.Bold))
                        using (var bInit = new SolidBrush(Color.White))
                        using (var sfInit = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        {
                            e.Graphics.DrawString(initial, fInit, bInit, avatarRect, sfInit);
                        }

                        Color nameCol = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
                        var nameRect = new Rectangle(avatarX + avatarSize + 10, e.CellBounds.Top, e.CellBounds.Width - (avatarSize + 24), e.CellBounds.Height);
                        using var fName = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        using var bName = new SolidBrush(nameCol);
                        using var sfName = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                        e.Graphics.DrawString(cellVal, fName, bName, nameRect, sfName);

                        e.Handled = true;
                        return;
                    }

                    // Column 2: Email
                    if (e.ColumnIndex == 2)
                    {
                        Color emailCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
                        var textRect = new Rectangle(e.CellBounds.Left + 14, e.CellBounds.Top, e.CellBounds.Width - 18, e.CellBounds.Height);
                        using var fEmail = new Font("Segoe UI", 9f, FontStyle.Regular);
                        using var bEmail = new SolidBrush(emailCol);
                        using var sfEmail = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                        e.Graphics.DrawString(cellVal, fEmail, bEmail, textRect, sfEmail);
                        e.Handled = true;
                        return;
                    }

                    // Column 3: Phone
                    if (e.ColumnIndex == 3)
                    {
                        Color phoneCol = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);
                        var textRect = new Rectangle(e.CellBounds.Left + 14, e.CellBounds.Top, e.CellBounds.Width - 18, e.CellBounds.Height);
                        using var fPhone = new Font("Segoe UI", 9f, FontStyle.Regular);
                        using var bPhone = new SolidBrush(phoneCol);
                        using var sfPhone = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                        e.Graphics.DrawString(cellVal, fPhone, bPhone, textRect, sfPhone);
                        e.Handled = true;
                        return;
                    }

                    // Column 4: City (Bootstrap soft pill badge)
                    if (e.ColumnIndex == 4)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int pillH = 24;
                        int pillW = Math.Min(e.CellBounds.Width - 16, 120);
                        int pillX = e.CellBounds.Left + (e.CellBounds.Width - pillW) / 2;
                        int pillY = e.CellBounds.Top + (e.CellBounds.Height - pillH) / 2;
                        var pillRect = new Rectangle(pillX, pillY, pillW, pillH);

                        Color pillBg = isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
                        Color pillBorder = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);
                        Color textCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

                        using (var path = CreateRoundedRectanglePath(pillRect, 6))
                        using (var brush = new SolidBrush(pillBg))
                        using (var pen = new Pen(pillBorder, 1f))
                        {
                            e.Graphics.FillPath(brush, path);
                            e.Graphics.DrawPath(pen, path);
                        }

                        using var fCity = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                        using var bText = new SolidBrush(textCol);
                        using var sfCity = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(cellVal, fCity, bText, pillRect, sfCity);
                        e.Handled = true;
                        return;
                    }

                    // Column 5: Country (Bootstrap 5 .badge rounded-pill with Glowing Dot)
                    if (e.ColumnIndex == 5)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int pillH = 24;
                        int pillW = Math.Min(e.CellBounds.Width - 20, 110);
                        int pillX = e.CellBounds.Left + (e.CellBounds.Width - pillW) / 2;
                        int pillY = e.CellBounds.Top + (e.CellBounds.Height - pillH) / 2;
                        var pillRect = new Rectangle(pillX, pillY, pillW, pillH);

                        string cUpper = cellVal.Trim().ToUpperInvariant();
                        Color dotCol;
                        Color pillBg;
                        Color pillBorder;
                        Color textCol;

                        if (cUpper.Contains("USA") || cUpper.Contains("US"))
                        {
                            dotCol = Color.FromArgb(16, 185, 129); // Emerald
                            pillBg = isDark ? Color.FromArgb(6, 78, 59) : Color.FromArgb(209, 250, 229);
                            pillBorder = isDark ? Color.FromArgb(5, 150, 105) : Color.FromArgb(110, 231, 183);
                            textCol = isDark ? Color.FromArgb(52, 211, 153) : Color.FromArgb(6, 95, 70);
                        }
                        else if (cUpper.Contains("UK") || cUpper.Contains("BRITAIN") || cUpper.Contains("ENGLAND"))
                        {
                            dotCol = Color.FromArgb(14, 165, 233); // Sky Blue
                            pillBg = isDark ? Color.FromArgb(12, 74, 110) : Color.FromArgb(224, 242, 254);
                            pillBorder = isDark ? Color.FromArgb(2, 132, 199) : Color.FromArgb(125, 211, 252);
                            textCol = isDark ? Color.FromArgb(56, 189, 248) : Color.FromArgb(7, 89, 133);
                        }
                        else if (cUpper.Contains("GERMANY") || cUpper.Contains("DE"))
                        {
                            dotCol = Color.FromArgb(245, 158, 11); // Amber
                            pillBg = isDark ? Color.FromArgb(69, 26, 3) : Color.FromArgb(254, 243, 199);
                            pillBorder = isDark ? Color.FromArgb(217, 119, 6) : Color.FromArgb(252, 211, 77);
                            textCol = isDark ? Color.FromArgb(251, 191, 36) : Color.FromArgb(146, 64, 14);
                        }
                        else if (cUpper.Contains("CANADA") || cUpper.Contains("CA"))
                        {
                            dotCol = Color.FromArgb(244, 63, 94); // Rose
                            pillBg = isDark ? Color.FromArgb(76, 5, 25) : Color.FromArgb(255, 228, 230);
                            pillBorder = isDark ? Color.FromArgb(225, 29, 72) : Color.FromArgb(253, 164, 175);
                            textCol = isDark ? Color.FromArgb(251, 113, 133) : Color.FromArgb(159, 18, 57);
                        }
                        else
                        {
                            dotCol = Color.FromArgb(99, 102, 241); // Indigo
                            pillBg = isDark ? Color.FromArgb(30, 27, 75) : Color.FromArgb(224, 231, 255);
                            pillBorder = isDark ? Color.FromArgb(79, 70, 229) : Color.FromArgb(165, 180, 252);
                            textCol = isDark ? Color.FromArgb(129, 140, 248) : Color.FromArgb(55, 48, 163);
                        }

                        using (var path = CreateRoundedRectanglePath(pillRect, 10))
                        using (var brush = new SolidBrush(pillBg))
                        using (var pen = new Pen(pillBorder, 1f))
                        {
                            e.Graphics.FillPath(brush, path);
                            e.Graphics.DrawPath(pen, path);
                        }

                        int dotSize = 6;
                        int dotX = pillX + 10;
                        int dotY = pillY + (pillH - dotSize) / 2;
                        using (var bDot = new SolidBrush(dotCol))
                        {
                            e.Graphics.FillEllipse(bDot, dotX, dotY, dotSize, dotSize);
                        }

                        var textRect = new Rectangle(dotX + dotSize + 6, pillY, pillW - (dotSize + 16), pillH);
                        using var fBadge = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        using var bText = new SolidBrush(textCol);
                        using var sfPill = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(cellVal, fBadge, bText, textRect, sfPill);
                        e.Handled = true;
                        return;
                    }

                    // Column 6: Actions — 2 Dedicated Bootstrap-Styled Action Buttons (Edit & Delete)
                    if (e.ColumnIndex == 6)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int btnW = 58;
                        int btnH = 26;
                        int gapBtn = 6;
                        int startX = e.CellBounds.Left + (e.CellBounds.Width - (btnW * 2 + gapBtn)) / 2;
                        int btnY = e.CellBounds.Top + (e.CellBounds.Height - btnH) / 2;

                        var rectEdit = new Rectangle(startX, btnY, btnW, btnH);
                        var rectDel = new Rectangle(startX + btnW + gapBtn, btnY, btnW, btnH);

                        // Edit Button (Bootstrap Info/Primary subtle pill)
                        Color editBg = isDark ? Color.FromArgb(30, 48, 75) : Color.FromArgb(224, 238, 255);
                        Color editBorder = isDark ? Color.FromArgb(45, 90, 150) : Color.FromArgb(180, 215, 255);
                        Color editTextCol = isDark ? Color.FromArgb(96, 165, 250) : Color.FromArgb(29, 78, 216);

                        using (var pEdit = CreateRoundedRectanglePath(rectEdit, 6))
                        using (var bEdit = new SolidBrush(editBg))
                        using (var penEdit = new Pen(editBorder, 1f))
                        {
                            e.Graphics.FillPath(bEdit, pEdit);
                            e.Graphics.DrawPath(penEdit, pEdit);
                        }
                        using var fEditBtn = new Font("Segoe UI", 8f, FontStyle.Bold);
                        using var bEditBtn = new SolidBrush(editTextCol);
                        using var sfEditBtn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("Edit", fEditBtn, bEditBtn, rectEdit, sfEditBtn);

                        // Delete Button (Bootstrap Danger subtle pill)
                        Color delBg = isDark ? Color.FromArgb(55, 25, 30) : Color.FromArgb(254, 226, 226);
                        Color delBorder = isDark ? Color.FromArgb(120, 35, 45) : Color.FromArgb(252, 165, 165);
                        Color delTextCol = isDark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(185, 28, 28);

                        using (var pDel = CreateRoundedRectanglePath(rectDel, 6))
                        using (var bDel = new SolidBrush(delBg))
                        using (var penDel = new Pen(delBorder, 1f))
                        {
                            e.Graphics.FillPath(bDel, pDel);
                            e.Graphics.DrawPath(penDel, pDel);
                        }
                        using var fDelBtn = new Font("Segoe UI", 8f, FontStyle.Bold);
                        using var bDelBtn = new SolidBrush(delTextCol);
                        using var sfDelBtn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("Del", fDelBtn, bDelBtn, rectDel, sfDelBtn);

                        e.Handled = true;
                        return;
                    }
                }
            };

            // Cell click on Column 6 for Edit / Delete actions
            _gridCustomers.CellMouseClick += async (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridCustomers.Rows.Count) return;
                if (e.ColumnIndex == 6)
                {
                    var row = _gridCustomers.Rows[e.RowIndex];
                    if (row.Cells[0].Value == null) return;
                    int id = Convert.ToInt32(row.Cells[0].Value);
                    var customer = _allLoadedCustomers.FirstOrDefault(c => c.CustomerId == id);
                    if (customer == null) return;

                    var cellBounds = _gridCustomers.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    int btnW = 58;
                    int btnH = 26;
                    int gapBtn = 6;
                    int startX = (cellBounds.Width - (btnW * 2 + gapBtn)) / 2;
                    int editEndX = startX + btnW;
                    int delStartX = editEndX + gapBtn;
                    int delEndX = delStartX + btnW;

                    int btnY = (cellBounds.Height - btnH) / 2;
                    if (e.Y >= btnY && e.Y <= btnY + btnH)
                    {
                        if (e.X >= startX && e.X <= editEndX)
                        {
                            ShowCustomerDialog(customer);
                        }
                        else if (e.X >= delStartX && e.X <= delEndX)
                        {
                            if (MessageBox.Show($"Are you sure you want to delete customer \"{customer.Name}\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                await _dbService.DeleteCustomerAsync(customer.CustomerId);
                                await LoadCustomerData();
                            }
                        }
                    }
                }
            };

            // Cursor styling for action buttons
            _gridCustomers.CellMouseMove += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 6)
                {
                    _gridCustomers.Cursor = Cursors.Hand;
                }
                else
                {
                    _gridCustomers.Cursor = Cursors.Default;
                }
            };

            // Attach Custom DarkScrollBar
            _customersGridWrapper.Controls.Add(_gridCustomers);

            _customerScrollBar = new DarkScrollBar
            {
                Location = new Point(_customersGridWrapper.Width - 9, 1),
                Size = new Size(8, _customersGridWrapper.Height - 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Visible = false
            };
            _customerScrollBar.AttachToGrid(_gridCustomers);
            _customersGridWrapper.Controls.Add(_customerScrollBar);
            _customerScrollBar.BringToFront();

            // Wire Search and Filter events
            _txtSearchCustomer.TextChanged += (s, e) => FilterCustomers();

            _cmbCustomerCountry.SelectedIndexChanged += (s, e) => FilterCustomers();

            // Double-click row to edit
            _gridCustomers.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridCustomers.Rows.Count) return;
                var row = _gridCustomers.Rows[e.RowIndex];
                if (row.Cells[0].Value == null) return;
                int id = Convert.ToInt32(row.Cells[0].Value);
                var customer = _allLoadedCustomers.FirstOrDefault(c => c.CustomerId == id);
                if (customer != null) ShowCustomerDialog(customer);
            };

            // Right-click context menu
            _gridCustomers.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = _gridCustomers.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        _gridCustomers.ClearSelection();
                        _gridCustomers.Rows[hit.RowIndex].Selected = true;

                        var row = _gridCustomers.Rows[hit.RowIndex];
                        if (row.Cells[0].Value == null) return;
                        int id = Convert.ToInt32(row.Cells[0].Value);
                        var customer = _allLoadedCustomers.FirstOrDefault(c => c.CustomerId == id);
                        if (customer == null) return;

                        var ctx = new ContextMenuStrip();
                        ctx.Items.Add("✏️ Edit Customer", null, (_, _) => ShowCustomerDialog(customer));
                        ctx.Items.Add("📑 View Invoices & History", null, (_, _) =>
                        {
                            using var histDlg = new CustomerHistoryDialog(customer);
                            histDlg.ShowDialog(this);
                        });
                        ctx.Items.Add("🗑️ Delete Customer", null, async (_, _) =>
                        {
                            if (MessageBox.Show($"Are you sure you want to delete customer \"{customer.Name}\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                await _dbService.DeleteCustomerAsync(customer.CustomerId);
                                await LoadCustomerData();
                            }
                        });
                        ctx.Show(_gridCustomers, new Point(e.X, e.Y));
                    }
                }
            };

            UpdateCustomersLayout();

            // Load initial customer data
            _ = LoadCustomerData();
        }

        private async Task LoadCustomerData()
        {
            var customers = await _dbService.GetCustomersAsync();
            _allLoadedCustomers = customers;

            // Populate country dropdown
            if (_cmbCustomerCountry != null)
            {
                string curSelected = _cmbCustomerCountry.SelectedItem?.ToString() ?? "All Countries";
                var countries = customers
                    .Select(c => c.Country)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                _cmbCustomerCountry.Items.Clear();
                _cmbCustomerCountry.Items.Add("All Countries");
                foreach (var country in countries)
                {
                    _cmbCustomerCountry.Items.Add(country);
                }
                _cmbCustomerCountry.SelectedItem = countries.Contains(curSelected) ? curSelected : "All Countries";
            }

            // Update KPI cards
            UpdateCustomerKPIs(customers);

            // Filter & populate grid
            FilterCustomers();
        }

        private void UpdateCustomerKPIs(List<Customer> customers)
        {
            if (_lblValTotalCustomers == null || _lblValActiveCustomers == null || _lblValTotalCities == null || _lblValTotalCountries == null) return;

            int total = customers.Count;
            int active = total;
            int cities = customers.Select(c => c.City).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().Count();
            int countries = customers.Select(c => c.Country).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().Count();

            _lblValTotalCustomers.Text = $"{total} Registered";
            _lblValActiveCustomers.Text = $"{active} Active";
            _lblValTotalCities.Text = $"{cities} Cities";
            _lblValTotalCountries.Text = $"{countries} Countries";

            if (_lblTrendTotalCustomers != null) _lblTrendTotalCustomers.Text = "Enterprise client base";
            if (_lblTrendActiveCustomers != null) _lblTrendActiveCustomers.Text = "100% operational status";
            if (_lblTrendTotalCities != null) _lblTrendTotalCities.Text = "Metropolitan coverage";
            if (_lblTrendTotalCountries != null) _lblTrendTotalCountries.Text = "Global presence";
        }

        private void FilterCustomers()
        {
            if (_allLoadedCustomers == null) return;

            string search = _txtSearchCustomer?.Text?.Trim().ToLowerInvariant() ?? "";
            string country = _cmbCustomerCountry?.SelectedItem?.ToString() ?? "All Countries";

            var filtered = _allLoadedCustomers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(c =>
                    c.CustomerId.ToString().Contains(search) ||
                    c.Name.ToLowerInvariant().Contains(search) ||
                    c.Email.ToLowerInvariant().Contains(search) ||
                    c.Phone.ToLowerInvariant().Contains(search) ||
                    c.City.ToLowerInvariant().Contains(search) ||
                    c.Country.ToLowerInvariant().Contains(search));
            }

            if (country != "All Countries" && !string.IsNullOrWhiteSpace(country))
            {
                filtered = filtered.Where(c => string.Equals(c.Country, country, StringComparison.OrdinalIgnoreCase));
            }

            _currentFilteredCustomers = filtered.ToList();

            if (_lblCustomerTableTitle != null)
            {
                string suffix = (country != "All Countries" && !string.IsNullOrEmpty(country)) ? $" — {country}" : "";
                _lblCustomerTableTitle.Text = $"👥  Customer Directory Register{suffix} ({_currentFilteredCustomers.Count})";
            }

            ApplyCustomerSorting();
        }

        private void ApplyCustomerSorting()
        {
            if (_gridCustomers == null) return;

            IEnumerable<Customer> sorted = _customerSortColumn switch
            {
                "ID" => _customerSortAscending ? _currentFilteredCustomers.OrderBy(c => c.CustomerId) : _currentFilteredCustomers.OrderByDescending(c => c.CustomerId),
                "Name" => _customerSortAscending ? _currentFilteredCustomers.OrderBy(c => c.Name) : _currentFilteredCustomers.OrderByDescending(c => c.Name),
                "Email" => _customerSortAscending ? _currentFilteredCustomers.OrderBy(c => c.Email) : _currentFilteredCustomers.OrderByDescending(c => c.Email),
                "Phone" => _customerSortAscending ? _currentFilteredCustomers.OrderBy(c => c.Phone) : _currentFilteredCustomers.OrderByDescending(c => c.Phone),
                "City" => _customerSortAscending ? _currentFilteredCustomers.OrderBy(c => c.City) : _currentFilteredCustomers.OrderByDescending(c => c.City),
                "Country" => _customerSortAscending ? _currentFilteredCustomers.OrderBy(c => c.Country) : _currentFilteredCustomers.OrderByDescending(c => c.Country),
                _ => _customerSortAscending ? _currentFilteredCustomers.OrderBy(c => c.CustomerId) : _currentFilteredCustomers.OrderByDescending(c => c.CustomerId)
            };

            _gridCustomers.Rows.Clear();
            foreach (var c in sorted)
            {
                int rIdx = _gridCustomers.Rows.Add(c.CustomerId, c.Name, c.Email, c.Phone, c.City, c.Country, "");
                if (rIdx >= 0 && rIdx < _gridCustomers.Rows.Count)
                {
                    _gridCustomers.Rows[rIdx].Cells[1].ToolTipText = $"Customer ID: #{c.CustomerId}\nName: {c.Name}\nCity: {c.City}, {c.Country}";
                }
            }

            if (_customerScrollBar != null)
            {
                _customerScrollBar.SyncWithGrid();
            }
        }

        private void SelectCustomerFilter(int index)
        {
            _selectedCustomerCardIndex = index;
            UpdateCustomerCardHighlight(index);

            if (index == 0)
            {
                if (_cmbCustomerCountry != null) _cmbCustomerCountry.SelectedIndex = 0;
                if (_txtSearchCustomer != null) _txtSearchCustomer.Text = "";
            }
            FilterCustomers();
        }

        private void UpdateCustomerCardHighlight(int activeIndex)
        {
            for (int i = 0; i < _customerMetricCards.Count; i++)
            {
                var card = _customerMetricCards[i];
                if (card == null) continue;

                if (i == activeIndex)
                {
                    card.CustomBorderColor = ThemeManager.AccentBlue;
                    card.BorderWidth = 2;
                }
                else
                {
                    card.CustomBorderColor = ThemeManager.CardBorder;
                    card.BorderWidth = 1;
                }
                card.Invalidate();
            }
        }

        private void ShowCustomerDialog(Customer? existing)
        {
            bool isEdit = existing != null;
            var dlg = new Form
            {
                Text = isEdit ? "Edit Customer — S TOM'S" : "Add Customer — S TOM'S",
                Size = new Size(465, 490),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeManager.Background
            };

            int y = 16;
            var lblName = CreateDialogLabel("Customer Name *:", y, dlg);
            var txtName = CreateDialogTextBox(existing?.Name ?? "", y + 20, dlg); y += 58;

            var lblEmail = CreateDialogLabel("Email Address *:", y, dlg);
            var txtEmail = CreateDialogTextBox(existing?.Email ?? "", y + 20, dlg); y += 58;

            var lblPhone = CreateDialogLabel("Phone Number:", y, dlg);
            var txtPhone = CreateDialogTextBox(existing?.Phone ?? "", y + 20, dlg); y += 58;

            var lblCity = CreateDialogLabel("City:", y, dlg);
            var txtCity = CreateDialogTextBox(existing?.City ?? "", y + 20, dlg); y += 58;

            var lblCountry = CreateDialogLabel("Country:", y, dlg);
            var txtCountry = CreateDialogTextBox(existing?.Country ?? "USA", y + 20, dlg); y += 65;

            var btnSave = new ModernButton
            {
                Text = isEdit ? "💾 Update Customer" : "✅ Add Customer",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(180, 38),
                Location = new Point(25, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BorderRadius = 10,
                ForeColor = Color.White
            };
            dlg.Controls.Add(btnSave);

            var btnCancel = new ModernButton
            {
                Text = "Cancel",
                NormalColor = Color.FromArgb(80, 85, 100),
                HoverColor = Color.FromArgb(100, 105, 120),
                Size = new Size(120, 38),
                Location = new Point(220, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BorderRadius = 10,
                ForeColor = Color.White
            };
            dlg.Controls.Add(btnCancel);
            btnCancel.Click += (s, e) => dlg.Close();

            btnSave.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Customer name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Email address is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var customer = new Customer(
                    existing?.CustomerId ?? 0,
                    txtName.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtPhone.Text.Trim(),
                    existing?.Address ?? "",
                    txtCity.Text.Trim(),
                    txtCountry.Text.Trim()
                );

                if (isEdit)
                    await _dbService.UpdateCustomerAsync(customer);
                else
                    await _dbService.AddCustomerAsync(customer);

                dlg.Close();
                await LoadCustomerData();
            };

            dlg.ShowDialog(this);
        }
        #endregion

        // =====================================================
        // PAGE: SETTINGS
        // =====================================================
        #region Settings Page
        private void LoadSettingsPage()
        {
            // Theme Section
            _settingsThemeCard = new RoundedPanel
            {
                Location = new Point(0, 0),
                Size = new Size(1060, 150),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_settingsThemeCard);

            var lblThemeTitle = new Label
            {
                Text = "🎨  Appearance & Theme",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 18),
                AutoSize = true
            };
            _settingsThemeCard.Controls.Add(lblThemeTitle);
            _themeTitleLabels.Add(lblThemeTitle);

            var lblThemeDesc = new Label
            {
                Text = "Switch between Light and Dark mode for the entire dashboard.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(18, 52),
                AutoSize = true
            };
            _settingsThemeCard.Controls.Add(lblThemeDesc);
            _themeSubLabels.Add(lblThemeDesc);

            _lblSettingsCurrentTheme = new Label
            {
                Text = $"Current Theme:  {ThemeManager.CurrentTheme}",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                Location = new Point(18, 85),
                AutoSize = true
            };
            _settingsThemeCard.Controls.Add(_lblSettingsCurrentTheme);

            var themeToggleLarge = new ThemeToggle
            {
                Location = new Point(250, 82),
                Size = new Size(80, 36)
            };
            _settingsThemeCard.Controls.Add(themeToggleLarge);

            _contentPanel.Controls.Add(_settingsThemeCard);

            // 2. Company Details & Invoice PDF Branding Section
            _settingsCompanyCard = new RoundedPanel
            {
                Location = new Point(0, 165),
                Size = new Size(1060, 230),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_settingsCompanyCard);

            var lblCompTitle = new Label
            {
                Text = "🏢  Company Details & Invoice PDF Branding",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 18),
                AutoSize = true
            };
            _settingsCompanyCard.Controls.Add(lblCompTitle);
            _themeTitleLabels.Add(lblCompTitle);

            var lblCompDesc = new Label
            {
                Text = "Manage company name, address, GSTIN, and bank/payment instructions printed on all invoices & PDF exports.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(18, 50),
                AutoSize = true
            };
            _settingsCompanyCard.Controls.Add(lblCompDesc);
            _themeSubLabels.Add(lblCompDesc);

            var btnEditComp = new ModernButton
            {
                Text = "✏️ Edit Company Details",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(190, 36),
                Location = new Point(690, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                NormalColor = Color.FromArgb(79, 70, 229),
                HoverColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White
            };
            _settingsCompanyCard.Controls.Add(btnEditComp);

            var btnPreviewSample = new ModernButton
            {
                Text = "👁️ Preview Sample PDF",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(160, 36),
                Location = new Point(890, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                NormalColor = Color.FromArgb(16, 185, 129),
                HoverColor = Color.FromArgb(52, 211, 153),
                ForeColor = Color.White
            };
            _settingsCompanyCard.Controls.Add(btnPreviewSample);

            _settingsCompanyPreviewPanel = new RoundedPanel
            {
                Location = new Point(18, 80),
                Size = new Size(1024, 135),
                BorderRadius = 10,
                BorderWidth = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _settingsCompanyCard.Controls.Add(_settingsCompanyPreviewPanel);

            btnEditComp.Click += async (s, e) =>
            {
                using var designer = new InvoiceDesignerForm();
                if (designer.ShowDialog(this) == DialogResult.OK)
                {
                    await RefreshCompanySettingsPreviewAsync(_settingsCompanyPreviewPanel);
                }
            };

            btnPreviewSample.Click += async (s, e) =>
            {
                var profile = await _billingService.GetBusinessProfileAsync();
                var sampleInv = _currentBillingInvoices.FirstOrDefault() 
                    ?? (await _billingService.GetInvoicesAsync(limit: 1)).FirstOrDefault()
                    ?? new Invoice
                    {
                        InvoiceNumber = "SAMPLE-INV-001",
                        CustomerName = "Sample Customer Ltd",
                        InvoiceDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(15),
                        Status = "Paid"
                    };
                if (sampleInv.Items.Count == 0)
                {
                    sampleInv.Items.Add(new InvoiceItem("Software Subscription", "Cloud Enterprise License", 1m, 25000m, 0m, 18m));
                    sampleInv.RecalculateTotals();
                }
                string path = _pdfService.GenerateInvoicePdf(sampleInv, profile);
                _pdfService.OpenPdf(path);
            };

            _ = RefreshCompanySettingsPreviewAsync(_settingsCompanyPreviewPanel);

            _contentPanel.Controls.Add(_settingsCompanyCard);

            // Application Info Section
            _settingsInfoCard = new RoundedPanel
            {
                Location = new Point(0, 410),
                Size = new Size(1060, 200),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_settingsInfoCard);

            var lblInfoTitle = new Label
            {
                Text = "ℹ️  Application Information",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 18),
                AutoSize = true
            };
            _settingsInfoCard.Controls.Add(lblInfoTitle);
            _themeTitleLabels.Add(lblInfoTitle);

            string[][] infoItems =
            {
                new[] { "Application", "Business Analytics Dashboard — S TOM'S Platform" },
                new[] { "Version", ".NET 8.0 (Windows Forms)" },
                new[] { "Developer", "Hardik Jariwala — Full-Stack Developer" },
                new[] { "Database", _dbService.IsDemoMode ? "Demo Mode (In-Memory)" : "SQL Server (Connected)" },
                new[] { "Repository", "https://github.com/Saumya25-hub/TEAM-SOFT-SB1" }
            };

            int infoY = 55;
            foreach (var item in infoItems)
            {
                var lblKey = new Label
                {
                    Text = item[0] + ":",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = ThemeManager.TextSecondary,
                    Location = new Point(18, infoY),
                    AutoSize = true
                };
                _settingsInfoCard.Controls.Add(lblKey);
                _themeSubLabels.Add(lblKey);

                var lblVal = new Label
                {
                    Text = item[1],
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                    ForeColor = ThemeManager.TextPrimary,
                    Location = new Point(180, infoY),
                    AutoSize = true
                };
                _settingsInfoCard.Controls.Add(lblVal);
                _themeTitleLabels.Add(lblVal);

                infoY += 28;
            }

            _contentPanel.Controls.Add(_settingsInfoCard);

            // User Session Card
            _settingsSessionCard = new RoundedPanel
            {
                Location = new Point(0, 625),
                Size = new Size(1060, 130),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_settingsSessionCard);

            var lblSessionTitle = new Label
            {
                Text = "👤  Current Session",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 18),
                AutoSize = true
            };
            _settingsSessionCard.Controls.Add(lblSessionTitle);
            _themeTitleLabels.Add(lblSessionTitle);

            var lblUser = new Label
            {
                Text = $"Signed in as:  {_currentUser.FullName}  ({_currentUser.Role})",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 55),
                AutoSize = true
            };
            _settingsSessionCard.Controls.Add(lblUser);
            _themeTitleLabels.Add(lblUser);

            var lblStatus = new Label
            {
                Text = _dbService.LastStatusMessage,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(18, 85),
                AutoSize = true
            };
            _settingsSessionCard.Controls.Add(lblStatus);
            _themeSubLabels.Add(lblStatus);

            _contentPanel.Controls.Add(_settingsSessionCard);

            UpdateSettingsLayout();
        }
        #endregion

        // =====================================================
        // SHARED HELPER METHODS
        // =====================================================
        #region Helper Methods

        private void UpdateLayoutForCurrentPage()
        {
            if (_contentPanel == null || _contentPanel.ClientSize.Width <= 0) return;

            switch (_activeMenuIndex)
            {
                case 0: UpdateAnalyticsLayout(); break;
                case 1: UpdateBillingLayout(); break;
                case 2: UpdateProductsLayout(); break;
                case 3: UpdateMessagesLayout(); break;
                case 4: UpdateCustomersLayout(); break;
                case 5: UpdateSettingsLayout(); break;
            }
        }

        private void UpdateBillingLayout()
        {
            if (_billingTableCard == null || _contentPanel == null) return;

            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int contentHeight = _contentPanel.ClientSize.Height;

            // 1. Metric cards (5 cards) - snug, uniform 8px gap and perfectly balanced widths
            int gap = 8;
            if (_billingMetricCards.Count == 5)
            {
                int totalGaps = gap * 4;
                int baseWidth = (contentWidth - totalGaps) / 5;
                int remainder = (contentWidth - totalGaps) % 5;
                int currentX = 0;
                for (int i = 0; i < 5; i++)
                {
                    int w = baseWidth + (i < remainder ? 1 : 0);
                    _billingMetricCards[i].Location = new Point(currentX, 0);
                    _billingMetricCards[i].Size = new Size(w, 110);
                    currentX += w + gap;
                }
            }

            // 2. Dedicated Toolbar Card (Directly below summary cards with 16px gap)
            int toolbarY = 110 + 16;
            if (_billingToolbarCard != null)
            {
                _billingToolbarCard.Location = new Point(0, toolbarY);

                int btnWCreate = 130;
                int btnWView = 120;
                int btnWCust = 110;
                int btnWProf = 120;
                int btnWRef = 95;
                int btnGap = 8;
                int totalBtnWidth = btnWCreate + btnGap + btnWView + btnGap + btnWCust + btnGap + btnWProf + btnGap + btnWRef; // 607px

                // Check if screen is wide enough for single desktop row (>= 1060px)
                bool isSingleRow = contentWidth >= 1060;

                if (isSingleRow)
                {
                    int toolbarHeight = 68;
                    _billingToolbarCard.Size = new Size(contentWidth, toolbarHeight);
                    int ctrlY = (toolbarHeight - 34) / 2;

                    // Left Side: Search Box + Status Label + Dropdown
                    int leftX = 16;
                    int searchWidth = Math.Clamp(contentWidth - totalBtnWidth - 250, 220, 340);
                    if (_txtSearchInvoice != null)
                    {
                        _txtSearchInvoice.Location = new Point(leftX, ctrlY);
                        _txtSearchInvoice.Size = new Size(searchWidth, 34);
                    }
                    leftX += searchWidth + 12;

                    if (_lblStatusFilter != null)
                    {
                        int lblY = ctrlY + (34 - _lblStatusFilter.PreferredHeight) / 2;
                        _lblStatusFilter.Location = new Point(leftX, lblY);
                        _lblStatusFilter.Visible = true;
                        leftX += _lblStatusFilter.PreferredWidth + 8;
                    }

                    if (_cmbFilterStatus != null)
                    {
                        _cmbFilterStatus.Location = new Point(leftX, ctrlY);
                        _cmbFilterStatus.Size = new Size(130, 34);
                    }

                    // Right Side: 5 Action Buttons pinned to right edge
                    int buttonsStartX = contentWidth - totalBtnWidth - 16;
                    int curX = buttonsStartX;

                    if (_btnCreateInvoice != null)
                    {
                        _btnCreateInvoice.Location = new Point(curX, ctrlY);
                        _btnCreateInvoice.Size = new Size(btnWCreate, 34);
                        curX += btnWCreate + btnGap;
                    }
                    if (_btnViewInvoices != null)
                    {
                        _btnViewInvoices.Location = new Point(curX, ctrlY);
                        _btnViewInvoices.Size = new Size(btnWView, 34);
                        curX += btnWView + btnGap;
                    }
                    if (_btnManageCustomers != null)
                    {
                        _btnManageCustomers.Location = new Point(curX, ctrlY);
                        _btnManageCustomers.Size = new Size(btnWCust, 34);
                        curX += btnWCust + btnGap;
                    }
                    if (_btnProfileSettings != null)
                    {
                        _btnProfileSettings.Location = new Point(curX, ctrlY);
                        _btnProfileSettings.Size = new Size(btnWProf, 34);
                        curX += btnWProf + btnGap;
                    }
                    if (_btnRefreshBilling != null)
                    {
                        _btnRefreshBilling.Location = new Point(curX, ctrlY);
                        _btnRefreshBilling.Size = new Size(btnWRef, 34);
                    }
                }
                else
                {
                    // Tablet / Compact Screen Mode: 2 clean structured rows
                    int toolbarHeight = 112;
                    _billingToolbarCard.Size = new Size(contentWidth, toolbarHeight);

                    int ctrlY1 = 14;
                    int ctrlY2 = 62;

                    // Row 1: Search Box & Status Filter
                    int cmbWidth = 130;
                    if (_cmbFilterStatus != null)
                    {
                        _cmbFilterStatus.Location = new Point(contentWidth - cmbWidth - 16, ctrlY1);
                        _cmbFilterStatus.Size = new Size(cmbWidth, 34);
                    }

                    if (_lblStatusFilter != null && _cmbFilterStatus != null)
                    {
                        int lblY = ctrlY1 + (34 - _lblStatusFilter.PreferredHeight) / 2;
                        _lblStatusFilter.Location = new Point(_cmbFilterStatus.Left - _lblStatusFilter.PreferredWidth - 8, lblY);
                        _lblStatusFilter.Visible = true;
                    }

                    if (_txtSearchInvoice != null && _lblStatusFilter != null)
                    {
                        int searchWidth = Math.Max(180, _lblStatusFilter.Left - 16 - 12);
                        _txtSearchInvoice.Location = new Point(16, ctrlY1);
                        _txtSearchInvoice.Size = new Size(searchWidth, 34);
                    }

                    // Row 2: Action Buttons
                    int curX = Math.Max(16, contentWidth - totalBtnWidth - 16);
                    if (_btnCreateInvoice != null)
                    {
                        _btnCreateInvoice.Location = new Point(curX, ctrlY2);
                        _btnCreateInvoice.Size = new Size(btnWCreate, 34);
                        curX += btnWCreate + btnGap;
                    }
                    if (_btnViewInvoices != null)
                    {
                        _btnViewInvoices.Location = new Point(curX, ctrlY2);
                        _btnViewInvoices.Size = new Size(btnWView, 34);
                        curX += btnWView + btnGap;
                    }
                    if (_btnManageCustomers != null)
                    {
                        _btnManageCustomers.Location = new Point(curX, ctrlY2);
                        _btnManageCustomers.Size = new Size(btnWCust, 34);
                        curX += btnWCust + btnGap;
                    }
                    if (_btnProfileSettings != null)
                    {
                        _btnProfileSettings.Location = new Point(curX, ctrlY2);
                        _btnProfileSettings.Size = new Size(btnWProf, 34);
                        curX += btnWProf + btnGap;
                    }
                    if (_btnRefreshBilling != null)
                    {
                        _btnRefreshBilling.Location = new Point(curX, ctrlY2);
                        _btnRefreshBilling.Size = new Size(btnWRef, 34);
                    }
                }
            }

            // 3. Table Card (Positioned dynamically below the toolbar card)
            int tableY = toolbarY + (_billingToolbarCard?.Height ?? 68) + 16;
            int tableHeight = Math.Max(420, contentHeight - tableY - 24);
            if (_billingTableCard != null)
            {
                _billingTableCard.Location = new Point(0, tableY);
                _billingTableCard.Size = new Size(contentWidth, tableHeight);

                if (_lblBillingHint != null && _lblBillingTableTitle != null)
                {
                    int hintW = _lblBillingHint.PreferredWidth;
                    int rightEdge = contentWidth - hintW - 18;
                    if (rightEdge > _lblBillingTableTitle.Right + 16)
                    {
                        _lblBillingHint.Location = new Point(rightEdge, 17);
                        _lblBillingHint.Visible = true;
                    }
                    else
                    {
                        _lblBillingHint.Visible = false;
                    }
                }

                LayoutBillingTopBadges();

                if (_billingGridWrapper != null)
                {
                    _billingGridWrapper.Location = new Point(18, 48);
                    _billingGridWrapper.Size = new Size(contentWidth - 36, tableHeight - 65);
                    if (_billingGridWrapper.Width > 0 && _billingGridWrapper.Height > 0)
                    {
                        using var p = CreateRoundedRectanglePath(_billingGridWrapper.ClientRectangle, 8);
                        _billingGridWrapper.Region = new Region(p);
                    }

                    if (_gridInvoices != null)
                    {
                        _gridInvoices.Location = new Point(1, 1);
                        _gridInvoices.Size = new Size(_billingGridWrapper.Width - 11, _billingGridWrapper.Height - 2);
                    }
                    if (_billingScrollBar != null)
                    {
                        _billingScrollBar.Location = new Point(_billingGridWrapper.Width - 9, 1);
                        _billingScrollBar.Size = new Size(8, _billingGridWrapper.Height - 2);
                        _billingScrollBar.SyncWithGrid();
                    }
                }
                else if (_gridInvoices != null)
                {
                    _gridInvoices.Location = new Point(18, 48);
                    _gridInvoices.Size = new Size(contentWidth - 36, tableHeight - 65);
                }
            }
        }

        private void UpdateAnalyticsLayout()
        {
            if (_analyticsTableCard == null || _contentPanel == null) return;
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int gap = 20;

            // Row 1: 4 cards
            if (_analyticsTopCards.Count == 4)
            {
                int cardW = (contentWidth - (gap * 3)) / 4;
                for (int i = 0; i < 4; i++)
                {
                    int x = i * (cardW + gap);
                    int w = (i == 3) ? (contentWidth - x) : cardW;
                    _analyticsTopCards[i].Location = new Point(x, 0);
                    _analyticsTopCards[i].Size = new Size(w, 110);
                }
            }

            // Row 2: 4 cards
            int row2Y = 110 + gap;
            if (_analyticsRow2Cards.Count == 4)
            {
                int cardW = (contentWidth - (gap * 3)) / 4;
                for (int i = 0; i < 4; i++)
                {
                    int x = i * (cardW + gap);
                    int w = (i == 3) ? (contentWidth - x) : cardW;
                    _analyticsRow2Cards[i].Location = new Point(x, row2Y);
                    _analyticsRow2Cards[i].Size = new Size(w, 110);
                }
            }

            // Row 3: 2 charts
            int row3Y = row2Y + 110 + gap;
            int chartW = (contentWidth - gap) / 2;
            int chartH = 270;
            if (_analyticsSalesCard != null)
            {
                _analyticsSalesCard.Location = new Point(0, row3Y);
                _analyticsSalesCard.Size = new Size(chartW, chartH);
                if (_analyticsBarChart != null)
                {
                    _analyticsBarChart.Location = new Point(10, 45);
                    _analyticsBarChart.Size = new Size(chartW - 20, chartH - 55);
                }
            }
            if (_analyticsActivityCard != null)
            {
                int actX = chartW + gap;
                _analyticsActivityCard.Location = new Point(actX, row3Y);
                _analyticsActivityCard.Size = new Size(contentWidth - actX, chartH);
                if (_analyticsSplineChart != null)
                {
                    _analyticsSplineChart.Location = new Point(10, 45);
                    _analyticsSplineChart.Size = new Size(_analyticsActivityCard.Width - 20, chartH - 55);
                }
            }

            // Row 4: Orders Table
            int row4Y = row3Y + chartH + gap;
            int tableCardHeight = 360;
            if (_analyticsTableCard != null)
            {
                _analyticsTableCard.Location = new Point(0, row4Y);
                _analyticsTableCard.Size = new Size(contentWidth, tableCardHeight);

                LayoutTableTopToolbar();
                LayoutTableTopBadges();

                if (_analyticsGridWrapper != null)
                {
                    _analyticsGridWrapper.Location = new Point(18, 50);
                    _analyticsGridWrapper.Size = new Size(contentWidth - 36, tableCardHeight - 65);

                    if (_analyticsGridWrapper.Width > 0 && _analyticsGridWrapper.Height > 0)
                    {
                        using var p = CreateRoundedRectanglePath(_analyticsGridWrapper.ClientRectangle, 12);
                        _analyticsGridWrapper.Region = new Region(p);
                    }

                    if (_analyticsGridOrders != null)
                    {
                        _analyticsGridOrders.Location = new Point(1, 1);
                        _analyticsGridOrders.Size = new Size(_analyticsGridWrapper.Width - 11, _analyticsGridWrapper.Height - 2);
                    }
                    if (_analyticsScrollBar != null)
                    {
                        _analyticsScrollBar.Location = new Point(_analyticsGridWrapper.Width - 9, 1);
                        _analyticsScrollBar.Size = new Size(8, _analyticsGridWrapper.Height - 2);
                        _analyticsScrollBar.SyncWithGrid();
                    }
                }
            }
        }


        private void UpdateProductsLayout()
        {
            if (_productsTableCard == null || _contentPanel == null) return;
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int contentHeight = _contentPanel.ClientSize.Height;

            // 1. Metric cards (4 cards)
            int cardHeight = 110;
            int gap = 10;
            if (_productMetricCards.Count == 4)
            {
                int totalGaps = gap * 3;
                int baseWidth = (contentWidth - totalGaps) / 4;
                int remainder = (contentWidth - totalGaps) % 4;
                int currentX = 0;
                for (int i = 0; i < 4; i++)
                {
                    int w = baseWidth + (i < remainder ? 1 : 0);
                    _productMetricCards[i].Location = new Point(currentX, 0);
                    _productMetricCards[i].Size = new Size(w, cardHeight);
                    currentX += w + gap;
                }
            }

            // 2. Toolbar Card
            int toolbarY = cardHeight + 14;
            if (_productsToolbarCard != null)
            {
                _productsToolbarCard.Location = new Point(0, toolbarY);
                _productsToolbarCard.Size = new Size(contentWidth, 68);

                if (_btnProductRefresh != null) _btnProductRefresh.Location = new Point(contentWidth - 100, 17);
                if (_btnProductScanBarcode != null) _btnProductScanBarcode.Location = new Point(contentWidth - 245, 17);
                if (_btnProductAdd != null) _btnProductAdd.Location = new Point(contentWidth - 380, 17);
            }

            // 3. Products Table Card & Wrapper
            int tableY = toolbarY + 68 + 14;
            int tableHeight = Math.Max(420, contentHeight - tableY - 20);

            _productsTableCard.Location = new Point(0, tableY);
            _productsTableCard.Size = new Size(contentWidth, tableHeight);

            if (_lblProductHint != null)
            {
                _lblProductHint.Location = new Point(contentWidth - 420, 16);
            }

            if (_productsGridWrapper != null)
            {
                _productsGridWrapper.Location = new Point(16, 44);
                _productsGridWrapper.Size = new Size(contentWidth - 32, tableHeight - 60);

                if (_gridProducts != null)
                {
                    _gridProducts.Location = new Point(1, 1);
                    _gridProducts.Size = new Size(_productsGridWrapper.Width - 11, _productsGridWrapper.Height - 2);
                }

                if (_productScrollBar != null)
                {
                    _productScrollBar.Location = new Point(_productsGridWrapper.Width - 9, 1);
                    _productScrollBar.Size = new Size(8, _productsGridWrapper.Height - 2);
                    _productScrollBar.SyncWithGrid();
                }
            }
        }

        private void UpdateCustomersLayout()
        {
            if (_customersTableCard == null || _contentPanel == null) return;
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int contentHeight = _contentPanel.ClientSize.Height;

            // 1. Metric cards (4 cards)
            int cardHeight = 110;
            int gap = 10;
            if (_customerMetricCards.Count == 4)
            {
                int totalGaps = gap * 3;
                int baseWidth = (contentWidth - totalGaps) / 4;
                int remainder = (contentWidth - totalGaps) % 4;
                int currentX = 0;
                for (int i = 0; i < 4; i++)
                {
                    int w = baseWidth + (i < remainder ? 1 : 0);
                    _customerMetricCards[i].Location = new Point(currentX, 0);
                    _customerMetricCards[i].Size = new Size(w, cardHeight);
                    currentX += w + gap;
                }
            }

            // 2. Toolbar Card
            int toolbarY = cardHeight + 14;
            if (_customersToolbarCard != null)
            {
                _customersToolbarCard.Location = new Point(0, toolbarY);
                _customersToolbarCard.Size = new Size(contentWidth, 68);

                if (_btnCustomerRefresh != null) _btnCustomerRefresh.Location = new Point(contentWidth - 100, 17);
                if (_btnCustomerAdd != null) _btnCustomerAdd.Location = new Point(contentWidth - 250, 17);
            }

            // 3. Customers Table Card & Wrapper
            int tableY = toolbarY + 68 + 14;
            int tableHeight = Math.Max(420, contentHeight - tableY - 20);

            _customersTableCard.Location = new Point(0, tableY);
            _customersTableCard.Size = new Size(contentWidth, tableHeight);

            if (_lblCustomerHint != null)
            {
                _lblCustomerHint.Location = new Point(Math.Max(280, contentWidth - 460), 16);
            }

            if (_customersGridWrapper != null)
            {
                _customersGridWrapper.Location = new Point(16, 44);
                _customersGridWrapper.Size = new Size(contentWidth - 32, tableHeight - 60);

                if (_gridCustomers != null)
                {
                    _gridCustomers.Location = new Point(1, 1);
                    _gridCustomers.Size = new Size(_customersGridWrapper.Width - 11, _customersGridWrapper.Height - 2);
                }

                if (_customerScrollBar != null)
                {
                    _customerScrollBar.Location = new Point(_customersGridWrapper.Width - 9, 1);
                    _customerScrollBar.Size = new Size(8, _customersGridWrapper.Height - 2);
                    _customerScrollBar.SyncWithGrid();
                }
            }
        }

        private void UpdateMessagesLayout()
        {
            if (_messagesListCard == null || _contentPanel == null) return;
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int contentHeight = _contentPanel.ClientSize.Height;

            if (_messagesHeaderCard != null)
            {
                _messagesHeaderCard.Location = new Point(0, 0);
                _messagesHeaderCard.Size = new Size(contentWidth, 70);
                if (_btnMessagesMarkAll != null) _btnMessagesMarkAll.Location = new Point(contentWidth - 170, 17);
            }

            int listHeight = Math.Max(480, contentHeight - 85 - 30);
            _messagesListCard.Location = new Point(0, 85);
            _messagesListCard.Size = new Size(contentWidth, listHeight);

            foreach (Control c in _messagesListCard.Controls)
            {
                if (c is RoundedPanel rp)
                {
                    rp.Width = contentWidth - 40;
                }
            }
        }

        private void UpdateSettingsLayout()
        {
            if (_settingsThemeCard == null || _contentPanel == null) return;
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);

            _settingsThemeCard.Location = new Point(0, 0);
            _settingsThemeCard.Size = new Size(contentWidth, 150);

            if (_settingsCompanyCard != null)
            {
                _settingsCompanyCard.Location = new Point(0, 165);
                _settingsCompanyCard.Size = new Size(contentWidth, 230);
            }

            if (_settingsInfoCard != null)
            {
                _settingsInfoCard.Location = new Point(0, 410);
                _settingsInfoCard.Size = new Size(contentWidth, 200);
            }

            if (_settingsSessionCard != null)
            {
                _settingsSessionCard.Location = new Point(0, 625);
                _settingsSessionCard.Size = new Size(contentWidth, 130);
            }
        }

        private async Task RefreshCompanySettingsPreviewAsync(Panel previewPnl)
        {
            if (previewPnl == null) return;
            var profile = await _billingService.GetBusinessProfileAsync();
            previewPnl.Controls.Clear();

            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
            Color panelBg = isDark ? Color.FromArgb(20, 23, 34) : Color.FromArgb(248, 250, 252);
            Color panelBorder = isDark ? Color.FromArgb(42, 45, 65) : Color.FromArgb(226, 232, 240);

            if (previewPnl is RoundedPanel rp)
            {
                rp.CustomBackgroundColor = panelBg;
                rp.CustomBorderColor = panelBorder;
                rp.Invalidate();
            }

            // High-contrast colors guaranteed for Light Mode & Dark Mode
            Color titleCol = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42); // Crisp dark text in light mode
            Color textDark = isDark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59); // Dark slate in light mode
            Color textMuted = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105); // Header slate in light mode
            Color textSubtle = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139); // Address gray
            Color accentCol = isDark ? Color.FromArgb(96, 165, 250) : Color.FromArgb(79, 70, 229); // Vibrant indigo in light mode
            Color gstinCol = isDark ? Color.FromArgb(52, 211, 153) : Color.FromArgb(5, 150, 105); // Rich green

            // Left Column: Business Name, Tagline & GSTIN
            var lblName = new Label
            {
                Text = profile.BusinessName,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = titleCol,
                Location = new Point(16, 14),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblName);

            var lblTag = new Label
            {
                Text = profile.Tagline,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = accentCol,
                Location = new Point(16, 40),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblTag);

            var lblGstin = new Label
            {
                Text = $"GSTIN: {profile.GSTIN}",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = gstinCol,
                Location = new Point(16, 68),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblGstin);

            var lblAddress = new Label
            {
                Text = $"📍 {profile.Address}, {profile.City}, {profile.Country}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textSubtle,
                Location = new Point(16, 94),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblAddress);

            // Middle Column: Contact Info
            int midX = Math.Max(380, previewPnl.Width / 3 + 20);
            var lblContactHeader = new Label
            {
                Text = "CONTACT INFORMATION",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = textMuted,
                Location = new Point(midX, 16),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblContactHeader);

            var lblPhone = new Label
            {
                Text = $"📞 {profile.Phone}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = textDark,
                Location = new Point(midX, 38),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblPhone);

            var lblEmail = new Label
            {
                Text = $"✉️ {profile.Email}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = textDark,
                Location = new Point(midX, 64),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblEmail);

            // Right Column: Bank & UPI Info
            int rightX = Math.Max(700, (previewPnl.Width * 2) / 3);
            var lblBankHeader = new Label
            {
                Text = "BANK & PAYMENT DETAILS",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = textMuted,
                Location = new Point(rightX, 16),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblBankHeader);

            var lblBank = new Label
            {
                Text = $"🏦 {profile.BankName} (A/C: {profile.AccountNumber})",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textDark,
                Location = new Point(rightX, 38),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblBank);

            var lblIfsc = new Label
            {
                Text = $"IFSC: {profile.IFSC}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textDark,
                Location = new Point(rightX, 64),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblIfsc);

            var lblUpi = new Label
            {
                Text = $"UPI: {profile.UPIId}",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = accentCol,
                Location = new Point(rightX, 90),
                AutoSize = true
            };
            previewPnl.Controls.Add(lblUpi);
        }

        private DataGridView CreateStyledGrid()
        {
            var grid = new DataGridView
            {
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9f),
                RowTemplate = { Height = 34 },
                BackgroundColor = ThemeManager.CardBg,
                GridColor = ThemeManager.CardBorder,
                ScrollBars = ScrollBars.Vertical
            };

            grid.DefaultCellStyle.BackColor = ThemeManager.CardBg;
            grid.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(45, 50, 75) : Color.FromArgb(230, 235, 255);
            grid.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TableHeaderBg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);
            grid.ColumnHeadersHeight = 38;

            _themeGrids.Add(grid);
            return grid;
        }

        private Label CreateDialogLabel(string text, int y, Form parent)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(25, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CreateDialogTextBox(string value, int y, Form parent)
        {
            var txt = new TextBox
            {
                Text = value,
                Font = new Font("Segoe UI", 10f),
                Size = new Size(385, 30),
                Location = new Point(25, y),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private RoundedPanel CreateMetricCard(string title, string value, string trend, bool isPositiveTrend, string icon, int x, int y, int w, int h)
        {
            return CreateMetricCard(title, value, trend, isPositiveTrend, icon, x, y, w, h, out _);
        }

        private RoundedPanel CreateMetricCard(string title, string value, string trend, bool isPositiveTrend, string icon, int x, int y, int w, int h, out Label valLabel)
        {
            return CreateMetricCard(title, value, trend, isPositiveTrend, icon, x, y, w, h, out valLabel, out _);
        }

        private RoundedPanel CreateMetricCard(string title, string value, string trend, bool isPositiveTrend, string icon, int x, int y, int w, int h, out Label valLabel, out Label trendLabel)
        {
            var card = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 14,
                BorderWidth = 1,
                EnableHoverEffect = true,
                Cursor = Cursors.Hand
            };
            _themeCards.Add(card);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(16, 14),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            _themeSubLabels.Add(lblTitle);

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.Transparent,
                Location = new Point(w - 38, 12),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(lblIcon);

            valLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(14, 38),
                AutoSize = true
            };
            card.Controls.Add(valLabel);
            _themeTitleLabels.Add(valLabel);

            trendLabel = new Label
            {
                Text = trend,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = isPositiveTrend ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68),
                BackColor = Color.Transparent,
                Location = new Point(16, 80),
                AutoSize = true
            };
            card.Controls.Add(trendLabel);

            return card;
        }

        private RoundedPanel CreateDonutMetricCard(string title, string value, List<DonutSlice> slices, int x, int y, int w, int h)
        {
            return CreateDonutMetricCard(title, value, slices, x, y, w, h, out _, out _);
        }

        private RoundedPanel CreateDonutMetricCard(string title, string value, List<DonutSlice> slices, int x, int y, int w, int h, out Label valLabel, out DonutChartControl donutCtrl)
        {
            var card = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 14,
                BorderWidth = 1,
                EnableHoverEffect = true,
                Cursor = Cursors.Hand
            };
            _themeCards.Add(card);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(16, 14),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            _themeSubLabels.Add(lblTitle);

            valLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(14, 38),
                AutoSize = true
            };
            card.Controls.Add(valLabel);
            _themeTitleLabels.Add(valLabel);

            donutCtrl = CreateDonutChart(slices, 160, 90);
            donutCtrl.Location = new Point(w - 170, 10);
            donutCtrl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            card.Controls.Add(donutCtrl);

            return card;
        }

        private DonutChartControl CreateDonutChart(List<DonutSlice> slices, int width = 160, int height = 90)
        {
            return new DonutChartControl
            {
                Size = new Size(width, height),
                Slices = slices,
                BackColor = ThemeManager.CardBg
            };
        }

        private RoundedPanel CreateCircleProgressCard(string title, string value, string subtitle, string badgeText, Color badgeColor, int x, int y, int w, int h)
        {
            return CreateCircleProgressCard(title, value, subtitle, badgeText, badgeColor, x, y, w, h, out _, out _, out _);
        }

        private RoundedPanel CreateCircleProgressCard(string title, string value, string subtitle, string badgeText, Color badgeColor, int x, int y, int w, int h, out Label valLabel, out Label subLabel, out Panel badgePanel)
        {
            var card = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 14,
                BorderWidth = 1,
                EnableHoverEffect = true,
                Cursor = Cursors.Hand
            };
            _themeCards.Add(card);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(16, 14),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            _themeSubLabels.Add(lblTitle);

            valLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(14, 38),
                AutoSize = true
            };
            card.Controls.Add(valLabel);
            _themeTitleLabels.Add(valLabel);

            subLabel = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(16, 80),
                AutoSize = true
            };
            card.Controls.Add(subLabel);
            _themeSubLabels.Add(subLabel);

            // Mini badge on right
            badgePanel = new Panel
            {
                Size = new Size(42, 42),
                Location = new Point(w - 55, 30),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            badgePanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var pen = new Pen(badgeColor, 3f))
                {
                    g.DrawArc(pen, 3, 3, 36, 36, -90, 260);
                }
                using var f = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                using var b = new SolidBrush(badgeColor);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(badgeText, f, b, new RectangleF(0, 0, 42, 42), sf);
            };
            card.Controls.Add(badgePanel);

            return card;
        }

        private static Color GetAvatarColor(string name)
        {
            if (string.IsNullOrEmpty(name)) return Color.FromArgb(76, 111, 255);
            int hash = Math.Abs(name.GetHashCode());
            Color[] palette = new[]
            {
                Color.FromArgb(76, 111, 255),
                Color.FromArgb(140, 82, 255),
                Color.FromArgb(16, 185, 129),
                Color.FromArgb(245, 158, 11),
                Color.FromArgb(236, 72, 153),
                Color.FromArgb(6, 182, 212)
            };
            return palette[hash % palette.Length];
        }

        private static Color GetAvatarStartColor(string name)
        {
            if (string.IsNullOrEmpty(name)) return Color.FromArgb(76, 111, 255);
            int hash = Math.Abs(name.GetHashCode());
            Color[] colors = {
                Color.FromArgb(236, 72, 153), // Pink
                Color.FromArgb(14, 165, 233), // Sky
                Color.FromArgb(16, 185, 129), // Emerald
                Color.FromArgb(168, 85, 247), // Purple
                Color.FromArgb(249, 115, 22), // Orange
                Color.FromArgb(99, 102, 241)  // Indigo
            };
            return colors[hash % colors.Length];
        }

        private static Color GetAvatarEndColor(string name)
        {
            if (string.IsNullOrEmpty(name)) return Color.FromArgb(45, 80, 230);
            int hash = Math.Abs(name.GetHashCode());
            Color[] colors = {
                Color.FromArgb(219, 39, 119), // Rose Pink
                Color.FromArgb(2, 132, 199),  // Deep Sky
                Color.FromArgb(5, 150, 105),  // Deep Emerald
                Color.FromArgb(147, 51, 234), // Deep Purple
                Color.FromArgb(234, 88, 12),  // Deep Orange
                Color.FromArgb(79, 70, 229)   // Deep Indigo
            };
            return colors[hash % colors.Length];
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
        #endregion
    }
}
