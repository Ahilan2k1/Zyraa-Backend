using System.ComponentModel.DataAnnotations;

namespace MyShop.Models
{
    public class CreatePaymentIntentDto
    {
        [Required]
        public int OrderId { get; set; }
    }
}