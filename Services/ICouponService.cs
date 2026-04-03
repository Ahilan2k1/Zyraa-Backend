using MyShop.Models;

public interface ICouponService
{
    Task<CouponValidationResult> ValidateCouponAsync(string code, decimal cartTotal);
    Task<Coupon> CreateCouponAsync(CreateCouponDto dto);
    Task<List<Coupon>> GetAllCouponsAsync();
    Task DeactivateCouponAsync(int couponId);
    Task IncrementUsageAsync(string code); // Called after order is placed
}