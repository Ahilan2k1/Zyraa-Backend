using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;
using MyShop.Data;
using MyShop.Models;
using Microsoft.Extensions.Configuration;

namespace MyShop.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RazorpayClient _razorpayClient;

        public RazorpayService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
            // Initialize Razorpay client
            _razorpayClient = new RazorpayClient(
                _configuration["Razorpay:KeyId"],
                _configuration["Razorpay:KeySecret"]
            );
        }

        public async Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(int orderId, int userId)
        {
            // 1. Get order with user details
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                throw new InvalidOperationException("Order not found");
            }

            // 2. Check if Razorpay order already exists
            if (!string.IsNullOrEmpty(order.PaymentIntentId))
            {
                return new RazorpayOrderResponseDto
                {
                    RazorpayOrderId = order.PaymentIntentId,
                    RazorpayKeyId = _configuration["Razorpay:KeyId"]!,
                    Amount = (long)(order.Total * 100),
                    Currency = "INR",
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = $"{order.User.FirstName} {order.User.LastName}",
                    CustomerEmail = order.User.Email,
                    CustomerPhone = order.PhoneNumber
                };
            }

            // 3. Create Razorpay order
            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", (long)(order.Total * 100) },  // Amount in paise
                { "currency", "INR" },
                { "receipt", order.OrderNumber },
                { "notes", new Dictionary<string, string>
                    {
                        { "order_id", order.Id.ToString() },
                        { "order_number", order.OrderNumber }
                    }
                }
            };

            Razorpay.Api.Order razorpayOrder = _razorpayClient.Order.Create(options);

            // 4. Save Razorpay order ID to our order
            order.PaymentIntentId = razorpayOrder["id"].ToString();
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 5. Return response for frontend
            return new RazorpayOrderResponseDto
            {
                RazorpayOrderId = razorpayOrder["id"].ToString(),
                RazorpayKeyId = _configuration["Razorpay:KeyId"]!,
                Amount = (long)(order.Total * 100),
                Currency = "INR",
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = $"{order.User.FirstName} {order.User.LastName}",
                CustomerEmail = order.User.Email,
                CustomerPhone = order.PhoneNumber
            };
        }

        public async Task<bool> VerifyPaymentSignatureAsync(RazorpayPaymentVerificationDto dto)
        {
            // 1. Generate signature
            string text = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
            string secret = _configuration["Razorpay:KeySecret"]!;

            var encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(text);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                string generatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // 2. Compare signatures
                if (generatedSignature != dto.RazorpaySignature.ToLower())
                {
                    return false;
                }
            }

            // 3. Signature valid - update order
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
            {
                return false;
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = OrderStatus.Processing;
            order.PaidAt = DateTime.UtcNow;
            order.PaymentMethod = "razorpay";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HandlePaymentSuccessAsync(string razorpayPaymentId, string razorpayOrderId)
        {
            // Find order by Razorpay order ID
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.PaymentIntentId == razorpayOrderId);

            if (order == null)
            {
                return false;
            }

            // Update order status
            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = OrderStatus.Processing;
            order.PaidAt = DateTime.UtcNow;
            order.PaymentMethod = "razorpay";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RefundPaymentAsync(int orderId, string? reason)
        {
            // 1. Get order
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                throw new InvalidOperationException("Order not found");
            }

            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                throw new InvalidOperationException("No payment to refund");
            }

            if (order.PaymentStatus != PaymentStatus.Paid)
            {
                throw new InvalidOperationException("Order is not paid");
            }

            // 2. Get payment details from Razorpay
            var payments = _razorpayClient.Payment.All(new Dictionary<string, object>
            {
                { "count", 1 }
            });

            // For simplicity, we'll just update the order status
            // In production, you'd call Razorpay Refund API

            // 3. Update order status
            order.PaymentStatus = PaymentStatus.Refunded;
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}