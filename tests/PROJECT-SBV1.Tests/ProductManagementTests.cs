using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;
using Xunit;

namespace PROJECT_SBV1.Tests
{
    public class ProductManagementTests
    {
        private static void RunInSta(Action action)
        {
            Exception? ex = null;
            var t = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    ex = e;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (ex != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }

        [Fact]
        public void TestProductModel_PropertiesAndDefaults()
        {
            var p = new Product(10, "Cloud Database Pro", "Cloud Services", 149.99m, 85, "Active");

            Assert.Equal(10, p.ProductId);
            Assert.Equal("Cloud Database Pro", p.Name);
            Assert.Equal("Cloud Services", p.Category);
            Assert.Equal(149.99m, p.Price);
            Assert.Equal(85, p.StockQuantity);
            Assert.Equal("Active", p.Status);
        }

        [Fact]
        public void TestProductCalculations_MetricsAndValuation()
        {
            var sampleProducts = new List<Product>
            {
                new Product(1, "AI Engine", "AI & ML", 850m, 80, "Active"),
                new Product(2, "Cloud Storage 1TB", "Cloud Services", 50m, 500, "Active"),
                new Product(3, "Dev API Access", "Developer Tools", 200m, 30, "Active"),   // Low stock (< 50)
                new Product(4, "Legacy Tool", "Software", 100m, 10, "Inactive"),            // Low stock & inactive
            };

            int total = sampleProducts.Count;
            int active = sampleProducts.Count(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase));
            int lowStock = sampleProducts.Count(p => p.StockQuantity < 50);
            decimal totalVal = sampleProducts.Sum(p => p.Price * p.StockQuantity);

            Assert.Equal(4, total);
            Assert.Equal(3, active);
            Assert.Equal(2, lowStock);

            // (850 * 80) + (50 * 500) + (200 * 30) + (100 * 10) = 68000 + 25000 + 6000 + 1000 = 100000
            Assert.Equal(100000m, totalVal);
        }

        [Fact]
        public void TestProductFiltering_MultiCriteria()
        {
            var products = new List<Product>
            {
                new Product(1, "AI Prediction Engine License", "AI & ML", 850m, 80, "Active"),
                new Product(2, "Cloud Storage Booster 1TB", "Cloud Services", 49.99m, 500, "Active"),
                new Product(3, "Developer API Access Key", "Developer Tools", 199m, 320, "Active"),
                new Product(4, "Enterprise Analytics Suite", "Software", 1200m, 150, "Active"),
                new Product(5, "Security & Compliance Audit Pack", "Security", 452m, 45, "Active"), // Stock < 50
                new Product(6, "Retired Gateway Hardware", "Hardware & IoT", 300m, 5, "Inactive")  // Inactive & low stock
            };

            // 1. Search filter
            var searchMatches = products.Where(p => p.Name.ToLowerInvariant().Contains("storage")).ToList();
            Assert.Single(searchMatches);
            Assert.Equal("Cloud Storage Booster 1TB", searchMatches[0].Name);

            // 2. Category filter
            var catMatches = products.Where(p => p.Category == "AI & ML").ToList();
            Assert.Single(catMatches);
            Assert.Equal("AI Prediction Engine License", catMatches[0].Name);

            // 3. Low stock filter (< 50)
            var lowStockMatches = products.Where(p => p.StockQuantity < 50).ToList();
            Assert.Equal(2, lowStockMatches.Count);

            // 4. Status filter
            var activeMatches = products.Where(p => p.Status == "Active").ToList();
            Assert.Equal(5, activeMatches.Count);
        }

        [Fact]
        public void TestProductSorting_AscendingAndDescending()
        {
            var products = new List<Product>
            {
                new Product(1, "B Product", "Cat B", 100m, 50, "Active"),
                new Product(2, "A Product", "Cat A", 300m, 20, "Active"),
                new Product(3, "C Product", "Cat C", 200m, 100, "Active"),
            };

            var byPriceAsc = products.OrderBy(p => p.Price).ToList();
            Assert.Equal(100m, byPriceAsc[0].Price);
            Assert.Equal(300m, byPriceAsc[2].Price);

            var byPriceDesc = products.OrderByDescending(p => p.Price).ToList();
            Assert.Equal(300m, byPriceDesc[0].Price);
            Assert.Equal(100m, byPriceDesc[2].Price);

            var byStockAsc = products.OrderBy(p => p.StockQuantity).ToList();
            Assert.Equal(20, byStockAsc[0].StockQuantity);

            var byNameAsc = products.OrderBy(p => p.Name).ToList();
            Assert.Equal("A Product", byNameAsc[0].Name);
        }

