namespace MyShop.Models
{
    public class CartSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }

    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
        public string? ImageUrl { get; set; }
    }
}