namespace MyShop.Models;

public enum DiscountType
{
    Percentage,  // e.g. 10% off
    Fixed        // e.g. ₹100 off
}

public class Coupon
{
    public int Id { get; set; }

    // The code users type in — e.g. "DIWALI20"
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    // For Percentage: 10 means 10%. For Fixed: 100 means ₹100
    public decimal DiscountValue { get; set; }

    // Minimum cart value to apply coupon — e.g. only orders above ₹500
    public decimal MinimumOrderAmount { get; set; } = 0;

    // Cap the discount — e.g. max ₹200 off even for 20% coupon on big orders
    public decimal? MaxDiscountAmount { get; set; }

    // How many times this coupon can be used total (null = unlimited)
    public int? UsageLimit { get; set; }

    // How many times it's been used so far
    public int UsageCount { get; set; } = 0;

    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}