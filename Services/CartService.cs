using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private const decimal TAX_RATE = 0.08m;  // 8% tax
        private const decimal SHIPPING_COST = 10.00m;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartItem> AddToCartAsync(int userId, AddToCartDto dto)
        {
            // 1. Check if product exists
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            // 2. Check stock
            if (product.Stock < dto.Quantity)
            {
                throw new InvalidOperationException($"Only {product.Stock} items in stock");
            }

            // 3. Check if item already in cart
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += dto.Quantity;
                
                // Check stock again
                if (product.Stock < existingItem.Quantity)
                {
                    throw new InvalidOperationException($"Only {product.Stock} items in stock");
                }

                await _context.SaveChangesAsync();
                return existingItem;
            }

            // 4. Create new cart item
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = product.Price,  // Snapshot current price
                CreatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            return cartItem;
        }

        public async Task<CartSummaryDto> GetCartAsync(int userId)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            var items = cartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                Price = ci.Price,
                Quantity = ci.Quantity,
                Subtotal = ci.Price * ci.Quantity,
                ImageUrl = ci.Product.Images.FirstOrDefault(i => i.IsPrimary)?.FilePath
                    ?? ci.Product.Images.FirstOrDefault()?.FilePath
            }).ToList();

            var subtotal = items.Sum(i => i.Subtotal);
            var tax = subtotal * TAX_RATE;
            var shipping = items.Any() ? SHIPPING_COST : 0;
            var total = subtotal + tax + shipping;

            return new CartSummaryDto
            {
                Items = items,
                Subtotal = subtotal,
                Tax = tax,
                ShippingCost = shipping,
                Total = total,
                ItemCount = items.Sum(i => i.Quantity)
            };
        }

        public async Task<CartItem?> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                return null;
            }

            // Check stock
            if (cartItem.Product.Stock < dto.Quantity)
            {
                throw new InvalidOperationException($"Only {cartItem.Product.Stock} items in stock");
            }

            cartItem.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            return cartItem;
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                return false;
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }
    }
}