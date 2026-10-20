namespace PROJECT_SBV1.Models
{
    public class AnalyticsSummary
    {
        public int TotalOrders { get; set; } = 201;
        public string OrdersTrend { get; set; } = "↑ 8.2% since last month";
        public bool OrdersTrendPositive { get; set; } = true;

        public int ApprovedOrders { get; set; } = 36;
        public string ApprovedTrend { get; set; } = "↑ 3.4% since last month";
        public bool ApprovedTrendPositive { get; set; } = true;

        public string TotalUsers { get; set; } = "4.890";
        public string ActiveSubscriptions { get; set; } = "1.201";

        public int MonthTotal { get; set; } = 1164084;
        public string MonthTotalTrend { get; set; } = "↓ 0.2% since last month";
        public bool MonthTotalTrendPositive { get; set; } = false;

        public int Revenue { get; set; } = 1352;
        public string RevenueTrend { get; set; } = "↓ 1.2% since last month";
        public bool RevenueTrendPositive { get; set; } = false;

        public string PaidInvoices { get; set; } = "₹1,21,745.00";
        public string PaidInvoicesBadge { get; set; } = "+15%";

        public string FundsReceived { get; set; } = "₹1,50,256.23";
        public string FundsReceivedBadge { get; set; } = "+59%";
    }
}
