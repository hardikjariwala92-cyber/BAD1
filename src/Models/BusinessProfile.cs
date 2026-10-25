namespace PROJECT_SBV1.Models
{
    public class BusinessProfile
    {
        public string BusinessName { get; set; } = "S TOM'S Digital & Cloud Services";
        public string Tagline { get; set; } = "Enterprise Software, Cloud Subscriptions & Digital Services";
        public string Address { get; set; } = "Plot 42, Silicon Infotech Park, SG Highway";
        public string City { get; set; } = "Ahmedabad, Gujarat";
        public string Country { get; set; } = "India";
        public string Phone { get; set; } = "+91 98765 43210";
        public string Email { get; set; } = "billing@stoms.io";
        public string GSTIN { get; set; } = "24AAACS1234F1Z5";
        public string BankName { get; set; } = "HDFC Bank Ltd.";
        public string BranchName { get; set; } = "SG Highway Branch";
        public string AccountNumber { get; set; } = "50200084920194";
        public string IFSC { get; set; } = "HDFC0000456";
        public string UPIId { get; set; } = "stoms.billing@hdfcbank";
        public string LogoPath { get; set; } = "";
        public string AccentColorHex { get; set; } = "#3b82f6";
        public string FooterNote { get; set; } = "Thank you for your business! For billing inquiries, contact billing@stoms.io";
        public string DefaultTerms { get; set; } = "1. Payment is due within 15 days of invoice issue date.\n2. Invoices unpaid after due date will incur 2% interest per month.\n3. All disputes are subject to local jurisdiction.";

        public BusinessProfile Clone()
        {
            return new BusinessProfile
            {
                BusinessName = this.BusinessName,
                Tagline = this.Tagline,
                Address = this.Address,
                City = this.City,
                Country = this.Country,
                Phone = this.Phone,
                Email = this.Email,
                GSTIN = this.GSTIN,
                BankName = this.BankName,
                BranchName = this.BranchName,
                AccountNumber = this.AccountNumber,
                IFSC = this.IFSC,
                UPIId = this.UPIId,
                LogoPath = this.LogoPath,
                AccentColorHex = this.AccentColorHex,
                FooterNote = this.FooterNote,
                DefaultTerms = this.DefaultTerms
            };
        }
    }
}
