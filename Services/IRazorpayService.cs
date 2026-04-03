using MyShop.Models;

namespace MyShop.Services
{
    public interface IRazorpayService
    {
        Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(int orderId, int userId);
        Task<bool> VerifyPaymentSignatureAsync(RazorpayPaymentVerificationDto dto);
        Task<bool> HandlePaymentSuccessAsync(string razorpayPaymentId, string razorpayOrderId);
        Task<bool> RefundPaymentAsync(int orderId, string? reason);
    }
}