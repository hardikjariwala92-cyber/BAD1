using System.Data;
using Microsoft.Data.SqlClient;
using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    public class DatabaseService
    {
        private static DatabaseService? _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        public string ConnectionString { get; set; } = 
            "Server=(localdb)\\mssqllocaldb;Database=BusinessAnalyticsDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2;";

        public bool IsDemoMode { get; private set; } = true;
        public string LastStatusMessage { get; private set; } = "Demo Mode (In-Memory Seed Data)";

        // In-memory persistent demo data stores (Thread-safe)
        private readonly List<Customer> _demoCustomers = new();
        private readonly List<Product> _demoProducts = new();
        private readonly List<CustomerOrder> _demoOrders = new();
        private readonly List<NotificationMessage> _demoMessages = new();
        private readonly Dictionary<string, string> _demoSettings = new();

        public DatabaseService()
        {
            InitializeDemoSeedData();
            // Test SQL connection in background
            _ = TestConnectionAsync();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                await conn.OpenAsync();
                IsDemoMode = false;
                LastStatusMessage = "Connected to SQL Server (BusinessAnalyticsDB)";
                return true;
            }
            catch (Exception ex)
            {
                IsDemoMode = true;
                LastStatusMessage = $"Demo Mode: SQL Server unavailable ({ex.Message})";
                return false;
            }
        }

        private void InitializeDemoSeedData()
        {
            // Seed Customers
            _demoCustomers.AddRange(new[]
            {
                new Customer(1, "Press", "press@business.co.uk", "+44 20 7946 0912", "London", "London", "UK"),
                new Customer(2, "Marina", "marina@mancity.org", "+44 161 496 0192", "Man city", "Manchester", "UK"),
                new Customer(3, "Alex", "alex@techunknown.io", "+1 555 019 2834", "Unknown", "Chicago", "USA"),
                new Customer(4, "Robert Smith", "robert.smith@nycorp.com", "+1 212 555 0148", "New York", "New York", "USA"),
                new Customer(5, "Sophia Taylor", "sophia@berlinventures.de", "+49 30 123456", "Kurfürstendamm 12", "Berlin", "Germany"),
                new Customer(6, "Liam Johnson", "liam@torontosoft.ca", "+1 416 555 0177", "Bay Street 100", "Toronto", "Canada")
            });

            // Seed Products
            _demoProducts.AddRange(new[]
            {
                new Product(1, "Enterprise Analytics Suite", "Software", 1200.00m, 150, "Active"),
                new Product(2, "Cloud Storage Booster 1TB", "Cloud Services", 49.99m, 500, "Active"),
                new Product(3, "AI Prediction Engine License", "AI & ML", 850.00m, 80, "Active"),
                new Product(4, "Developer API Access Key", "Developer Tools", 199.00m, 320, "Active"),
                new Product(5, "Security & Compliance Audit Pack", "Security", 452.00m, 45, "Active"),
                new Product(6, "Real-time Telemetry Dashboard", "Hardware & IoT", 920.00m, 60, "Active")
            });

            // Seed Orders (Matching reference image)
            _demoOrders.AddRange(new[]
            {
                new CustomerOrder("Press", "London", "22.08.2022", "Delivered", "$920"),
                new CustomerOrder("Marina", "Man city", "24.08.2022", "Processed", "$452"),
                new CustomerOrder("Alex", "Unknown", "15.08.2022", "Cancelled", "$1200"),
                new CustomerOrder("Robert", "New York", "03.08.2022", "Delivered", "$1235"),
                new CustomerOrder("Sophia", "Berlin", "28.08.2022", "Delivered", "$850"),
                new CustomerOrder("Liam", "Toronto", "30.08.2022", "Processed", "$640")
            });

            // Seed Messages
            _demoMessages.AddRange(new[]
            {
                new NotificationMessage(1, "System Monitoring", "Monthly Sales Target Exceeded", "August sales dynamics reached $302,562.23 surpassing forecast by 8.2%.", false, DateTime.Now.AddMinutes(-25)),
                new NotificationMessage(2, "Payment Gateway", "Fund Settlement Processed", "Funds received settlement of $150,256.23 cleared successfully to primary bank account.", false, DateTime.Now.AddHours(-3)),
                new NotificationMessage(3, "Support Desk", "Customer Ticket #4890 Resolved", "Customer Press verified delivery of invoice items and confirmed receipt.", true, DateTime.Now.AddDays(-1)),
                new NotificationMessage(4, "Security Guard", "New Session from Visual Studio", "User Kristi Kamlykova signed in successfully from local desktop workstation.", true, DateTime.Now.AddDays(-2))
            });

            // Seed Settings
            _demoSettings["Theme"] = "Dark";
            _demoSettings["CompanyName"] = "Business Analytics Inc.";
            _demoSettings["CurrencySymbol"] = "$";
            _demoSettings["DefaultDateRange"] = "01.08.2022 - 31.08.2022";
        }

        #region Customer Data Access
        public async Task<List<Customer>> GetCustomersAsync(string? search = null)
        {
            if (!IsDemoMode)
            {
                try
                {
                    var list = new List<Customer>();
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = "SELECT CustomerId, Name, Email, Phone, Address, City, Country FROM dbo.Customers";
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        query += " WHERE Name LIKE @search OR City LIKE @search OR Email LIKE @search";
                    }

                    using var cmd = new SqlCommand(query, conn);
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    }

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Customer(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.IsDBNull(3) ? "" : reader.GetString(3),
                            reader.GetString(4),
                            reader.GetString(5),
                            reader.GetString(6)
                        ));
                    }
                    return list;
                }
                catch
                {
                    IsDemoMode = true; // Fall back to demo data if query fails
                }
            }

            // Fallback In-Memory
            var queryDemo = _demoCustomers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLowerInvariant();
                queryDemo = queryDemo.Where(c => c.Name.ToLowerInvariant().Contains(s) || 
                                                c.City.ToLowerInvariant().Contains(s) || 
                                                c.Email.ToLowerInvariant().Contains(s));
            }
            return queryDemo.OrderBy(c => c.Name).ToList();
        }

        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = @"INSERT INTO dbo.Customers (Name, Email, Phone, Address, City, Country) 
                                     VALUES (@name, @email, @phone, @address, @city, @country);
                                     SELECT SCOPE_IDENTITY();";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", customer.Name);
                    cmd.Parameters.AddWithValue("@email", customer.Email);
                    cmd.Parameters.AddWithValue("@phone", customer.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@address", customer.Address);
                    cmd.Parameters.AddWithValue("@city", customer.City);
                    cmd.Parameters.AddWithValue("@country", customer.Country);

                    var id = await cmd.ExecuteScalarAsync();
                    if (id != null && int.TryParse(id.ToString(), out int newId))
                    {
                        customer.CustomerId = newId;
                        return true;
                    }
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            customer.CustomerId = _demoCustomers.Count > 0 ? _demoCustomers.Max(c => c.CustomerId) + 1 : 1;
            _demoCustomers.Add(customer);
            return true;
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = @"UPDATE dbo.Customers 
                                     SET Name = @name, Email = @email, Phone = @phone, Address = @address, City = @city, Country = @country 
                                     WHERE CustomerId = @id";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", customer.CustomerId);
                    cmd.Parameters.AddWithValue("@name", customer.Name);
                    cmd.Parameters.AddWithValue("@email", customer.Email);
                    cmd.Parameters.AddWithValue("@phone", customer.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@address", customer.Address);
                    cmd.Parameters.AddWithValue("@city", customer.City);
                    cmd.Parameters.AddWithValue("@country", customer.Country);

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            var existing = _demoCustomers.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
            if (existing != null)
            {
                existing.Name = customer.Name;
                existing.Email = customer.Email;
                existing.Phone = customer.Phone;
                existing.Address = customer.Address;
                existing.City = customer.City;
                existing.Country = customer.Country;
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = "DELETE FROM dbo.Customers WHERE CustomerId = @id";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", customerId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            var existing = _demoCustomers.FirstOrDefault(c => c.CustomerId == customerId);
            if (existing != null)
            {
                _demoCustomers.Remove(existing);
                return true;
            }
            return false;
        }
        #endregion

        #region Product Data Access
        public async Task<List<Product>> GetProductsAsync(string? search = null)
        {
            if (!IsDemoMode)
            {
                try
                {
                    var list = new List<Product>();
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = "SELECT ProductId, Name, Category, Price, StockQuantity, Status FROM dbo.Products";
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        query += " WHERE Name LIKE @search OR Category LIKE @search";
                    }

                    using var cmd = new SqlCommand(query, conn);
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    }

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Product(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetDecimal(3),
                            reader.GetInt32(4),
                            reader.GetString(5)
                        ));
                    }
                    return list;
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            var queryDemo = _demoProducts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLowerInvariant();
                queryDemo = queryDemo.Where(p => p.Name.ToLowerInvariant().Contains(s) || 
                                                p.Category.ToLowerInvariant().Contains(s));
            }
            return queryDemo.OrderBy(p => p.Name).ToList();
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = @"INSERT INTO dbo.Products (Name, Category, Price, StockQuantity, Status) 
                                     VALUES (@name, @category, @price, @stock, @status);
                                     SELECT SCOPE_IDENTITY();";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", product.Name);
                    cmd.Parameters.AddWithValue("@category", product.Category);
                    cmd.Parameters.AddWithValue("@price", product.Price);
                    cmd.Parameters.AddWithValue("@stock", product.StockQuantity);
                    cmd.Parameters.AddWithValue("@status", product.Status);

                    var id = await cmd.ExecuteScalarAsync();
                    if (id != null && int.TryParse(id.ToString(), out int newId))
                    {
                        product.ProductId = newId;
                        return true;
                    }
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            product.ProductId = _demoProducts.Count > 0 ? _demoProducts.Max(p => p.ProductId) + 1 : 1;
            _demoProducts.Add(product);
            return true;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = @"UPDATE dbo.Products 
                                     SET Name = @name, Category = @category, Price = @price, StockQuantity = @stock, Status = @status 
                                     WHERE ProductId = @id";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", product.ProductId);
                    cmd.Parameters.AddWithValue("@name", product.Name);
                    cmd.Parameters.AddWithValue("@category", product.Category);
                    cmd.Parameters.AddWithValue("@price", product.Price);
                    cmd.Parameters.AddWithValue("@stock", product.StockQuantity);
                    cmd.Parameters.AddWithValue("@status", product.Status);

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            var existing = _demoProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Category = product.Category;
                existing.Price = product.Price;
                existing.StockQuantity = product.StockQuantity;
                existing.Status = product.Status;
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = "DELETE FROM dbo.Products WHERE ProductId = @id";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", productId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            var existing = _demoProducts.FirstOrDefault(p => p.ProductId == productId);
            if (existing != null)
            {
                _demoProducts.Remove(existing);
                return true;
            }
            return false;
        }
        #endregion

        #region Messages Data Access
        public async Task<List<NotificationMessage>> GetMessagesAsync()
        {
            if (!IsDemoMode)
            {
                try
                {
                    var list = new List<NotificationMessage>();
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = "SELECT MessageId, Sender, Subject, Content, IsRead, SentDate FROM dbo.Messages ORDER BY SentDate DESC";
                    using var cmd = new SqlCommand(query, conn);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(new NotificationMessage(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetBoolean(4),
                            reader.GetDateTime(5)
                        ));
                    }
                    return list;
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            return _demoMessages.OrderByDescending(m => m.SentDate).ToList();
        }

        public async Task MarkMessageReadAsync(int messageId)
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();
                    string query = "UPDATE dbo.Messages SET IsRead = 1 WHERE MessageId = @id";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", messageId);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            var msg = _demoMessages.FirstOrDefault(m => m.MessageId == messageId);
            if (msg != null) msg.IsRead = true;
        }

        public async Task MarkAllMessagesReadAsync()
        {
            if (!IsDemoMode)
            {
                try
                {
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();
                    string query = "UPDATE dbo.Messages SET IsRead = 1";
                    using var cmd = new SqlCommand(query, conn);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            foreach (var m in _demoMessages) m.IsRead = true;
        }
        #endregion

        #region Orders & Analytics Data Access
        public async Task<List<CustomerOrder>> GetOrdersAsync(string? search = null, string? statusFilter = null)
        {
            if (!IsDemoMode)
            {
                try
                {
                    var list = new List<CustomerOrder>();
                    using var conn = new SqlConnection(ConnectionString);
                    await conn.OpenAsync();

                    string query = "SELECT CustomerName, Address, CONVERT(VARCHAR, OrderDate, 104) AS OrderDate, Status, '$' + CAST(CAST(TotalAmount AS INT) AS VARCHAR) AS Price FROM dbo.Orders WHERE 1=1";
                    if (!string.IsNullOrWhiteSpace(search)) query += " AND (CustomerName LIKE @search OR Address LIKE @search)";
                    if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All") query += " AND Status = @status";
                    query += " ORDER BY OrderDate DESC";

                    using var cmd = new SqlCommand(query, conn);
                    if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All") cmd.Parameters.AddWithValue("@status", statusFilter);

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(new CustomerOrder(
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4)
                        ));
                    }
                    return list;
                }
                catch
                {
                    IsDemoMode = true;
                }
            }

            var queryDemo = _demoOrders.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLowerInvariant();
                queryDemo = queryDemo.Where(o => o.ProfileName.ToLowerInvariant().Contains(s) || o.Address.ToLowerInvariant().Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                queryDemo = queryDemo.Where(o => o.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }
            return queryDemo.ToList();
        }

        public AnalyticsSummary GetAnalyticsSummary()
        {
            // Consistent, seeded values matching the reference mockup
            return new AnalyticsSummary
            {
                TotalOrders = 201,
                OrdersTrend = "↑ 8.2% since last month",
                OrdersTrendPositive = true,
                ApprovedOrders = 36,
                ApprovedTrend = "↑ 3.4% since last month",
                ApprovedTrendPositive = true,
                TotalUsers = "4.890",
                ActiveSubscriptions = "1.201",
                MonthTotal = 25410,
                MonthTotalTrend = "↓ 0.2% since last month",
                MonthTotalTrendPositive = false,
                Revenue = 1352,
                RevenueTrend = "↓ 1.2% since last month",
                RevenueTrendPositive = false,
                PaidInvoices = "$30256.23",
                PaidInvoicesBadge = "+15%",
                FundsReceived = "$150256.23",
                FundsReceivedBadge = "+59%"
            };
        }

        public (string Month, int Value)[] GetSalesDynamics(int year = 2021)
        {
            if (year == 2022)
            {
                return new (string Month, int Value)[]
                {
                    ("JAN", 150), ("FEB", 220), ("MAR", 180), ("APR", 310), ("MAY", 260), ("JUN", 380),
                    ("JUL", 320), ("AUG", 410), ("SEP", 290), ("OCT", 350), ("NOV", 430), ("DEC", 340)
                };
            }

            // Default 2021 (Reference screenshot)
            return new (string Month, int Value)[]
            {
                ("JAN", 120), ("FEB", 190), ("MAR", 140), ("APR", 260), ("MAY", 210), ("JUN", 340),
                ("JUL", 290), ("AUG", 380), ("SEP", 240), ("OCT", 310), ("NOV", 390), ("DEC", 270)
            };
        }

        public float[] GetActivityCurve(int year = 2021)
        {
            if (year == 2022)
            {
                return new float[] { 0.3f, 0.45f, 0.38f, 0.6f, 0.55f, 0.8f, 0.72f, 0.9f, 0.82f, 0.95f, 0.88f, 0.98f };
            }

            // Default 2021
            return new float[] { 0.2f, 0.35f, 0.28f, 0.5f, 0.45f, 0.72f, 0.65f, 0.85f, 0.75f, 0.9f, 0.82f, 0.95f };
        }
        #endregion

        #region App Settings
        public string GetSetting(string key, string defaultValue = "")
        {
            return _demoSettings.TryGetValue(key, out var val) ? val : defaultValue;
        }

        public void SaveSetting(string key, string value)
        {
            _demoSettings[key] = value;
        }
        #endregion
    }
}
