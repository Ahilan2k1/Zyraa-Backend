using System.ComponentModel.DataAnnotations;

namespace MyShop.Models
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}