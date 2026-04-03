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
    public class RazorpayController : ControllerBase
    {
        private readonly IRazorpayService _razorpayService;

        public RazorpayController(IRazorpayService razorpayService)
        {
            _razorpayService = razorpayService;
        }

        /// <summary>
        /// Create Razorpay order for payment
        /// </summary>
        [HttpPost("create-order")]
        public async Task<ActionResult<RazorpayOrderResponseDto>> CreateOrder([FromBody] CreatePaymentIntentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var razorpayOrder = await _razorpayService.CreateRazorpayOrderAsync(dto.OrderId, userId);
                return Ok(razorpayOrder);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Verify payment signature after successful payment
        /// </summary>
        [HttpPost("verify-payment")]
        public async Task<ActionResult> VerifyPayment([FromBody] RazorpayPaymentVerificationDto dto)
        {
            var isValid = await _razorpayService.VerifyPaymentSignatureAsync(dto);

            if (!isValid)
            {
                return BadRequest(new { message = "Invalid payment signature" });
            }

            return Ok(new { message = "Payment verified successfully" });
        }

        /// <summary>
        /// Razorpay webhook endpoint
        /// </summary>
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // TODO: Verify webhook signature
            // For now, just log it
            Console.WriteLine($"Razorpay webhook received: {json}");

            return Ok();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}