using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Models;

[ApiController]
[Route("api/coupons")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;
    public CouponsController(ICouponService couponService) => _couponService = couponService;

    // POST /api/coupons/validate
    // Any logged-in user can validate a coupon
    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponDto dto)
    {
        var result = await _couponService.ValidateCouponAsync(dto.Code, dto.CartTotal);
        if (!result.IsValid)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            discountAmount = result.DiscountAmount,
            couponCode = result.Coupon!.Code,
            discountType = result.Coupon.DiscountType.ToString()
        });
    }

    // Admin-only endpoints below
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _couponService.GetAllCouponsAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
    {
        try
        {
            var coupon = await _couponService.CreateCouponAsync(dto);
            return CreatedAtAction(nameof(GetAll), coupon);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _couponService.DeactivateCouponAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}