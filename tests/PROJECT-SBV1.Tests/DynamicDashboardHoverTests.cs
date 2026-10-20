using System.Drawing;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;
using Xunit;

namespace PROJECT_SBV1.Tests
{
    public class DynamicDashboardHoverTests
    {
        [Fact]
        public void TestMetricDetailInfo_ModelProperties()
        {
            var info = new MetricDetailInfo
            {
                Title = "Customer Orders Performance",
                Icon = "📦",
                CurrentValue = "201 Orders",
                PreviousValue = "185 Orders",
                GrowthText = "↑ 8.2% since last month",
                IsPositiveGrowth = true,
                Category = "Sales & Fulfillment",
                Stats = new List<(string Label, string Value)>
                {
                    ("Delivered Orders", "164"),
                    ("Processed in Transit", "28")
                }
            };

            Assert.Equal("Customer Orders Performance", info.Title);
            Assert.Equal("📦", info.Icon);
            Assert.Equal("201 Orders", info.CurrentValue);
            Assert.Equal("185 Orders", info.PreviousValue);
            Assert.True(info.IsPositiveGrowth);
            Assert.Equal(2, info.Stats.Count);
        }

        [Fact]
        public void TestBarChartControl_SetDataAndProperties()
        {
            var chart = new BarChartControl();
            var data = new (string Label, int Value)[]
            {
                ("JAN", 120), ("FEB", 190), ("MAR", 140), ("APR", 260)
            };

            chart.SetData(data, 500);

            Assert.Equal(4, chart.DataPoints.Length);
            Assert.Equal(500, chart.MaxValue);
            Assert.Equal("JAN", chart.DataPoints[0].Label);
            Assert.Equal(120, chart.DataPoints[0].Value);
        }

