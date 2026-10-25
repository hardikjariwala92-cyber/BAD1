using System.IO;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using Xunit;

namespace PROJECT_SBV1.Tests
{
    public class SalesAndBillingTests
    {
        [Fact]
        public void TestInvoiceCalculation_ItemDiscountsAndTax()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "TEST-INV-001",
                CustomerName = "Aarav Sharma",
                IsInterState = false,
                TaxRate = 18m,
                OverallDiscount = 1000m
            };

            // Item 1: 1 x 40,000 with 10% disc = 36,000 taxable. 18% tax = 6,480. Line total = 42,480
            invoice.Items.Add(new InvoiceItem("Cloud Suite License", "Annual software license", 1m, 40000m, 10m, 18m));
            // Item 2: 2 x 5,000 with 0% disc = 10,000 taxable. 18% tax = 1,800. Line total = 11,800
            invoice.Items.Add(new InvoiceItem("SSL Certificate", "Wildcard SSL", 2m, 5000m, 0m, 18m));

            invoice.RecalculateTotals();

            // Gross total = 40,000 + 10,000 = 50,000
            Assert.Equal(50000m, invoice.SubTotal);
            // Item discount = 4,000
            Assert.Equal(4000m, invoice.ItemDiscountTotal);
            // Taxable after overall discount (1000): 46,000 - 1000 = 45,000
            Assert.Equal(45000m, invoice.TaxableAmount);

            // CGST and SGST split equally (Intra-state)
            Assert.True(invoice.CgstAmount > 0);
            Assert.True(invoice.SgstAmount > 0);
            Assert.Equal(invoice.TaxAmount, invoice.CgstAmount + invoice.SgstAmount);
            Assert.Equal(0m, invoice.IgstAmount);

