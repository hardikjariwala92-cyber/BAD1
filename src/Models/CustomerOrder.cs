namespace PROJECT_SBV1.Models
{
    public class CustomerOrder
    {
        public string ProfileName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Delivered, Processed, Cancelled
        public string Price { get; set; } = string.Empty;

        public CustomerOrder() { }

        public CustomerOrder(string profileName, string address, string orderDate, string status, string price)
        {
            ProfileName = profileName;
            Address = address;
            OrderDate = orderDate;
            Status = status;
            Price = price;
        }
    }
}
