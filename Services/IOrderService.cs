using MyShop.Models;

namespace MyShop.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int userId, CreateOrderDto dto);
        Task<List<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int userId, int orderId);
        Task<Order?> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<List<Order>> GetAllOrdersAsync();  // Admin only
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
    }
}