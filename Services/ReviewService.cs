// ReviewService.cs
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewResponseDto> AddReviewAsync(int productId, int userId, CreateReviewDto dto)
    {
        // Prevent duplicate reviews — one user, one review per product
        var existing = await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

        if (existing)
            throw new InvalidOperationException("You have already reviewed this product.");

        // Optional: only allow review if user actually purchased the product
        var purchased = await _context.OrderItems
            .AnyAsync(oi => oi.Order.UserId == userId
                         && oi.ProductId == productId
                         && oi.Order.Status == OrderStatus.Delivered);

        if (!purchased)
            throw new InvalidOperationException("You can only review products you have purchased.");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Update the product's cached average rating
        await UpdateProductRatingAsync(productId);

        // Load user for the response
        await _context.Entry(review).Reference(r => r.User).LoadAsync();

        return MapToDto(review);
    }

    public async Task<List<ReviewResponseDto>> GetProductReviewsAsync(int productId)
{
    var reviews = await _context.Reviews
        .Where(r => r.ProductId == productId && r.IsApproved)
        .Include(r => r.User)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    return reviews.Select(r => new ReviewResponseDto
    {
        Id = r.Id,
        Rating = r.Rating,
        Comment = r.Comment,
        UserName = r.User!.Email.Split('@')[0],
        CreatedAt = r.CreatedAt
    }).ToList();
}

    public async Task<ProductRatingSummaryDto> GetRatingSummaryAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync();

        if (!reviews.Any())
            return new ProductRatingSummaryDto();

        return new ProductRatingSummaryDto
        {
            // Math.Round to 1 decimal place — e.g. 4.3
            AverageRating = Math.Round(reviews.Average(r => r.Rating), 1),
            TotalReviews = reviews.Count,
            // GroupBy star value, count each group
            Distribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task DeleteReviewAsync(int reviewId, int userId, bool isAdmin)
    {
        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Review not found.");

        // Users can only delete their own reviews; admins can delete any
        if (!isAdmin && review.UserId != userId)
            throw new UnauthorizedAccessException("You cannot delete this review.");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        // Recalculate product rating after deletion
        await UpdateProductRatingAsync(review.ProductId);
    }

    // Private helper: recalculates and saves average rating to Product table
    private async Task UpdateProductRatingAsync(int productId)
    {
        var avg = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .AverageAsync(r => (double?)r.Rating) ?? 0; // null if no reviews → default 0

        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.Rating = Math.Round(avg, 1);
            await _context.SaveChangesAsync();
        }
    }

    private static ReviewResponseDto MapToDto(Review r) => new()
    {
        Id = r.Id,
        Rating = r.Rating,
        Comment = r.Comment,
        UserName = r.User!.Email.Split('@')[0],
        CreatedAt = r.CreatedAt
    };
}