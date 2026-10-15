using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    public class ProductService
    {
        private readonly DatabaseService _db = DatabaseService.Instance;

        public async Task<List<Product>> GetAllProductsAsync(string? search = null)
        {
            return await _db.GetProductsAsync(search);
        }

        public async Task<(bool Success, string Message)> AddProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return (false, "Product Name is required.");

            if (product.Price < 0)
                return (false, "Price cannot be negative.");

            if (product.StockQuantity < 0)
                return (false, "Stock quantity cannot be negative.");

            bool ok = await _db.AddProductAsync(product);
            return ok ? (true, "Product added successfully.") : (false, "Failed to add product to database.");
        }

        public async Task<(bool Success, string Message)> UpdateProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return (false, "Product Name is required.");

            if (product.Price < 0)
                return (false, "Price cannot be negative.");

            bool ok = await _db.UpdateProductAsync(product);
            return ok ? (true, "Product updated successfully.") : (false, "Failed to update product in database.");
        }

        public async Task<(bool Success, string Message)> DeleteProductAsync(int productId)
        {
            bool ok = await _db.DeleteProductAsync(productId);
            return ok ? (true, "Product deleted successfully.") : (false, "Failed to delete product.");
        }
    }
}
