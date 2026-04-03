using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products.Include(p => p.Images).ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                Category = dto.Category,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return null;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Category = dto.Category;
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category.ToLower() == category.ToLower())
                .ToListAsync();
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                           p.Description.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsPagedAsync(int page, int pageSize)
        {
            return await _context.Products
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
        }
        public Task<int> GetTotalProductCountAsync()
        {
            return _context.Products.CountAsync();
        }
        public Task<List<Product>> GetLowStockProductsAsync(int threshold)
        {
            return _context.Products
                .Where(p => p.Stock < threshold)
                .ToListAsync();
        }

        public async Task<Product?> UpdateRatingAsync(int id, double newRating)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return null;
            // Update average rating
            product.Rating = ((product.Rating * product.ReviewCount) + newRating) / (product.ReviewCount + 1);
            product.ReviewCount += 1;
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<List<Product>> SearchAdvancedAsync(string? searchTerm, decimal? minPrice, decimal? maxPrice, string? category, bool? inStockOnly)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                                         p.Description.ToLower().Contains(searchTerm.ToLower()));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }

            if (inStockOnly.HasValue && inStockOnly.Value)
            {
                query = query.Where(p => p.Stock > 0);
            }

            return await query.ToListAsync();
        }
    }
}