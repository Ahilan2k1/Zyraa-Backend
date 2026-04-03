using Hangfire;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobs;
        private const decimal TAX_RATE = 0.08m;
        private const decimal SHIPPING_COST = 10.00m;

        public OrderService(AppDbContext context, IBackgroundJobClient backgroundJobs )
        {
            _context = context;
            _backgroundJobs = backgroundJobs;
        }

        public async Task<Order> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            // 1. Get user's cart
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty");
            }

            // 2. Verify stock for all items
            foreach (var item in cartItems)
            {
                if (item.Product.Stock < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for {item.Product.Name}. Only {item.Product.Stock} available."
                    );
                }
            }

            // 3. Calculate totals
            var subtotal = cartItems.Sum(ci => ci.Price * ci.Quantity);
            var tax = subtotal * TAX_RATE;
            var shipping = SHIPPING_COST;
            var total = subtotal + tax + shipping;

            // 4. Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            // 5. Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                Status = OrderStatus.Pending,
                Subtotal = subtotal,
                Tax = tax,
                ShippingCost = shipping,
                Total = total,
                ShippingAddress = dto.ShippingAddress,
                ShippingCity = dto.ShippingCity,
                ShippingState = dto.ShippingState,
                ShippingZipCode = dto.ShippingZipCode,
                ShippingCountry = dto.ShippingCountry,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();  // Save to get order ID

            _backgroundJobs.Enqueue<EmailJobService>(j => j.SendOrderConfirmationJobAsync(order.Id));

            // 6. Create order items from cart
            var orderItems = cartItems.Select(ci => new OrderItem
            {
                OrderId = order.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,  // Snapshot
                Price = ci.Price,
                Quantity = ci.Quantity,
                Subtotal = ci.Price * ci.Quantity
            }).ToList();

            _context.OrderItems.AddRange(orderItems);

            // 7. Update product stock
            foreach (var item in cartItems)
            {
                item.Product.Stock -= item.Quantity;
            }

            // 8. Clear cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            // 9. Load navigation properties for response
            await _context.Entry(order)
                .Collection(o => o.OrderItems)
                .LoadAsync();

            return order;
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int userId, int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<Order?> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            
            if (order == null)
            {
                return null;
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (order.Status == OrderStatus.Shipped)
            {
                var trackingNumber = $"MYSHOP{order.Id:D8}"; // or accept from request body
                _backgroundJobs.Enqueue<EmailJobService>(j => j.SendShippingNotificationJobAsync(order.Id, trackingNumber));
            }

            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        // HELPER: Generate unique order number
        private async Task<string> GenerateOrderNumberAsync()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith($"ORD-{date}"))
                .CountAsync();

            return $"ORD-{date}-{(count + 1):D4}";
        }
    }
}