namespace MyShop.Models
{
    public class Order
    {
        public int Id {get; set; }
        public int UserId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string? PaymentIntentId { get; set; }  // Stripe Payment Intent ID
        public string? PaymentMethod { get; set; }  // e.g., "card", "paypal"
        public DateTime? PaidAt { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingZipCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    };

    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Refunded = 3
    }
}