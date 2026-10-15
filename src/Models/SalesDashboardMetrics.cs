namespace PROJECT_SBV1.Models
{
    public class SalesDashboardMetrics
    {
        public decimal TotalSales { get; set; } = 0m;
        public int TotalInvoices { get; set; } = 0;
        public int PaidInvoices { get; set; } = 0;
        public int UnpaidInvoices { get; set; } = 0;
        public int PartiallyPaidInvoices { get; set; } = 0;
        public int OverdueInvoices { get; set; } = 0;
        public decimal OutstandingBalance { get; set; } = 0m;
        public decimal TotalCollected { get; set; } = 0m;
        public List<Invoice> RecentInvoices { get; set; } = new();
    }
}
