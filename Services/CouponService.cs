


using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;

public class CouponService : ICouponService
{
    private readonly AppDbContext _context;
    public CouponService(AppDbContext context) => _context = context;

    public async Task<CouponValidationResult> ValidateCouponAsync(string code, decimal cartTotal)
    {
        // Case-insensitive lookup
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

        if (coupon == null)
            return Fail("Coupon not found.");

        if (!coupon.IsActive)
            return Fail("This coupon is no longer active.");

        // Check expiry
        if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.UtcNow)
            return Fail("This coupon has expired.");

        // Check usage limit
        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            return Fail("This coupon has reached its usage limit.");

        // Check minimum order amount
        if (cartTotal < coupon.MinimumOrderAmount)
            return Fail($"Minimum order amount is ₹{coupon.MinimumOrderAmount:N0}.");

        // Calculate actual discount
        decimal discount = coupon.DiscountType == DiscountType.Percentage
            ? cartTotal * (coupon.DiscountValue / 100) // e.g. 20% of ₹1000 = ₹200
            : coupon.DiscountValue;                    // Fixed ₹100 off

        // Apply cap if set
        if (coupon.MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

        // Discount can't exceed cart total
        discount = Math.Min(discount, cartTotal);

        return new CouponValidationResult
        {
            IsValid = true,
            DiscountAmount = Math.Round(discount, 2),
            Coupon = coupon
        };
    }

    public async Task IncrementUsageAsync(string code)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

        if (coupon != null)
        {
            coupon.UsageCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Coupon> CreateCouponAsync(CreateCouponDto dto)
    {
        // Ensure code is unique and uppercase
        var code = dto.Code.ToUpper().Trim();
        var exists = await _context.Coupons.AnyAsync(c => c.Code == code);
        if (exists) throw new InvalidOperationException("Coupon code already exists.");

        var coupon = new Coupon
        {
            Code = code,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinimumOrderAmount = dto.MinimumOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            UsageLimit = dto.UsageLimit,
            ExpiryDate = dto.ExpiryDate
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        return coupon;
    }

    public async Task<List<Coupon>> GetAllCouponsAsync() =>
        await _context.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task DeactivateCouponAsync(int couponId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId)
            ?? throw new KeyNotFoundException("Coupon not found.");
        coupon.IsActive = false;
        await _context.SaveChangesAsync();
    }

    // Helper to return a failed result
    private static CouponValidationResult Fail(string msg) =>
        new() { IsValid = false, ErrorMessage = msg };
}