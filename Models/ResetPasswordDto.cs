using System.ComponentModel.DataAnnotations;

namespace MyShop.Models
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}