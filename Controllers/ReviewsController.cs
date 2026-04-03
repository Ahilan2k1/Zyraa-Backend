using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Models;
using MyShop.Services;

[ApiController]
[Route("api/products/{productId}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // GET /api/products/5/reviews
    [HttpGet]
    public async Task<IActionResult> GetReviews(int productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(reviews);
    }

    // GET /api/products/5/reviews/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(int productId)
    {
        var summary = await _reviewService.GetRatingSummaryAsync(productId);
        return Ok(summary);
    }

    // POST /api/products/5/reviews
    [HttpPost]
    [Authorize] // Must be logged in
    public async Task<IActionResult> AddReview(int productId, [FromBody] CreateReviewDto dto)
    {
        // Extract userId from JWT token claims
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        try
        {
            var review = await _reviewService.AddReviewAsync(productId, userId, dto);
            return CreatedAtAction(nameof(GetReviews), new { productId }, review);
        }
        catch (InvalidOperationException ex)
        {
            // 409 Conflict — duplicate review or not purchased
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/products/5/reviews/3
    [HttpDelete("{reviewId}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int productId, int reviewId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        try
        {
            await _reviewService.DeleteReviewAsync(reviewId, userId, isAdmin);
            return NoContent(); // 204 — success, nothing to return
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid( ex.Message ); }
    }
}   