using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;
using Xunit;

namespace PROJECT_SBV1.Tests
{
    public class CustomerManagementTests
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
        public void TestCustomerModel_PropertiesAndConstructors()
        {
            var customer = new Customer(101, "Acme Corp", "contact@acme.com", "+1 800 555 1234", "100 Market St", "San Francisco", "USA");

            Assert.Equal(101, customer.CustomerId);
            Assert.Equal("Acme Corp", customer.Name);
            Assert.Equal("contact@acme.com", customer.Email);
            Assert.Equal("+1 800 555 1234", customer.Phone);
            Assert.Equal("100 Market St", customer.Address);
            Assert.Equal("San Francisco", customer.City);
            Assert.Equal("USA", customer.Country);
        }

        [Fact]
        public void TestCustomerKPIs_Calculations()
        {
            var sampleCustomers = new List<Customer>
            {
                new Customer(1, "Press", "press@business.co.uk", "+44 20 7946 0912", "London", "London", "UK"),
                new Customer(2, "Marina", "marina@mancity.org", "+44 161 496 0192", "Man city", "Manchester", "UK"),
                new Customer(3, "Alex", "alex@techunknown.io", "+1 555 019 2834", "Unknown", "Chicago", "USA"),
                new Customer(4, "Robert Smith", "robert.smith@nycorp.com", "+1 212 555 0148", "New York", "New York", "USA"),
                new Customer(5, "Sophia Taylor", "sophia@berlinventures.de", "+49 30 123456", "Kurfürstendamm 12", "Berlin", "Germany"),
                new Customer(6, "Liam Johnson", "liam@torontosoft.ca", "+1 416 555 0177", "Bay Street 100", "Toronto", "Canada")
            };

            int total = sampleCustomers.Count;
            int distinctCities = sampleCustomers.Select(c => c.City).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().Count();
            int distinctCountries = sampleCustomers.Select(c => c.Country).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().Count();

            Assert.Equal(6, total);
            Assert.Equal(6, distinctCities);
            Assert.Equal(4, distinctCountries);
        }