        [Fact]
        public void TestSplineChartControl_SetDataAndPoints()
        {
            var chart = new SplineChartControl();
            float[] curve = { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

            chart.SetData(curve, 500);

            Assert.Equal(5, chart.NormalizedPoints.Length);
            Assert.Equal(500, chart.MaxValue);
            Assert.Equal(0.2f, chart.NormalizedPoints[0]);
            Assert.Equal(1.0f, chart.NormalizedPoints[4]);
        }

        [Fact]
        public void TestDonutChartControl_SlicesAndSetSlices()
        {
            var donut = new DonutChartControl();
            var slices = new List<DonutSlice>
            {
                new("Paid", 70f, Color.Blue),
                new("Trial", 30f, Color.Cyan)
            };

            donut.SetSlices(slices);

            Assert.Equal(2, donut.Slices.Count);
            Assert.Equal("Paid", donut.Slices[0].Label);
            Assert.Equal(70f, donut.Slices[0].Percentage);
            Assert.Equal("Trial", donut.Slices[1].Label);
            Assert.Equal(30f, donut.Slices[1].Percentage);
        }

        [Fact]
        public void TestRoundedPanel_HoverAndMetricInfoSupport()
        {
            var panel = new RoundedPanel
            {
                EnableHoverEffect = true,
                MetricInfo = new MetricDetailInfo
                {
                    Title = "Revenue Card",
                    CurrentValue = "$1,352"
                }
            };

            Assert.True(panel.EnableHoverEffect);
            Assert.NotNull(panel.MetricInfo);
            Assert.Equal("Revenue Card", panel.MetricInfo.Title);
        }

        [Fact]
        public void TestDatabaseService_DynamicSalesDynamics_MultiYear()
        {
            var db = DatabaseService.Instance;
            var points2021 = db.GetSalesDynamics(2021);
            var points2022 = db.GetSalesDynamics(2022);

            Assert.NotNull(points2021);
            Assert.Equal(12, points2021.Length);
            Assert.NotNull(points2022);
            Assert.Equal(12, points2022.Length);

            // 2022 values differ from 2021
            Assert.NotEqual(points2021[0].Value, points2022[0].Value);
        }

        [Fact]
        public void TestDatabaseService_DynamicActivityCurve_MultiYear()
        {
            var db = DatabaseService.Instance;
            var curve2021 = db.GetActivityCurve(2021);
            var curve2022 = db.GetActivityCurve(2022);

            Assert.NotNull(curve2021);
            Assert.Equal(12, curve2021.Length);
            Assert.NotNull(curve2022);
            Assert.Equal(12, curve2022.Length);
        }

        [Fact]
        public async Task TestDatabaseService_UpdateAndToggleOrderStatus()
        {
            var db = DatabaseService.Instance;
            var orders = await db.GetOrdersAsync();
            Assert.NotEmpty(orders);

            string initialStatus = orders[0].Status;
            string nextStatus = await db.ToggleOrderStatusAsync(orders[0].ProfileName);
            Assert.NotEqual(initialStatus, nextStatus);

            bool updated = await db.UpdateOrderStatusAsync(orders[0].ProfileName, "Delivered");
            Assert.True(updated);

            var updatedOrders = await db.GetOrdersAsync();
            var target = updatedOrders.FirstOrDefault(o => o.ProfileName == orders[0].ProfileName);
            Assert.NotNull(target);
            Assert.Equal("Delivered", target.Status);
        }

        [Fact]
        public async Task TestDatabaseService_AddOrderDynamic()
        {
            var db = DatabaseService.Instance;
            var newOrder = new CustomerOrder("Test Customer " + Guid.NewGuid().ToString("N")[..6], "Tokyo", "15.10.2026", "Delivered", "$1500");
            bool added = await db.AddOrderAsync(newOrder);
            Assert.True(added);

            var orders = await db.GetOrdersAsync("Tokyo");
            Assert.Contains(orders, o => o.ProfileName == newOrder.ProfileName);
        }

        [Fact]
        public void TestDarkScrollBar_DefaultValuesAndProperties()
        {
            var scrollBar = new DarkScrollBar
            {
                Minimum = 0,
                Maximum = 50,
                LargeChange = 10,
                Value = 15
            };

            Assert.Equal(0, scrollBar.Minimum);
            Assert.Equal(50, scrollBar.Maximum);
            Assert.Equal(10, scrollBar.LargeChange);
            Assert.Equal(15, scrollBar.Value);

            // Clamping check
            scrollBar.Value = 999;
            Assert.Equal(40, scrollBar.Value); // Maximum - LargeChange = 40
        }

        [Fact]
        public void TestDarkScrollBar_ThemeSwitching()
        {
            var scrollBar = new DarkScrollBar();
            scrollBar.ApplyTheme(Theme.AppTheme.Light);
            Assert.Equal(Color.FromArgb(241, 245, 249), scrollBar.TrackColor);
            Assert.Equal(Color.FromArgb(203, 213, 225), scrollBar.ThumbColor);

            scrollBar.ApplyTheme(Theme.AppTheme.Dark);
            Assert.Equal(Color.FromArgb(20, 22, 33), scrollBar.TrackColor);
            Assert.Equal(Color.FromArgb(65, 75, 110), scrollBar.ThumbColor);
        }

        [Fact]
        public void TestRoundedPanel_BorderRadiusProperty()
        {
            var panel = new RoundedPanel
            {
                BorderRadius = 12,
                BorderWidth = 1
            };

            Assert.Equal(12, panel.BorderRadius);
            Assert.Equal(1, panel.BorderWidth);
        }

        [Fact]
        public async Task TestDatabaseService_DateRangeFiltering()
        {
            var db = DatabaseService.Instance;
            var augustOrders = await db.GetOrdersAsync(startDate: new DateTime(2022, 8, 1), endDate: new DateTime(2022, 8, 31));
            Assert.NotEmpty(augustOrders);

            // Filter for only early August (01.08.2022 - 18.08.2022)
            var earlyAugust = await db.GetOrdersAsync(startDate: new DateTime(2022, 8, 1), endDate: new DateTime(2022, 8, 18));
            Assert.NotEmpty(earlyAugust);
            Assert.All(earlyAugust, o =>
            {
                Assert.True(DateTime.TryParseExact(o.OrderDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var dt));
                Assert.True(dt.Day <= 18);
            });

            // Filter for empty range (e.g. year 2025)
            var futureOrders = await db.GetOrdersAsync(startDate: new DateTime(2025, 1, 1), endDate: new DateTime(2025, 1, 31));
            Assert.Empty(futureOrders);
        }

        [Fact]
        public void TestDatabaseService_IndianCurrencyValues()
        {
            var db = DatabaseService.Instance;
            var summary = db.GetAnalyticsSummary();

            Assert.StartsWith("₹", summary.PaidInvoices);
            Assert.StartsWith("₹", summary.FundsReceived);
            Assert.Equal(1164084, summary.MonthTotal);
            Assert.Equal(1352, summary.Revenue);
        }

        [Fact]
        public void TestBarChartControl_DefaultIndianCurrency()
        {
            var chart = new BarChartControl();
            Assert.Equal("₹", chart.CurrencySymbol);
        }

        [Fact]
        public void TestThemeToggle_SmoothAnimationProperties()
        {
            var toggle = new ThemeToggle();
            Assert.Equal(64, toggle.Width);
            Assert.Equal(30, toggle.Height);
            Assert.False(toggle.IsAnimating);
            Assert.InRange(toggle.AnimationProgress, 0f, 1f);

            // Trigger smooth toggle
            toggle.ToggleWithSmoothAnimation();
            Assert.True(toggle.IsAnimating);
        }

        [Fact]
        public void TestThemeManager_ThemePersistenceSaveAndLoad()
        {
            // Save Light theme
            ThemeManager.SetTheme(Theme.AppTheme.Light);
            Assert.Equal(Theme.AppTheme.Light, ThemeManager.CurrentTheme);

            // Re-save Dark theme
            ThemeManager.SetTheme(Theme.AppTheme.Dark);
            Assert.Equal(Theme.AppTheme.Dark, ThemeManager.CurrentTheme);
        }

        [Fact]
        public void TestDashboardForm_SignOutState()
        {
            var form = new DashboardForm(new User("tester", "Tester Name", "Role", "TN"));
            Assert.False(form.IsSignedOut);
            form.SignOut();
            Assert.True(form.IsSignedOut);
        }

        [Fact]
        public void TestLoginForm_SetUsername()
        {
            var login = new LoginForm();
            login.SetUsername("Hardik");
            var auth = new AuthService();
            bool valid = auth.ValidateCredentials("hardik", "1234", out var user, out _);
            Assert.True(valid);
            Assert.NotNull(user);
            Assert.Equal("Hardik Jariwala", user.FullName);
        }

        [Fact]
        public void TestRoundedSearchBox_PropertiesAndTextDelegation()
        {
            var searchBox = new RoundedSearchBox
            {
                BorderRadius = 15,
                PlaceholderText = "Search customer, city..."
            };

            Assert.Equal(15, searchBox.BorderRadius);
            Assert.Equal("Search customer, city...", searchBox.PlaceholderText);
            Assert.NotNull(searchBox.InnerTextBox);

            searchBox.Text = "Alex Chicago";
            Assert.Equal("Alex Chicago", searchBox.Text);
            Assert.Equal("Alex Chicago", searchBox.InnerTextBox.Text);

            // Theme toggle check
            searchBox.ApplyTheme(AppTheme.Light);
            Assert.Equal(Color.FromArgb(248, 250, 252), searchBox.BoxBackgroundColor);
            searchBox.ApplyTheme(AppTheme.Dark);
            Assert.Equal(Color.FromArgb(22, 25, 37), searchBox.BoxBackgroundColor);

            // SearchTriggered event check
            bool triggered = false;
            searchBox.SearchTriggered += (s, e) => triggered = true;
            var searchIconBtn = searchBox.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "🔍");
            Assert.NotNull(searchIconBtn);
            searchIconBtn.PerformClick();
            Assert.True(triggered);
        }

