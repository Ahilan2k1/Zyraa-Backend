using MyShop.Models;

namespace MyShop.Services;

public static class EmailTemplates
{
    private static string BaseTemplate(string content)
    {
        return @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset=""utf-8""/>
            <style>
                body { font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 0; }
                 .container { max-width: 600px; margin: 30px auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
                 .header { background: #2d6a4f; color: white; padding: 24px; text-align: center; }
                 .header h1 { margin: 0; font-size: 24px; }
                 .body { padding: 32px; color: #333; }
                 .footer { background: #f0f0f0; text-align: center; padding: 16px; font-size: 12px; color: #888; }
                 .btn { display: inline-block; background: #2d6a4f; color: white; padding: 12px 28px; border-radius: 4px; text-decoration: none; margin-top: 16px; }
                 table { width: 100%; border-collapse: collapse; margin: 16px 0; }
                 th { background: #f0f0f0; padding: 10px; text-align: left; font-size: 13px; }
                 td { padding: 10px; border-bottom: 1px solid #eee; font-size: 14px; }
                 .total-row td { font-weight: bold; background: #f9f9f9; }
                 .badge { display: inline-block; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: bold; }
                 .badge-green { background: #d4edda; color: #155724; }
                 .badge-blue { background: #d1ecf1; color: #0c5460; }
            </style>
        </head>
        <body>
        <div class=""container"">
            <div class=""header"">
              <h1>MyShop India</h1>
            </div>
            <div class=""body"">
              " + content + @"
            </div>
            <div class=""footer"">
              © 2025 MyShop India · All prices include 18% GST · Support: support@myshop.in
            </div>
          </div>
        </body>
    </html>";
    }
    public static string WelcomeEmail(string userName) => BaseTemplate($"""
        <h2>Welcome to MyShop India, {userName}! 🎉</h2>
        <p>We're thrilled to have you on board. Start exploring thousands of products at the best prices.</p>
        <p>✅ Secure payments via Razorpay (UPI, Cards, Net Banking)</p>
        <p>✅ Fast delivery across India</p>
        <p>✅ 18% GST included in all prices</p>
        <a href="https://myshop.in/products" class="btn">Start Shopping</a>
        """);

    public static string OrderConfirmationEmail(Order order, string userName) =>
        BaseTemplate($"""
        <h2>Order Confirmed! ✅</h2>
        <p>Hi {userName}, your order has been placed successfully.</p>

        <p>
            <strong>Order Number:</strong> {order.OrderNumber}<br/>
            <strong>Date:</strong> {order.CreatedAt:dd MMM yyyy, hh:mm tt}<br/>
            <strong>Status:</strong> <span class="badge badge-green">Confirmed</span>
        </p>

        <table>
            <tr>
                <th>Product</th>
                <th>Qty</th>
                <th>Price</th>
                <th>Subtotal</th>
            </tr>
            {string.Join("", order.OrderItems.Select(item => $"""
                <tr>
                    <td>{item.ProductName}</td>
                    <td>{item.Quantity}</td>
                    <td>₹{item.Price:N2}</td>
                    <td>₹{item.Price * item.Quantity:N2}</td>
                </tr>
            """))}
            <tr>
                <td colspan="3">Shipping</td>
                <td>₹100.00</td>
            </tr>
            <tr>
                <td colspan="3">GST (18%)</td>
                <td>₹{order.Total * 0.18m:N2}</td>
            </tr>
            <tr class="total-row">
                <td colspan="3">Total</td>
                <td>₹{order.Total:N2}</td>
            </tr>
        </table>

        <p><strong>Shipping To:</strong><br/>{order.ShippingAddress}</p>
        <p>We'll notify you once your order ships. Thank you for shopping with MyShop India! 🇮🇳</p>
        """);

    public static string ShippingNotificationEmail(Order order, string userName, string trackingNumber) =>
        BaseTemplate($"""
        <h2>Your Order Has Shipped! 🚚</h2>
        <p>Hi {userName}, great news — your order is on its way!</p>

        <p>
            <strong>Order Number:</strong> {order.OrderNumber}<br/>
            <strong>Tracking Number:</strong> {trackingNumber}<br/>
            <strong>Status:</strong> <span class="badge badge-blue">Shipped</span>
        </p>

        <p>Expected delivery: <strong>3-5 business days</strong></p>
        <p><strong>Delivering To:</strong><br/>{order.ShippingAddress}</p>

        <a href="https://myshop.in/orders/{order.Id}" class="btn">Track Your Order</a>
        """);
}