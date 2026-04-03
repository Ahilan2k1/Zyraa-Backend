namespace MyShop.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Snapshot of product name at time of order
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Snapshot of price at time of order
       public decimal Subtotal { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}