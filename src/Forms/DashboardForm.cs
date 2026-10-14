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

        private Panel _sidebarPanel = null!;
        private Panel _headerPanel = null!;
        private Panel _contentPanel = null!;
        private Label _lblHeaderTitle = null!;
        private Label _lblProfileName = null!;
        private RoundedPanel _datePill = null!;
        private ThemeToggle _themeToggle = null!;
        private DataGridView _gridOrders = null!;
        private readonly List<RoundedPanel> _themeCards = new();
        private readonly List<Label> _themeTitleLabels = new();
        private readonly List<Label> _themeSubLabels = new();

        public DashboardForm(User? user = null)
        {
            _currentUser = user ?? new User("admin", "Kristi Kamlykova", "Administrator", "KK");

            Text = "Business Analytics Dashboard — S TOM'S Platform";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1340, 860);
            MinimumSize = new Size(1150, 720);
            DoubleBuffered = true;

            ThemeManager.OnThemeChanged += OnThemeUpdated;

            BuildLayout();
            ApplyThemeColors();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
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
            _headerPanel.BackColor = ThemeManager.Background;
            _contentPanel.BackColor = ThemeManager.Background;

            _lblHeaderTitle.ForeColor = ThemeManager.TextPrimary;
            _lblProfileName.ForeColor = ThemeManager.TextPrimary;

            _datePill.CustomBackgroundColor = ThemeManager.CardBg;
            _datePill.CustomBorderColor = ThemeManager.CardBorder;

            foreach (var card in _themeCards)
            {
                card.CustomBackgroundColor = ThemeManager.CardBg;
                card.CustomBorderColor = ThemeManager.CardBorder;
                card.Invalidate();
            }

            foreach (var lbl in _themeTitleLabels)
            {
                lbl.ForeColor = ThemeManager.TextPrimary;
            }

            foreach (var lbl in _themeSubLabels)
            {
                lbl.ForeColor = ThemeManager.TextSecondary;
            }

            // DataGridView Theme
            if (_gridOrders != null)
            {
                _gridOrders.BackgroundColor = ThemeManager.CardBg;
                _gridOrders.DefaultCellStyle.BackColor = ThemeManager.CardBg;
                _gridOrders.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
                _gridOrders.DefaultCellStyle.SelectionBackColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(45, 50, 75) : Color.FromArgb(230, 235, 255);
                _gridOrders.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
                _gridOrders.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TableHeaderBg;
                _gridOrders.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
                _gridOrders.GridColor = ThemeManager.CardBorder;
            }

            Invalidate(true);
        }

        private void BuildLayout()
        {
            // 1. Sidebar (Left: 220px)
            BuildSidebar();

            // 2. Header (Top: 75px)
            BuildHeader();

            // 3. Main Content (Scrollable Flow/Panels)
            BuildMainContent();
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
            var brandBox = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };
            brandBox.Paint += (s, e) =>
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
            _sidebarPanel.Controls.Add(brandBox);

            // Menu Items Container
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 15, 0, 0)
            };
            _sidebarPanel.Controls.Add(menuPanel);
            menuPanel.BringToFront();

            string[] menuItems = { "Analytics", "Products", "Messages", "Customers", "Settings", "Sign Out" };
            string[] menuIcons = { "📊", "🛍️", "✉️", "👥", "⚙️", "🚪" };

            int btnY = 10;
            for (int i = 0; i < menuItems.Length; i++)
            {
                string title = menuItems[i];
                string icon = menuIcons[i];
                bool isActive = i == 0; // Analytics is active

                var btn = new Button
                {
                    Text = $"   {icon}   {title}",
                    TextAlign = ContentAlignment.MiddleLeft,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, isActive ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isActive ? Color.White : ThemeManager.TextSecondary,
                    BackColor = isActive ? ThemeManager.AccentBlue : Color.Transparent,
                    Size = new Size(190, 42),
                    Location = new Point(0, btnY),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;

                if (title == "Sign Out")
                {
                    btn.Click += (s, e) =>
                    {
                        var confirm = MessageBox.Show("Are you sure you want to sign out?", "Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (confirm == DialogResult.Yes)
                        {
                            Close();
                        }
                    };
                }

                menuPanel.Controls.Add(btn);
                btnY += 48;
            }

            // Need Help Card at bottom of sidebar
            var helpCard = new RoundedPanel
            {
                Dock = DockStyle.Bottom,
                Height = 135,
                BorderRadius = 14,
                CustomBackgroundColor = ThemeManager.CurrentTheme == AppTheme.Dark ? Color.FromArgb(28, 30, 42) : Color.FromArgb(238, 242, 250),
                CustomBorderColor = Color.Transparent
            };
            var lblHelp = new Label
            {
                Text = "Need help?\nFeel free to contact",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(14, 15),
                AutoSize = true
            };
            helpCard.Controls.Add(lblHelp);
            _themeTitleLabels.Add(lblHelp);

            var btnSupport = new ModernButton
            {
                Text = "Get support",
                BorderRadius = 8,
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(110, 32),
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Location = new Point(14, 80)
            };
            btnSupport.Click += (s, e) =>
            {
                MessageBox.Show("Support & Documentation:\n\nS TOM'S Platform — Project SBV1\nDeveloped by Hardik (Full-Stack)\nRepo: https://github.com/Saumya25-hub/TEAM-SOFT-SB1", "Customer Support", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            helpCard.Controls.Add(btnSupport);
            _sidebarPanel.Controls.Add(helpCard);
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
                Padding = new Padding(25, 15, 25, 15)
            };
            Controls.Add(_headerPanel);

            // Title "Analytics"
            _lblHeaderTitle = new Label
            {
                Text = "Analytics",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                AutoSize = true,
                Location = new Point(25, 18)
            };
            _headerPanel.Controls.Add(_lblHeaderTitle);

            // Date Range Pill
            _datePill = new RoundedPanel
            {
                BorderRadius = 10,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder,
                Size = new Size(190, 36),
                Location = new Point(180, 20)
            };
            var lblDate = new Label
            {
                Text = "01.08.2022 - 31.08.2022  📅",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                AutoSize = true,
                Location = new Point(14, 9)
            };
            _datePill.Controls.Add(lblDate);
            _themeSubLabels.Add(lblDate);
            _headerPanel.Controls.Add(_datePill);

            // User Profile on Right
            var profileContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                BackColor = Color.Transparent
            };
            _headerPanel.Controls.Add(profileContainer);

            // Theme Toggle
            _themeToggle = new ThemeToggle
            {
                Location = new Point(30, 22)
            };
            profileContainer.Controls.Add(_themeToggle);

            // User Avatar (Circle with Initials)
            var avatarBox = new Panel
            {
                Size = new Size(36, 36),
                Location = new Point(115, 18),
                BackColor = Color.Transparent
            };
            avatarBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var b = new SolidBrush(Color.FromArgb(234, 150, 100));
                g.FillEllipse(b, 0, 0, 35, 35);

                using var font = new Font("Segoe UI", 10f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_currentUser.AvatarInitials, font, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
            };
            profileContainer.Controls.Add(avatarBox);

            // User Profile Name
            _lblProfileName = new Label
            {
                Text = _currentUser.FullName,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                AutoSize = true,
                Location = new Point(160, 26)
            };
            profileContainer.Controls.Add(_lblProfileName);
        }
        #endregion

        #region Main Content
        private void BuildMainContent()
        {
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Background,
                AutoScroll = true,
                Padding = new Padding(25, 0, 25, 25)
            };
            Controls.Add(_contentPanel);
            _contentPanel.BringToFront();

            int cardWidth = 245;
            int cardHeight = 110;
            int gap = 20;

            // ==========================================
            // ROW 1: 4 Top Metric Cards
            // ==========================================
            // Card 1: Orders (201, +8.2%)
            var cardOrders = CreateMetricCard("Orders", "201", "↑ 8.2% since last month", true, "📦", 0, 0, cardWidth, cardHeight);
            _contentPanel.Controls.Add(cardOrders);

            // Card 2: Approved (36, +3.4%)
            var cardApproved = CreateMetricCard("Approved", "36", "↑ 3.4% since last month", true, "✅", cardWidth + gap, 0, cardWidth, cardHeight);
            _contentPanel.Controls.Add(cardApproved);

            // Card 3: Users (4.890) + Donut
            var cardUsers = CreateDonutMetricCard("Users", "4.890", new List<DonutSlice>
            {
                new("New", 63f, Color.FromArgb(245, 158, 11)),       // Yellow
                new("Returning", 25f, Color.FromArgb(234, 88, 12)),  // Orange
                new("Inactive", 12f, Color.FromArgb(100, 116, 139))  // Gray
            }, (cardWidth + gap) * 2, 0, cardWidth + 30, cardHeight);
            _contentPanel.Controls.Add(cardUsers);

            // Card 4: Subscriptions (1.201) + Donut
            var cardSubs = CreateDonutMetricCard("Subscriptions", "1.201", new List<DonutSlice>
            {
                new("Paid", 70f, ThemeManager.AccentBlue),            // Blue
                new("Trial", 30f, ThemeManager.AccentCyan)            // Sky Blue
            }, (cardWidth + gap) * 3 + 30, 0, cardWidth + 30, cardHeight);
            _contentPanel.Controls.Add(cardSubs);

            // ==========================================
            // ROW 2: 4 Sub-metrics (Month total, Revenue, Invoices, Funds)
            // ==========================================
            int row2Y = cardHeight + gap;
            var cardMonthTotal = CreateMetricCard("Month total", "25410", "↓ 0.2% since last month", false, "💲", 0, row2Y, cardWidth, cardHeight);
            _contentPanel.Controls.Add(cardMonthTotal);

            var cardRevenue = CreateMetricCard("Revenue", "1352", "↓ 1.2% since last month", false, "💳", cardWidth + gap, row2Y, cardWidth, cardHeight);
            _contentPanel.Controls.Add(cardRevenue);

            var cardInvoices = CreateCircleProgressCard("Paid Invoices", "$30256.23", "Current Financial Year", "+15%", ThemeManager.AccentPurple, (cardWidth + gap) * 2, row2Y, cardWidth + 30, cardHeight);
            _contentPanel.Controls.Add(cardInvoices);

            var cardFunds = CreateCircleProgressCard("Funds received", "$150256.23", "Current Financial Year", "+59%", ThemeManager.AccentGreen, (cardWidth + gap) * 3 + 30, row2Y, cardWidth + 30, cardHeight);
            _contentPanel.Controls.Add(cardFunds);

            // ==========================================
            // ROW 3: Large Charts (Sales Dynamics & User Activity)
            // ==========================================
            int row3Y = row2Y + cardHeight + gap;
            int chartCardWidth = 515;
            int chartCardHeight = 270;

            // Chart 1: Sales Dynamics (Bar Chart)
            var cardSales = new RoundedPanel
            {
                Location = new Point(0, row3Y),
                Size = new Size(chartCardWidth, chartCardHeight),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(cardSales);

            var lblSalesTitle = new Label
            {
                Text = "Sales dynamics",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(18, 14),
                AutoSize = true
            };
            cardSales.Controls.Add(lblSalesTitle);
            _themeTitleLabels.Add(lblSalesTitle);

            var lblYearSales = new Label
            {
                Text = "2021  ▼",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(chartCardWidth - 75, 16),
                AutoSize = true
            };
            cardSales.Controls.Add(lblYearSales);
            _themeSubLabels.Add(lblYearSales);

            var barChart = new BarChartControl
            {
                Location = new Point(10, 45),
                Size = new Size(chartCardWidth - 20, chartCardHeight - 55),
                DataPoints = _dataService.GetSalesDynamics()
            };
            cardSales.Controls.Add(barChart);
            _contentPanel.Controls.Add(cardSales);

            // Chart 2: Overall User Activity (Spline Chart)
            var cardActivity = new RoundedPanel
            {
                Location = new Point(chartCardWidth + gap, row3Y),
                Size = new Size(chartCardWidth + 60, chartCardHeight),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(cardActivity);

            var lblActTitle = new Label
            {
                Text = "Overall User Activity",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(18, 14),
                AutoSize = true
            };
            cardActivity.Controls.Add(lblActTitle);
            _themeTitleLabels.Add(lblActTitle);

            var lblYearAct = new Label
            {
                Text = "2021  ▼",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(chartCardWidth - 15, 16),
                AutoSize = true
            };
            cardActivity.Controls.Add(lblYearAct);
            _themeSubLabels.Add(lblYearAct);

            var splineChart = new SplineChartControl
            {
                Location = new Point(10, 45),
                Size = new Size(chartCardWidth + 40, chartCardHeight - 55),
                NormalizedPoints = _dataService.GetActivityCurve(),
                LineColor = Color.FromArgb(190, 80, 240)
            };
            cardActivity.Controls.Add(splineChart);
            _contentPanel.Controls.Add(cardActivity);

            // ==========================================
            // ROW 4: Customer Order Table
            // ==========================================
            int row4Y = row3Y + chartCardHeight + gap;
            int tableCardWidth = chartCardWidth + gap + chartCardWidth + 60;
            int tableCardHeight = 250;

            var cardTable = new RoundedPanel
            {
                Location = new Point(0, row4Y),
                Size = new Size(tableCardWidth, tableCardHeight),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(cardTable);

            var lblTableTitle = new Label
            {
                Text = "Customer order",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(18, 14),
                AutoSize = true
            };
            cardTable.Controls.Add(lblTableTitle);
            _themeTitleLabels.Add(lblTableTitle);

            // Table DataGridView
            _gridOrders = new DataGridView
            {
                Location = new Point(18, 45),
                Size = new Size(tableCardWidth - 36, tableCardHeight - 55),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9f),
                RowTemplate = { Height = 34 }
            };

            _gridOrders.Columns.Add("Profile", "Profile");
            _gridOrders.Columns.Add("Address", "Address");
            _gridOrders.Columns.Add("Date", "Date");
            _gridOrders.Columns.Add("Status", "Status");
            _gridOrders.Columns.Add("Price", "Price");

            _gridOrders.Columns[0].Width = 160;
            _gridOrders.Columns[1].Width = 200;
            _gridOrders.Columns[2].Width = 140;
            _gridOrders.Columns[3].Width = 140;
            _gridOrders.Columns[4].Width = 120;

            var orders = _dataService.GetRecentOrders();
            foreach (var o in orders)
            {
                _gridOrders.Rows.Add(o.ProfileName, o.Address, o.OrderDate, o.Status, o.Price);
            }

            // Cell formatting for Status column (Pill colors)
            _gridOrders.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 3 && e.Value != null && e.CellStyle != null)
                {
                    string status = e.Value.ToString() ?? "";
                    if (status == "Delivered")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129); // Green
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else if (status == "Processed")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11); // Yellow/Orange
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else if (status == "Cancelled")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(239, 68, 68); // Red
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                }
            };


            cardTable.Controls.Add(_gridOrders);
            _contentPanel.Controls.Add(cardTable);
        }
        #endregion

        #region Helper Metric Card Builders
        private RoundedPanel CreateMetricCard(string title, string value, string trend, bool isPositiveTrend, string icon, int x, int y, int w, int h)
        {
            var card = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(card);

            // Title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(16, 14),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            _themeSubLabels.Add(lblTitle);

            // Icon
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 11f),
                Location = new Point(w - 38, 12),
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

            // Value
            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                Location = new Point(14, 38),
                AutoSize = true
            };
            card.Controls.Add(lblVal);
            _themeTitleLabels.Add(lblVal);

            // Trend
            var lblTrend = new Label
            {
                Text = trend,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = isPositiveTrend ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68),
                Location = new Point(16, 80),
                AutoSize = true
            };
            card.Controls.Add(lblTrend);

            return card;
        }

        private RoundedPanel CreateDonutMetricCard(string title, string value, List<DonutSlice> slices, int x, int y, int w, int h)
        {
            var card = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(card);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(16, 14),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            _themeSubLabels.Add(lblTitle);

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                Location = new Point(14, 38),
                AutoSize = true
            };
            card.Controls.Add(lblVal);
            _themeTitleLabels.Add(lblVal);

            var donut = new DonutChartControl
            {
                Location = new Point(w - 170, 10),
                Size = new Size(160, 90),
                Slices = slices
            };
            card.Controls.Add(donut);

            return card;
        }

        private RoundedPanel CreateCircleProgressCard(string title, string value, string subtitle, string badgeText, Color badgeColor, int x, int y, int w, int h)
        {
            var card = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 14,
                BorderWidth = 1
            };
            _themeCards.Add(card);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(16, 14),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            _themeSubLabels.Add(lblTitle);

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Location = new Point(14, 38),
                AutoSize = true
            };
            card.Controls.Add(lblVal);
            _themeTitleLabels.Add(lblVal);

            var lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(16, 80),
                AutoSize = true
            };
            card.Controls.Add(lblSub);
            _themeSubLabels.Add(lblSub);

            // Mini badge on right
            var badge = new Panel
            {
                Size = new Size(42, 42),
                Location = new Point(w - 55, 30),
                BackColor = Color.Transparent
            };
            badge.Paint += (s, e) =>
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
            card.Controls.Add(badge);

            return card;
        }
        #endregion
    }
}