        [Fact]
        public void TestModernButton_GradientAndGlowProperties()
        {
            var btn = new ModernButton
            {
                Text = "+ Order",
                BorderRadius = 15,
                UseGradient = true,
                HasGlowOnHover = true,
                GradientColorStart = Color.FromArgb(67, 97, 238),
                GradientColorEnd = Color.FromArgb(79, 70, 229)
            };

            Assert.True(btn.UseGradient);
            Assert.True(btn.HasGlowOnHover);
            Assert.Equal(15, btn.BorderRadius);
            Assert.Equal(Color.FromArgb(67, 97, 238), btn.GradientColorStart);
            Assert.Equal(Color.FromArgb(79, 70, 229), btn.GradientColorEnd);
        }

        [Fact]
        public void TestRoundedComboBox_ItemsAndSelection()
        {
            var cmb = new RoundedComboBox
            {
                BorderRadius = 15
            };
            cmb.Items.AddRange(new string[] { "All", "Delivered", "Processed", "Cancelled" });

            Assert.Equal(4, cmb.Items.Count);
            Assert.Equal(15, cmb.BorderRadius);
            Assert.Equal(-1, cmb.SelectedIndex);

            bool indexChanged = false;
            cmb.SelectedIndexChanged += (s, e) => indexChanged = true;

            cmb.SelectedIndex = 1;
            Assert.Equal(1, cmb.SelectedIndex);
            Assert.Equal("Delivered", cmb.SelectedItem);
            Assert.True(indexChanged);

            cmb.SelectedItem = "Cancelled";
            Assert.Equal(3, cmb.SelectedIndex);
            Assert.Equal("Cancelled", cmb.SelectedItem);

            // Theme switching
            cmb.ApplyTheme(AppTheme.Light);
            Assert.Equal(Color.FromArgb(248, 250, 252), cmb.BoxBackgroundColor);
            cmb.ApplyTheme(AppTheme.Dark);
            Assert.Equal(Color.FromArgb(22, 25, 37), cmb.BoxBackgroundColor);
        }

