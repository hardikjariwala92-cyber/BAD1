namespace PROJECT_SBV1.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string CustomerGSTIN { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(15);
        public string PaymentTerms { get; set; } = "Net 15"; // Due on Receipt, Net 15, Net 30, Net 60
        public string Status { get; set; } = "Unpaid"; // Draft, Unpaid, Partially Paid, Paid, Overdue, Cancelled

        // Financial figures (decimal arithmetic)
        public decimal SubTotal { get; set; } = 0m;
        public decimal ItemDiscountTotal { get; set; } = 0m;
        public decimal OverallDiscount { get; set; } = 0m;
        public decimal TaxableAmount { get; set; } = 0m;
        public decimal TaxRate { get; set; } = 18m;
        public decimal TaxAmount { get; set; } = 0m;
        public decimal CgstAmount { get; set; } = 0m;
        public decimal SgstAmount { get; set; } = 0m;
        public decimal IgstAmount { get; set; } = 0m;
        public bool IsInterState { get; set; } = false;

        public decimal GrandTotal { get; set; } = 0m;
        public decimal AmountPaid { get; set; } = 0m;
        public decimal BalanceDue { get; set; } = 0m;

        public string Notes { get; set; } = string.Empty;
        public string TermsAndConditions { get; set; } = "1. Payment is due within 15 days of invoice date.\n2. Please mention Invoice Number in payment reference.";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<InvoiceItem> Items { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();

        public void RecalculateTotals()
        {
            decimal grossTotal = 0m;
            decimal itemDiscounts = 0m;
            decimal taxableSum = 0m;
            decimal totalTax = 0m;

            foreach (var item in Items)
            {
                decimal itemGross = item.Quantity * item.UnitPrice;
                decimal itemDisc = itemGross * (item.DiscountPercent / 100m);
                decimal itemTaxable = itemGross - itemDisc;
                decimal itemTax = itemTaxable * (item.TaxPercent / 100m);

                grossTotal += itemGross;
                itemDiscounts += itemDisc;
                taxableSum += itemTaxable;
                totalTax += itemTax;

                item.LineTotal = Math.Round(itemTaxable + itemTax, 2);
            }

            SubTotal = Math.Round(grossTotal, 2);
            ItemDiscountTotal = Math.Round(itemDiscounts, 2);

            // Apply overall discount to taxable base proportionally
            decimal totalDiscount = ItemDiscountTotal + OverallDiscount;
            TaxableAmount = Math.Max(0m, Math.Round(SubTotal - totalDiscount, 2));

            // If overall discount was applied, adjust tax on remaining taxable amount
            if (OverallDiscount > 0 && SubTotal > 0)
            {
                decimal effectiveTaxRate = grossTotal > 0 ? (totalTax / grossTotal) : (TaxRate / 100m);
                TaxAmount = Math.Round(TaxableAmount * effectiveTaxRate, 2);
            }
            else
            {
                TaxAmount = Math.Round(totalTax, 2);
            }

            if (IsInterState)
            {
                IgstAmount = TaxAmount;
                CgstAmount = 0m;
                SgstAmount = 0m;
            }
            else
            {
                IgstAmount = 0m;
                CgstAmount = Math.Round(TaxAmount / 2m, 2);
                SgstAmount = TaxAmount - CgstAmount; // Avoid rounding difference
            }

            GrandTotal = Math.Round(TaxableAmount + TaxAmount, 2);

            // Re-calculate payments
            AmountPaid = Math.Round(Payments.Sum(p => p.Amount), 2);
            BalanceDue = Math.Max(0m, Math.Round(GrandTotal - AmountPaid, 2));

            // Automatically determine status
            if (Status != "Cancelled" && Status != "Draft")
            {
                if (GrandTotal > 0 && AmountPaid >= GrandTotal)
                {
                    Status = "Paid";
                }
                else if (AmountPaid > 0 && AmountPaid < GrandTotal)
                {
                    Status = "Partially Paid";
                }
                else if (DueDate < DateTime.Today && AmountPaid < GrandTotal)
                {
                    Status = "Overdue";
                }
                else
                {
                    Status = "Unpaid";
                }
            }
        }
    }
}
