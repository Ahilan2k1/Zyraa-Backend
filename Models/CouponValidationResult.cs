namespace MyShop.Models;
public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; } // Actual rupees to discount
    public Coupon? Coupon { get; set; }
}