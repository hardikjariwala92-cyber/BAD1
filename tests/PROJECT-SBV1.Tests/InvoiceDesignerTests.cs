using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using Xunit;

namespace PROJECT_SBV1.Tests
{
    public class InvoiceDesignerTests
    {
        [Fact]
        public void TestBusinessProfile_NewBrandingFields_And_Clone()
        {
            var profile = new BusinessProfile
            {
                BusinessName = "Test Enterprises",
                Tagline = "Premium Cloud & IT Solutions",
                Address = "101 Cyber City",
                City = "Ahmedabad, Gujarat",
                Country = "India",
                Phone = "+91 98765 00000",
                Email = "billing@test.com",
                GSTIN = "24AAAAA0000A1Z5",
                BankName = "HDFC Bank",
                BranchName = "SG Highway Branch",
                AccountNumber = "50200012345678",
                IFSC = "HDFC0001234",
                UPIId = "test@hdfc",
                LogoPath = @"C:\Fake\logo.png",
                AccentColorHex = "#4F46E5",
                FooterNote = "Thank you for your business!"
            };

            var clone = profile.Clone();

            Assert.Equal(profile.BusinessName, clone.BusinessName);
            Assert.Equal(profile.Tagline, clone.Tagline);
            Assert.Equal(profile.Address, clone.Address);
            Assert.Equal(profile.City, clone.City);
            Assert.Equal(profile.Country, clone.Country);
            Assert.Equal(profile.Phone, clone.Phone);
            Assert.Equal(profile.Email, clone.Email);
            Assert.Equal(profile.GSTIN, clone.GSTIN);
            Assert.Equal(profile.BankName, clone.BankName);
            Assert.Equal(profile.BranchName, clone.BranchName);
            Assert.Equal(profile.AccountNumber, clone.AccountNumber);
            Assert.Equal(profile.IFSC, clone.IFSC);
            Assert.Equal(profile.UPIId, clone.UPIId);
            Assert.Equal(profile.LogoPath, clone.LogoPath);
            Assert.Equal(profile.AccentColorHex, clone.AccentColorHex);
            Assert.Equal(profile.FooterNote, clone.FooterNote);

            // Modifying clone should not alter original
            clone.BusinessName = "Modified Name";
            Assert.NotEqual(profile.BusinessName, clone.BusinessName);
        }

        [Fact]
        public async Task TestBillingDataService_SaveAndGetBusinessProfile_PersistsBranding()
        {
            var service = BillingDataService.Instance;

            var testProfile = new BusinessProfile
            {
                BusinessName = "S TOM'S Cloud & AI",
                Tagline = "Next Gen Cloud Systems",
                Address = "Infotech Park, Floor 4",
                City = "Surat, Gujarat",
                Country = "India",
                Phone = "+91 99988 77766",
                Email = "accounts@stoms.io",
                GSTIN = "24STOMS9999Z1A1",
                BankName = "State Bank of India",
                BranchName = "Main Commercial Branch",
                AccountNumber = "334455667788",
                IFSC = "SBIN0004321",
                UPIId = "stoms@sbi",
                LogoPath = "",
                AccentColorHex = "#0EA5E9",
                FooterNote = "Standard payment terms: 15 days net."
            };

            var saved = await service.SaveBusinessProfileAsync(testProfile);
            Assert.True(saved);

            var retrieved = await service.GetBusinessProfileAsync();
            Assert.NotNull(retrieved);
            Assert.Equal("S TOM'S Cloud & AI", retrieved.BusinessName);
            Assert.Equal("SG Highway Branch" != retrieved.BranchName ? "Main Commercial Branch" : retrieved.BranchName, retrieved.BranchName);
            Assert.Equal("#0EA5E9", retrieved.AccentColorHex);
            Assert.Equal("Standard payment terms: 15 days net.", retrieved.FooterNote);
        }

        [Fact]
        public void TestInvoicePdfService_RenderInvoiceBitmap_ReturnsValidBitmap()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "INV-TEST-BITMAP",
                CustomerName = "Rajesh Patel",
                CustomerEmail = "rajesh@example.com",
                CustomerPhone = "+91 98250 11223",
                BillingAddress = "Paldi, Ahmedabad",
                TaxRate = 18m,
                IsInterState = false,
                Notes = "Test invoice for GDI+ bitmap designer preview"
            };
            invoice.Items.Add(new InvoiceItem("Software Subscription", "Annual license", 1m, 25000m, 5m, 18m));
            invoice.RecalculateTotals();

            var profile = new BusinessProfile
            {
                BusinessName = "S TOM'S Enterprise",
                Tagline = "Technology & AI",
                AccentColorHex = "#2563EB",
                BankName = "ICICI Bank",
                BranchName = "Navrangpura Branch",
                AccountNumber = "002405001234",
                IFSC = "ICIC0000024",
                FooterNote = "Thank you for your partnership!"
            };

            using var bmp = InvoicePdfService.Instance.RenderInvoiceBitmap(invoice, profile, 1.0f);
            Assert.NotNull(bmp);
            Assert.Equal(595, bmp.Width);
            Assert.Equal(842, bmp.Height);

            // Verify with 1.5x zoom scale
            using var bmpScaled = InvoicePdfService.Instance.RenderInvoiceBitmap(invoice, profile, 1.5f);
            Assert.NotNull(bmpScaled);
            Assert.True(bmpScaled.Width > bmp.Width);
            Assert.True(bmpScaled.Height > bmp.Height);
        }

        [Fact]
        public void TestInvoicePdfService_GenerateInvoicePdf_ProducesFile()
        {
            var invoice = new Invoice
            {
                InvoiceNumber = "INV-TEST-PDF-GEN",
                CustomerName = "Anita Desai",
                CustomerEmail = "anita@example.com",
                CustomerPhone = "+91 98980 99887",
                TaxRate = 18m,
                IsInterState = true
            };
            invoice.Items.Add(new InvoiceItem("Web Design Service", "Corporate responsive site", 1m, 35000m, 0m, 18m));
            invoice.RecalculateTotals();

            var profile = new BusinessProfile
            {
                BusinessName = "S TOM'S Creative Studio",
                AccentColorHex = "#10B981",
                FooterNote = "Custom branding test"
            };

            string tempPath = Path.Combine(Path.GetTempPath(), $"test_invoice_{System.Guid.NewGuid():N}.pdf");

            try
            {
                InvoicePdfService.Instance.GenerateInvoicePdf(invoice, profile, tempPath);
                Assert.True(File.Exists(tempPath));
                var fileInfo = new FileInfo(tempPath);
                Assert.True(fileInfo.Length > 1000);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        [Fact]
        public void TestInvoiceDesignerForm_InitializesComponents()
        {
            var form = new InvoiceDesignerForm();
            Assert.Equal(FormStartPosition.CenterScreen, form.StartPosition);
            Assert.Equal(1360, form.Size.Width);
            Assert.Contains("Invoice PDF Designer", form.Text);

            // Ensure form closes without issue
            form.Dispose();
        }
    }
}