        [Fact]
        public void TestCustomerFiltering_SearchAndCountry()
        {
            var customers = new List<Customer>
            {
                new Customer(1, "Press", "press@business.co.uk", "+44 20 7946 0912", "London", "London", "UK"),
                new Customer(2, "Marina", "marina@mancity.org", "+44 161 496 0192", "Man city", "Manchester", "UK"),
                new Customer(3, "Alex", "alex@techunknown.io", "+1 555 019 2834", "Unknown", "Chicago", "USA"),
                new Customer(4, "Robert Smith", "robert.smith@nycorp.com", "+1 212 555 0148", "New York", "New York", "USA"),
                new Customer(5, "Sophia Taylor", "sophia@berlinventures.de", "+49 30 123456", "Kurfürstendamm 12", "Berlin", "Germany"),
                new Customer(6, "Liam Johnson", "liam@torontosoft.ca", "+1 416 555 0177", "Bay Street 100", "Toronto", "Canada")
            };

            // 1. Search filter by name
            var searchName = customers.Where(c => c.Name.ToLowerInvariant().Contains("alex")).ToList();
            Assert.Single(searchName);
            Assert.Equal("Alex", searchName[0].Name);

            // 2. Search filter by city
            var searchCity = customers.Where(c => c.City.ToLowerInvariant().Contains("berlin")).ToList();
            Assert.Single(searchCity);
            Assert.Equal("Sophia Taylor", searchCity[0].Name);

            // 3. Country filter
            var searchCountry = customers.Where(c => string.Equals(c.Country, "UK", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.Equal(2, searchCountry.Count);
        }

        [Fact]
        public void TestCustomerSorting_AscendingDescending()
        {
            var customers = new List<Customer>
            {
                new Customer(3, "Alex", "alex@techunknown.io", "+1 555 019 2834", "", "Chicago", "USA"),
                new Customer(1, "Press", "press@business.co.uk", "+44 20 7946 0912", "", "London", "UK"),
                new Customer(2, "Marina", "marina@mancity.org", "+44 161 496 0192", "", "Manchester", "UK"),
            };

            var sortedByIdAsc = customers.OrderBy(c => c.CustomerId).ToList();
            Assert.Equal(1, sortedByIdAsc[0].CustomerId);
            Assert.Equal(2, sortedByIdAsc[1].CustomerId);
            Assert.Equal(3, sortedByIdAsc[2].CustomerId);

            var sortedByNameAsc = customers.OrderBy(c => c.Name).ToList();
            Assert.Equal("Alex", sortedByNameAsc[0].Name);
            Assert.Equal("Marina", sortedByNameAsc[1].Name);
            Assert.Equal("Press", sortedByNameAsc[2].Name);
        }

        [Fact]
        public async Task TestCustomerDatabase_CrudWorkflow()
        {
            var db = new DatabaseService();

            // 1. Initial seeded count
            var initialList = await db.GetCustomersAsync();
            int initialCount = initialList.Count;
            Assert.True(initialCount >= 6);

            // 2. Add customer
            var newCust = new Customer(0, "Test Innovators", "test@innovators.io", "+91 98765 43210", "Tech Park", "Ahmedabad", "India");
            await db.AddCustomerAsync(newCust);

            var afterAdd = await db.GetCustomersAsync();
            Assert.Equal(initialCount + 1, afterAdd.Count);
            var added = afterAdd.FirstOrDefault(c => c.Name == "Test Innovators");
            Assert.NotNull(added);
            Assert.Equal("Ahmedabad", added.City);
            Assert.Equal("India", added.Country);

            // 3. Update customer
            added.City = "Surat";
            await db.UpdateCustomerAsync(added);

            var afterUpdate = await db.GetCustomersAsync();
            var updated = afterUpdate.FirstOrDefault(c => c.CustomerId == added.CustomerId);
            Assert.NotNull(updated);
            Assert.Equal("Surat", updated.City);

            // 4. Search query
            var searchResults = await db.GetCustomersAsync("Surat");
            Assert.Contains(searchResults, c => c.CustomerId == added.CustomerId);

            // 5. Delete customer
            await db.DeleteCustomerAsync(added.CustomerId);
            var afterDelete = await db.GetCustomersAsync();
            Assert.Equal(initialCount, afterDelete.Count);
            Assert.DoesNotContain(afterDelete, c => c.CustomerId == added.CustomerId);
        }

        [Fact]
        public void TestCustomerPage_UIComponentsAndLayout()
        {
            RunInSta(() =>
            {
                using var form = new DashboardForm();
                form.Size = new Size(1340, 860);

                // Navigate to Customers (Index 4)
                form.NavigateToMenu(4);

                // Find controls via reflection
                var t = typeof(DashboardForm);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                var gridField = t.GetField("_gridCustomers", flags);
                var grid = gridField?.GetValue(form) as DataGridView;
                Assert.NotNull(grid);
                Assert.Equal(7, grid.Columns.Count);
                Assert.Equal("ID", grid.Columns[0].Name);
                Assert.Equal("Name", grid.Columns[1].Name);
                Assert.Equal("Email", grid.Columns[2].Name);
                Assert.Equal("Phone", grid.Columns[3].Name);
                Assert.Equal("City", grid.Columns[4].Name);
                Assert.Equal("Country", grid.Columns[5].Name);
                Assert.Equal("Actions", grid.Columns[6].Name);

                var searchField = t.GetField("_txtSearchCustomer", flags);
                var search = searchField?.GetValue(form) as RoundedSearchBox;
                Assert.NotNull(search);
                Assert.Equal(8, search.BorderRadius);

                var countryCmbField = t.GetField("_cmbCustomerCountry", flags);
                var cmbCountry = countryCmbField?.GetValue(form) as RoundedComboBox;
                Assert.NotNull(cmbCountry);
                Assert.Contains("All Countries", cmbCountry.Items.Cast<string>());

                var gridWrapperField = t.GetField("_customersGridWrapper", flags);
                var wrapper = gridWrapperField?.GetValue(form) as RoundedPanel;
                Assert.NotNull(wrapper);
                Assert.Equal(8, wrapper.BorderRadius);

                var cardsField = t.GetField("_customerMetricCards", flags);
                var cards = cardsField?.GetValue(form) as List<RoundedPanel>;
                Assert.NotNull(cards);
                Assert.Equal(4, cards.Count);
            });
        }
    }
}
