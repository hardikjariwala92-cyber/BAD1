namespace PROJECT_SBV1.Models
{
    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }
        public int InvoiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1m;
        public decimal UnitPrice { get; set; } = 0m;
        public decimal DiscountPercent { get; set; } = 0m;
        public decimal TaxPercent { get; set; } = 18m;
        public decimal LineTotal { get; set; } = 0m;

        public InvoiceItem() { }

        public InvoiceItem(string serviceName, string description, decimal quantity, decimal unitPrice, decimal discountPercent = 0m, decimal taxPercent = 18m)
        {
            ServiceName = serviceName;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
            DiscountPercent = discountPercent;
            TaxPercent = taxPercent;
            CalculateTotal();
        }

        public void CalculateTotal()
        {
            decimal gross = Quantity * UnitPrice;
            decimal discountAmount = gross * (DiscountPercent / 100m);
            decimal taxable = gross - discountAmount;
            decimal taxAmount = taxable * (TaxPercent / 100m);
            LineTotal = Math.Round(taxable + taxAmount, 2);
        }
    }
}