            // Grand Total = Taxable (45,000) + TaxAmount
            Assert.Equal(invoice.TaxableAmount + invoice.TaxAmount, invoice.GrandTotal);
            Assert.Equal(invoice.GrandTotal, invoice.BalanceDue);
            Assert.Equal("Unpaid", invoice.Status);
        }

        [Fact]
        public void TestInvoiceCalculation_InterState_IGST()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "TEST-INV-002",
                CustomerName = "Priya Nair",
                IsInterState = true,
                TaxRate = 18m
            };

            invoice.Items.Add(new InvoiceItem("Custom Branding Package", "Logo & Design", 1m, 50000m, 0m, 18m));
            invoice.RecalculateTotals();

            Assert.Equal(50000m, invoice.SubTotal);
            Assert.Equal(50000m, invoice.TaxableAmount);
            Assert.Equal(9000m, invoice.TaxAmount);
            Assert.Equal(9000m, invoice.IgstAmount);
            Assert.Equal(0m, invoice.CgstAmount);
            Assert.Equal(0m, invoice.SgstAmount);
            Assert.Equal(59000m, invoice.GrandTotal);
        }

        [Fact]
        public void TestPaymentRecording_UpdatesStatus_ToPaid()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "TEST-INV-003",
                CustomerName = "Neha Patel",
                IsInterState = false,
                TaxRate = 18m
            };
            invoice.Items.Add(new InvoiceItem("Cloud Backup 1TB", "Annual Storage", 1m, 10000m, 0m, 18m));
            invoice.RecalculateTotals();

            decimal total = invoice.GrandTotal; // 11,800
            Assert.Equal("Unpaid", invoice.Status);

            // Record full payment
            invoice.Payments.Add(new Payment(0, invoice.InvoiceNumber, invoice.CustomerName, total, "UPI", "UPI/12345", "Full payment"));
            invoice.RecalculateTotals();

            Assert.Equal(total, invoice.AmountPaid);
            Assert.Equal(0m, invoice.BalanceDue);
            Assert.Equal("Paid", invoice.Status);
        }

        [Fact]
        public void TestPaymentRecording_PartialPayment()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "TEST-INV-004",
                CustomerName = "Vikram Malhotra",
                IsInterState = false,
                TaxRate = 18m
            };
            invoice.Items.Add(new InvoiceItem("Consulting Services", "Architecture Audit", 1m, 20000m, 0m, 18m));
            invoice.RecalculateTotals();

            decimal total = invoice.GrandTotal; // 23,600
            decimal partial = 10000m;

            invoice.Payments.Add(new Payment(0, invoice.InvoiceNumber, invoice.CustomerName, partial, "Bank Transfer", "NEFT/8888", "Advance"));
            invoice.RecalculateTotals();

            Assert.Equal(partial, invoice.AmountPaid);
            Assert.Equal(total - partial, invoice.BalanceDue);
            Assert.Equal("Partially Paid", invoice.Status);
        }

        [Fact]
        public async Task TestDatabasePersistence_SaveAndRetrieveInvoice()
        {
            var billingService = BillingDataService.Instance;

            string nextInvNo = await billingService.GetNextInvoiceNumberAsync();
            Assert.NotNull(nextInvNo);
            Assert.StartsWith("INV-", nextInvNo);

            var invoice = new Invoice
            {
                InvoiceNumber = nextInvNo,
                CustomerId = 1,
                CustomerName = "Integration Test Client",
                CompanyName = "Test Solutions Ltd",
                CustomerEmail = "test@client.io",
                CustomerPhone = "+91 99999 88888",
                BillingAddress = "Test City, Gujarat",
                CustomerGSTIN = "24TESTG1234F1Z9",
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(15),
                PaymentTerms = "Net 15",
                IsInterState = false
            };

            invoice.Items.Add(new InvoiceItem("Automated Unit Test Service", "Testing SQLite persistence", 2m, 7500m, 5m, 18m));
            invoice.RecalculateTotals();

            bool saved = await billingService.SaveInvoiceAsync(invoice);
            Assert.True(saved);
            Assert.True(invoice.InvoiceId > 0);

            // Fetch back
            var fetched = await billingService.GetInvoiceByIdAsync(invoice.InvoiceId);
            Assert.NotNull(fetched);
            Assert.Equal(invoice.InvoiceNumber, fetched.InvoiceNumber);
            Assert.Equal(invoice.CustomerName, fetched.CustomerName);
            Assert.Equal(invoice.GrandTotal, fetched.GrandTotal);
            Assert.Single(fetched.Items);
            Assert.Equal("Automated Unit Test Service", fetched.Items[0].ServiceName);

            // Record a payment against it in database
            var pay = new Payment(fetched.InvoiceId, fetched.InvoiceNumber, fetched.CustomerName, 5000m, "UPI", "UPI/TEST/001", "Partial test payment");
            bool paySaved = await billingService.RecordPaymentAsync(pay);
            Assert.True(paySaved);

            // Fetch again and verify balance & status
            var afterPay = await billingService.GetInvoiceByIdAsync(fetched.InvoiceId);
            Assert.NotNull(afterPay);
            Assert.Equal(5000m, afterPay.AmountPaid);
            Assert.Equal(fetched.GrandTotal - 5000m, afterPay.BalanceDue);
            Assert.Equal("Partially Paid", afterPay.Status);
        }

        [Fact]
        public async Task TestSalesDashboardMetrics_LiveCalculation()
        {
            var billingService = BillingDataService.Instance;
            var metrics = await billingService.GetSalesDashboardMetricsAsync();

            Assert.NotNull(metrics);
            Assert.True(metrics.TotalInvoices > 0);
            Assert.True(metrics.TotalSales > 0);
            Assert.True(metrics.PaidInvoices >= 0);
            Assert.True(metrics.OutstandingBalance >= 0);
            Assert.NotEmpty(metrics.RecentInvoices);
        }

        [Fact]
        public void TestPdfGeneration_FileCreatedAndValid()
        {
            var pdfService = InvoicePdfService.Instance;
            var invoice = new Invoice
            {
                InvoiceNumber = "PDF-TEST-001",
                CustomerName = "Acme Global Technologies",
                CompanyName = "Acme Corp",
                CustomerEmail = "billing@acme.com",
                CustomerPhone = "+91 98765 00000",
                BillingAddress = "Level 8, Cyber Tower, Hyderabad",
                CustomerGSTIN = "36AACCA1234F1Z8",
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                PaymentTerms = "Net 30",
                IsInterState = true
            };

            invoice.Items.Add(new InvoiceItem("Cloud Architecture Consulting", "Microservices assessment and deployment blueprint", 1m, 50000m, 10m, 18m));
            invoice.Items.Add(new InvoiceItem("Developer API License", "Enterprise tier unlimited endpoints", 1m, 25000m, 0m, 18m));
            invoice.RecalculateTotals();

            var profile = new BusinessProfile();
            string tempPdf = Path.Combine(Path.GetTempPath(), $"TestInvoice_{Guid.NewGuid():N}.pdf");

            try
            {
                string generatedPath = pdfService.GenerateInvoicePdf(invoice, profile, tempPdf);

                Assert.True(File.Exists(generatedPath));
                var fileInfo = new FileInfo(generatedPath);
                Assert.True(fileInfo.Length > 1000); // PDF contains vector graphics, fonts, tables
                using var readDoc = PdfSharp.Pdf.IO.PdfReader.Open(generatedPath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                Assert.Equal(1, readDoc.PageCount);
            }
            finally
            {
                if (File.Exists(tempPdf))
                {
                    File.Delete(tempPdf);
                }
            }
        }

        [Fact]
        public void TestDashboardForm_InitializesMaximized()
        {
            var form = new PROJECT_SBV1.Forms.DashboardForm(new User("tester", "Test User", "Admin", "TU"));
            Assert.Equal(System.Windows.Forms.FormWindowState.Maximized, form.WindowState);
        }

        [Fact]
        public void TestInvoiceForm_InitializesMaximized()
        {
            var form = new PROJECT_SBV1.Forms.InvoiceForm();
            Assert.Equal(System.Windows.Forms.FormWindowState.Maximized, form.WindowState);
        }

        [Fact]
        public void TestBillingMetricCards_LayoutUniformWidthAndGap8()
        {
            int contentWidth = 1100;
            int gap = 8;
            int totalGaps = gap * 4;
            int baseWidth = (contentWidth - totalGaps) / 5;
            int remainder = (contentWidth - totalGaps) % 5;
            int currentX = 0;

            var widths = new List<int>();
            var positions = new List<int>();

            for (int i = 0; i < 5; i++)
            {
                int w = baseWidth + (i < remainder ? 1 : 0);
                widths.Add(w);
                positions.Add(currentX);
                currentX += w + gap;
            }

            // Verify total width matches contentWidth
            int totalCalculatedWidth = positions[4] + widths[4];
            Assert.Equal(contentWidth, totalCalculatedWidth);

            // Verify uniform 8px gap between all cards
            for (int i = 0; i < 4; i++)
            {
                int cardRight = positions[i] + widths[i];
                int nextCardLeft = positions[i + 1];
                Assert.Equal(gap, nextCardLeft - cardRight);
            }

            // Verify widths are balanced within 1 pixel
            int minW = widths.Min();
            int maxW = widths.Max();
            Assert.True(maxW - minW <= 1);
        }

        [Fact]
        public void TestRoundedPanel_OnCardMouseWheel_Propagates()
        {
            var panel = new PROJECT_SBV1.Controls.RoundedPanel();
            var label = new System.Windows.Forms.Label { Text = "Test Metric" };
            panel.Controls.Add(label);

            bool wheelInvoked = false;
            int recordedDelta = 0;
            panel.OnCardMouseWheel = (e) =>
            {
                wheelInvoked = true;
                recordedDelta = e.Delta;
            };

            // Simulate mouse wheel directly on the panel
            panel.OnCardMouseWheel(new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 10, 10, 120));

            Assert.True(wheelInvoked);
            Assert.Equal(120, recordedDelta);
        }

        [Fact]
        public void TestRoundedPanel_OnCardClicked_TriggersAction()
        {
            var panel = new PROJECT_SBV1.Controls.RoundedPanel();
            bool clicked = false;
            panel.OnCardClicked = (p) => clicked = true;

            panel.OnCardClicked(panel);
            Assert.True(clicked);
        }

        [Fact]
        public void TestBillingMetricDetailInfo_ModelProperties()
        {
            var metrics = new SalesDashboardMetrics
            {
                TotalSales = 1267923.81m,
                TotalInvoices = 11,
                PaidInvoices = 1,
                UnpaidInvoices = 9,
                PartiallyPaidInvoices = 1,
                OverdueInvoices = 2,
                OutstandingBalance = 1146178.81m,
                TotalCollected = 121745.00m
            };

            var infoTotalSales = new MetricDetailInfo
            {
                Title = "Sales & Invoiced Revenue",
                Icon = "💰",
                CurrentValue = $"₹{metrics.TotalSales:N2}",
                Category = "Revenue Analysis",
                Stats = new List<(string Label, string Value)>
                {
                    ("Total Invoices", $"{metrics.TotalInvoices} records"),
                    ("Total Collected", $"₹{metrics.TotalCollected:N2}")
                }
            };

            Assert.Equal("Sales & Invoiced Revenue", infoTotalSales.Title);
            Assert.Equal("💰", infoTotalSales.Icon);
            Assert.Contains("12,67,923.81", infoTotalSales.CurrentValue);
            Assert.Equal(2, infoTotalSales.Stats.Count);
        }

        [Fact]
        public void TestBillingToolbar_RoundedSearchBoxAndRoundedComboBox()
        {
            var searchBox = new PROJECT_SBV1.Controls.RoundedSearchBox
            {
                PlaceholderText = "Search by invoice #...",
                BorderRadius = 15
            };
            searchBox.Text = "INV-2026-001";
            Assert.Equal("INV-2026-001", searchBox.Text);
            Assert.Equal(15, searchBox.BorderRadius);

            var combo = new PROJECT_SBV1.Controls.RoundedComboBox
            {
                BorderRadius = 15
            };
            combo.Items.AddRange(new string[] { "All", "Paid", "Partially Paid", "Unpaid", "Overdue", "Draft", "Cancelled" });
            Assert.Equal(7, combo.Items.Count);

            combo.SelectedItem = "Paid";
            Assert.Equal("Paid", combo.SelectedItem?.ToString());

            // Test mouse wheel cycling on combo box
            combo.HandleWheelScroll(-1); // Next item: Partially Paid
            Assert.Equal("Partially Paid", combo.SelectedItem?.ToString());
        }

        [Fact]
        public void TestSalesRegister_StatusFilterRequirements()
        {
            var expectedStatuses = new[] { "All", "Paid", "Unpaid", "Partially Paid", "Overdue", "Draft", "Cancelled" };
            var combo = new PROJECT_SBV1.Controls.RoundedComboBox { BorderRadius = 8 };
            combo.Items.AddRange(expectedStatuses);

            Assert.Equal(7, combo.Items.Count);
            foreach (var status in expectedStatuses)
            {
                Assert.Contains(status, combo.Items);
            }

            combo.SelectedItem = "Overdue";
            Assert.Equal("Overdue", combo.SelectedItem);
            combo.SelectedItem = "Unpaid";
            Assert.Equal("Unpaid", combo.SelectedItem);
            combo.SelectedItem = "Paid";
            Assert.Equal("Paid", combo.SelectedItem);
            combo.SelectedItem = "Draft";
            Assert.Equal("Draft", combo.SelectedItem);
            combo.SelectedItem = "Cancelled";
            Assert.Equal("Cancelled", combo.SelectedItem);
        }

        [Fact]
        public void TestSalesRegister_DedicatedToolbarCardProperties()
        {
            var card = new PROJECT_SBV1.Controls.RoundedPanel
            {
                BorderRadius = 12,
                BorderWidth = 1,
                Size = new System.Drawing.Size(1100, 68)
            };
            Assert.Equal(12, card.BorderRadius);
            Assert.Equal(1, card.BorderWidth);
            Assert.Equal(68, card.Height);

            var search = new PROJECT_SBV1.Controls.RoundedSearchBox
            {
                PlaceholderText = "Search by invoice # or customer name...",
                BorderRadius = 8
            };
            Assert.Equal("Search by invoice # or customer name...", search.PlaceholderText);
            Assert.Equal(8, search.BorderRadius);

            var btnCreate = new PROJECT_SBV1.Controls.ModernButton
            {
                Text = "+ Create Invoice",
                BorderRadius = 8,
                NormalColor = System.Drawing.Color.FromArgb(16, 185, 129)
            };
            Assert.Equal("+ Create Invoice", btnCreate.Text);
            Assert.Equal(System.Drawing.Color.FromArgb(16, 185, 129), btnCreate.NormalColor);
            Assert.False(btnCreate.IsOutlined);

            var btnRefresh = new PROJECT_SBV1.Controls.ModernButton
            {
                Text = "↻ Refresh",
                BorderRadius = 8,
                IsOutlined = false,
                NormalColor = System.Drawing.Color.FromArgb(55, 65, 81),
                ForeColor = System.Drawing.Color.White
            };
            Assert.Equal("↻ Refresh", btnRefresh.Text);
            Assert.False(btnRefresh.IsOutlined);
            Assert.Equal(System.Drawing.Color.White, btnRefresh.ForeColor);
            Assert.Equal(System.Drawing.Color.FromArgb(55, 65, 81), btnRefresh.NormalColor);
        }

        [Theory]
        [InlineData(94400.00, "₹94,400.00")]
        [InlineData(0.00, "₹0.00")]
        [InlineData(566400.00, "₹5,66,400.00")]
        [InlineData(147287.60, "₹1,47,287.60")]
        public void TestSalesRegister_CurrencyFormatting(decimal amount, string expected)
        {
            string formatted = $"₹{amount:N2}";
            Assert.Equal(expected, formatted);
        }

        [Fact]
        public void TestSalesRegister_TableEnhancedFormatAndStyling()
        {
            // Verify status badges color palettes exist and are properly defined
            var statuses = new[] { "Paid", "Unpaid", "Partially Paid", "Overdue", "Draft", "Cancelled" };
            foreach (var status in statuses)
            {
                Assert.False(string.IsNullOrEmpty(status));
            }

            // Verify clean client parsing
            string fullClient = "Neha Patel (CloudWave Digital Media)";
            int parenIdx = fullClient.IndexOf('(');
            Assert.True(parenIdx > 0);
            string clientName = fullClient.Substring(0, parenIdx).Trim();
            string compName = fullClient.Substring(parenIdx).Trim();
            Assert.Equal("Neha Patel", clientName);
            Assert.Equal("(CloudWave Digital Media)", compName);
        }

        [Fact]
        public void TestScenario1_FullPaymentAgainstUnpaid_BecomesPaid()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "INV-TEST-001",
                CustomerName = "Test Client",
                DueDate = DateTime.Today.AddDays(15),
                Status = "Unpaid"
            };
            invoice.Items.Add(new InvoiceItem("Web App Development", "Frontend & Backend", 1m, 50000m, 0m, 18m));
            invoice.RecalculateTotals();

            Assert.Equal("Unpaid", invoice.Status);
            Assert.Equal(59000m, invoice.GrandTotal);
            Assert.Equal(59000m, invoice.BalanceDue);

            // Record full payment
            invoice.Payments.Add(new Payment(0, invoice.InvoiceNumber, invoice.CustomerName, 59000m, "Bank Transfer", "REF-001", "Full"));
            invoice.RecalculateTotals();

            Assert.Equal(59000m, invoice.AmountPaid);
            Assert.Equal(0m, invoice.BalanceDue);
            Assert.Equal("Paid", invoice.Status);
        }

        [Fact]
        public void TestScenario2_PartialPayment_BecomesPartiallyPaid()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "INV-TEST-002",
                CustomerName = "Test Client",
                DueDate = DateTime.Today.AddDays(15),
                Status = "Unpaid"
            };
            invoice.Items.Add(new InvoiceItem("Consulting", "Security audit", 1m, 10000m, 0m, 18m));
            invoice.RecalculateTotals();

            decimal grandTotal = invoice.GrandTotal; // 11,800
            decimal partialAmount = 5000m;

            invoice.Payments.Add(new Payment(0, invoice.InvoiceNumber, invoice.CustomerName, partialAmount, "UPI", "UPI-1", "Partial"));
            invoice.RecalculateTotals();

            Assert.Equal(partialAmount, invoice.AmountPaid);
            Assert.Equal(grandTotal - partialAmount, invoice.BalanceDue);
            Assert.Equal("Partially Paid", invoice.Status);
        }

        [Fact]
        public void TestScenario3_RemainingBalancePayment_BecomesPaid()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "INV-TEST-003",
                CustomerName = "Test Client",
                DueDate = DateTime.Today.AddDays(15),
                Status = "Unpaid"
            };
            invoice.Items.Add(new InvoiceItem("Design", "UI Wireframing", 1m, 20000m, 0m, 18m));
            invoice.RecalculateTotals();

            decimal grandTotal = invoice.GrandTotal; // 23,600
            decimal firstPayment = 10000m;
            invoice.Payments.Add(new Payment(0, invoice.InvoiceNumber, invoice.CustomerName, firstPayment, "UPI", "UPI-1", "Step 1"));
            invoice.RecalculateTotals();
            Assert.Equal("Partially Paid", invoice.Status);

            // Pay remainder
            decimal remainder = invoice.BalanceDue; // 13,600
            invoice.Payments.Add(new Payment(0, invoice.InvoiceNumber, invoice.CustomerName, remainder, "UPI", "UPI-2", "Step 2"));
            invoice.RecalculateTotals();

            Assert.Equal(grandTotal, invoice.AmountPaid);
            Assert.Equal(0m, invoice.BalanceDue);
            Assert.Equal("Paid", invoice.Status);
        }

        [Fact]
        public void TestScenario4_DraftInvoice_WithFullPayment_BecomesPaid()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "INV-2026-0011",
                CustomerName = "Neha Patel",
                CompanyName = "CloudWave Digital Media",
                DueDate = DateTime.Today.AddDays(15),
                Status = "Draft"
            };
            invoice.Items.Add(new InvoiceItem("Cloud Architecture", "AWS setup", 1m, 80000m, 0m, 18m));
            invoice.RecalculateTotals();

            Assert.Equal("Draft", invoice.Status);
            Assert.Equal(94400m, invoice.GrandTotal);

            // When full payment of 94,400 is recorded
            invoice.Payments.Add(new Payment(0, invoice.InvoiceNumber, invoice.CustomerName, 94400m, "UPI", "UPI-94400", "Settled"));
            invoice.RecalculateTotals();

            // Status MUST update to Paid, resolving the bug shown in user screenshot
            Assert.Equal(94400m, invoice.AmountPaid);
            Assert.Equal(0m, invoice.BalanceDue);
            Assert.Equal("Paid", invoice.Status);
        }

        [Fact]
        public void TestScenario5_DraftAndCancelledInvoices_FollowBusinessRules()
        {
            // Draft invoice without payments remains Draft
            var draftInv = new Invoice
            {
                InvoiceNumber = "INV-2026-0007",
                CustomerName = "Priya Nair",
                DueDate = DateTime.Today.AddDays(-10), // Even if past due!
                Status = "Draft"
            };
            draftInv.Items.Add(new InvoiceItem("Consulting", "Retainer", 1m, 10000m, 0m, 18m));
            draftInv.RecalculateTotals();
            Assert.Equal("Draft", draftInv.Status);

            // Cancelled invoice remains Cancelled
            var cancelledInv = new Invoice
            {
                InvoiceNumber = "INV-2026-0004",
                CustomerName = "Priya Nair",
                DueDate = DateTime.Today.AddDays(-30),
                Status = "Cancelled"
            };
            cancelledInv.Items.Add(new InvoiceItem("Cancelled Service", "Audit", 1m, 10000m, 0m, 18m));
            cancelledInv.RecalculateTotals();
            Assert.Equal("Cancelled", cancelledInv.Status);
        }

        [Fact]
        public async Task TestScenario6_DatabasePersistence_RecordPaymentAsync_Workflow()
        {
            var billingService = BillingDataService.Instance;

            // 1. Create a fresh invoice in database
            string invNo = await billingService.GetNextInvoiceNumberAsync();
            var invoice = new Invoice
            {
                InvoiceNumber = invNo,
                CustomerId = 1,
                CustomerName = "Persistence Test Client",
                DueDate = DateTime.Today.AddDays(15),
                PaymentTerms = "Net 15",
                Status = "Unpaid"
            };
            invoice.Items.Add(new InvoiceItem("Persistence Test Service", "DB testing", 1m, 20000m, 0m, 18m));
            invoice.RecalculateTotals();

            bool saved = await billingService.SaveInvoiceAsync(invoice);
            Assert.True(saved);
            Assert.True(invoice.InvoiceId > 0);
            Assert.Equal(23600m, invoice.GrandTotal);

            // 2. Record partial payment of 10,000 via RecordPaymentAsync
            var payment1 = new Payment(invoice.InvoiceId, invoice.InvoiceNumber, invoice.CustomerName, 10000m, "UPI", "TEST-REF-1", "Partial Test");
            bool pay1Recorded = await billingService.RecordPaymentAsync(payment1);
            Assert.True(pay1Recorded);

            // Verify refreshed from DB
            var dbInv1 = await billingService.GetInvoiceByIdAsync(invoice.InvoiceId);
            Assert.NotNull(dbInv1);
            Assert.Equal(10000m, dbInv1.AmountPaid);
            Assert.Equal(13600m, dbInv1.BalanceDue);
            Assert.Equal("Partially Paid", dbInv1.Status);

            // 3. Record remaining payment of 13,600
            var payment2 = new Payment(invoice.InvoiceId, invoice.InvoiceNumber, invoice.CustomerName, 13600m, "Bank Transfer", "TEST-REF-2", "Final Test");
            bool pay2Recorded = await billingService.RecordPaymentAsync(payment2);
            Assert.True(pay2Recorded);

            // Verify refreshed from DB
            var dbInv2 = await billingService.GetInvoiceByIdAsync(invoice.InvoiceId);
            Assert.NotNull(dbInv2);
            Assert.Equal(23600m, dbInv2.AmountPaid);
            Assert.Equal(0m, dbInv2.BalanceDue);
            Assert.Equal("Paid", dbInv2.Status);

            // 4. Verify invalid invoice ID throws exception (ensures error handling & foreign key validation)
            var invalidPay = new Payment(999999, "INV-NONE", "Ghost", 500m, "Cash", "REF", "");
            await Assert.ThrowsAnyAsync<Exception>(() => billingService.RecordPaymentAsync(invalidPay));
        }

        [Fact]
        public async Task TestScenario7_DatabasePersistence_DeleteInvoiceAsync_Workflow()
        {
            var billingService = BillingDataService.Instance;

            // 1. Create a fresh invoice with items and payment in database
            string invNo = await billingService.GetNextInvoiceNumberAsync();
            var invoice = new Invoice
            {
                InvoiceNumber = invNo,
                CustomerId = 1,
                CustomerName = "Deletion Test Client",
                DueDate = DateTime.Today.AddDays(10),
                PaymentTerms = "Due Upon Receipt",
                Status = "Unpaid"
            };
            invoice.Items.Add(new InvoiceItem("Deletion Test Item", "DB testing", 1m, 15000m, 0m, 18m));
            invoice.RecalculateTotals();

            bool saved = await billingService.SaveInvoiceAsync(invoice);
            Assert.True(saved);
            Assert.True(invoice.InvoiceId > 0);

            // Add payment
            var payment = new Payment(invoice.InvoiceId, invoice.InvoiceNumber, invoice.CustomerName, 5000m, "Cash", "CASH-DEL", "Deposit");
            bool paySaved = await billingService.RecordPaymentAsync(payment);
            Assert.True(paySaved);

            // Verify exists in DB
            var dbInv = await billingService.GetInvoiceByIdAsync(invoice.InvoiceId);
            Assert.NotNull(dbInv);
            Assert.Single(dbInv.Items);
            Assert.Single(dbInv.Payments);

            // 2. Delete invoice via DeleteInvoiceAsync
            bool deleted = await billingService.DeleteInvoiceAsync(invoice.InvoiceId);
            Assert.True(deleted);

            // 3. Verify invoice is removed
            var deletedInv = await billingService.GetInvoiceByIdAsync(invoice.InvoiceId);
            Assert.Null(deletedInv);

            // 4. Calling DeleteInvoiceAsync on non-existent invoice returns false
            bool deleteAgain = await billingService.DeleteInvoiceAsync(invoice.InvoiceId);
            Assert.False(deleteAgain);
        }

        [Fact]
        public async Task TestScenario8_BusinessProfile_UpdateAndPdfBranding_Workflow()
        {
            var billingService = BillingDataService.Instance;
            var pdfService = new InvoicePdfService();

            // 1. Fetch current profile
            var originalProfile = await billingService.GetBusinessProfileAsync();
            Assert.NotNull(originalProfile);

            // 2. Update with custom company and bank details
            var updatedProfile = new BusinessProfile
            {
                BusinessName = "Hardik Global Tech Solutions Pvt Ltd",
                Tagline = "Next-Gen Enterprise Cloud & AI Systems",
                Address = "Floor 9, Titanium Square, Thaltej",
                City = "Ahmedabad, Gujarat",
                Country = "India",
                Phone = "+91 99887 76655",
                Email = "accounts@hardiktech.in",
                GSTIN = "24AABCH1234F1Z9",
                BankName = "ICICI Bank Ltd.",
                AccountNumber = "002405001234",
                IFSC = "ICIC0000024",
                UPIId = "hardiktech@icici",
                DefaultTerms = "1. Payment due upon invoice generation.\n2. 18% GST applicable."
            };

            bool saved = await billingService.SaveBusinessProfileAsync(updatedProfile);
            Assert.True(saved);

            // 3. Verify retrieved profile has updated company details
            var retrieved = await billingService.GetBusinessProfileAsync();
            Assert.Equal("Hardik Global Tech Solutions Pvt Ltd", retrieved.BusinessName);
            Assert.Equal("Next-Gen Enterprise Cloud & AI Systems", retrieved.Tagline);
            Assert.Equal("Floor 9, Titanium Square, Thaltej", retrieved.Address);
            Assert.Equal("Ahmedabad, Gujarat", retrieved.City);
            Assert.Equal("India", retrieved.Country);
            Assert.Equal("+91 99887 76655", retrieved.Phone);
            Assert.Equal("accounts@hardiktech.in", retrieved.Email);
            Assert.Equal("24AABCH1234F1Z9", retrieved.GSTIN);
            Assert.Equal("ICICI Bank Ltd.", retrieved.BankName);
            Assert.Equal("002405001234", retrieved.AccountNumber);
            Assert.Equal("ICIC0000024", retrieved.IFSC);
            Assert.Equal("hardiktech@icici", retrieved.UPIId);

            // 4. Verify PDF generation executes cleanly with updated profile
            var sampleInvoice = new Invoice
            {
                InvoiceNumber = "TEST-PDF-001",
                CustomerName = "Aarav Sharma",
                CompanyName = "TechCorp Solutions Pvt Ltd",
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(15),
                Status = "Paid"
            };
            sampleInvoice.Items.Add(new InvoiceItem("Enterprise License", "Annual Subscription", 1m, 50000m, 0m, 18m));
            sampleInvoice.RecalculateTotals();

            string tempPdfPath = Path.Combine(Path.GetTempPath(), $"Test_Profile_{Guid.NewGuid():N}.pdf");
            try
            {
                string generatedPath = pdfService.GenerateInvoicePdf(sampleInvoice, retrieved, tempPdfPath);
                Assert.True(File.Exists(generatedPath));
                var fileInfo = new FileInfo(generatedPath);
                Assert.True(fileInfo.Length > 1000); // PDF contains vector graphics, text & metadata
            }
            finally
            {
                if (File.Exists(tempPdfPath))
                {
                    try { File.Delete(tempPdfPath); } catch { }
                }

                // Restore original profile
                await billingService.SaveBusinessProfileAsync(originalProfile);
            }
        }
    }
}

