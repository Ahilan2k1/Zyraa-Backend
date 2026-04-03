using MyShop.Models;

namespace MyShop.Services
{
    public interface ICartService
    {
        Task<CartItem> AddToCartAsync(int userId, AddToCartDto dto);
        Task<CartSummaryDto> GetCartAsync(int userId);
        Task<CartItem?> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
        Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
        Task<bool> ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
    }
}