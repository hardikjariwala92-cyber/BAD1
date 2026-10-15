using System.Globalization;
using Microsoft.Data.Sqlite;
using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    public class BillingDataService
    {
        private static BillingDataService? _instance;
        public static BillingDataService Instance => _instance ??= new BillingDataService();

        private readonly string _connectionString;
        private readonly string _dbPath;

        public BillingDataService()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sales_billing.db");
            _connectionString = $"Data Source={_dbPath};";
            InitDatabase();
        }

        private void InitDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string ddl = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    CompanyName TEXT,
                    Email TEXT,
                    Phone TEXT,
                    Address TEXT,
                    City TEXT,
                    Country TEXT DEFAULT 'India',
                    GSTIN TEXT,
                    CreatedAt TEXT
                );

                CREATE TABLE IF NOT EXISTS Invoices (
                    InvoiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT UNIQUE NOT NULL,
                    CustomerId INTEGER,
                    CustomerName TEXT NOT NULL,
                    CompanyName TEXT,
                    CustomerPhone TEXT,
                    CustomerEmail TEXT,
                    BillingAddress TEXT,
                    CustomerGSTIN TEXT,
                    InvoiceDate TEXT NOT NULL,
                    DueDate TEXT NOT NULL,
                    PaymentTerms TEXT,
                    Status TEXT NOT NULL,
                    SubTotal REAL NOT NULL,
                    ItemDiscountTotal REAL NOT NULL,
                    OverallDiscount REAL NOT NULL,
                    TaxableAmount REAL NOT NULL,
                    TaxRate REAL NOT NULL,
                    TaxAmount REAL NOT NULL,
                    CgstAmount REAL NOT NULL,
                    SgstAmount REAL NOT NULL,
                    IgstAmount REAL NOT NULL,
                    IsInterState INTEGER NOT NULL,
                    GrandTotal REAL NOT NULL,
                    AmountPaid REAL NOT NULL,
                    BalanceDue REAL NOT NULL,
                    Notes TEXT,
                    TermsAndConditions TEXT,
                    CreatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS InvoiceItems (
                    InvoiceItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER NOT NULL,
                    ServiceName TEXT NOT NULL,
                    Description TEXT,
                    Quantity REAL NOT NULL,
                    UnitPrice REAL NOT NULL,
                    DiscountPercent REAL NOT NULL,
                    TaxPercent REAL NOT NULL,
                    LineTotal REAL NOT NULL,
                    FOREIGN KEY(InvoiceId) REFERENCES Invoices(InvoiceId) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Payments (
                    PaymentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER NOT NULL,
                    InvoiceNumber TEXT NOT NULL,
                    CustomerName TEXT NOT NULL,
                    PaymentDate TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    PaymentMethod TEXT NOT NULL,
                    ReferenceNumber TEXT,
                    Notes TEXT,
                    RecordedBy TEXT,
                    FOREIGN KEY(InvoiceId) REFERENCES Invoices(InvoiceId) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS BusinessSettings (
                    SettingKey TEXT PRIMARY KEY,
                    SettingValue TEXT
                );
            ";

            using var cmd = new SqliteCommand(ddl, conn);
            cmd.ExecuteNonQuery();

            SeedDefaultDataIfEmpty(conn);
        }

        private void SeedDefaultDataIfEmpty(SqliteConnection conn)
        {
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Invoices", conn);
            long count = (long)(checkCmd.ExecuteScalar() ?? 0L);
            if (count > 0) return;

            // Seed Customers
            var custCmd = new SqliteCommand(@"
                INSERT INTO Customers (Name, CompanyName, Email, Phone, Address, City, Country, GSTIN, CreatedAt)
                VALUES 
                ('Aarav Sharma', 'TechCorp Solutions Pvt Ltd', 'aarav@techcorp.in', '+91 98250 11223', 'A-402 Shapath Hexa, SG Highway', 'Ahmedabad', 'India', '24AABCT1234F1Z1', datetime('now')),
                ('Neha Patel', 'CloudWave Digital Media', 'neha@cloudwave.io', '+91 97120 44556', '801 Titanium City Centre, Prahladnagar', 'Ahmedabad', 'India', '24AAACP5678B1Z8', datetime('now')),
                ('Vikram Malhotra', 'Malhotra Logistics & Supply', 'vikram@malhotralogistics.com', '+91 99090 77889', 'Plot 18, GIDC Industrial Estate', 'Vadodara', 'India', '24AAAFM9012D1Z3', datetime('now')),
                ('Priya Nair', 'Vertex Financial Advisory', 'priya@vertexfin.com', '+91 98450 33445', 'B-12 Brigade Gateway, Rajajinagar', 'Bengaluru', 'India', '29AAACV3456K1Z9', datetime('now')),
                ('Rohan Mehta', 'Mehta Design & Branding Studio', 'rohan@mehtadesign.co', '+91 98200 99887', '5th Floor, Trade World, BKC', 'Mumbai', 'India', '27AAAPM7890M1Z2', datetime('now'));
            ", conn);
            custCmd.ExecuteNonQuery();

            // Seed Invoices and Items with Transactions
            SeedInvoiceWithItems(conn, "INV-2026-0001", 1, "Aarav Sharma", "TechCorp Solutions Pvt Ltd", "aarav@techcorp.in", "+91 98250 11223", "A-402 Shapath Hexa, SG Highway, Ahmedabad", "24AABCT1234F1Z1",
                DateTime.Today.AddDays(-20), DateTime.Today.AddDays(-5), "Net 15", "Paid", 0m, 18m, false,
                new[]
                {
                    ("Enterprise Software License", "Annual license for Cloud CRM Suite (10 Users)", 1m, 45000.00m, 5m, 18m),
                    ("Custom Software Development", "Module development for automated GST billing", 1m, 35000.00m, 0m, 18m)
                },
                new[]
                {
                    (91745.00m, "Bank Transfer", "NEFT/HDFC/20261001/00823", "Payment cleared via HDFC corporate net banking")
                });

            SeedInvoiceWithItems(conn, "INV-2026-0002", 2, "Neha Patel", "CloudWave Digital Media", "neha@cloudwave.io", "+91 97120 44556", "801 Titanium City Centre, Prahladnagar, Ahmedabad", "24AAACP5678B1Z8",
                DateTime.Today.AddDays(-12), DateTime.Today.AddDays(3), "Net 15", "Partially Paid", 2000.00m, 18m, false,
                new[]
                {
                    ("Cloud Subscription Tier 3", "AWS Managed Kubernetes cluster & Cloud Ops Booster", 2m, 22000.00m, 0m, 18m),
                    ("Digital Signature Certificate", "Class 3 DSC (2 Years Validity with USB Crypto Token)", 3m, 2500.00m, 10m, 18m)
                },
                new[]
                {
                    (30000.00m, "UPI", "UPI/6290192841/neha@okaxis", "Advance token payment received via PhonePe UPI")
                });

            SeedInvoiceWithItems(conn, "INV-2026-0003", 3, "Vikram Malhotra", "Malhotra Logistics & Supply", "vikram@malhotralogistics.com", "+91 99090 77889", "Plot 18, GIDC Industrial Estate, Vadodara", "24AAAFM9012D1Z3",
                DateTime.Today.AddDays(-8), DateTime.Today.AddDays(7), "Net 15", "Unpaid", 0m, 18m, false,
                new[]
                {
                    ("Annual Maintenance Contract (AMC)", "24x7 Server Monitoring, Patching & Security Audit AMC", 1m, 50000.00m, 5m, 18m),
                    ("Cloud Backup & Disaster Recovery", "Offsite encrypted backup replication 5TB", 12m, 1500.00m, 0m, 18m)
                },
                Array.Empty<(decimal, string, string, string)>());

            SeedInvoiceWithItems(conn, "INV-2026-0004", 4, "Priya Nair", "Vertex Financial Advisory", "priya@vertexfin.com", "+91 98450 33445", "B-12 Brigade Gateway, Rajajinagar, Bengaluru", "29AAACV3456K1Z9",
                DateTime.Today.AddDays(-35), DateTime.Today.AddDays(-20), "Net 15", "Overdue", 0m, 18m, true,
                new[]
                {
                    ("Custom Branding & UI/UX Package", "Complete corporate identity system, iconography & UI kit", 1m, 60000.00m, 0m, 18m)
                },
                Array.Empty<(decimal, string, string, string)>());

            SeedInvoiceWithItems(conn, "INV-2026-0005", 5, "Rohan Mehta", "Mehta Design & Branding Studio", "rohan@mehtadesign.co", "+91 98200 99887", "5th Floor, BKC, Mumbai", "27AAAPM7890M1Z2",
                DateTime.Today, DateTime.Today.AddDays(30), "Net 30", "Draft", 0m, 18m, true,
                new[]
                {
                    ("Software Consulting & Architecture Review", "Cloud-native microservices architecture blueprint", 40m, 1800.00m, 10m, 18m)
                },
                Array.Empty<(decimal, string, string, string)>());
        }

        private void SeedInvoiceWithItems(
            SqliteConnection conn,
            string invNo, int custId, string custName, string compName, string email, string phone, string address, string gstin,
            DateTime invDate, DateTime dueDate, string terms, string status, decimal overallDisc, decimal taxRate, bool isInterstate,
            (string name, string desc, decimal qty, decimal rate, decimal disc, decimal tax)[] items,
            (decimal amount, string method, string refNo, string notes)[] payments)
        {
            var invoice = new Invoice
            {
                InvoiceNumber = invNo,
                CustomerId = custId,
                CustomerName = custName,
                CompanyName = compName,
                CustomerEmail = email,
                CustomerPhone = phone,
                BillingAddress = address,
                CustomerGSTIN = gstin,
                InvoiceDate = invDate,
                DueDate = dueDate,
                PaymentTerms = terms,
                Status = status,
                OverallDiscount = overallDisc,
                TaxRate = taxRate,
                IsInterState = isInterstate,
                CreatedAt = invDate
            };

            foreach (var item in items)
            {
                invoice.Items.Add(new InvoiceItem(item.name, item.desc, item.qty, item.rate, item.disc, item.tax));
            }

            foreach (var p in payments)
            {
                invoice.Payments.Add(new Payment(0, invNo, custName, p.amount, p.method, p.refNo, p.notes));
            }

            invoice.RecalculateTotals();
            if (status == "Draft") invoice.Status = "Draft";
            if (status == "Overdue") invoice.Status = "Overdue";

            // Save to DB
            using var trans = conn.BeginTransaction();
            string insertInv = @"
                INSERT INTO Invoices (
                    InvoiceNumber, CustomerId, CustomerName, CompanyName, CustomerPhone, CustomerEmail, 
                    BillingAddress, CustomerGSTIN, InvoiceDate, DueDate, PaymentTerms, Status, 
                    SubTotal, ItemDiscountTotal, OverallDiscount, TaxableAmount, TaxRate, TaxAmount, 
                    CgstAmount, SgstAmount, IgstAmount, IsInterState, GrandTotal, AmountPaid, BalanceDue, 
                    Notes, TermsAndConditions, CreatedAt
                ) VALUES (
                    @invNo, @custId, @custName, @compName, @phone, @email, 
                    @addr, @gstin, @invDate, @dueDate, @terms, @status, 
                    @subTotal, @itemDisc, @overallDisc, @taxable, @taxRate, @taxAmt, 
                    @cgst, @sgst, @igst, @interstate, @grandTotal, @amtPaid, @balDue, 
                    @notes, @tandc, @createdAt
                );
                SELECT last_insert_rowid();";

            using var cmdInv = new SqliteCommand(insertInv, conn, trans);
            cmdInv.Parameters.AddWithValue("@invNo", invoice.InvoiceNumber);
            cmdInv.Parameters.AddWithValue("@custId", invoice.CustomerId);
            cmdInv.Parameters.AddWithValue("@custName", invoice.CustomerName);
            cmdInv.Parameters.AddWithValue("@compName", invoice.CompanyName);
            cmdInv.Parameters.AddWithValue("@phone", invoice.CustomerPhone);
            cmdInv.Parameters.AddWithValue("@email", invoice.CustomerEmail);
            cmdInv.Parameters.AddWithValue("@addr", invoice.BillingAddress);
            cmdInv.Parameters.AddWithValue("@gstin", invoice.CustomerGSTIN);
            cmdInv.Parameters.AddWithValue("@invDate", invoice.InvoiceDate.ToString("yyyy-MM-dd"));
            cmdInv.Parameters.AddWithValue("@dueDate", invoice.DueDate.ToString("yyyy-MM-dd"));
            cmdInv.Parameters.AddWithValue("@terms", invoice.PaymentTerms);
            cmdInv.Parameters.AddWithValue("@status", invoice.Status);
            cmdInv.Parameters.AddWithValue("@subTotal", (double)invoice.SubTotal);
            cmdInv.Parameters.AddWithValue("@itemDisc", (double)invoice.ItemDiscountTotal);
            cmdInv.Parameters.AddWithValue("@overallDisc", (double)invoice.OverallDiscount);
            cmdInv.Parameters.AddWithValue("@taxable", (double)invoice.TaxableAmount);
            cmdInv.Parameters.AddWithValue("@taxRate", (double)invoice.TaxRate);
            cmdInv.Parameters.AddWithValue("@taxAmt", (double)invoice.TaxAmount);
            cmdInv.Parameters.AddWithValue("@cgst", (double)invoice.CgstAmount);
            cmdInv.Parameters.AddWithValue("@sgst", (double)invoice.SgstAmount);
            cmdInv.Parameters.AddWithValue("@igst", (double)invoice.IgstAmount);
            cmdInv.Parameters.AddWithValue("@interstate", invoice.IsInterState ? 1 : 0);
            cmdInv.Parameters.AddWithValue("@grandTotal", (double)invoice.GrandTotal);
            cmdInv.Parameters.AddWithValue("@amtPaid", (double)invoice.AmountPaid);
            cmdInv.Parameters.AddWithValue("@balDue", (double)invoice.BalanceDue);
            cmdInv.Parameters.AddWithValue("@notes", invoice.Notes);
            cmdInv.Parameters.AddWithValue("@tandc", invoice.TermsAndConditions);
            cmdInv.Parameters.AddWithValue("@createdAt", invoice.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            long newId = (long)(cmdInv.ExecuteScalar() ?? 0L);
            invoice.InvoiceId = (int)newId;

            // Insert Items
            foreach (var item in invoice.Items)
            {
                string insertItem = @"
                    INSERT INTO InvoiceItems (InvoiceId, ServiceName, Description, Quantity, UnitPrice, DiscountPercent, TaxPercent, LineTotal)
                    VALUES (@invId, @name, @desc, @qty, @price, @disc, @tax, @total)";
                using var cmdItem = new SqliteCommand(insertItem, conn, trans);
                cmdItem.Parameters.AddWithValue("@invId", invoice.InvoiceId);
                cmdItem.Parameters.AddWithValue("@name", item.ServiceName);
                cmdItem.Parameters.AddWithValue("@desc", item.Description);
                cmdItem.Parameters.AddWithValue("@qty", (double)item.Quantity);
                cmdItem.Parameters.AddWithValue("@price", (double)item.UnitPrice);
                cmdItem.Parameters.AddWithValue("@disc", (double)item.DiscountPercent);
                cmdItem.Parameters.AddWithValue("@tax", (double)item.TaxPercent);
                cmdItem.Parameters.AddWithValue("@total", (double)item.LineTotal);
                cmdItem.ExecuteNonQuery();
            }

            // Insert Payments
            foreach (var p in invoice.Payments)
            {
                string insertPay = @"
                    INSERT INTO Payments (InvoiceId, InvoiceNumber, CustomerName, PaymentDate, Amount, PaymentMethod, ReferenceNumber, Notes, RecordedBy)
                    VALUES (@invId, @invNo, @custName, @payDate, @amount, @method, @refNo, @notes, @recordedBy)";
                using var cmdPay = new SqliteCommand(insertPay, conn, trans);
                cmdPay.Parameters.AddWithValue("@invId", invoice.InvoiceId);
                cmdPay.Parameters.AddWithValue("@invNo", invoice.InvoiceNumber);
                cmdPay.Parameters.AddWithValue("@custName", invoice.CustomerName);
                cmdPay.Parameters.AddWithValue("@payDate", p.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmdPay.Parameters.AddWithValue("@amount", (double)p.Amount);
                cmdPay.Parameters.AddWithValue("@method", p.PaymentMethod);
                cmdPay.Parameters.AddWithValue("@refNo", p.ReferenceNumber);
                cmdPay.Parameters.AddWithValue("@notes", p.Notes);
                cmdPay.Parameters.AddWithValue("@recordedBy", p.RecordedBy);
                cmdPay.ExecuteNonQuery();
            }

            trans.Commit();
        }

        #region Dashboard Metrics
        public async Task<SalesDashboardMetrics> GetSalesDashboardMetricsAsync()
        {
            var metrics = new SalesDashboardMetrics();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    COALESCE(SUM(CASE WHEN Status != 'Cancelled' THEN GrandTotal ELSE 0 END), 0) as TotalSales,
                    COUNT(CASE WHEN Status != 'Cancelled' THEN 1 END) as TotalInvoices,
                    COUNT(CASE WHEN Status = 'Paid' THEN 1 END) as PaidInvoices,
                    COUNT(CASE WHEN Status = 'Unpaid' THEN 1 END) as UnpaidInvoices,
                    COUNT(CASE WHEN Status = 'Partially Paid' THEN 1 END) as PartiallyPaidInvoices,
                    COUNT(CASE WHEN Status = 'Overdue' THEN 1 END) as OverdueInvoices,
                    COALESCE(SUM(CASE WHEN Status != 'Cancelled' THEN BalanceDue ELSE 0 END), 0) as OutstandingBalance,
                    COALESCE(SUM(CASE WHEN Status != 'Cancelled' THEN AmountPaid ELSE 0 END), 0) as TotalCollected
                FROM Invoices";

            using var cmd = new SqliteCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                metrics.TotalSales = Convert.ToDecimal(reader["TotalSales"]);
                metrics.TotalInvoices = Convert.ToInt32(reader["TotalInvoices"]);
                metrics.PaidInvoices = Convert.ToInt32(reader["PaidInvoices"]);
                metrics.UnpaidInvoices = Convert.ToInt32(reader["UnpaidInvoices"]);
                metrics.PartiallyPaidInvoices = Convert.ToInt32(reader["PartiallyPaidInvoices"]);
                metrics.OverdueInvoices = Convert.ToInt32(reader["OverdueInvoices"]);
                metrics.OutstandingBalance = Convert.ToDecimal(reader["OutstandingBalance"]);
                metrics.TotalCollected = Convert.ToDecimal(reader["TotalCollected"]);
            }

            metrics.RecentInvoices = await GetInvoicesAsync(limit: 6);
            return metrics;
        }
        #endregion

        #region Invoices CRUD
        public async Task<List<Invoice>> GetInvoicesAsync(string? search = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int limit = 0)
        {
            var list = new List<Invoice>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT * FROM Invoices WHERE 1=1";
            var parameters = new List<SqliteParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (InvoiceNumber LIKE @search OR CustomerName LIKE @search OR CompanyName LIKE @search)";
                parameters.Add(new SqliteParameter("@search", $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query += " AND Status = @status";
                parameters.Add(new SqliteParameter("@status", status));
            }

            if (fromDate.HasValue)
            {
                query += " AND InvoiceDate >= @fromDate";
                parameters.Add(new SqliteParameter("@fromDate", fromDate.Value.ToString("yyyy-MM-dd")));
            }

            if (toDate.HasValue)
            {
                query += " AND InvoiceDate <= @toDate";
                parameters.Add(new SqliteParameter("@toDate", toDate.Value.ToString("yyyy-MM-dd")));
            }

            query += " ORDER BY InvoiceId DESC";
            if (limit > 0)
            {
                query += $" LIMIT {limit}";
            }

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapInvoiceFromReader(reader));
            }

            return list;
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM Invoices WHERE InvoiceId = @id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", invoiceId);

            Invoice? invoice = null;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    invoice = MapInvoiceFromReader(reader);
                }
            }

            if (invoice != null)
            {
                // Load items
                string itemQuery = "SELECT * FROM InvoiceItems WHERE InvoiceId = @id ORDER BY InvoiceItemId ASC";
                using var itemCmd = new SqliteCommand(itemQuery, conn);
                itemCmd.Parameters.AddWithValue("@id", invoiceId);
                using var itemReader = await itemCmd.ExecuteReaderAsync();
                while (await itemReader.ReadAsync())
                {
                    invoice.Items.Add(new InvoiceItem
                    {
                        InvoiceItemId = Convert.ToInt32(itemReader["InvoiceItemId"]),
                        InvoiceId = Convert.ToInt32(itemReader["InvoiceId"]),
                        ServiceName = itemReader["ServiceName"].ToString() ?? "",
                        Description = itemReader["Description"].ToString() ?? "",
                        Quantity = Convert.ToDecimal(itemReader["Quantity"]),
                        UnitPrice = Convert.ToDecimal(itemReader["UnitPrice"]),
                        DiscountPercent = Convert.ToDecimal(itemReader["DiscountPercent"]),
                        TaxPercent = Convert.ToDecimal(itemReader["TaxPercent"]),
                        LineTotal = Convert.ToDecimal(itemReader["LineTotal"])
                    });
                }

                // Load payments
                string payQuery = "SELECT * FROM Payments WHERE InvoiceId = @id ORDER BY PaymentDate DESC";
                using var payCmd = new SqliteCommand(payQuery, conn);
                payCmd.Parameters.AddWithValue("@id", invoiceId);
                using var payReader = await payCmd.ExecuteReaderAsync();
                while (await payReader.ReadAsync())
                {
                    invoice.Payments.Add(new Payment
                    {
                        PaymentId = Convert.ToInt32(payReader["PaymentId"]),
                        InvoiceId = Convert.ToInt32(payReader["InvoiceId"]),
                        InvoiceNumber = payReader["InvoiceNumber"].ToString() ?? "",
                        CustomerName = payReader["CustomerName"].ToString() ?? "",
                        PaymentDate = DateTime.Parse(payReader["PaymentDate"].ToString() ?? DateTime.Now.ToString()),
                        Amount = Convert.ToDecimal(payReader["Amount"]),
                        PaymentMethod = payReader["PaymentMethod"].ToString() ?? "UPI",
                        ReferenceNumber = payReader["ReferenceNumber"].ToString() ?? "",
                        Notes = payReader["Notes"].ToString() ?? "",
                        RecordedBy = payReader["RecordedBy"].ToString() ?? "Admin"
                    });
                }
            }

            return invoice;
        }

        public async Task<string> GetNextInvoiceNumberAsync()
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT MAX(InvoiceId) FROM Invoices";
            using var cmd = new SqliteCommand(query, conn);
            var maxId = await cmd.ExecuteScalarAsync();
            int nextId = (maxId != null && int.TryParse(maxId.ToString(), out int val)) ? val + 1 : 1;
            int year = DateTime.Now.Year;
            return $"INV-{year}-{nextId:D4}";
        }

        public async Task<bool> SaveInvoiceAsync(Invoice invoice)
        {
            invoice.RecalculateTotals();

            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                if (invoice.InvoiceId <= 0)
                {
                    // Ensure unique invoice number
                    string checkSql = "SELECT COUNT(*) FROM Invoices WHERE InvoiceNumber = @num";
                    using var checkCmd = new SqliteCommand(checkSql, conn, trans);
                    checkCmd.Parameters.AddWithValue("@num", invoice.InvoiceNumber);
                    long exists = (long)(await checkCmd.ExecuteScalarAsync() ?? 0L);
                    if (exists > 0)
                    {
                        invoice.InvoiceNumber = await GetNextInvoiceNumberAsync();
                    }

                    string insertSql = @"
                        INSERT INTO Invoices (
                            InvoiceNumber, CustomerId, CustomerName, CompanyName, CustomerPhone, CustomerEmail, 
                            BillingAddress, CustomerGSTIN, InvoiceDate, DueDate, PaymentTerms, Status, 
                            SubTotal, ItemDiscountTotal, OverallDiscount, TaxableAmount, TaxRate, TaxAmount, 
                            CgstAmount, SgstAmount, IgstAmount, IsInterState, GrandTotal, AmountPaid, BalanceDue, 
                            Notes, TermsAndConditions, CreatedAt
                        ) VALUES (
                            @invNo, @custId, @custName, @compName, @phone, @email, 
                            @addr, @gstin, @invDate, @dueDate, @terms, @status, 
                            @subTotal, @itemDisc, @overallDisc, @taxable, @taxRate, @taxAmt, 
                            @cgst, @sgst, @igst, @interstate, @grandTotal, @amtPaid, @balDue, 
                            @notes, @tandc, @createdAt
                        );
                        SELECT last_insert_rowid();";

                    using var cmd = new SqliteCommand(insertSql, conn, trans);
                    BindInvoiceParameters(cmd, invoice);
                    long newId = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
                    invoice.InvoiceId = (int)newId;
                }
                else
                {
                    string updateSql = @"
                        UPDATE Invoices SET 
                            CustomerId = @custId, CustomerName = @custName, CompanyName = @compName, 
                            CustomerPhone = @phone, CustomerEmail = @email, BillingAddress = @addr, CustomerGSTIN = @gstin, 
                            InvoiceDate = @invDate, DueDate = @dueDate, PaymentTerms = @terms, Status = @status, 
                            SubTotal = @subTotal, ItemDiscountTotal = @itemDisc, OverallDiscount = @overallDisc, 
                            TaxableAmount = @taxable, TaxRate = @taxRate, TaxAmount = @taxAmt, 
                            CgstAmount = @cgst, SgstAmount = @sgst, IgstAmount = @igst, IsInterState = @interstate, 
                            GrandTotal = @grandTotal, AmountPaid = @amtPaid, BalanceDue = @balDue, 
                            Notes = @notes, TermsAndConditions = @tandc
                        WHERE InvoiceId = @invId";

                    using var cmd = new SqliteCommand(updateSql, conn, trans);
                    cmd.Parameters.AddWithValue("@invId", invoice.InvoiceId);
                    BindInvoiceParameters(cmd, invoice);
                    await cmd.ExecuteNonQueryAsync();

                    // Delete existing items to replace with updated list
                    string deleteItems = "DELETE FROM InvoiceItems WHERE InvoiceId = @invId";
                    using var delCmd = new SqliteCommand(deleteItems, conn, trans);
                    delCmd.Parameters.AddWithValue("@invId", invoice.InvoiceId);
                    await delCmd.ExecuteNonQueryAsync();
                }

                // Insert Items
                foreach (var item in invoice.Items)
                {
                    string insertItem = @"
                        INSERT INTO InvoiceItems (InvoiceId, ServiceName, Description, Quantity, UnitPrice, DiscountPercent, TaxPercent, LineTotal)
                        VALUES (@invId, @name, @desc, @qty, @price, @disc, @tax, @total)";
                    using var itemCmd = new SqliteCommand(insertItem, conn, trans);
                    itemCmd.Parameters.AddWithValue("@invId", invoice.InvoiceId);
                    itemCmd.Parameters.AddWithValue("@name", item.ServiceName);
                    itemCmd.Parameters.AddWithValue("@desc", item.Description);
                    itemCmd.Parameters.AddWithValue("@qty", (double)item.Quantity);
                    itemCmd.Parameters.AddWithValue("@price", (double)item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@disc", (double)item.DiscountPercent);
                    itemCmd.Parameters.AddWithValue("@tax", (double)item.TaxPercent);
                    itemCmd.Parameters.AddWithValue("@total", (double)item.LineTotal);
                    await itemCmd.ExecuteNonQueryAsync();
                }

                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> CancelInvoiceAsync(int invoiceId, string reason = "")
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                UPDATE Invoices 
                SET Status = 'Cancelled', 
                    Notes = CASE WHEN @reason != '' THEN COALESCE(Notes, '') || ' [Cancelled: ' || @reason || ']' ELSE Notes END 
                WHERE InvoiceId = @id";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", invoiceId);
            cmd.Parameters.AddWithValue("@reason", reason);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> RecordPaymentAsync(Payment payment)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                // 1. Insert Payment
                string insertPay = @"
                    INSERT INTO Payments (InvoiceId, InvoiceNumber, CustomerName, PaymentDate, Amount, PaymentMethod, ReferenceNumber, Notes, RecordedBy)
                    VALUES (@invId, @invNo, @custName, @payDate, @amount, @method, @refNo, @notes, @recordedBy);
                    SELECT last_insert_rowid();";
                using var payCmd = new SqliteCommand(insertPay, conn, trans);
                payCmd.Parameters.AddWithValue("@invId", payment.InvoiceId);
                payCmd.Parameters.AddWithValue("@invNo", payment.InvoiceNumber);
                payCmd.Parameters.AddWithValue("@custName", payment.CustomerName);
                payCmd.Parameters.AddWithValue("@payDate", payment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                payCmd.Parameters.AddWithValue("@amount", (double)payment.Amount);
                payCmd.Parameters.AddWithValue("@method", payment.PaymentMethod);
                payCmd.Parameters.AddWithValue("@refNo", payment.ReferenceNumber);
                payCmd.Parameters.AddWithValue("@notes", payment.Notes);
                payCmd.Parameters.AddWithValue("@recordedBy", payment.RecordedBy);
                long payId = (long)(await payCmd.ExecuteScalarAsync() ?? 0L);
                payment.PaymentId = (int)payId;

                // 2. Fetch Invoice financial figures
                string selectInv = "SELECT GrandTotal, Status, DueDate FROM Invoices WHERE InvoiceId = @id";
                using var selCmd = new SqliteCommand(selectInv, conn, trans);
                selCmd.Parameters.AddWithValue("@id", payment.InvoiceId);

                decimal grandTotal = 0m;
                string currentStatus = "Unpaid";
                DateTime dueDate = DateTime.Today;

                using (var reader = await selCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        grandTotal = Convert.ToDecimal(reader["GrandTotal"]);
                        currentStatus = reader["Status"].ToString() ?? "Unpaid";
                        dueDate = DateTime.Parse(reader["DueDate"].ToString() ?? DateTime.Today.ToString());
                    }
                }

                // 3. Compute total payments against this invoice
                string sumPay = "SELECT COALESCE(SUM(Amount), 0) FROM Payments WHERE InvoiceId = @id";
                using var sumCmd = new SqliteCommand(sumPay, conn, trans);
                sumCmd.Parameters.AddWithValue("@id", payment.InvoiceId);
                decimal totalPaid = Convert.ToDecimal(await sumCmd.ExecuteScalarAsync() ?? 0.0);
                decimal balanceDue = Math.Max(0m, grandTotal - totalPaid);

                // Determine new status
                string newStatus = currentStatus;
                if (currentStatus != "Cancelled")
                {
                    if (totalPaid >= grandTotal && grandTotal > 0)
                        newStatus = "Paid";
                    else if (totalPaid > 0)
                        newStatus = "Partially Paid";
                    else if (dueDate < DateTime.Today)
                        newStatus = "Overdue";
                    else
                        newStatus = "Unpaid";
                }

                // 4. Update Invoice
                string updateInv = @"
                    UPDATE Invoices 
                    SET AmountPaid = @paid, BalanceDue = @bal, Status = @status 
                    WHERE InvoiceId = @id";
                using var updCmd = new SqliteCommand(updateInv, conn, trans);
                updCmd.Parameters.AddWithValue("@paid", (double)totalPaid);
                updCmd.Parameters.AddWithValue("@bal", (double)balanceDue);
                updCmd.Parameters.AddWithValue("@status", newStatus);
                updCmd.Parameters.AddWithValue("@id", payment.InvoiceId);
                await updCmd.ExecuteNonQueryAsync();

                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<List<Payment>> GetPaymentsForInvoiceAsync(int invoiceId)
        {
            var list = new List<Payment>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM Payments WHERE InvoiceId = @id ORDER BY PaymentDate DESC";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", invoiceId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Payment
                {
                    PaymentId = Convert.ToInt32(reader["PaymentId"]),
                    InvoiceId = Convert.ToInt32(reader["InvoiceId"]),
                    InvoiceNumber = reader["InvoiceNumber"].ToString() ?? "",
                    CustomerName = reader["CustomerName"].ToString() ?? "",
                    PaymentDate = DateTime.Parse(reader["PaymentDate"].ToString() ?? DateTime.Now.ToString()),
                    Amount = Convert.ToDecimal(reader["Amount"]),
                    PaymentMethod = reader["PaymentMethod"].ToString() ?? "UPI",
                    ReferenceNumber = reader["ReferenceNumber"].ToString() ?? "",
                    Notes = reader["Notes"].ToString() ?? "",
                    RecordedBy = reader["RecordedBy"].ToString() ?? "Admin"
                });
            }
            return list;
        }

        public async Task<List<Invoice>> GetCustomerInvoicesAsync(int customerId)
        {
            var list = new List<Invoice>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM Invoices WHERE CustomerId = @id ORDER BY InvoiceDate DESC";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", customerId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapInvoiceFromReader(reader));
            }
            return list;
        }

        public async Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT COALESCE(SUM(BalanceDue), 0) FROM Invoices WHERE CustomerId = @id AND Status != 'Cancelled'";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", customerId);

            var val = await cmd.ExecuteScalarAsync();
            return val != null ? Convert.ToDecimal(val) : 0m;
        }
        #endregion

        #region Customers
        public async Task<List<Customer>> GetAllCustomersAsync(string? search = null)
        {
            var list = new List<Customer>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM Customers";
            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " WHERE Name LIKE @search OR CompanyName LIKE @search OR Email LIKE @search OR City LIKE @search";
            }
            query += " ORDER BY Name ASC";

            using var cmd = new SqliteCommand(query, conn);
            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@search", $"%{search}%");
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Customer
                {
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    Name = reader["Name"].ToString() ?? "",
                    CompanyName = reader["CompanyName"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    Phone = reader["Phone"].ToString() ?? "",
                    Address = reader["Address"].ToString() ?? "",
                    City = reader["City"].ToString() ?? "",
                    Country = reader["Country"].ToString() ?? "India",
                    GSTIN = reader["GSTIN"].ToString() ?? ""
                });
            }
            return list;
        }

        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                INSERT INTO Customers (Name, CompanyName, Email, Phone, Address, City, Country, GSTIN, CreatedAt)
                VALUES (@name, @compName, @email, @phone, @addr, @city, @country, @gstin, datetime('now'));
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", customer.Name);
            cmd.Parameters.AddWithValue("@compName", customer.CompanyName);
            cmd.Parameters.AddWithValue("@email", customer.Email);
            cmd.Parameters.AddWithValue("@phone", customer.Phone);
            cmd.Parameters.AddWithValue("@addr", customer.Address);
            cmd.Parameters.AddWithValue("@city", customer.City);
            cmd.Parameters.AddWithValue("@country", customer.Country);
            cmd.Parameters.AddWithValue("@gstin", customer.GSTIN);

            long id = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
            customer.CustomerId = (int)id;
            return true;
        }
        #endregion

        #region Business Profile
        public async Task<BusinessProfile> GetBusinessProfileAsync()
        {
            var profile = new BusinessProfile();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT SettingKey, SettingValue FROM BusinessSettings";
            using var cmd = new SqliteCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string key = reader["SettingKey"].ToString() ?? "";
                string val = reader["SettingValue"].ToString() ?? "";

                switch (key)
                {
                    case "BusinessName": profile.BusinessName = val; break;
                    case "Tagline": profile.Tagline = val; break;
                    case "Address": profile.Address = val; break;
                    case "City": profile.City = val; break;
                    case "Country": profile.Country = val; break;
                    case "Phone": profile.Phone = val; break;
                    case "Email": profile.Email = val; break;
                    case "GSTIN": profile.GSTIN = val; break;
                    case "BankName": profile.BankName = val; break;
                    case "AccountNumber": profile.AccountNumber = val; break;
                    case "IFSC": profile.IFSC = val; break;
                    case "UPIId": profile.UPIId = val; break;
                    case "DefaultTerms": profile.DefaultTerms = val; break;
                }
            }

            return profile;
        }

        public async Task<bool> SaveBusinessProfileAsync(BusinessProfile profile)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            string sql = "INSERT OR REPLACE INTO BusinessSettings (SettingKey, SettingValue) VALUES (@key, @val)";

            var dict = new Dictionary<string, string>
            {
                ["BusinessName"] = profile.BusinessName,
                ["Tagline"] = profile.Tagline,
                ["Address"] = profile.Address,
                ["City"] = profile.City,
                ["Country"] = profile.Country,
                ["Phone"] = profile.Phone,
                ["Email"] = profile.Email,
                ["GSTIN"] = profile.GSTIN,
                ["BankName"] = profile.BankName,
                ["AccountNumber"] = profile.AccountNumber,
                ["IFSC"] = profile.IFSC,
                ["UPIId"] = profile.UPIId,
                ["DefaultTerms"] = profile.DefaultTerms
            };

            foreach (var kvp in dict)
            {
                using var cmd = new SqliteCommand(sql, conn, trans);
                cmd.Parameters.AddWithValue("@key", kvp.Key);
                cmd.Parameters.AddWithValue("@val", kvp.Value);
                await cmd.ExecuteNonQueryAsync();
            }

            trans.Commit();
            return true;
        }
        #endregion

        #region Helper Mappings
        private static void BindInvoiceParameters(SqliteCommand cmd, Invoice invoice)
        {
            cmd.Parameters.AddWithValue("@invNo", invoice.InvoiceNumber);
            cmd.Parameters.AddWithValue("@custId", invoice.CustomerId);
            cmd.Parameters.AddWithValue("@custName", invoice.CustomerName);
            cmd.Parameters.AddWithValue("@compName", invoice.CompanyName);
            cmd.Parameters.AddWithValue("@phone", invoice.CustomerPhone);
            cmd.Parameters.AddWithValue("@email", invoice.CustomerEmail);
            cmd.Parameters.AddWithValue("@addr", invoice.BillingAddress);
            cmd.Parameters.AddWithValue("@gstin", invoice.CustomerGSTIN);
            cmd.Parameters.AddWithValue("@invDate", invoice.InvoiceDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@dueDate", invoice.DueDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@terms", invoice.PaymentTerms);
            cmd.Parameters.AddWithValue("@status", invoice.Status);
            cmd.Parameters.AddWithValue("@subTotal", (double)invoice.SubTotal);
            cmd.Parameters.AddWithValue("@itemDisc", (double)invoice.ItemDiscountTotal);
            cmd.Parameters.AddWithValue("@overallDisc", (double)invoice.OverallDiscount);
            cmd.Parameters.AddWithValue("@taxable", (double)invoice.TaxableAmount);
            cmd.Parameters.AddWithValue("@taxRate", (double)invoice.TaxRate);
            cmd.Parameters.AddWithValue("@taxAmt", (double)invoice.TaxAmount);
            cmd.Parameters.AddWithValue("@cgst", (double)invoice.CgstAmount);
            cmd.Parameters.AddWithValue("@sgst", (double)invoice.SgstAmount);
            cmd.Parameters.AddWithValue("@igst", (double)invoice.IgstAmount);
            cmd.Parameters.AddWithValue("@interstate", invoice.IsInterState ? 1 : 0);
            cmd.Parameters.AddWithValue("@grandTotal", (double)invoice.GrandTotal);
            cmd.Parameters.AddWithValue("@amtPaid", (double)invoice.AmountPaid);
            cmd.Parameters.AddWithValue("@balDue", (double)invoice.BalanceDue);
            cmd.Parameters.AddWithValue("@notes", invoice.Notes);
            cmd.Parameters.AddWithValue("@tandc", invoice.TermsAndConditions);
            cmd.Parameters.AddWithValue("@createdAt", invoice.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private static Invoice MapInvoiceFromReader(SqliteDataReader reader)
        {
            return new Invoice
            {
                InvoiceId = Convert.ToInt32(reader["InvoiceId"]),
                InvoiceNumber = reader["InvoiceNumber"].ToString() ?? "",
                CustomerId = Convert.ToInt32(reader["CustomerId"]),
                CustomerName = reader["CustomerName"].ToString() ?? "",
                CompanyName = reader["CompanyName"].ToString() ?? "",
                CustomerPhone = reader["CustomerPhone"].ToString() ?? "",
                CustomerEmail = reader["CustomerEmail"].ToString() ?? "",
                BillingAddress = reader["BillingAddress"].ToString() ?? "",
                CustomerGSTIN = reader["CustomerGSTIN"].ToString() ?? "",
                InvoiceDate = DateTime.Parse(reader["InvoiceDate"].ToString() ?? DateTime.Today.ToString()),
                DueDate = DateTime.Parse(reader["DueDate"].ToString() ?? DateTime.Today.ToString()),
                PaymentTerms = reader["PaymentTerms"].ToString() ?? "Net 15",
                Status = reader["Status"].ToString() ?? "Unpaid",
                SubTotal = Convert.ToDecimal(reader["SubTotal"]),
                ItemDiscountTotal = Convert.ToDecimal(reader["ItemDiscountTotal"]),
                OverallDiscount = Convert.ToDecimal(reader["OverallDiscount"]),
                TaxableAmount = Convert.ToDecimal(reader["TaxableAmount"]),
                TaxRate = Convert.ToDecimal(reader["TaxRate"]),
                TaxAmount = Convert.ToDecimal(reader["TaxAmount"]),
                CgstAmount = Convert.ToDecimal(reader["CgstAmount"]),
                SgstAmount = Convert.ToDecimal(reader["SgstAmount"]),
                IgstAmount = Convert.ToDecimal(reader["IgstAmount"]),
                IsInterState = Convert.ToInt32(reader["IsInterState"]) == 1,
                GrandTotal = Convert.ToDecimal(reader["GrandTotal"]),
                AmountPaid = Convert.ToDecimal(reader["AmountPaid"]),
                BalanceDue = Convert.ToDecimal(reader["BalanceDue"]),
                Notes = reader["Notes"].ToString() ?? "",
                TermsAndConditions = reader["TermsAndConditions"].ToString() ?? "",
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString() ?? DateTime.Now.ToString())
            };
        }
        #endregion
    }
}
