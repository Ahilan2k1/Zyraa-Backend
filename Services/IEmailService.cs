using MyShop.Models;

namespace MyShop.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendOrderConfirmationAsync(string toEmail, string userName, Order order);
        Task SendShippingNotificationAsync(string toEmail, string userName, Order order, string trackingNumber);
    }
}