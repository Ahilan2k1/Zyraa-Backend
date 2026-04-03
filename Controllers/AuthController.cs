using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(dto);

            if (result == null)
            {
                return BadRequest(new { message = "Email is already in use" });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<ActionResult> GetProfile()
        {
            var UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (UserId == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            int userIdInt = int.Parse(UserId);

            var user = await _authService.GetUserByIdAsync(userIdInt);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new 
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin-only")]
        public ActionResult GetAdminOnlyData()
        {
            return Ok(new { message = "This is protected data for Admins only" });  
        }

        [HttpPost("forgot-password")] 
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool result = await _authService.ForgotPasswordAsync(dto);

            if (!result)
            {
                return BadRequest(new { message = "Failed to generate password reset token" });
            }

            return Ok(new { message = "Password reset token generated and email sent (simulated)" });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            };

            var result = await _authService.ResetPasswordAsync(dto);

            if (!result)
            {
                return BadRequest(new { message = "Invalid token or token expired" });
            }

            return Ok(new { message = "Password has been reset successfully" });
        }
    }
}