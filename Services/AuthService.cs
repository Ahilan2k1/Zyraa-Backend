using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobClient _backgroundJobs;

        public AuthService(AppDbContext context, IConfiguration configuration,IEmailService emailService,IBackgroundJobClient backgroundJobs)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _backgroundJobs = backgroundJobs;
        }

        public async  Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            if(await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return null; // User with this email already exists
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _backgroundJobs.Enqueue<EmailJobService>(j => j.SendWelcomeEmailJobAsync(user.Id));

            string token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if(user == null)
            {
                return null; // User not found
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if(!isPasswordValid)
            {
                return null; // Invalid password
            }

            string token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return true; // User not found
            }

            var resetToken = GeneratePasswordResetToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || user.PasswordResetToken != dto.Token || user.PasswordResetTokenExpires < DateTime.UtcNow)
            {
                return false; // Invalid token or token expired
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role)
            };
            
            var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!";
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "MyShopAPI",
                audience: _configuration["Jwt:Audience"] ?? "MyShopAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GeneratePasswordResetToken()
        {   
            byte[] tokenData = new byte[32];
            using(var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenData);
            }
            
            string token = Convert.ToBase64String(tokenData)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
            
            return token;
        }
    }
}