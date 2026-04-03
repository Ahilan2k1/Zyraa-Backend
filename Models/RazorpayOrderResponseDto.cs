namespace MyShop.Models
{
    public class RazorpayOrderResponseDto
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayKeyId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }
}