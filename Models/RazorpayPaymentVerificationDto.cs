using System.ComponentModel.DataAnnotations;

namespace MyShop.Models
{
    public class RazorpayPaymentVerificationDto
    {
        [Required]
        public string RazorpayOrderId { get; set; } = string.Empty;
        
        [Required]
        public string RazorpayPaymentId { get; set; } = string.Empty;
        
        [Required]
        public string RazorpaySignature { get; set; } = string.Empty;
        
        [Required]
        public int OrderId { get; set; }
    }
}