        [Fact]
        public void TestModernButton_OutlineAndTextHover()
        {
            var btn = new ModernButton
            {
                Text = "🔄 Refresh",
                IsOutlined = true,
                BorderRadius = 15,
                BorderOutlineColor = Color.FromArgb(70, 85, 130),
                TextHoverColor = Color.White
            };

            Assert.True(btn.IsOutlined);
            Assert.Equal(15, btn.BorderRadius);
            Assert.Equal(Color.FromArgb(70, 85, 130), btn.BorderOutlineColor);
            Assert.Equal(Color.White, btn.TextHoverColor);
        }

        [Fact]
        public void TestBillingRefreshButton_ThemeColorsAndText()
        {
            var btn = new ModernButton
            {
                Text = "🔄 Refresh",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(100, 34),
                BorderRadius = 15,
                IsOutlined = true,
                BorderOutlineColor = Color.FromArgb(203, 213, 225),
                NormalColor = Color.FromArgb(241, 245, 249),
                HoverColor = Color.FromArgb(226, 232, 240),
                PressedColor = Color.FromArgb(203, 213, 225),
                ForeColor = Color.FromArgb(51, 65, 85),
                TextHoverColor = Color.FromArgb(15, 23, 42),
                HasGlowOnHover = true,
                GlowColor = Color.FromArgb(99, 102, 241)
            };

            Assert.Equal("🔄 Refresh", btn.Text);
            Assert.True(btn.IsOutlined);
            Assert.Equal(100, btn.Width);
            Assert.Equal(34, btn.Height);
            Assert.Equal(Color.FromArgb(51, 65, 85), btn.ForeColor);
            Assert.Equal(Color.FromArgb(15, 23, 42), btn.TextHoverColor);
        }

