namespace PROJECT_SBV1.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = "Active";
        public string Barcode { get; set; } = string.Empty;

        public Product() { }

        public Product(int productId, string name, string category, decimal price, int stockQuantity, string status = "Active", string barcode = "")
        {
            ProductId = productId;
            Name = name;
            Category = category;
            Price = price;
            StockQuantity = stockQuantity;
            Status = status;
            Barcode = string.IsNullOrEmpty(barcode) ? $"890123400{productId:D3}" : barcode;
        }
    }
}
