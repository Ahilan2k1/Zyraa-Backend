using MyShop.Models;

public interface IWishlistService
{
    Task<List<WishlistResponseDto>> GetWishlistAsync(int userId);
    Task AddToWishlistAsync(int userId, int productId);
    Task RemoveFromWishlistAsync(int userId, int productId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
}