        [Fact]
        public void TestRoundedComboBox_MouseWheelScrolling()
        {
            var cmb = new RoundedComboBox
            {
                BorderRadius = 15,
                EnableMouseWheelScroll = true
            };
            cmb.Items.AddRange(new string[] { "All", "Delivered", "Processed", "Cancelled" });
            cmb.SelectedIndex = 0; // Starts at "All"

            // Scroll down (delta < 0) -> should advance to index 1 ("Delivered")
            cmb.HandleWheelScroll(-120);
            Assert.Equal(1, cmb.SelectedIndex);
            Assert.Equal("Delivered", cmb.SelectedItem);

            // Scroll down again -> index 2 ("Processed")
            cmb.HandleWheelScroll(-120);
            Assert.Equal(2, cmb.SelectedIndex);
            Assert.Equal("Processed", cmb.SelectedItem);

            // Scroll down again -> index 3 ("Cancelled")
            cmb.HandleWheelScroll(-120);
            Assert.Equal(3, cmb.SelectedIndex);
            Assert.Equal("Cancelled", cmb.SelectedItem);

            // Scroll down again at end -> wraps around to index 0 ("All")
            cmb.HandleWheelScroll(-120);
            Assert.Equal(0, cmb.SelectedIndex);
            Assert.Equal("All", cmb.SelectedItem);

            // Scroll up (delta > 0) from 0 -> wraps around to index 3 ("Cancelled")
            cmb.HandleWheelScroll(120);
            Assert.Equal(3, cmb.SelectedIndex);
            Assert.Equal("Cancelled", cmb.SelectedItem);

            // Scroll up again -> index 2 ("Processed")
            cmb.HandleWheelScroll(120);
            Assert.Equal(2, cmb.SelectedIndex);
            Assert.Equal("Processed", cmb.SelectedItem);
        }

        [Fact]
        public void TestMetricHoverPopup_SizeAndNoCollision()
        {
            var popup = MetricHoverPopup.Instance;
            var anchor = new Control { Width = 200, Height = 100 };
            var info = new MetricDetailInfo
            {
                Title = "Total Outstanding Receivables",
                CurrentValue = "₹10,75,378.81",
                PreviousValue = "Awaiting payment",
                GrowthText = "0 Overdue invoices",
                Stats = new List<(string Label, string Value)>
                {
                    ("Total Due", "₹10,75,378.81"),
                    ("Overdue Records", "0 records"),
                    ("Settlement Status", "Zero outstanding balance on these records with long text"),
                    ("Recovery Action", "Send Payment Reminders")
                }
            };

            popup.ShowDetails(anchor, info);
            Assert.Equal(350, popup.Width);
            // Sized dynamically with at least 1 multi-line row
            Assert.True(popup.Height >= 280);
            popup.HideDetails();
        }

        [Fact]
        public void TestOrderDetailsDialog_InstantiationAndTheme()
        {
            var order = new CustomerOrder("Alex", "Unknown", "15.08.2022", "Cancelled", "₹1,200");
            using var dlg = new PROJECT_SBV1.Forms.OrderDetailsDialog(order);
            Assert.NotNull(dlg);
            Assert.Equal(680, dlg.Width);
            Assert.Equal(530, dlg.Height);
            Assert.Equal("Order Details — Alex", dlg.Text);

            // Find RoundedComboBox inside dialog
            var combo = dlg.Controls.OfType<RoundedPanel>()
                .SelectMany(p => p.Controls.OfType<RoundedComboBox>())
                .FirstOrDefault();
            Assert.NotNull(combo);
            Assert.Equal("Cancelled", combo.SelectedItem);
            Assert.Equal(3, combo.Items.Count);
        }

        [Fact]
        public void TestBillingGridWrapper_RoundedContainerProperties()
        {
            using var wrapper = new RoundedPanel
            {
                Size = new Size(1000, 400),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = Color.White,
                CustomBorderColor = Color.FromArgb(226, 232, 240)
            };

            Assert.Equal(12, wrapper.BorderRadius);
            Assert.Equal(1, wrapper.BorderWidth);
            Assert.Equal(1000, wrapper.Width);
            Assert.Equal(400, wrapper.Height);

            using var grid = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            wrapper.Controls.Add(grid);
            Assert.Single(wrapper.Controls);
            Assert.Same(grid, wrapper.Controls[0]);
        }

