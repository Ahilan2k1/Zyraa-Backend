using MyShop.Models;

namespace MyShop.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(CreateProductDto dto);
        Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<List<Product>> SearchProductsAsync(string searchTerm);
        Task<List<Product>> GetProductsByCategoryAsync(string category);
        Task<List<Product>> GetProductsPagedAsync(int page, int pageSize);
        Task<int> GetTotalProductCountAsync();
        Task<List<Product>> GetLowStockProductsAsync(int threshold);
        Task<Product?> UpdateRatingAsync(int id, double newRating);
        Task<List<Product>> SearchAdvancedAsync(string? searchTerm, decimal? minPrice, decimal? maxPrice, string? category, bool? inStockOnly);
    }
}