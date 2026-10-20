using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace PROJECT_SBV1.Tests
{
    public class AddCustomerOrderTests
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
        public void TestAddCustomerOrder_DefaultControlProperties()
        {
            RunInSta(() =>
            {
                using var form = new AddCustomerOrder();

                Assert.Equal("Add New Customer Order", form.Text);
                Assert.Equal(FormStartPosition.CenterParent, form.StartPosition);
                Assert.False(form.MaximizeBox);
                Assert.False(form.MinimizeBox);

                // Accept and Cancel button wiring
                Assert.NotNull(form.AcceptButton);
                Assert.NotNull(form.CancelButton);

                // Check field defaults using reflection
                var cmbStatusField = typeof(AddCustomerOrder).GetField("_cmbStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var cmbStatus = cmbStatusField?.GetValue(form) as ComboBox;
                Assert.NotNull(cmbStatus);
                Assert.Equal("Processed", cmbStatus.SelectedItem?.ToString());

                var dtpDateField = typeof(AddCustomerOrder).GetField("_dtpOrderDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dtpDate = dtpDateField?.GetValue(form) as DateTimePicker;
                Assert.NotNull(dtpDate);
                Assert.Equal(DateTime.Today.Date, dtpDate.Value.Date);
            });
        }

        [Theory]
        [InlineData("", "Chicago", "950", "Please enter or select a customer name.")]
        [InlineData("   ", "Chicago", "950", "Please enter or select a customer name.")]
        [InlineData("Alex", "", "950", "Please enter a valid city or delivery address.")]
        [InlineData("Alex", "   ", "950", "Please enter a valid city or delivery address.")]
        [InlineData("Alex", "Chicago", "", "Please enter the order price.")]
        [InlineData("Alex", "Chicago", "abc", "Order price must be a valid numeric value.")]
        [InlineData("Alex", "Chicago", "0", "Order price must be a positive amount greater than zero.")]
        [InlineData("Alex", "Chicago", "-150", "Order price must be a positive amount greater than zero.")]
        public void TestAddCustomerOrder_ValidationFails(string name, string address, string price, string expectedSubError)
        {
            RunInSta(() =>
            {
                using var form = new AddCustomerOrder();

                var cmbCustomerField = typeof(AddCustomerOrder).GetField("_cmbCustomer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var txtAddressField = typeof(AddCustomerOrder).GetField("_txtAddress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var txtPriceField = typeof(AddCustomerOrder).GetField("_txtPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var cmbCustomer = cmbCustomerField?.GetValue(form) as ComboBox;
                var txtAddress = txtAddressField?.GetValue(form) as TextBox;
                var txtPrice = txtPriceField?.GetValue(form) as TextBox;

                Assert.NotNull(cmbCustomer);
                Assert.NotNull(txtAddress);
                Assert.NotNull(txtPrice);

                cmbCustomer.Text = name;
                txtAddress.Text = address;
                txtPrice.Text = price;

                bool isValid = form.ValidateInputs(out string error, out var focusControl);

                Assert.False(isValid);
                Assert.Contains(expectedSubError, error);
                Assert.NotNull(focusControl);
            });
        }

        [Theory]
        [InlineData("Daniel", "San Francisco", "1250")]
        [InlineData("Sarah Connor", "Los Angeles, CA", "₹2,500")]
        [InlineData("Wayne Enterprises", "Gotham", "999.95")]
        public void TestAddCustomerOrder_ValidationPasses(string name, string address, string price)
        {
            RunInSta(() =>
            {
                using var form = new AddCustomerOrder();

                var cmbCustomerField = typeof(AddCustomerOrder).GetField("_cmbCustomer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var txtAddressField = typeof(AddCustomerOrder).GetField("_txtAddress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var txtPriceField = typeof(AddCustomerOrder).GetField("_txtPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var cmbCustomer = cmbCustomerField?.GetValue(form) as ComboBox;
                var txtAddress = txtAddressField?.GetValue(form) as TextBox;
                var txtPrice = txtPriceField?.GetValue(form) as TextBox;

                Assert.NotNull(cmbCustomer);
                Assert.NotNull(txtAddress);
                Assert.NotNull(txtPrice);

                cmbCustomer.Text = name;
                txtAddress.Text = address;
                txtPrice.Text = price;

                bool isValid = form.ValidateInputs(out string error, out var focusControl);

                Assert.True(isValid);
                Assert.Empty(error);
                Assert.Null(focusControl);
            });
        }

        [Fact]
        public async Task TestDatabaseService_AddOrderAsync_SavesWithCorrectDateAndFormat()
        {
            var db = DatabaseService.Instance;
            string testCustomer = $"TestCustomer_{Guid.NewGuid():N}";
            string testDate = "15.09.2026";
            var order = new CustomerOrder(testCustomer, "Metro City", testDate, "Processed", "₹3,450");

            bool result = await db.AddOrderAsync(order);
            Assert.True(result);

            var orders = await db.GetOrdersAsync(search: testCustomer);
            Assert.NotEmpty(orders);
            var retrieved = orders.First(o => o.ProfileName == testCustomer);
            Assert.Equal("Metro City", retrieved.Address);
            Assert.Equal("Processed", retrieved.Status);
            Assert.Equal("₹3,450", retrieved.Price);
            Assert.Equal(testDate, retrieved.OrderDate);
        }

        [Fact]
        public void TestAddCustomerOrder_ApplyTheme_TogglesGracefully()
        {
            RunInSta(() =>
            {
                using var form = new AddCustomerOrder();

                // Toggle Dark
                ThemeManager.SetTheme(AppTheme.Dark);
                form.ApplyCurrentTheme();
                Assert.Equal(ThemeManager.Background, form.BackColor);

                // Toggle Light
                ThemeManager.SetTheme(AppTheme.Light);
                form.ApplyCurrentTheme();
                Assert.Equal(ThemeManager.Background, form.BackColor);
            });
        }

        [Fact]
        public void TestAddCustomerOrder_WithCustomDefaultDate()
        {
            RunInSta(() =>
            {
                var customDate = new DateTime(2022, 8, 25);
                using var form = new AddCustomerOrder(customDate);

                var dtpDateField = typeof(AddCustomerOrder).GetField("_dtpOrderDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dtpDate = dtpDateField?.GetValue(form) as DateTimePicker;
                Assert.NotNull(dtpDate);
                Assert.Equal(customDate.Date, dtpDate.Value.Date);
            });
        }

        [Fact]
        public void TestOrderSuccessDialog_InitializesAndRendersProperties()
        {
            RunInSta(() =>
            {
                var order = new CustomerOrder("Hardik Jariwala", "Surat, Gujarat", "25.08.2022", "Processed", "₹4,500");
                using var dlg = new OrderSuccessDialog(order);

                Assert.Equal("Order Saved Successfully", dlg.Text);
                Assert.Equal(FormStartPosition.CenterParent, dlg.StartPosition);
                Assert.NotNull(dlg.AcceptButton);
            });
        }
    }
}
