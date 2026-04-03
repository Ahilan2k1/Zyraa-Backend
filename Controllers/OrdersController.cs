using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Create order from cart (Checkout)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var order = await _orderService.CreateOrderAsync(userId, dto);

                return CreatedAtAction(
                    nameof(GetOrder), 
                    new { id = order.Id }, 
                    order
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's orders (order history)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetMyOrders()
        {
            var userId = GetCurrentUserId();
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        /// <summary>
        /// Get specific order details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.GetOrderByIdAsync(userId, id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(order);
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<Order>> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByOrderNumberAsync(orderNumber);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            // Verify user owns this order
            var userId = GetCurrentUserId();
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return Ok(order);
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, status);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(new 
            { 
                message = "Order status updated",
                order 
            });
        }

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // HELPER: Get current user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}