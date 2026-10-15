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
    }
}
