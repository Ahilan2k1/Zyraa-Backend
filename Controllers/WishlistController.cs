using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/wishlist")]
[Authorize] // All wishlist endpoints require login
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    public WishlistController(IWishlistService wishlistService) =>
        _wishlistService = wishlistService;

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetWishlist() =>
        Ok(await _wishlistService.GetWishlistAsync(GetUserId()));

    // POST /api/wishlist/5
    [HttpPost("{productId}")]
    public async Task<IActionResult> Add(int productId)
    {
        try
        {
            await _wishlistService.AddToWishlistAsync(GetUserId(), productId);
            return Ok(new { message = "Added to wishlist" });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // DELETE /api/wishlist/5
    [HttpDelete("{productId}")]
    public async Task<IActionResult> Remove(int productId)
    {
        try
        {
            await _wishlistService.RemoveFromWishlistAsync(GetUserId(), productId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // GET /api/wishlist/check/5
    [HttpGet("check/{productId}")]
    public async Task<IActionResult> Check(int productId) =>
        Ok(new { isInWishlist = await _wishlistService.IsInWishlistAsync(GetUserId(), productId) });
}