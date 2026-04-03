using System.ComponentModel.DataAnnotations;

namespace MyShop.Models
{
    public class AddToCartDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    };
}