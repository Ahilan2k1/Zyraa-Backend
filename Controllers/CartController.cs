using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // All cart endpoints require authentication
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get current user's cart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CartSummaryDto>> GetCart()
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var cartItem = await _cartService.AddToCartAsync(userId, dto);
                
                return Ok(new 
                { 
                    message = "Item added to cart",
                    cartItem,
                    cartCount = await _cartService.GetCartItemCountAsync(userId)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("items/{cartItemId}")]
        public async Task<ActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var cartItem = await _cartService.UpdateCartItemAsync(userId, cartItemId, dto);

                if (cartItem == null)
                {
                    return NotFound(new { message = "Cart item not found" });
                }

                return Ok(new 
                { 
                    message = "Cart updated",
                    cartItem 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("items/{cartItemId}")]
        public async Task<ActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetCurrentUserId();
            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (!success)
            {
                return NotFound(new { message = "Cart item not found" });
            }

            return Ok(new 
            { 
                message = "Item removed from cart",
                cartCount = await _cartService.GetCartItemCountAsync(userId)
            });
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);

            return Ok(new { message = "Cart cleared" });
        }

        /// <summary>
        /// Get cart item count
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCartCount()
        {
            var userId = GetCurrentUserId();
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Ok(new { count });
        }

        // HELPER: Get current user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}