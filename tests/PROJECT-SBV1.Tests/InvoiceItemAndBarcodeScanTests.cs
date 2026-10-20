using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using Xunit;

namespace PROJECT_SBV1.Tests
{
    public class InvoiceItemAndBarcodeScanTests
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
                throw new AggregateException("STA thread encountered an exception.", ex);
            }
        }

        [Fact]
        public async Task TestProduct_BarcodeAssignmentAndLookup()
        {
            var db = DatabaseService.Instance;

            // 1. Lookup by Barcode for seeded product 1
            var p1 = await db.GetProductByBarcodeAsync("890123400001");
            Assert.NotNull(p1);
            Assert.Equal(1, p1.ProductId);
            Assert.Equal("Enterprise Analytics Suite", p1.Name);

            // 2. Lookup by Product ID string
            var p2 = await db.GetProductByBarcodeAsync("2");
            Assert.NotNull(p2);
            Assert.Equal(2, p2.ProductId);
            Assert.Equal("Cloud Storage Booster 1TB", p2.Name);

            // 3. Lookup by SKU prefix
            var p3 = await db.GetProductByBarcodeAsync("PRD-003");
            Assert.NotNull(p3);
            Assert.Equal(3, p3.ProductId);
            Assert.Equal("AI Prediction Engine License", p3.Name);

            // 4. Non-existent barcode returns null
            var nonExistent = await db.GetProductByBarcodeAsync("999999999999");
            Assert.Null(nonExistent);
        }

        [Fact]
        public void TestInvoiceForm_AddOrIncrementProductItem()
        {
            RunInSta(() =>
            {
                using var invForm = new InvoiceForm();
                invForm.Show();

                var gridField = typeof(InvoiceForm).GetField("_gridItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var grid = gridField?.GetValue(invForm) as DataGridView;
                Assert.NotNull(grid);

                int initialRowCount = grid.Rows.Count;

                var sampleProduct = new Product(10, "Test Barcode Software", "Software", 2500m, 100, "Active", "890123400010");

                // Scan / Add product once
                invForm.AddOrIncrementProductItem(sampleProduct);
                Assert.Equal(initialRowCount + 1, grid.Rows.Count);

                var lastRow = grid.Rows[grid.Rows.Count - 1];
                Assert.Equal("Test Barcode Software", lastRow.Cells[0].Value);
                Assert.Equal(1m, Convert.ToDecimal(lastRow.Cells[2].Value));
                Assert.Equal(2500m, Convert.ToDecimal(lastRow.Cells[3].Value));

                // Scan / Add product second time -> should increment quantity to 2
                invForm.AddOrIncrementProductItem(sampleProduct);
                Assert.Equal(initialRowCount + 1, grid.Rows.Count); // Count does not increase
                Assert.Equal(2m, Convert.ToDecimal(lastRow.Cells[2].Value)); // Qty incremented to 2

                invForm.Close();
            });
        }

        [Fact]
        public void TestManualAddItemDialog_UIInitialization()
        {
            RunInSta(() =>
            {
                using var dlg = new ManualAddItemDialog();
                Assert.NotNull(dlg);
                Assert.Equal("Add Product / Service Item — S TOM'S", dlg.Text);

                // Check created item is initially null
                Assert.Null(dlg.CreatedItem);
                Assert.False(dlg.AddBlankRowRequested);

                dlg.Close();
            });
        }

        [Fact]
        public void TestBarcodeScannerDialog_ScanningFlow()
        {
            RunInSta(() =>
            {
                Product? scannedProduct = null;
                using var dlg = new BarcodeScannerDialog(p =>
                {
                    scannedProduct = p;
                });

                Assert.NotNull(dlg);
                Assert.Equal(0, dlg.ScannedCount);

                // Simulate scanning product barcode
                var txtBarcodeField = typeof(BarcodeScannerDialog).GetField("_txtBarcode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var txtBarcode = txtBarcodeField?.GetValue(dlg) as TextBox;
                Assert.NotNull(txtBarcode);

                // Invoke private ProcessBarcodeAsync via reflection
                var processMethod = typeof(BarcodeScannerDialog).GetMethod("ProcessBarcodeAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(processMethod);

                var task = processMethod.Invoke(dlg, new object[] { "890123400001" }) as Task;
                task?.Wait();

                Assert.NotNull(scannedProduct);
                Assert.Equal(1, scannedProduct.ProductId);
                Assert.Equal(1, dlg.ScannedCount);

                dlg.Close();
            });
        }
    }
}
