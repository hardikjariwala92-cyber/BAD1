namespace PROJECT_SBV1.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal Amount { get; set; } = 0m;
        public string PaymentMethod { get; set; } = "UPI"; // Cash, UPI, Bank Transfer, Card, Other
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = "Admin";

        public Payment() { }

        public Payment(int invoiceId, string invoiceNumber, string customerName, decimal amount, string paymentMethod, string referenceNumber, string notes, string recordedBy = "Admin")
        {
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            CustomerName = customerName;
            Amount = amount;
            PaymentMethod = paymentMethod;
            ReferenceNumber = referenceNumber;
            Notes = notes;
            RecordedBy = recordedBy;
            PaymentDate = DateTime.Now;
        }
    }
}
