using System.ComponentModel.DataAnnotations;

namespace MyShop.Models
{
    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required]
        public string ShippingCity { get; set; } = string.Empty;
        
        [Required]
        public string ShippingState { get; set; } = string.Empty;
        
        [Required]
        public string ShippingZipCode { get; set; } = string.Empty;
        
        [Required]
        public string ShippingCountry { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}