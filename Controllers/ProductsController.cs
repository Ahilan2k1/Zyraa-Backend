using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly AppDbContext _context;

        public ProductsController(IProductService productService, AppDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProductAsync([FromBody] CreateProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Product name is required" });
            }

            if (dto.Price <= 0)
            {
                return BadRequest(new { message = "Price must be greater than 0" });
            }

            var createdProduct = await _productService.CreateProductAsync(dto);

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = createdProduct.Id },
                createdProduct
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var updatedProduct = await _productService.UpdateProductAsync(id, dto);

            if (updatedProduct == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(updatedProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);

            if (!success)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(new { message = "Product deleted successfully" });
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Product>>> SearchProducts([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(new { message = "Search term is required" });
            }

            var products = await _productService.SearchProductsAsync(term);
            return Ok(products);
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<Product>>> GetByCategory(string category)
        {
            var products = await _productService.GetProductsByCategoryAsync(category);
            return Ok(products);
        }

        [Authorize]
        [HttpGet("paged")]
        public async Task<ActionResult<List<Product>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var products = await _productService.GetProductsPagedAsync(page, pageSize);
            var totalCount = await _productService.GetTotalProductCountAsync();

            return Ok(new
            {
                products,
                totalItems = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page,
                pageSize
            });
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<List<Product>>> GetLowStock([FromQuery] int threshold = 5)
        {
            var products = await _productService.GetLowStockProductsAsync(threshold);
            return Ok(products);
        }

        [HttpPatch("{id}/rating")]
        public async Task<ActionResult> UpdateRating(int id, [FromBody] double rating)
        {
            if (rating < 1 || rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            var product = await _productService.UpdateRatingAsync(id, rating);
            if (product == null) return NotFound();

            return Ok(product);
        }

        [HttpGet("search-advanced")]
        public async Task<ActionResult<List<Product>>> SearchAdvanced(
            [FromQuery] string? searchTerm,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? category,
            [FromQuery] bool? inStockOnly)
        {
            var products = await _productService.SearchAdvancedAsync(searchTerm, minPrice, maxPrice, category, inStockOnly);
            return Ok(products);
        }

        // In ProductsController.cs — replace basic GetAll with this:

        [HttpGet("query-search")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductSearchDto searchDto)
        {
            // Start with all products — IQueryable, nothing executed yet
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // --- FILTERING ---

            // Text search: check if name OR description contains the search term
            // ToLower() for case-insensitive comparison
            if (!string.IsNullOrWhiteSpace(searchDto.Search))
            {
                var term = searchDto.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            // Category filter
            if (searchDto.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == searchDto.CategoryId.Value);

            // Price range filter
            if (searchDto.MinPrice.HasValue)
                query = query.Where(p => p.Price >= searchDto.MinPrice.Value);

            if (searchDto.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);

            // Rating filter
            if (searchDto.MinRating.HasValue)
                query = query.Where(p => p.Rating >= searchDto.MinRating.Value);

            // Stock filter — only show products with stock > 0
            if (searchDto.InStock == true)
                query = query.Where(p => p.Stock > 0);

            // --- SORTING ---
            query = searchDto.SortBy switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.Rating),

                _ => query.OrderByDescending(p => p.CreatedAt) // Default: newest first
            };

            // --- PAGINATION ---
            // Count BEFORE paging for TotalCount (one DB query)
            var totalCount = await query.CountAsync();

            // Skip past previous pages, take current page
            var items = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    Rating = p.Rating,
                    CategoryName = p.Category,
                    PrimaryImage = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.FilePath)
                        .FirstOrDefault()
                })
                .ToListAsync(); // Query executes HERE — all filters applied in SQL

            return Ok(new PagedResult<ProductResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            });
        }

    }
}