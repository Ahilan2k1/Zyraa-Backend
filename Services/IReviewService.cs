// IReviewService.cs
using MyShop.Models;

namespace MyShop.Services;

public interface IReviewService
{
    Task<ReviewResponseDto> AddReviewAsync(int productId, int userId, CreateReviewDto dto);
    Task<List<ReviewResponseDto>> GetProductReviewsAsync(int productId);
    Task<ProductRatingSummaryDto> GetRatingSummaryAsync(int productId);
    Task DeleteReviewAsync(int reviewId, int userId, bool isAdmin);
}