        [Fact]
        public async Task TestDatabaseService_ProductOperations()
        {
            var db = new DatabaseService();

            var initialProducts = await db.GetProductsAsync();
            Assert.NotNull(initialProducts);
            Assert.True(initialProducts.Count > 0);

            var newProd = new Product(0, "Test Temporary Product Unit", "Quality Assurance", 99.95m, 120, "Active");
            bool added = await db.AddProductAsync(newProd);
            Assert.True(added);
            Assert.True(newProd.ProductId > 0);

            var searchResult = await db.GetProductsAsync("Temporary Product");
            Assert.Contains(searchResult, p => p.Name == "Test Temporary Product Unit");

            bool deleted = await db.DeleteProductAsync(newProd.ProductId);
            Assert.True(deleted);

            var afterDelete = await db.GetProductsAsync("Temporary Product");
            Assert.DoesNotContain(afterDelete, p => p.Name == "Test Temporary Product Unit");
        }

        [Fact]
        public void TestDashboardForm_NavigateToProductsPage()
        {
            RunInSta(() =>
            {
                using var form = new DashboardForm(new User { Username = "Tester", FullName = "Tester", Role = "UI/UX Developer" });
                form.Show();

                // Trigger navigation to Products page (index 2)
                form.NavigateToMenu(2);

                // Verify product grid and cards exist
                var gridField = typeof(DashboardForm).GetField("_gridProducts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var grid = gridField?.GetValue(form) as DataGridView;
                Assert.NotNull(grid);
                Assert.True(grid.Columns.Count >= 6);
                Assert.True(grid.Columns.Contains("ID"));
                Assert.True(grid.Columns.Contains("Name"));
                Assert.True(grid.Columns.Contains("Category"));
                Assert.True(grid.Columns.Contains("Price"));
                Assert.True(grid.Columns.Contains("Stock"));
                Assert.True(grid.Columns.Contains("Status"));
                Assert.True(grid.Columns.Contains("Actions"));
                Assert.Equal("Price (₹)", grid.Columns["Price"].HeaderText);

                var searchField = typeof(DashboardForm).GetField("_txtSearchProduct", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(searchField?.GetValue(form));

                var catComboField = typeof(DashboardForm).GetField("_cmbProductCategory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(catComboField?.GetValue(form));

                var statusComboField = typeof(DashboardForm).GetField("_cmbProductStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(statusComboField?.GetValue(form));

                form.Close();
            });
        }

        [Fact]
        public void TestProductPage_INR_CurrencyAndThemeSync()
        {
            RunInSta(() =>
            {
                using var form = new DashboardForm(new User { Username = "Tester", FullName = "Tester", Role = "UI/UX Developer" });
                form.Show();
                form.NavigateToMenu(2);

                var searchField = typeof(DashboardForm).GetField("_txtSearchProduct", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var searchBox = searchField?.GetValue(form) as RoundedSearchBox;
                Assert.NotNull(searchBox);

                var catComboField = typeof(DashboardForm).GetField("_cmbProductCategory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var catCombo = catComboField?.GetValue(form) as RoundedComboBox;
                Assert.NotNull(catCombo);

                var statusComboField = typeof(DashboardForm).GetField("_cmbProductStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var statusCombo = statusComboField?.GetValue(form) as RoundedComboBox;
                Assert.NotNull(statusCombo);

                // Switch to Light Theme
                ThemeManager.SetTheme(AppTheme.Light);
                Assert.Equal(Color.FromArgb(248, 250, 252), searchBox.BoxBackgroundColor);
                Assert.Equal(Color.FromArgb(248, 250, 252), catCombo.BoxBackgroundColor);
                Assert.Equal(Color.FromArgb(248, 250, 252), statusCombo.BoxBackgroundColor);

                // Switch to Dark Theme
                ThemeManager.SetTheme(AppTheme.Dark);
                Assert.Equal(Color.FromArgb(22, 25, 37), searchBox.BoxBackgroundColor);
                Assert.Equal(Color.FromArgb(22, 25, 37), catCombo.BoxBackgroundColor);
                Assert.Equal(Color.FromArgb(22, 25, 37), statusCombo.BoxBackgroundColor);

                // Restore to Light
                ThemeManager.SetTheme(AppTheme.Light);

                var valLabelField = typeof(DashboardForm).GetField("_lblValTotalStockValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var valLabel = valLabelField?.GetValue(form) as Label;
                Assert.NotNull(valLabel);
                Assert.StartsWith("₹", valLabel.Text);

                form.Close();
            });
        }

        [Fact]
        public void TestProductPage_BarcodeScanButton_Initialization()
        {
            RunInSta(() =>
            {
                using var form = new DashboardForm(new User { Username = "Tester", FullName = "Tester", Role = "UI/UX Developer" });
                form.Show();
                form.NavigateToMenu(2); // Products

                var scanBtnField = typeof(DashboardForm).GetField("_btnProductScanBarcode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var scanBtn = scanBtnField?.GetValue(form) as ModernButton;
                Assert.NotNull(scanBtn);
                Assert.Equal("📷 Scan Barcode", scanBtn.Text);

                var addBtnField = typeof(DashboardForm).GetField("_btnProductAdd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var addBtn = addBtnField?.GetValue(form) as ModernButton;
                Assert.NotNull(addBtn);

                var refreshBtnField = typeof(DashboardForm).GetField("_btnProductRefresh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var refreshBtn = refreshBtnField?.GetValue(form) as ModernButton;
                Assert.NotNull(refreshBtn);

                // Verify buttons are laid out cleanly
                Assert.True(addBtn.Left < scanBtn.Left);
                Assert.True(scanBtn.Left < refreshBtn.Left);

                form.Close();
            });
        }

        [Fact]
        public async Task TestProduct_AddAndUpdate_BarcodeSupport()
        {
            var db = DatabaseService.Instance;
            string testBarcode = $"890123999{DateTime.Now.Millisecond:D3}";

            var newProd = new Product(0, "Test Barcode Scanner Widget", "Hardware", 499.00m, 25, "Active", testBarcode);
            bool addSuccess = await db.AddProductAsync(newProd);
            Assert.True(addSuccess);
            Assert.True(newProd.ProductId > 0);
            Assert.Equal(testBarcode, newProd.Barcode);

            // Lookup by barcode
            var found = await db.GetProductByBarcodeAsync(testBarcode);
            Assert.NotNull(found);
            Assert.Equal("Test Barcode Scanner Widget", found.Name);

            // Update product and verify barcode remains intact
            found.Price = 549.00m;
            found.StockQuantity = 30;
            bool updateSuccess = await db.UpdateProductAsync(found);
            Assert.True(updateSuccess);

            var updated = await db.GetProductByBarcodeAsync(testBarcode);
            Assert.NotNull(updated);
            Assert.Equal(549.00m, updated.Price);
            Assert.Equal(30, updated.StockQuantity);
            Assert.Equal(testBarcode, updated.Barcode);
        }

        [Fact]
        public void TestBarcodeScannerDialog_InventoryLookupMode()
        {
            RunInSta(() =>
            {
                Product? foundProduct = null;
                Product? editedProduct = null;
                string? registeredBarcode = null;

                using var dlg = new BarcodeScannerDialog(
                    onProductScanned: p => foundProduct = p,
                    mode: BarcodeScannerMode.InventoryLookup,
                    onRegisterNewRequested: code => registeredBarcode = code,
                    onEditProductRequested: p => editedProduct = p
                );

                Assert.NotNull(dlg);
                Assert.Equal(BarcodeScannerMode.InventoryLookup, dlg.Mode);
                Assert.Equal("Barcode Scanner & Product Inventory — S TOM'S", dlg.Text);

                // Test barcode lookup process
                var processMethod = typeof(BarcodeScannerDialog).GetMethod("ProcessBarcodeAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(processMethod);

                var task = processMethod.Invoke(dlg, new object[] { "890123400001" }) as Task;
                task?.Wait();

                Assert.NotNull(foundProduct);
                Assert.Equal(1, foundProduct.ProductId);
                Assert.Equal(1, dlg.ScannedCount);

                dlg.Close();
            });
        }
    }
}
