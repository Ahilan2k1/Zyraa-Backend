using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MyShop.Models;

namespace MyShop.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Email:FromName"],
                _configuration["Email:FromAddress"]!
            ));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Password Reset Request";

            var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={resetToken}&email={toEmail}";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <h2>Password Reset Request</h2>
                    <p>You requested to reset your password.</p>
                    <p>Click the link below to reset your password:</p>
                    <a href='{resetLink}'>Reset Password</a>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                "
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Email:SmtpServer"],
                int.Parse(_configuration["Email:SmtpPort"]!),
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_configuration["Email:FromName"],_configuration["Email:FromAddress"]!));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:SmtpServer"],int.Parse(_configuration["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName) =>
            await SendAsync(toEmail, userName, "Welcome to MyShop India! 🎉",
                EmailTemplates.WelcomeEmail(userName));

        public async Task SendOrderConfirmationAsync(string toEmail, string userName, Order order) =>
            await SendAsync(toEmail, userName, $"Order Confirmed: {order.OrderNumber} ✅",
                EmailTemplates.OrderConfirmationEmail(order, userName));

        public async Task SendShippingNotificationAsync(string toEmail, string userName, Order order, string trackingNumber) =>
            await SendAsync(toEmail, userName, $"Your Order Has Shipped! 🚚 — {order.OrderNumber}",
                EmailTemplates.ShippingNotificationEmail(order, userName, trackingNumber));
    }
}