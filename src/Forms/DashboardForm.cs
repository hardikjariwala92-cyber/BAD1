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
        private TextBox? _txtSearchInvoice;
        private Label? _lblStatusFilter;
        private ComboBox? _cmbFilterStatus;
        private readonly List<RoundedPanel> _billingMetricCards = new();
        private RoundedPanel? _billingToolbarCard;
        private RoundedPanel? _billingTableCard;
        private ModernButton? _btnCreateInvoice;
        private ModernButton? _btnViewInvoices;
        private ModernButton? _btnManageCustomers;
        private ModernButton? _btnProfileSettings;
        private ModernButton? _btnRefreshBilling;

        // Products Page Controls
        private DataGridView? _gridProducts;
        private RoundedPanel? _productsHeaderCard;
        private RoundedPanel? _productsTableCard;
        private ModernButton? _btnProductAdd;
        private ModernButton? _btnProductRefresh;

        // Customers Page Controls
        private DataGridView? _gridCustomers;
        private RoundedPanel? _customersHeaderCard;
        private RoundedPanel? _customersTableCard;
        private ModernButton? _btnCustomerAdd;
        private ModernButton? _btnCustomerRefresh;

        // Messages Page Controls
        private RoundedPanel? _messagesHeaderCard;
        private RoundedPanel? _messagesListCard;
        private ModernButton? _btnMessagesMarkAll;

        // Settings Page Controls
        private RoundedPanel? _settingsThemeCard;
        private RoundedPanel? _settingsInfoCard;
        private RoundedPanel? _settingsSessionCard;

        // Analytics Page Controls
        private readonly List<RoundedPanel> _analyticsTopCards = new();
        private readonly List<RoundedPanel> _analyticsRow2Cards = new();
        private RoundedPanel? _analyticsSalesCard;
        private RoundedPanel? _analyticsActivityCard;
        private RoundedPanel? _analyticsTableCard;
        private BarChartControl? _analyticsBarChart;
        private SplineChartControl? _analyticsSplineChart;
        private DataGridView? _analyticsGridOrders;

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
        private TextBox? _txtSearchAnalyticsOrders;
        private ComboBox? _cmbAnalyticsStatus;
        private ModernButton? _btnRefreshAnalytics;
        private Label? _lblAnalyticsOrderCount;
        private System.Windows.Forms.Timer? _analyticsRefreshTimer;

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
            BackColor = ThemeManager.Background;
            _sidebarPanel.BackColor = ThemeManager.SidebarBg;
            if (_menuPanel != null) _menuPanel.BackColor = ThemeManager.SidebarBg;
            if (_brandBox != null)
            {
                _brandBox.BackColor = ThemeManager.SidebarBg;
                _brandBox.Invalidate();
            }
            _headerPanel.BackColor = ThemeManager.Background;
            _contentPanel.BackColor = ThemeManager.Background;

            if (_profileContainer != null) _profileContainer.BackColor = ThemeManager.Background;
            if (_avatarBox != null)
            {
                _avatarBox.BackColor = ThemeManager.Background;
                _avatarBox.Invalidate();
            }

            _lblHeaderTitle.ForeColor = ThemeManager.TextPrimary;
            _lblHeaderTitle.BackColor = Color.Transparent;
            _lblProfileName.ForeColor = ThemeManager.TextPrimary;
            _lblProfileName.BackColor = Color.Transparent;

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

            foreach (var card in _themeCards)
            {
                card.BackColor = ThemeManager.CardBg;
                card.CustomBackgroundColor = ThemeManager.CardBg;
                card.CustomBorderColor = ThemeManager.CardBorder;
                card.Invalidate(true);
            }

            foreach (var lbl in _themeTitleLabels)
            {
                lbl.ForeColor = ThemeManager.TextPrimary;
                lbl.BackColor = Color.Transparent;
            }

            foreach (var lbl in _themeSubLabels)
            {
                lbl.ForeColor = ThemeManager.TextSecondary;
                lbl.BackColor = Color.Transparent;
            }

            foreach (var grid in _themeGrids)
            {
                grid.BackgroundColor = ThemeManager.CardBg;
                grid.DefaultCellStyle.BackColor = ThemeManager.CardBg;
                grid.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
                grid.DefaultCellStyle.SelectionBackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(45, 50, 75) : Color.FromArgb(230, 235, 255);
                grid.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
                grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TableHeaderBg;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
                grid.GridColor = ThemeManager.CardBorder;
            }

            foreach (var tb in _themeTextBoxes)
            {
                tb.BackColor = ThemeManager.CardBg;
                tb.ForeColor = ThemeManager.TextPrimary;
            }

            foreach (var cb in _themeComboBoxes)
            {
                cb.BackColor = ThemeManager.CardBg;
                cb.ForeColor = ThemeManager.TextPrimary;
            }

            // Update sidebar menu button colors
            for (int i = 0; i < _menuButtons.Count; i++)
            {
                bool isActive = i == _activeMenuIndex;
                _menuButtons[i].ForeColor = isActive ? Color.White : ThemeManager.TextSecondary;
                _menuButtons[i].BackColor = isActive ? ThemeManager.AccentBlue : ThemeManager.SidebarBg;
                _menuButtons[i].Font = new Font("Segoe UI", 9.5f, isActive ? FontStyle.Bold : FontStyle.Regular);
            }

            _contentPanel.Invalidate(true);
            Invalidate(true);
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
                    Close();
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

            _gridProducts = null;
            _productsHeaderCard = null;
            _productsTableCard = null;
            _btnProductAdd = null;
            _btnProductRefresh = null;

            _gridCustomers = null;
            _customersHeaderCard = null;
            _customersTableCard = null;
            _btnCustomerAdd = null;
            _btnCustomerRefresh = null;

            _messagesHeaderCard = null;
            _messagesListCard = null;
            _btnMessagesMarkAll = null;

            _settingsThemeCard = null;
            _settingsInfoCard = null;
            _settingsSessionCard = null;

            _analyticsTopCards.Clear();
            _analyticsRow2Cards.Clear();
            _analyticsSalesCard = null;
            _analyticsActivityCard = null;
            _analyticsTableCard = null;
            _analyticsBarChart = null;
            _analyticsSplineChart = null;
            _analyticsGridOrders = null;

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

            // Date Range Pill - perfectly aligned at Y=19 with height 36
            _datePill = new RoundedPanel
            {
                BorderRadius = 10,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder,
                Size = new Size(215, 36),
                Location = new Point(_lblHeaderTitle.Right + 20, 19)
            };
            _lblDate = new Label
            {
                Text = "01.08.2022 - 31.08.2022  📅",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
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
            _cardMonthTotal = CreateMetricCard("Month total", "25410", "↓ 0.2% since last month", false, "💲", 0, row2Y, cardWidth, cardHeight, out _lblValMonthTotal!, out _lblTrendMonthTotal!);
            _analyticsRow2Cards.Add(_cardMonthTotal);
            _contentPanel.Controls.Add(_cardMonthTotal);

            _cardRevenue = CreateMetricCard("Revenue", "1352", "↓ 1.2% since last month", false, "💳", cardWidth + gap, row2Y, cardWidth, cardHeight, out _lblValRevenue!, out _lblTrendRevenue!);
            _analyticsRow2Cards.Add(_cardRevenue);
            _contentPanel.Controls.Add(_cardRevenue);

            _cardInvoices = CreateCircleProgressCard("Paid Invoices", "$30256.23", "Current Financial Year", "+15%", ThemeManager.AccentPurple, (cardWidth + gap) * 2, row2Y, cardWidth + 30, cardHeight, out _lblValInvoices!, out _lblSubInvoices!, out _badgeInvoices!);
            _cardInvoices.Cursor = Cursors.Hand;
            _cardInvoices.OnCardClicked = (c) => NavigateToMenu(1); // Jump to Sales & Billing
            _analyticsRow2Cards.Add(_cardInvoices);
            _contentPanel.Controls.Add(_cardInvoices);

            _cardFunds = CreateCircleProgressCard("Funds received", "$150256.23", "Current Financial Year", "+59%", ThemeManager.AccentGreen, (cardWidth + gap) * 3 + 30, row2Y, cardWidth + 30, cardHeight, out _lblValFunds!, out _lblSubFunds!, out _badgeFunds!);
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
                DataPoints = _dbService.GetSalesDynamics(2021)
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

            // ROW 4: Customer Order Table with Working Search, Filter, Sort, and Hover
            int row4Y = row3Y + chartCardHeight + gap;
            int tableCardWidth = chartCardWidth + gap + chartCardWidth + 60;
            int tableCardHeight = 310;

            _analyticsTableCard = new RoundedPanel
            {
                Location = new Point(0, row4Y),
                Size = new Size(tableCardWidth, tableCardHeight),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_analyticsTableCard);

            var lblTableTitle = new Label
            {
                Text = "Customer order",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(18, 14),
                AutoSize = true
            };
            _analyticsTableCard.Controls.Add(lblTableTitle);
            _themeTitleLabels.Add(lblTableTitle);

            // Bootstrap-style Order Count Badge (Next to title, no text collision!)
            _lblAnalyticsOrderCount = new Label
            {
                Text = "6 orders",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(145, 185, 255),
                BackColor = Color.FromArgb(35, 45, 75),
                Location = new Point(165, 16),
                Padding = new Padding(6, 2, 6, 2),
                AutoSize = true
            };
            _analyticsTableCard.Controls.Add(_lblAnalyticsOrderCount);

            // Search box (Bootstrap input style)
            _txtSearchAnalyticsOrders = new TextBox
            {
                PlaceholderText = "🔍 Search customer or city...",
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(220, 28),
                Location = new Point(tableCardWidth - 465, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(24, 26, 38),
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearchAnalyticsOrders.TextChanged += async (s, e) => await FilterAnalyticsOrdersAsync();
            _analyticsTableCard.Controls.Add(_txtSearchAnalyticsOrders);
            _themeTextBoxes.Add(_txtSearchAnalyticsOrders);

            // Status Filter ComboBox (Bootstrap flat dark dropdown)
            _cmbAnalyticsStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 22,
                Font = new Font("Segoe UI", 9f),
                Size = new Size(125, 28),
                Location = new Point(tableCardWidth - 235, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(24, 26, 38),
                ForeColor = ThemeManager.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _cmbAnalyticsStatus.Items.AddRange(new object[] { "All", "Delivered", "Processed", "Cancelled" });
            _cmbAnalyticsStatus.SelectedIndex = 0;
            _cmbAnalyticsStatus.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Graphics == null) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color itemBg = isSelected ? Color.FromArgb(48, 54, 82) : Color.FromArgb(24, 26, 38);
                using (var b = new SolidBrush(itemBg)) e.Graphics.FillRectangle(b, e.Bounds);
                string txt = _cmbAnalyticsStatus.Items[e.Index]?.ToString() ?? "";
                using (var f = new SolidBrush(ThemeManager.TextPrimary))
                {
                    e.Graphics.DrawString(txt, _cmbAnalyticsStatus.Font, f, e.Bounds.X + 8, e.Bounds.Y + 3);
                }
            };
            _cmbAnalyticsStatus.SelectedIndexChanged += async (s, e) => await FilterAnalyticsOrdersAsync();
            _analyticsTableCard.Controls.Add(_cmbAnalyticsStatus);
            _themeComboBoxes.Add(_cmbAnalyticsStatus);

            // Refresh Button (Bootstrap primary button)
            _btnRefreshAnalytics = new ModernButton
            {
                Text = "🔄 Refresh",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(95, 28),
                Location = new Point(tableCardWidth - 105, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(95, 130, 255),
                BorderRadius = 6
            };
            _btnRefreshAnalytics.Click += async (s, e) => await RefreshAnalyticsDataAsync();
            _analyticsTableCard.Controls.Add(_btnRefreshAnalytics);

            _analyticsGridOrders = CreateStyledGrid();
            _analyticsGridOrders.Location = new Point(18, 48);
            _analyticsGridOrders.Size = new Size(tableCardWidth - 36, tableCardHeight - 60);
            _analyticsGridOrders.RowTemplate.Height = 42; // Spacious Bootstrap padding
            _analyticsGridOrders.ColumnHeadersHeight = 40;
            _analyticsGridOrders.CellBorderStyle = DataGridViewCellBorderStyle.None;
            _analyticsGridOrders.EnableHeadersVisualStyles = false;

            _analyticsGridOrders.Columns.Add("Profile", "Profile");
            _analyticsGridOrders.Columns.Add("Address", "Address");
            _analyticsGridOrders.Columns.Add("Date", "Date");
            _analyticsGridOrders.Columns.Add("Status", "Status");
            _analyticsGridOrders.Columns.Add("Price", "Price");

            _analyticsGridOrders.Columns[0].Width = 170;
            _analyticsGridOrders.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _analyticsGridOrders.Columns[1].MinimumWidth = 180;
            _analyticsGridOrders.Columns[2].Width = 130;
            _analyticsGridOrders.Columns[3].Width = 145;
            _analyticsGridOrders.Columns[4].Width = 120;

            // Bootstrap Custom Cell Painting (Avatars, Pill Badges, Striped rows, Dividers)
            _analyticsGridOrders.CellPainting += (s, e) =>
            {
                if (e.Graphics == null) return;

                if (e.RowIndex == -1)
                {
                    // Header Painting (Bootstrap style with Active Tab Blue on Profile)
                    e.PaintBackground(e.CellBounds, true);
                    bool isProfileCol = (e.ColumnIndex == 0);
                    Color headerBg = isProfileCol ? Color.FromArgb(0, 122, 255) : Color.FromArgb(20, 22, 33);
                    using (var b = new SolidBrush(headerBg))
                    {
                        e.Graphics.FillRectangle(b, e.CellBounds);
                    }
                    using (var divPen = new Pen(Color.FromArgb(42, 45, 65), 1f))
                    {
                        e.Graphics.DrawLine(divPen, e.CellBounds.Right - 1, e.CellBounds.Top + 8, e.CellBounds.Right - 1, e.CellBounds.Bottom - 8);
                        e.Graphics.DrawLine(divPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }
                    string headerText = _analyticsGridOrders.Columns[e.ColumnIndex].HeaderText;
                    using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(isProfileCol ? Color.White : ThemeManager.TextSecondary))
                    {
                        e.Graphics.DrawString(headerText, font, textBrush, e.CellBounds.X + 16, e.CellBounds.Y + 12);
                    }
                    if (isProfileCol)
                    {
                        using var sf = new SolidBrush(Color.White);
                        var pts = new PointF[]
                        {
                            new(e.CellBounds.Right - 20, e.CellBounds.Y + 22),
                            new(e.CellBounds.Right - 15, e.CellBounds.Y + 15),
                            new(e.CellBounds.Right - 10, e.CellBounds.Y + 22)
                        };
                        e.Graphics.FillPolygon(sf, pts);
                    }
                    e.Handled = true;
                    return;
                }

                if (e.RowIndex >= 0)
                {
                    // Row Background (Bootstrap table-striped & table-hover)
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                    Color rowBg = isSelected 
                        ? Color.FromArgb(40, 45, 70) 
                        : (e.RowIndex % 2 == 0 ? Color.FromArgb(25, 27, 38) : Color.FromArgb(20, 22, 31));

                    using (var brush = new SolidBrush(rowBg))
                    {
                        e.Graphics.FillRectangle(brush, e.CellBounds);
                    }

                    // Subtle horizontal border (Bootstrap table-bordered divider)
                    using (var linePen = new Pen(Color.FromArgb(34, 37, 54), 1f))
                    {
                        e.Graphics.DrawLine(linePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    }

                    string cellVal = e.Value?.ToString() ?? "";

                    // Column 0: Profile with Avatar Circle Initials Badge
                    if (e.ColumnIndex == 0)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int avatarSize = 26;
                        int avatarX = e.CellBounds.X + 14;
                        int avatarY = e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2;

                        Color avatarBg = GetAvatarColor(cellVal);
                        using (var avBrush = new SolidBrush(avatarBg))
                        {
                            e.Graphics.FillEllipse(avBrush, avatarX, avatarY, avatarSize, avatarSize);
                        }

                        string initial = cellVal.Length > 0 ? cellVal[0].ToString().ToUpperInvariant() : "?";
                        using (var fInitial = new Font("Segoe UI", 9f, FontStyle.Bold))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        {
                            e.Graphics.DrawString(initial, fInitial, Brushes.White, new RectangleF(avatarX, avatarY, avatarSize, avatarSize), sf);
                        }

                        using (var fName = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                        using (var bName = new SolidBrush(ThemeManager.TextPrimary))
                        {
                            e.Graphics.DrawString(cellVal, fName, bName, avatarX + avatarSize + 10, e.CellBounds.Y + 12);
                        }
                        e.Handled = true;
                    }
                    // Column 3: Status Badge (Bootstrap rounded-pill badge)
                    else if (e.ColumnIndex == 3)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        Color pillBg, pillBorder, pillText;
                        if (cellVal == "Delivered")
                        {
                            pillBg = Color.FromArgb(22, 52, 40);
                            pillBorder = Color.FromArgb(16, 185, 129);
                            pillText = Color.FromArgb(52, 211, 153);
                        }
                        else if (cellVal == "Processed")
                        {
                            pillBg = Color.FromArgb(52, 42, 18);
                            pillBorder = Color.FromArgb(245, 158, 11);
                            pillText = Color.FromArgb(251, 191, 36);
                        }
                        else if (cellVal == "Cancelled")
                        {
                            pillBg = Color.FromArgb(52, 24, 28);
                            pillBorder = Color.FromArgb(239, 68, 68);
                            pillText = Color.FromArgb(248, 113, 113);
                        }
                        else
                        {
                            pillBg = Color.FromArgb(35, 38, 54);
                            pillBorder = ThemeManager.AccentBlue;
                            pillText = Color.White;
                        }

                        using var fBadge = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                        var textSize = e.Graphics.MeasureString(cellVal, fBadge);
                        int badgeW = (int)textSize.Width + 24;
                        int badgeH = 22;
                        int badgeX = e.CellBounds.X + 12;
                        int badgeY = e.CellBounds.Y + (e.CellBounds.Height - badgeH) / 2;

                        var badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);
                        using (var bp = CreateRoundedRectanglePath(badgeRect, 10))
                        {
                            using var bBrush = new SolidBrush(pillBg);
                            e.Graphics.FillPath(bBrush, bp);
                            using var bPen = new Pen(pillBorder, 1f);
                            e.Graphics.DrawPath(bPen, bp);
                        }

                        using (var dotBrush = new SolidBrush(pillBorder))
                        {
                            e.Graphics.FillEllipse(dotBrush, badgeX + 7, badgeY + (badgeH - 6) / 2, 6, 6);
                        }

                        using (var tBrush = new SolidBrush(pillText))
                        {
                            e.Graphics.DrawString(cellVal, fBadge, tBrush, badgeX + 16, badgeY + 2);
                        }

                        e.Handled = true;
                    }
                    // Column 4: Price
                    else if (e.ColumnIndex == 4)
                    {
                        using (var fPrice = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                        using (var bPrice = new SolidBrush(ThemeManager.TextPrimary))
                        {
                            e.Graphics.DrawString(cellVal, fPrice, bPrice, e.CellBounds.X + 12, e.CellBounds.Y + 12);
                        }
                        e.Handled = true;
                    }
                    // Column 1 (Address) & Column 2 (Date)
                    else
                    {
                        using (var fText = new Font("Segoe UI", 9f, FontStyle.Regular))
                        using (var bText = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            e.Graphics.DrawString(cellVal, fText, bText, e.CellBounds.X + 12, e.CellBounds.Y + 12);
                        }
                        e.Handled = true;
                    }
                }
            };

            // Interactive row hover highlight (table-hover)
            _analyticsGridOrders.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _analyticsGridOrders.Rows.Count)
                {
                    _analyticsGridOrders.Rows[e.RowIndex].Selected = true;
                }
            };

            _analyticsTableCard.Controls.Add(_analyticsGridOrders);
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
            string search = _txtSearchAnalyticsOrders?.Text.Trim() ?? "";
            string status = _cmbAnalyticsStatus?.SelectedItem?.ToString() ?? "All";
            var orders = await _dbService.GetOrdersAsync(search, status);

            _analyticsGridOrders.Rows.Clear();
            foreach (var o in orders)
            {
                _analyticsGridOrders.Rows.Add(o.ProfileName, o.Address, o.OrderDate, o.Status, o.Price);
            }
            if (_lblAnalyticsOrderCount != null)
            {
                _lblAnalyticsOrderCount.Text = $"Showing {orders.Count} order{(orders.Count == 1 ? "" : "s")}";
            }
        }

        private async Task RefreshAnalyticsDataAsync()
        {
            try
            {
                var summary = _dbService.GetAnalyticsSummary();
                var billingMetrics = await _billingService.GetSalesDashboardMetricsAsync();
                var orders = await _dbService.GetOrdersAsync(_txtSearchAnalyticsOrders?.Text, _cmbAnalyticsStatus?.SelectedItem?.ToString());

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
                                ("Avg Contract Value", "$125.00 / mo")
                            },
                            FooterTip = "Click to view Products & catalog"
                        };
                    }
                }

                // 5. Month total Card
                if (_lblValMonthTotal != null)
                {
                    decimal mTotal = billingMetrics.TotalSales > 0 ? billingMetrics.TotalSales : summary.MonthTotal;
                    _lblValMonthTotal.Text = mTotal >= 1000 ? $"${mTotal:N0}" : $"${mTotal:N2}";
                    if (_lblTrendMonthTotal != null) _lblTrendMonthTotal.Text = summary.MonthTotalTrend;
                    if (_cardMonthTotal != null)
                    {
                        _cardMonthTotal.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Monthly Gross Billing",
                            Icon = "💲",
                            CurrentValue = $"${mTotal:N2}",
                            PreviousValue = "$25,461.00",
                            GrowthText = summary.MonthTotalTrend,
                            IsPositiveGrowth = summary.MonthTotalTrendPositive,
                            Category = "Financial Turnover",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Daily Average Run-Rate", $"${mTotal / 30m:N2} / day"),
                                ("Projected Month Close", $"${mTotal * 1.08m:N2}"),
                                ("Total Invoices Recorded", $"{billingMetrics.TotalInvoices}"),
                                ("Outstanding Receivables", $"${billingMetrics.OutstandingBalance:N2}")
                            }
                        };
                    }
                }

                // 6. Revenue Card
                if (_lblValRevenue != null)
                {
                    _lblValRevenue.Text = $"${summary.Revenue:N0}";
                    if (_lblTrendRevenue != null) _lblTrendRevenue.Text = summary.RevenueTrend;
                    if (_cardRevenue != null)
                    {
                        _cardRevenue.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Net Operational Revenue",
                            Icon = "💳",
                            CurrentValue = $"${summary.Revenue:N0}",
                            PreviousValue = "$1,368.00",
                            GrowthText = summary.RevenueTrend,
                            IsPositiveGrowth = summary.RevenueTrendPositive,
                            Category = "Operating Margins",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Operating Margin", "68.4% gross profit"),
                                ("ARPU (Average/User)", "$27.60"),
                                ("Cost of Goods (COGS)", "$428.00"),
                                ("Billing DB Balance", $"${billingMetrics.OutstandingBalance:N2}")
                            }
                        };
                    }
                }

                // 7. Paid Invoices Card
                if (_lblValInvoices != null)
                {
                    string paidVal = billingMetrics.TotalCollected > 0 ? $"${billingMetrics.TotalCollected:N2}" : summary.PaidInvoices;
                    _lblValInvoices.Text = paidVal;
                    if (_cardInvoices != null)
                    {
                        _cardInvoices.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Settled Invoices & Collections",
                            Icon = "🧾",
                            CurrentValue = paidVal,
                            PreviousValue = "$26,300.00",
                            GrowthText = summary.PaidInvoicesBadge + " vs last year pace",
                            IsPositiveGrowth = true,
                            Category = "Cash Flow & Collections",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Paid Invoice Count", $"{billingMetrics.PaidInvoices} cleared"),
                                ("Unpaid Invoices", $"{billingMetrics.UnpaidInvoices} pending"),
                                ("Partially Paid", $"{billingMetrics.PartiallyPaidInvoices} partial"),
                                ("FY Target Goal", "$35,000.00 (86.4% Met)")
                            },
                            FooterTip = "Click to jump to full Invoices table"
                        };
                    }
                }

                // 8. Funds received Card
                if (_lblValFunds != null)
                {
                    _lblValFunds.Text = summary.FundsReceived;
                    if (_cardFunds != null)
                    {
                        _cardFunds.MetricInfo = new MetricDetailInfo
                        {
                            Title = "Cleared Bank Funds & Liquidity",
                            Icon = "🏦",
                            CurrentValue = summary.FundsReceived,
                            PreviousValue = "$94,500.00",
                            GrowthText = summary.FundsReceivedBadge + " FY Target Pace",
                            IsPositiveGrowth = true,
                            Category = "Capital & Treasury",
                            LastUpdated = DateTime.Now,
                            Stats = new List<(string Label, string Value)>
                            {
                                ("Direct Wire Cleared", "62% ($93,158.86)"),
                                ("Online Payment Gateway", "28% ($42,071.74)"),
                                ("Cheque & Cash Deposits", "10% ($15,025.63)"),
                                ("Settlement Latency", "< 24 hours SLA")
                            },
                            FooterTip = "Click to view financial statements"
                        };
                    }
                }

                // Refresh orders grid
                if (_analyticsGridOrders != null)
                {
                    _analyticsGridOrders.Rows.Clear();
                    foreach (var o in orders)
                    {
                        _analyticsGridOrders.Rows.Add(o.ProfileName, o.Address, o.OrderDate, o.Status, o.Price);
                    }
                    if (_lblAnalyticsOrderCount != null)
                    {
                        _lblAnalyticsOrderCount.Text = $"Showing {orders.Count} order{(orders.Count == 1 ? "" : "s")}";
                    }
                }
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
            int cardWidth = 200;
            int cardHeight = 110;
            int gap = 15;

            // 1. TOP METRICS ROW: 5 Live Metric Cards from Database!
            var cardTotalSales = CreateMetricCard("Total Sales", "₹0.00", "Across active invoices", true, "💰", 0, 0, cardWidth, cardHeight, out _lblValTotalSales!);
            _billingMetricCards.Add(cardTotalSales);
            _contentPanel.Controls.Add(cardTotalSales);

            var cardTotalInvoices = CreateMetricCard("Total Invoices", "0", "Billing records", true, "📑", cardWidth + gap, 0, cardWidth, cardHeight, out _lblValTotalInvoices!);
            _billingMetricCards.Add(cardTotalInvoices);
            _contentPanel.Controls.Add(cardTotalInvoices);

            var cardPaidInvoices = CreateMetricCard("Paid Invoices", "0", "Cleared settlements", true, "✅", (cardWidth + gap) * 2, 0, cardWidth, cardHeight, out _lblValPaidInvoices!);
            _billingMetricCards.Add(cardPaidInvoices);
            _contentPanel.Controls.Add(cardPaidInvoices);

            var cardUnpaidInvoices = CreateMetricCard("Unpaid Invoices", "0", "Pending settlements", false, "⏳", (cardWidth + gap) * 3, 0, cardWidth, cardHeight, out _lblValUnpaidInvoices!);
            _billingMetricCards.Add(cardUnpaidInvoices);
            _contentPanel.Controls.Add(cardUnpaidInvoices);

            var cardOutstanding = CreateMetricCard("Outstanding Balance", "₹0.00", "Total receivables due", false, "⚠️", (cardWidth + gap) * 4, 0, cardWidth + 50, cardHeight, out _lblValOutstanding!);
            _billingMetricCards.Add(cardOutstanding);
            _contentPanel.Controls.Add(cardOutstanding);

            // 2. QUICK ACTIONS & FILTER TOOLBAR
            int toolbarY = cardHeight + 20;

            _billingToolbarCard = new RoundedPanel
            {
                Location = new Point(0, toolbarY),
                Size = new Size(1110, 80),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_billingToolbarCard);

            // Search Box
            _txtSearchInvoice = new TextBox
            {
                PlaceholderText = "🔍  Search by invoice #, customer name, or company...",
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(270, 28),
                Location = new Point(16, 25),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _billingToolbarCard.Controls.Add(_txtSearchInvoice);
            _themeTextBoxes.Add(_txtSearchInvoice);

            // Status Filter ComboBox
            _lblStatusFilter = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(300, 8),
                AutoSize = true
            };
            _billingToolbarCard.Controls.Add(_lblStatusFilter);
            _themeSubLabels.Add(_lblStatusFilter);

            _cmbFilterStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(130, 28),
                Location = new Point(300, 25),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary
            };
            _cmbFilterStatus.Items.AddRange(new object[] { "All", "Unpaid", "Partially Paid", "Paid", "Overdue", "Draft", "Cancelled" });
            _cmbFilterStatus.SelectedIndex = 0;
            _billingToolbarCard.Controls.Add(_cmbFilterStatus);
            _themeComboBoxes.Add(_cmbFilterStatus);

            // Quick Action: + Create Invoice
            _btnCreateInvoice = new ModernButton
            {
                Text = "+ Create Invoice",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(140, 36),
                Location = new Point(445, 21),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 8
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

            // Quick Action: View Invoices (focus grid)
            _btnViewInvoices = new ModernButton
            {
                Text = "📄 View Invoices",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(130, 36),
                Location = new Point(595, 21),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 8
            };
            _billingToolbarCard.Controls.Add(_btnViewInvoices);
            _btnViewInvoices.Click += (s, e) =>
            {
                if (_gridInvoices != null && _gridInvoices.Rows.Count > 0)
                {
                    _gridInvoices.Focus();
                }
            };

            // Quick Action: Manage Customers
            _btnManageCustomers = new ModernButton
            {
                Text = "👥 Customers",
                NormalColor = ThemeManager.AccentPurple,
                HoverColor = Color.FromArgb(200, 100, 255),
                Size = new Size(115, 36),
                Location = new Point(735, 21),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 8
            };
            _billingToolbarCard.Controls.Add(_btnManageCustomers);
            _btnManageCustomers.Click += (s, e) => NavigateToMenu(4); // Switch to Customers tab

            // Quick Action: Business Profile
            _btnProfileSettings = new ModernButton
            {
                Text = "⚙️ Business Info",
                NormalColor = Color.FromArgb(70, 80, 105),
                HoverColor = Color.FromArgb(90, 100, 130),
                Size = new Size(125, 36),
                Location = new Point(860, 21),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8
            };
            _billingToolbarCard.Controls.Add(_btnProfileSettings);
            _btnProfileSettings.Click += (s, e) =>
            {
                using var profDlg = new BusinessProfileDialog();
                profDlg.ShowDialog(this);
            };

            // Refresh Button
            _btnRefreshBilling = new ModernButton
            {
                Text = "↻ Refresh",
                NormalColor = ThemeManager.CardBorder,
                HoverColor = ThemeManager.CardBg,
                Size = new Size(95, 36),
                Location = new Point(995, 21),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 8
            };
            _billingToolbarCard.Controls.Add(_btnRefreshBilling);
            _btnRefreshBilling.Click += async (s, e) => await ReloadBillingDataAsync();

            _contentPanel.Controls.Add(_billingToolbarCard);

            // 3. INVOICES TABLE CARD
            int tableY = toolbarY + 95;
            int tableHeight = 490;

            _billingTableCard = new RoundedPanel
            {
                Location = new Point(0, tableY),
                Size = new Size(1110, tableHeight),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_billingTableCard);

            var lblTableTitle = new Label
            {
                Text = "Invoices & Billing Register",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 14),
                AutoSize = true
            };
            _billingTableCard.Controls.Add(lblTableTitle);
            _themeTitleLabels.Add(lblTableTitle);

            var lblHint = new Label
            {
                Text = "💡 Double-click any row to view/edit invoice. Right-click for payment recording, PDF export & print.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(250, 18),
                AutoSize = true
            };
            _billingTableCard.Controls.Add(lblHint);
            _themeSubLabels.Add(lblHint);

            _gridInvoices = CreateStyledGrid();
            _gridInvoices.ScrollBars = ScrollBars.Vertical;
            _gridInvoices.Location = new Point(18, 48);
            _gridInvoices.Size = new Size(1074, tableHeight - 65);

            _gridInvoices.Columns.Add("InvoiceId", "ID");
            _gridInvoices.Columns.Add("InvoiceNo", "Invoice #");
            _gridInvoices.Columns.Add("Customer", "Client / Company");
            _gridInvoices.Columns.Add("InvoiceDate", "Issue Date");
            _gridInvoices.Columns.Add("DueDate", "Due Date");
            _gridInvoices.Columns.Add("Total", "Total (₹)");
            _gridInvoices.Columns.Add("Paid", "Paid (₹)");
            _gridInvoices.Columns.Add("Balance", "Balance Due (₹)");
            _gridInvoices.Columns.Add("Status", "Status");

            _gridInvoices.Columns[0].Visible = false;
            _gridInvoices.Columns[1].Width = 140;
            _gridInvoices.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridInvoices.Columns[2].MinimumWidth = 200;
            _gridInvoices.Columns[3].Width = 115;
            _gridInvoices.Columns[4].Width = 115;
            _gridInvoices.Columns[5].Width = 130;
            _gridInvoices.Columns[6].Width = 130;
            _gridInvoices.Columns[7].Width = 140;
            _gridInvoices.Columns[8].Width = 125;

            // Cell formatting for Status badges
            _gridInvoices.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 8 && e.Value != null)
                {
                    string status = e.Value.ToString() ?? "";
                    e.CellStyle!.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle!.ForeColor = status switch
                    {
                        "Paid" => Color.FromArgb(16, 185, 129),
                        "Partially Paid" => Color.FromArgb(245, 158, 11),
                        "Overdue" => Color.FromArgb(239, 68, 68),
                        "Cancelled" => Color.FromArgb(100, 116, 139),
                        "Draft" => Color.FromArgb(148, 163, 184),
                        _ => Color.FromArgb(249, 115, 22)
                    };
                }
            };

            // Double click to view / edit invoice
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
                    if (hit.RowIndex >= 0)
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

                        ctx.Show(_gridInvoices, new Point(e.X, e.Y));
                    }
                }
            };

            _billingTableCard.Controls.Add(_gridInvoices);
            _contentPanel.Controls.Add(_billingTableCard);

            // Filter events
            _txtSearchInvoice.TextChanged += async (s, e) => await FilterInvoicesAsync();
            _cmbFilterStatus.SelectedIndexChanged += async (s, e) => await FilterInvoicesAsync();

            // Apply responsive layout to fill available width and height
            UpdateBillingLayout();

            // Initial load
            _ = ReloadBillingDataAsync();
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

            var invoices = await _billingService.GetInvoicesAsync(
                search: string.IsNullOrWhiteSpace(search) ? null : search,
                status: status
            );

            _gridInvoices.Rows.Clear();
            foreach (var inv in invoices)
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
                    inv.Status
                );
            }
        }
        #endregion

        // =====================================================
        // PAGE: PRODUCTS (Add / Edit / Delete / Search)
        // =====================================================
        #region Products Page
        private void LoadProductsPage()
        {
            // Title + Search bar
            _productsHeaderCard = new RoundedPanel
            {
                Location = new Point(0, 0),
                Size = new Size(1060, 70),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_productsHeaderCard);

            var lblTitle = new Label
            {
                Text = "🛍️  Product Management",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 20),
                AutoSize = true
            };
            _productsHeaderCard.Controls.Add(lblTitle);
            _themeTitleLabels.Add(lblTitle);

            // Search textbox
            var txtSearch = new TextBox
            {
                PlaceholderText = "🔍  Search products by name or category...",
                Font = new Font("Segoe UI", 10f),
                Size = new Size(300, 30),
                Location = new Point(380, 18),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _productsHeaderCard.Controls.Add(txtSearch);
            _themeTextBoxes.Add(txtSearch);

            // Add Product button
            _btnProductAdd = new ModernButton
            {
                Text = "+ Add Product",
                BorderRadius = 10,
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(140, 34),
                Location = new Point(700, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _productsHeaderCard.Controls.Add(_btnProductAdd);

            // Refresh button
            _btnProductRefresh = new ModernButton
            {
                Text = "↻ Refresh",
                BorderRadius = 10,
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(100, 34),
                Location = new Point(850, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _productsHeaderCard.Controls.Add(_btnProductRefresh);

            _contentPanel.Controls.Add(_productsHeaderCard);

            // Products DataGridView Card
            _productsTableCard = new RoundedPanel
            {
                Location = new Point(0, 85),
                Size = new Size(1060, 550),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_productsTableCard);

            _gridProducts = CreateStyledGrid();
            _gridProducts.ScrollBars = ScrollBars.Vertical;
            _gridProducts.Location = new Point(18, 18);
            _gridProducts.Size = new Size(1024, 510);

            _gridProducts.Columns.Add("ID", "ID");
            _gridProducts.Columns.Add("Name", "Product Name");
            _gridProducts.Columns.Add("Category", "Category");
            _gridProducts.Columns.Add("Price", "Price ($)");
            _gridProducts.Columns.Add("Stock", "Stock");
            _gridProducts.Columns.Add("Status", "Status");

            _gridProducts.Columns[0].Width = 60;
            _gridProducts.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridProducts.Columns[1].MinimumWidth = 200;
            _gridProducts.Columns[2].Width = 180;
            _gridProducts.Columns[3].Width = 120;
            _gridProducts.Columns[4].Width = 100;
            _gridProducts.Columns[5].Width = 120;

            // Status color formatting
            _gridProducts.CellFormatting += (s, e) =>
            {
                if (_gridProducts.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    string status = e.Value.ToString() ?? "";
                    if (status == "Active")
                    {
                        e.CellStyle!.ForeColor = Color.FromArgb(16, 185, 129);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle!.ForeColor = Color.FromArgb(239, 68, 68);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                }
            };

            _productsTableCard.Controls.Add(_gridProducts);
            _contentPanel.Controls.Add(_productsTableCard);

            UpdateProductsLayout();

            // Load data
            _ = LoadProductData();

            // Wire events
            txtSearch.TextChanged += async (s, e) =>
            {
                var products = await _dbService.GetProductsAsync(txtSearch.Text);
                RefreshProductGrid(products);
            };

            if (_btnProductAdd != null) _btnProductAdd.Click += (s, e) => ShowProductDialog(null);
            if (_btnProductRefresh != null) _btnProductRefresh.Click += async (s, e) => await LoadProductData();

            // Double-click to edit
            _gridProducts.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridProducts.Rows.Count) return;
                var row = _gridProducts.Rows[e.RowIndex];
                if (row.Cells[0].Value == null) return;

                var product = new Product(
                    Convert.ToInt32(row.Cells[0].Value),
                    row.Cells[1].Value?.ToString() ?? "",
                    row.Cells[2].Value?.ToString() ?? "",
                    Convert.ToDecimal(row.Cells[3].Value ?? 0),
                    Convert.ToInt32(row.Cells[4].Value ?? 0),
                    row.Cells[5].Value?.ToString() ?? "Active"
                );
                ShowProductDialog(product);
            };

            // Right-click delete
            _gridProducts.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = _gridProducts.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        _gridProducts.ClearSelection();
                        _gridProducts.Rows[hit.RowIndex].Selected = true;

                        var ctx = new ContextMenuStrip();
                        ctx.Items.Add("✏️ Edit", null, (_, _) =>
                        {
                            var row = _gridProducts.Rows[hit.RowIndex];
                            if (row.Cells[0].Value == null) return;
                            var product = new Product(
                                Convert.ToInt32(row.Cells[0].Value),
                                row.Cells[1].Value?.ToString() ?? "",
                                row.Cells[2].Value?.ToString() ?? "",
                                Convert.ToDecimal(row.Cells[3].Value ?? 0),
                                Convert.ToInt32(row.Cells[4].Value ?? 0),
                                row.Cells[5].Value?.ToString() ?? "Active"
                            );
                            ShowProductDialog(product);
                        });
                        ctx.Items.Add("🗑️ Delete", null, async (_, _) =>
                        {
                            var row = _gridProducts.Rows[hit.RowIndex];
                            if (row.Cells[0].Value == null) return;
                            int id = Convert.ToInt32(row.Cells[0].Value);
                            string name = row.Cells[1].Value?.ToString() ?? "";
                            if (MessageBox.Show($"Delete product \"{name}\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                await _dbService.DeleteProductAsync(id);
                                await LoadProductData();
                            }
                        });
                        ctx.Show(_gridProducts, new Point(e.X, e.Y));
                    }
                }
            };
        }

        private async Task LoadProductData()
        {
            var products = await _dbService.GetProductsAsync();
            RefreshProductGrid(products);
        }

        private void RefreshProductGrid(List<Product> products)
        {
            if (_gridProducts == null) return;
            _gridProducts.Rows.Clear();
            foreach (var p in products)
            {
                _gridProducts.Rows.Add(p.ProductId, p.Name, p.Category, p.Price, p.StockQuantity, p.Status);
            }
        }

        private void ShowProductDialog(Product? existing)
        {
            bool isEdit = existing != null;
            var dlg = new Form
            {
                Text = isEdit ? "Edit Product" : "Add Product",
                Size = new Size(450, 420),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeManager.Background
            };

            int y = 20;
            var lblName = CreateDialogLabel("Product Name:", y, dlg);
            var txtName = CreateDialogTextBox(existing?.Name ?? "", y + 22, dlg); y += 65;

            var lblCat = CreateDialogLabel("Category:", y, dlg);
            var txtCat = CreateDialogTextBox(existing?.Category ?? "", y + 22, dlg); y += 65;

            var lblPrice = CreateDialogLabel("Price ($):", y, dlg);
            var txtPrice = CreateDialogTextBox(existing?.Price.ToString("F2") ?? "0.00", y + 22, dlg); y += 65;

            var lblStock = CreateDialogLabel("Stock Quantity:", y, dlg);
            var txtStock = CreateDialogTextBox(existing?.StockQuantity.ToString() ?? "0", y + 22, dlg); y += 65;

            var lblStatus = CreateDialogLabel("Status:", y, dlg);
            var cmbStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Active", "Inactive" },
                Font = new Font("Segoe UI", 10f),
                Location = new Point(25, y + 22),
                Size = new Size(385, 30),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary
            };
            cmbStatus.SelectedItem = existing?.Status ?? "Active";
            dlg.Controls.Add(cmbStatus); y += 65;

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

                var product = new Product(
                    existing?.ProductId ?? 0,
                    txtName.Text.Trim(),
                    txtCat.Text.Trim(),
                    price,
                    stock,
                    cmbStatus.SelectedItem?.ToString() ?? "Active"
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
            _customersHeaderCard = new RoundedPanel
            {
                Location = new Point(0, 0),
                Size = new Size(1060, 70),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_customersHeaderCard);

            var lblTitle = new Label
            {
                Text = "👥  Customer Management",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 20),
                AutoSize = true
            };
            _customersHeaderCard.Controls.Add(lblTitle);
            _themeTitleLabels.Add(lblTitle);

            var txtSearch = new TextBox
            {
                PlaceholderText = "🔍  Search customers by name, city, or email...",
                Font = new Font("Segoe UI", 10f),
                Size = new Size(300, 30),
                Location = new Point(380, 18),
                BackColor = ThemeManager.CardBg,
                ForeColor = ThemeManager.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _customersHeaderCard.Controls.Add(txtSearch);
            _themeTextBoxes.Add(txtSearch);

            _btnCustomerAdd = new ModernButton
            {
                Text = "+ Add Customer",
                NormalColor = ThemeManager.AccentGreen,
                HoverColor = Color.FromArgb(20, 200, 140),
                Size = new Size(150, 34),
                Location = new Point(700, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 10
            };
            _customersHeaderCard.Controls.Add(_btnCustomerAdd);

            _btnCustomerRefresh = new ModernButton
            {
                Text = "↻ Refresh",
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(100, 34),
                Location = new Point(860, 17),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BorderRadius = 10
            };
            _customersHeaderCard.Controls.Add(_btnCustomerRefresh);
            _contentPanel.Controls.Add(_customersHeaderCard);

            // Customers Grid Card
            _customersTableCard = new RoundedPanel
            {
                Location = new Point(0, 85),
                Size = new Size(1060, 550),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(_customersTableCard);

            _gridCustomers = CreateStyledGrid();
            _gridCustomers.ScrollBars = ScrollBars.Vertical;
            _gridCustomers.Location = new Point(18, 18);
            _gridCustomers.Size = new Size(1024, 510);

            _gridCustomers.Columns.Add("ID", "ID");
            _gridCustomers.Columns.Add("Name", "Customer Name");
            _gridCustomers.Columns.Add("Email", "Email");
            _gridCustomers.Columns.Add("Phone", "Phone");
            _gridCustomers.Columns.Add("City", "City");
            _gridCustomers.Columns.Add("Country", "Country");

            _gridCustomers.Columns[0].Width = 60;
            _gridCustomers.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridCustomers.Columns[1].MinimumWidth = 200;
            _gridCustomers.Columns[2].Width = 240;
            _gridCustomers.Columns[3].Width = 160;
            _gridCustomers.Columns[4].Width = 150;
            _gridCustomers.Columns[5].Width = 120;

            _customersTableCard.Controls.Add(_gridCustomers);
            _contentPanel.Controls.Add(_customersTableCard);

            UpdateCustomersLayout();

            _ = LoadCustomerData();

            txtSearch.TextChanged += async (s, e) =>
            {
                var customers = await _dbService.GetCustomersAsync(txtSearch.Text);
                RefreshCustomerGrid(customers);
            };

            if (_btnCustomerAdd != null) _btnCustomerAdd.Click += (s, e) => ShowCustomerDialog(null);
            if (_btnCustomerRefresh != null) _btnCustomerRefresh.Click += async (s, e) => await LoadCustomerData();

            _gridCustomers.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _gridCustomers.Rows.Count) return;
                var row = _gridCustomers.Rows[e.RowIndex];
                if (row.Cells[0].Value == null) return;

                var cust = new Customer(
                    Convert.ToInt32(row.Cells[0].Value),
                    row.Cells[1].Value?.ToString() ?? "",
                    row.Cells[2].Value?.ToString() ?? "",
                    row.Cells[3].Value?.ToString() ?? "",
                    "",
                    row.Cells[4].Value?.ToString() ?? "",
                    row.Cells[5].Value?.ToString() ?? ""
                );
                ShowCustomerDialog(cust);
            };

            _gridCustomers.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = _gridCustomers.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        _gridCustomers.ClearSelection();
                        _gridCustomers.Rows[hit.RowIndex].Selected = true;

                        var ctx = new ContextMenuStrip();
                        ctx.Items.Add("✏️ Edit", null, (_, _) =>
                        {
                            var row = _gridCustomers.Rows[hit.RowIndex];
                            if (row.Cells[0].Value == null) return;
                            var cust = new Customer(
                                Convert.ToInt32(row.Cells[0].Value),
                                row.Cells[1].Value?.ToString() ?? "",
                                row.Cells[2].Value?.ToString() ?? "",
                                row.Cells[3].Value?.ToString() ?? "",
                                "",
                                row.Cells[4].Value?.ToString() ?? "",
                                row.Cells[5].Value?.ToString() ?? ""
                            );
                            ShowCustomerDialog(cust);
                        });
                        ctx.Items.Add("📑 View Invoices & History", null, async (_, _) =>
                        {
                            var row = _gridCustomers.Rows[hit.RowIndex];
                            if (row.Cells[0].Value == null) return;
                            int id = Convert.ToInt32(row.Cells[0].Value);
                            var customers = await _billingService.GetAllCustomersAsync();
                            var cust = customers.FirstOrDefault(c => c.CustomerId == id)
                                ?? new Customer(id, row.Cells[1].Value?.ToString() ?? "", row.Cells[2].Value?.ToString() ?? "", row.Cells[3].Value?.ToString() ?? "", "", row.Cells[4].Value?.ToString() ?? "", row.Cells[5].Value?.ToString() ?? "");
                            using var histDlg = new CustomerHistoryDialog(cust);
                            histDlg.ShowDialog(this);
                        });
                        ctx.Items.Add("🗑️ Delete", null, async (_, _) =>
                        {
                            var row = _gridCustomers.Rows[hit.RowIndex];
                            if (row.Cells[0].Value == null) return;
                            int id = Convert.ToInt32(row.Cells[0].Value);
                            string name = row.Cells[1].Value?.ToString() ?? "";
                            if (MessageBox.Show($"Delete customer \"{name}\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                await _dbService.DeleteCustomerAsync(id);
                                await LoadCustomerData();
                            }
                        });
                        ctx.Show(_gridCustomers, new Point(e.X, e.Y));
                    }
                }
            };
        }

        private async Task LoadCustomerData()
        {
            var customers = await _dbService.GetCustomersAsync();
            RefreshCustomerGrid(customers);
        }

        private void RefreshCustomerGrid(List<Customer> customers)
        {
            if (_gridCustomers == null) return;
            _gridCustomers.Rows.Clear();
            foreach (var c in customers)
            {
                _gridCustomers.Rows.Add(c.CustomerId, c.Name, c.Email, c.Phone, c.City, c.Country);
            }
        }

        private void ShowCustomerDialog(Customer? existing)
        {
            bool isEdit = existing != null;
            var dlg = new Form
            {
                Text = isEdit ? "Edit Customer" : "Add Customer",
                Size = new Size(450, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeManager.Background
            };

            int y = 20;
            var lblName = CreateDialogLabel("Customer Name:", y, dlg);
            var txtName = CreateDialogTextBox(existing?.Name ?? "", y + 22, dlg); y += 60;

            var lblEmail = CreateDialogLabel("Email:", y, dlg);
            var txtEmail = CreateDialogTextBox(existing?.Email ?? "", y + 22, dlg); y += 60;

            var lblPhone = CreateDialogLabel("Phone:", y, dlg);
            var txtPhone = CreateDialogTextBox(existing?.Phone ?? "", y + 22, dlg); y += 60;

            var lblCity = CreateDialogLabel("City:", y, dlg);
            var txtCity = CreateDialogTextBox(existing?.City ?? "", y + 22, dlg); y += 60;

            var lblCountry = CreateDialogLabel("Country:", y, dlg);
            var txtCountry = CreateDialogTextBox(existing?.Country ?? "", y + 22, dlg); y += 65;

            var btnSave = new ModernButton
            {
                Text = isEdit ? "💾 Update" : "✅ Add Customer",
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
                    MessageBox.Show("Customer name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Email is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var customer = new Customer(
                    existing?.CustomerId ?? 0,
                    txtName.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtPhone.Text.Trim(),
                    "",
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

            var lblCurrentTheme = new Label
            {
                Text = $"Current Theme:  {ThemeManager.CurrentTheme}",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ThemeManager.AccentBlue,
                Location = new Point(18, 85),
                AutoSize = true
            };
            _settingsThemeCard.Controls.Add(lblCurrentTheme);

            var themeToggleLarge = new ThemeToggle
            {
                Location = new Point(250, 82),
                Size = new Size(80, 36)
            };
            _settingsThemeCard.Controls.Add(themeToggleLarge);

            _contentPanel.Controls.Add(_settingsThemeCard);

            // Application Info Section
            _settingsInfoCard = new RoundedPanel
            {
                Location = new Point(0, 170),
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
                Location = new Point(0, 390),
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

            // 1. Metric cards (5 cards)
            int gap = 14;
            if (_billingMetricCards.Count == 5)
            {
                int cardWidth = (contentWidth - (gap * 4)) / 5;
                for (int i = 0; i < 5; i++)
                {
                    int x = i * (cardWidth + gap);
                    int w = (i == 4) ? (contentWidth - x) : cardWidth;
                    _billingMetricCards[i].Location = new Point(x, 0);
                    _billingMetricCards[i].Size = new Size(w, 110);
                }
            }

            // 2. Toolbar
            int toolbarY = 110 + 20;
            if (_billingToolbarCard != null)
            {
                _billingToolbarCard.Location = new Point(0, toolbarY);
                _billingToolbarCard.Size = new Size(contentWidth, 80);

                int searchWidth = Math.Clamp(contentWidth / 5, 240, 320);
                if (_txtSearchInvoice != null)
                {
                    _txtSearchInvoice.Location = new Point(16, 25);
                    _txtSearchInvoice.Size = new Size(searchWidth, 28);
                }

                if (_lblStatusFilter != null)
                {
                    _lblStatusFilter.Location = new Point(searchWidth + 24, 8);
                }

                if (_cmbFilterStatus != null)
                {
                    _cmbFilterStatus.Location = new Point(searchWidth + 24, 25);
                    _cmbFilterStatus.Size = new Size(120, 28);
                }

                int btnWCreate = 140;
                int btnWView = 125;
                int btnWCust = 115;
                int btnWProf = 125;
                int btnWRef = 95;
                int btnGap = 8;
                int totalBtnWidth = btnWCreate + btnGap + btnWView + btnGap + btnWCust + btnGap + btnWProf + btnGap + btnWRef;

                int statusRight = (searchWidth + 24) + 120;
                int buttonsStartX = Math.Max(statusRight + 16, contentWidth - totalBtnWidth - 16);

                int curX = buttonsStartX;
                if (_btnCreateInvoice != null)
                {
                    _btnCreateInvoice.Location = new Point(curX, 21);
                    _btnCreateInvoice.Size = new Size(btnWCreate, 36);
                    curX += btnWCreate + btnGap;
                }
                if (_btnViewInvoices != null)
                {
                    _btnViewInvoices.Location = new Point(curX, 21);
                    _btnViewInvoices.Size = new Size(btnWView, 36);
                    curX += btnWView + btnGap;
                }
                if (_btnManageCustomers != null)
                {
                    _btnManageCustomers.Location = new Point(curX, 21);
                    _btnManageCustomers.Size = new Size(btnWCust, 36);
                    curX += btnWCust + btnGap;
                }
                if (_btnProfileSettings != null)
                {
                    _btnProfileSettings.Location = new Point(curX, 21);
                    _btnProfileSettings.Size = new Size(btnWProf, 36);
                    curX += btnWProf + btnGap;
                }
                if (_btnRefreshBilling != null)
                {
                    _btnRefreshBilling.Location = new Point(curX, 21);
                    _btnRefreshBilling.Size = new Size(btnWRef, 36);
                }
            }

            // 3. Table Card
            int tableY = toolbarY + 95;
            int tableHeight = Math.Max(480, contentHeight - tableY - 30);
            if (_billingTableCard != null)
            {
                _billingTableCard.Location = new Point(0, tableY);
                _billingTableCard.Size = new Size(contentWidth, tableHeight);

                if (_gridInvoices != null)
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
            int tableCardHeight = 310;
            if (_analyticsTableCard != null)
            {
                _analyticsTableCard.Location = new Point(0, row4Y);
                _analyticsTableCard.Size = new Size(contentWidth, tableCardHeight);
                if (_analyticsGridOrders != null)
                {
                    _analyticsGridOrders.Location = new Point(18, 48);
                    _analyticsGridOrders.Size = new Size(contentWidth - 36, tableCardHeight - 60);
                }
            }
        }

        private void UpdateProductsLayout()
        {
            if (_productsTableCard == null || _contentPanel == null) return;
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int contentHeight = _contentPanel.ClientSize.Height;

            if (_productsHeaderCard != null)
            {
                _productsHeaderCard.Location = new Point(0, 0);
                _productsHeaderCard.Size = new Size(contentWidth, 70);
                if (_btnProductRefresh != null) _btnProductRefresh.Location = new Point(contentWidth - 118, 17);
                if (_btnProductAdd != null) _btnProductAdd.Location = new Point(contentWidth - 275, 17);
            }

            int tableHeight = Math.Max(480, contentHeight - 85 - 30);
            _productsTableCard.Location = new Point(0, 85);
            _productsTableCard.Size = new Size(contentWidth, tableHeight);

            if (_gridProducts != null)
            {
                _gridProducts.Location = new Point(18, 18);
                _gridProducts.Size = new Size(contentWidth - 36, tableHeight - 36);
            }
        }

        private void UpdateCustomersLayout()
        {
            if (_customersTableCard == null || _contentPanel == null) return;
            int contentWidth = Math.Max(1050, _contentPanel.ClientSize.Width - 50);
            int contentHeight = _contentPanel.ClientSize.Height;

            if (_customersHeaderCard != null)
            {
                _customersHeaderCard.Location = new Point(0, 0);
                _customersHeaderCard.Size = new Size(contentWidth, 70);
                if (_btnCustomerRefresh != null) _btnCustomerRefresh.Location = new Point(contentWidth - 118, 17);
                if (_btnCustomerAdd != null) _btnCustomerAdd.Location = new Point(contentWidth - 285, 17);
            }

            int tableHeight = Math.Max(480, contentHeight - 85 - 30);
            _customersTableCard.Location = new Point(0, 85);
            _customersTableCard.Size = new Size(contentWidth, tableHeight);

            if (_gridCustomers != null)
            {
                _gridCustomers.Location = new Point(18, 18);
                _gridCustomers.Size = new Size(contentWidth - 36, tableHeight - 36);
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

            if (_settingsInfoCard != null)
            {
                _settingsInfoCard.Location = new Point(0, 170);
                _settingsInfoCard.Size = new Size(contentWidth, 200);
            }

            if (_settingsSessionCard != null)
            {
                _settingsSessionCard.Location = new Point(0, 390);
                _settingsSessionCard.Size = new Size(contentWidth, 130);
            }
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
