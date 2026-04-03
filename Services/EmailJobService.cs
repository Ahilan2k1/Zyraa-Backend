using Hangfire;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;

namespace MyShop.Services;

public class EmailJobService
{
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public EmailJobService(IEmailService emailService, AppDbContext context)
    {
        _emailService = emailService;
        _context = context;
    }

    // Called after registration — runs in background
    [AutomaticRetry(Attempts = 3)]
    public async Task SendWelcomeEmailJobAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new Exception($"User {userId} not found");

        await _emailService.SendWelcomeEmailAsync(user.Email, user.Email.Split('@')[0]);
    }

    // Called after order creation — runs in background
    [AutomaticRetry(Attempts = 3)]
    public async Task SendOrderConfirmationJobAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new Exception($"Order {orderId} not found");

        await _emailService.SendOrderConfirmationAsync(
            order.User!.Email,
            order.User.Email.Split('@')[0],
            order);
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendShippingNotificationJobAsync(int orderId, string trackingNumber)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new Exception($"Order {orderId} not found");

        await _emailService.SendShippingNotificationAsync(
            order.User!.Email,
            order.User.Email.Split('@')[0],
            order,
            trackingNumber);
    }
}