-- =========================================================================
-- S TOM'S — Business Analytics Platform Database Setup Script
-- Target: Microsoft SQL Server 2019 / 2022 / LocalDB
-- Database: BusinessAnalyticsDB
-- =========================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BusinessAnalyticsDB')
BEGIN
    CREATE DATABASE BusinessAnalyticsDB;
END
GO

USE BusinessAnalyticsDB;
GO

-- 1. Users Table
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Administrator',
        AvatarInitials NVARCHAR(10) NOT NULL DEFAULT 'KK',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 2. Customers Table
IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        CustomerId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(50) NULL,
        Address NVARCHAR(255) NOT NULL,
        City NVARCHAR(100) NOT NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'USA',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 3. Products Table
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        ProductId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 4. Orders Table
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NULL FOREIGN KEY REFERENCES dbo.Customers(CustomerId) ON DELETE SET NULL,
        CustomerName NVARCHAR(100) NOT NULL,
        Address NVARCHAR(255) NOT NULL,
        OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(50) NOT NULL DEFAULT 'Delivered', -- Delivered, Processed, Cancelled
        TotalAmount DECIMAL(18,2) NOT NULL
    );
END
GO

-- 5. OrderItems Table
IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL FOREIGN KEY REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
        ProductId INT NULL FOREIGN KEY REFERENCES dbo.Products(ProductId),
        ProductName NVARCHAR(100) NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(18,2) NOT NULL
    );
END
GO

-- 6. Subscriptions Table
IF OBJECT_ID('dbo.Subscriptions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Subscriptions (
        SubscriptionId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NULL,
        PlanType NVARCHAR(50) NOT NULL DEFAULT 'Pro', -- Paid, Trial
        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        MonthlyFee DECIMAL(18,2) NOT NULL DEFAULT 49.99,
        StartDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 7. Invoices Table
IF OBJECT_ID('dbo.Invoices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Invoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NULL,
        InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
        Amount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Paid',
        DueDate DATETIME NOT NULL
    );
END
GO

-- 8. Messages Table
IF OBJECT_ID('dbo.Messages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Messages (
        MessageId INT IDENTITY(1,1) PRIMARY KEY,
        Sender NVARCHAR(100) NOT NULL,
        Subject NVARCHAR(150) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        SentDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 9. AppSettings Table
IF OBJECT_ID('dbo.AppSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppSettings (
        SettingKey NVARCHAR(50) PRIMARY KEY,
        SettingValue NVARCHAR(255) NOT NULL,
        UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- =========================================================================
-- SEED SAMPLE DATA (Matching Reference Analytics Dashboard)
-- =========================================================================

-- Seed Users
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, AvatarInitials)
    VALUES ('admin', 'admin123', 'Kristi Kamlykova', 'Administrator', 'KK');
END
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'hardik')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, AvatarInitials)
    VALUES ('hardik', '1234', 'Hardik Jariwala', 'Lead Full-Stack Developer', 'HJ');
END

-- Seed Customers
IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
    INSERT INTO dbo.Customers (Name, Email, Phone, Address, City, Country) VALUES
    ('Press', 'press@business.co.uk', '+44 20 7946 0912', 'London', 'London', 'UK'),
    ('Marina', 'marina@mancity.org', '+44 161 496 0192', 'Man city', 'Manchester', 'UK'),
    ('Alex', 'alex@techunknown.io', '+1 555 019 2834', 'Unknown', 'Chicago', 'USA'),
    ('Robert', 'robert.smith@nycorp.com', '+1 212 555 0148', 'New York', 'New York', 'USA'),
    ('Sophia Taylor', 'sophia@berlinventures.de', '+49 30 123456', 'Kurfürstendamm 12', 'Berlin', 'Germany'),
    ('Liam Johnson', 'liam@torontosoft.ca', '+1 416 555 0177', 'Bay Street 100', 'Toronto', 'Canada');
END

-- Seed Products
IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (Name, Category, Price, StockQuantity, Status) VALUES
    ('Enterprise Analytics Suite', 'Software', 1200.00, 150, 'Active'),
    ('Cloud Storage Booster 1TB', 'Cloud Services', 49.99, 500, 'Active'),
    ('AI Prediction Engine License', 'AI & ML', 850.00, 80, 'Active'),
    ('Developer API Access Key', 'Developer Tools', 199.00, 320, 'Active'),
    ('Security & Compliance Audit Pack', 'Security', 452.00, 45, 'Active'),
    ('Real-time Telemetry Dashboard', 'Hardware & IoT', 920.00, 60, 'Active');
END

-- Seed Orders (Matching Screenshot)
IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    INSERT INTO dbo.Orders (CustomerName, Address, OrderDate, Status, TotalAmount) VALUES
    ('Press', 'London', '2022-08-22', 'Delivered', 920.00),
    ('Marina', 'Man city', '2022-08-24', 'Processed', 452.00),
    ('Alex', 'Unknown', '2022-08-15', 'Cancelled', 1200.00),
    ('Robert', 'New York', '2022-08-03', 'Delivered', 1235.00),
    ('Sophia Taylor', 'Berlin', '2022-08-28', 'Delivered', 850.00),
    ('Liam Johnson', 'Toronto', '2022-08-30', 'Processed', 640.00);
END

-- Seed Messages
IF NOT EXISTS (SELECT 1 FROM dbo.Messages)
BEGIN
    INSERT INTO dbo.Messages (Sender, Subject, Content, IsRead, SentDate) VALUES
    ('System Monitoring', 'Monthly Sales Target Exceeded', 'August sales dynamics reached $302,562.23 surpassing forecast by 8.2%.', 0, GETDATE()),
    ('Payment Gateway', 'Fund Settlement Processed', 'Funds received settlement of $150,256.23 cleared successfully to primary bank account.', 0, DATEADD(HOUR, -3, GETDATE())),
    ('Support Desk', 'Customer Ticket #4890 Resolved', 'Customer Press verified delivery of invoice items and confirmed receipt.', 1, DATEADD(DAY, -1, GETDATE())),
    ('Security Guard', 'New Session from Visual Studio', 'User Kristi Kamlykova signed in successfully from local desktop workstation.', 1, DATEADD(DAY, -2, GETDATE()));
END

-- Seed AppSettings
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings)
BEGIN
    INSERT INTO dbo.AppSettings (SettingKey, SettingValue) VALUES
    ('Theme', 'Dark'),
    ('CompanyName', 'Business Analytics Inc.'),
    ('CurrencySymbol', '$'),
    ('DefaultDateRange', '01.08.2022 - 31.08.2022');
END
GO

PRINT 'BusinessAnalyticsDB schema and seed data created successfully!';