        [Fact]
        public void TestBillingInvoices_SortingLogic()
        {
            var invoices = new List<Invoice>
            {
                new Invoice { InvoiceId = 1, InvoiceNumber = "INV-2026-0003", CustomerName = "Vikram", GrandTotal = 77290m, BalanceDue = 77290m, Status = "Unpaid" },
                new Invoice { InvoiceId = 2, InvoiceNumber = "INV-2026-0001", CustomerName = "Aarav", GrandTotal = 91745m, BalanceDue = 0m, Status = "Paid" },
                new Invoice { InvoiceId = 3, InvoiceNumber = "INV-2026-0002", CustomerName = "Neha", GrandTotal = 57397m, BalanceDue = 27397m, Status = "Partially Paid" }
            };

            // Test Sort by InvoiceNo Ascending & Descending
            var sortedByNoAsc = invoices.OrderBy(i => i.InvoiceNumber).ToList();
            Assert.Equal("INV-2026-0001", sortedByNoAsc[0].InvoiceNumber);
            Assert.Equal("INV-2026-0003", sortedByNoAsc[2].InvoiceNumber);

            var sortedByNoDesc = invoices.OrderByDescending(i => i.InvoiceNumber).ToList();
            Assert.Equal("INV-2026-0003", sortedByNoDesc[0].InvoiceNumber);
            Assert.Equal("INV-2026-0001", sortedByNoDesc[2].InvoiceNumber);

            // Test Sort by Total Descending
            var sortedByTotalDesc = invoices.OrderByDescending(i => i.GrandTotal).ToList();
            Assert.Equal(91745m, sortedByTotalDesc[0].GrandTotal);
            Assert.Equal(57397m, sortedByTotalDesc[2].GrandTotal);

            // Test Sort by Balance Due Descending
            var sortedByBalance = invoices.OrderByDescending(i => i.BalanceDue).ToList();
            Assert.Equal(77290m, sortedByBalance[0].BalanceDue);
            Assert.Equal(0m, sortedByBalance[2].BalanceDue);
        }

        [Fact]
        public void TestModernButton_BootstrapStyle()
        {
            using var btn = new ModernButton
            {
                Text = "🔄 Refresh",
                Size = new Size(100, 34),
                BorderRadius = 15,
                IsOutlined = true,
                BorderOutlineColor = Color.FromArgb(203, 213, 225),
                HasGlowOnHover = true,
                GlowColor = Color.FromArgb(99, 102, 241)
            };

            Assert.True(btn.IsOutlined);
            Assert.True(btn.HasGlowOnHover);
            Assert.Equal(15, btn.BorderRadius);
            Assert.Equal(100, btn.Width);
            Assert.Equal(34, btn.Height);
            Assert.Equal(Color.FromArgb(99, 102, 241), btn.GlowColor);
        }

        [Fact]
        public void TestBillingGridWrapper_EmbeddedGridAndCustomScrollbar()
        {
            using var wrapper = new RoundedPanel
            {
                Size = new Size(1000, 400),
                BorderRadius = 12,
                BorderWidth = 1,
                CustomBackgroundColor = Color.White,
                CustomBorderColor = Color.FromArgb(226, 232, 240)
            };

            using var grid = new DataGridView
            {
                Location = new Point(1, 1),
                Size = new Size(wrapper.Width - 11, wrapper.Height - 2),
                ScrollBars = ScrollBars.None
            };
            wrapper.Controls.Add(grid);

            using var scrollBar = new DarkScrollBar
            {
                Location = new Point(wrapper.Width - 9, 1),
                Size = new Size(8, wrapper.Height - 2)
            };
            scrollBar.AttachToGrid(grid);
            wrapper.Controls.Add(scrollBar);

            Assert.Equal(2, wrapper.Controls.Count);
            Assert.Same(grid, wrapper.Controls[0]);
            Assert.Same(scrollBar, wrapper.Controls[1]);
            Assert.Equal(ScrollBars.None, grid.ScrollBars);
        }
    }
}



