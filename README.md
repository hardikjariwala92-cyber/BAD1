# BAD1 — S TOM'S Sales, Billing & Business Analytics Suite

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12-239120.svg?style=flat-square&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20Desktop-informational.svg?style=flat-square&logo=windows)](https://microsoft.com/windows)
[![Tests Passing](https://img.shields.io/badge/Tests-16%2F16%20Passing-brightgreen.svg?style=flat-square)](docs/TEST_REPORT.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

**BAD1** is a modern, high-performance desktop enterprise software suite built with **.NET 8.0 Windows Forms** and **C# 12**. Designed with **Clean Architecture**, a state-of-the-art **Dark/Light Theme Engine**, and rich interactive visualizations, BAD1 provides businesses with full-featured sales invoicing, CRM customer management, inventory control, and real-time business intelligence analytics.

GitHub Repository: [https://github.com/hardikjariwala92-cyber/BAD1](https://github.com/hardikjariwala92-cyber/BAD1)

---

## 🌟 Key Features

### 1. 📊 Dynamic Business Intelligence Dashboard
- **8 Live Database-Backed Metric Cards**: Real-time sales, revenue, active customers, orders, growth velocity, and subscription metrics.
- **Micro-Interactions & Hover Popups**: Smooth card elevation, glowing border highlights (`ThemeManager.AccentBlue`), and dynamic floating tooltips (`MetricHoverPopup`) displaying historical metrics, category breakdown, and growth badges.
- **Interactive Multi-Year Charts**:
  - **Sales Dynamics Bar Chart**: Anti-aliased vertical bar chart with glowing caps, gradient hover highlights, multi-year switching (`2021 ▼`, `2022 ▼`), and currency tooltips.
  - **User Activity Spline Curve**: High-precision Bezier curve rendering with nearest-point mouse tracking, vertical dashed guides, and user engagement tooltips.
  - **Donut Visualizers**: Multi-slice breakdown for active users and subscription tiers with 4px outward slice expansion on hover.

### 2. 🎨 Modern CSS & Bootstrap 5 Styled Customer Orders Table
- **Bootstrap 5 Table Dark Theme**: Seamless dark palette with `table-striped` alternating rows (`#191b26` vs `#14161f`) and subtle 1px dividers.
- **Interactive `table-hover`**: Instant smooth row highlighting (`#282d46`) on mouse movement.
- **Initials Avatar Badges**: Circular avatar badges in the `Profile` column displaying uppercase customer initials, dynamically color-coded by name hash across 6 vibrant palettes.
- **Rounded-Pill Status Badges**: Authentic Bootstrap `.badge rounded-pill` tags with glowing indicator dots:
  - 🟢 **Delivered**: Emerald green badge (`#163428` bg, `#10b981` border, `#34d399` text)
  - 🟡 **Processed**: Amber warning badge (`#342a12` bg, `#f59e0b` border, `#fbbf24` text)
  - 🔴 **Cancelled**: Crimson red badge (`#34181c` bg, `#ef4444` border, `#f87171` text)
- **Live Filtering & Search**: Instant customer/city search box, flat dark owner-drawn status filter dropdown, and 15-second background auto-refresh timer.

### 3. 🧾 Invoice & Billing Management Engine
- **Itemized Billing Grid**: Add, update, and manage services and products with instant quantity, unit price, and subtotal calculations.
- **Financial Calculations**: Automated subtotal, discount, GST/sales tax calculation, and net payable summaries.
- **PDF Generation & Export**: Generate clean, professional PDF invoices directly with embedded company branding and customer billing details (`InvoicePdfService`).
- **Payment Tracking**: Record partial and full payments with method tracking (Cash, UPI, Bank Transfer, Card) via `RecordPaymentDialog`.

### 4. 👥 Customer Relationship Management (CRM)
- Customer profiles with addresses, contact information, outstanding balances, and credit limits.
- Complete customer transaction history dialog (`CustomerHistoryDialog`) showing previous invoices and ledger entries.

### 5. 📦 Product & Inventory Management
- Product catalog management with SKU, pricing, category classifications, and inventory statuses.

### 6. 🌓 Seamless Dark & Light Theme System
- Built-in `ThemeManager` with curated high-contrast color tokens.
- Instant toggle between dark mode and light mode without reloading views.

---

## 🏗️ Architecture & Solution Structure

BAD1 is architected following **Clean Architecture** principles to separate UI presentation from business logic and data persistence:

```text
BAD1/
├── src/
│   ├── Controls/                      # Custom GDI+ UI Components
│   │   ├── BarChartControl.cs         # Dynamic interactive sales bar chart
│   │   ├── SplineChartControl.cs      # Smooth user activity curve chart
│   │   ├── DonutChartControl.cs       # Interactive donut visualizer
│   │   ├── RoundedPanel.cs            # Rounded card with elevation & hover glow
│   │   ├── MetricHoverPopup.cs        # Non-activating floating metric tooltip
│   │   ├── ModernButton.cs            # Custom styled button with hover states
│   │   └── ThemeToggle.cs             # Animated theme toggle switch
│   ├── Forms/                         # WinForms UI Presentation Layer
│   │   ├── DashboardForm.cs           # Main maximized dashboard & navigation shell
│   │   ├── InvoiceForm.cs             # Full-screen responsive invoice editor
│   │   ├── BusinessProfileDialog.cs   # Business profile configuration modal
│   │   ├── CustomerHistoryDialog.cs   # Customer ledger & invoice history modal
│   │   └── RecordPaymentDialog.cs     # Payment recording dialog
│   ├── Models/                        # Core Domain Models & Transfer Objects
│   │   ├── Invoice.cs / InvoiceItem.cs# Invoicing domain entities
│   │   ├── Customer.cs                # CRM customer model
│   │   ├── Product.cs                 # Inventory product model
│   │   ├── Payment.cs                 # Payment transaction entity
│   │   ├── MetricDetailInfo.cs        # Hover tooltip metric metadata
│   │   └── SalesDashboardMetrics.cs   # Summary metric models
│   └── Services/                      # Application Business Logic & Persistence
│       ├── DatabaseService.cs         # SQLite persistence & database operations
│       ├── BillingDataService.cs      # Billing calculations & workflow service
│       ├── ProductService.cs          # Product catalog management
│       └── InvoicePdfService.cs       # Professional PDF invoice generator
├── tests/
│   └── PROJECT-SBV1.Tests/            # Comprehensive automated test suite (16 tests)
│       ├── SalesAndBillingTests.cs    # Billing, invoices & form initialization tests
│       └── DynamicDashboardHoverTests.cs # Charts, hover popups & metrics tests
├── docs/                              # Project documentation & logs
│   ├── TASKS.md                       # Task backlog & progress tracking
│   ├── TEST_REPORT.md                 # Test execution logs & verification evidence
│   └── TEAM_ROSTER.md                 # Team roles & governance
├── scripts/
│   └── setup_database.sql             # Database schema initialization script
├── .gitignore                         # Build artifact and secret exclusion rules
├── README.md                          # Repository documentation
└── PROJECT-SBV1.slnx                  # Solution configuration
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
- Windows 10 / Windows 11.
- Git for Windows.

### Installation & Setup

1. **Clone the repository**:
   ```powershell
   git clone https://github.com/hardikjariwala92-cyber/BAD1.git
   cd BAD1
   ```

2. **Restore dependencies & build**:
   ```powershell
   dotnet build
   ```

3. **Run automated unit tests**:
   ```powershell
   dotnet test
   ```
   *All 16 unit tests will execute and pass with 0 errors.*

4. **Launch the application**:
   ```powershell
   dotnet run --project PROJECT-SBV1.csproj
   ```

---

## 🧪 Testing & Quality Assurance

All features are covered by automated unit and integration tests located in `tests/PROJECT-SBV1.Tests`:

| Test Suite | Tests | Result | Description |
| :--- | :---: | :---: | :--- |
| `SalesAndBillingTests` | 9 | **PASS** | Form maximization, invoice calculations, layout responsiveness |
| `DynamicDashboardHoverTests` | 7 | **PASS** | Dynamic metric models, interactive charts, hover popups, multi-year SQLite queries |
| **Total** | **16** | **100% PASS** | Zero warnings, zero errors |

Verification logs are recorded in [`docs/TEST_REPORT.md`](docs/TEST_REPORT.md).

---

## 👨‍💻 Author & Maintainer

- **Developer**: Hardik Jariwala
- **GitHub**: [@hardikjariwala92-cyber](https://github.com/hardikjariwala92-cyber)
- **Role**: Full-Stack UI/UX & Engine Developer

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
