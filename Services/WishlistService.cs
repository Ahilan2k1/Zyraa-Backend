using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    public WishlistService(AppDbContext context) => _context = context;

    public async Task<List<WishlistResponseDto>> GetWishlistAsync(int userId)
    {
        return await _context.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
            .ThenInclude(p => p!.Images) // Load product images too
            .Select(w => new WishlistResponseDto
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product!.Name,
                Price = w.Product.Price,
                // Get the primary image path, fallback to first image
                ImageUrl = w.Product.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.FilePath)
                    .FirstOrDefault()
                    ?? w.Product.Images.Select(i => i.FilePath).FirstOrDefault(),
                AddedAt = w.AddedAt
            })
            .ToListAsync();
    }

    public async Task AddToWishlistAsync(int userId, int productId)
    {
        // Idempotent — don't throw if already exists, just ignore
        var exists = await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

        if (exists) return; // Already in wishlist, do nothing

        // Verify product exists before adding
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
            throw new KeyNotFoundException("Product not found.");

        _context.WishlistItems.Add(new WishlistItem
        {
            UserId = userId,
            ProductId = productId
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromWishlistAsync(int userId, int productId)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId)
            ?? throw new KeyNotFoundException("Item not in wishlist.");

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId) =>
        await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
}