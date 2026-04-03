using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

// What admin sends when creating a coupon
public class CreateCouponDto
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string Code { get; set; } = string.Empty;         // e.g. "DIWALI20"

    [Required]
    public DiscountType DiscountType { get; set; }           // Percentage or Fixed

    [Range(0.01, 100000)]
    public decimal DiscountValue { get; set; }               // 20 for 20%, or 100 for ₹100

    [Range(0, double.MaxValue)]
    public decimal MinimumOrderAmount { get; set; } = 0;     // Min cart value to apply

    public decimal? MaxDiscountAmount { get; set; }          // Cap on discount amount

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }                     // null = unlimited

    public DateTime? ExpiryDate { get; set; }                // null = never expires
}

// What user sends when validating a coupon at checkout
public class ValidateCouponDto
{
    [Required]
    public string Code { get; set; } = string.Empty;         // e.g. "DIWALI20"

    [Range(0.01, double.MaxValue)]
    public decimal CartTotal { get; set; }                   // Current cart total in ₹
}