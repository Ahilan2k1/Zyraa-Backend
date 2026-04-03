using Microsoft.EntityFrameworkCore;
using MyShop.Models;

namespace MyShop.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Product> Products { get; set; }
        public DbSet<Models.Category> Categories { get; set; }
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.ProductImage> ProductImages { get; set; }

        public DbSet<Models.CartItem> CartItems { get; set; }
        public DbSet<Models.Order> Orders { get; set; }
        public DbSet<Models.OrderItem> OrderItems { get; set; }
        public DbSet<Models.Review> Reviews { get; set; }
        public DbSet<Models.WishlistItem> WishlistItems { get; set; }
        public DbSet<Models.Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedDate = new DateTime(2026, 1, 1, 12, 0, 0);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Footwear" },
                new Category { Id = 2, Name = "Apparel" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product 
                { 
                    Id = 1, 
                    Name = "Nike Air Max", 
                    Description = "Comfortable running shoes", 
                    Price = 129.99m, 
                    Stock = 50, 
                    Category = "Footwear",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CategoryId = 1,
                    Rating = 0.0,
                    ReviewCount = 0  
                },
                new Product 
                { 
                    Id = 2, 
                    Name = "Adidas Ultraboost", 
                    Description = "Premium running shoes", 
                    Price = 180.00m, 
                    Stock = 30, 
                    Category = "Footwear",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CategoryId = 1,
                    Rating = 0.0,
                    ReviewCount = 0 
                },
                new Product 
                { 
                    Id = 3, 
                    Name = "Puma T-Shirt", 
                    Description = "Cotton blend t-shirt", 
                    Price = 24.99m, 
                    Stock = 100, 
                    Category = "Apparel",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CategoryId = 2,
                    Rating = 0.0,
                    ReviewCount = 0  
                }
            );

            modelBuilder.Entity<Product>()
                .HasOne(p => p.CategoryNaviagation)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@myshop.com",
                    // Password: "admin123" hashed
                    PasswordHash = "$2a$11$QKE2p9YYlaxP3zIs7uYpLO5rM0f.xQsUelbkSXDjS7hR2LOQyPi7W",
                    FirstName = "Admin",
                    LastName = "User",
                    Role = "Admin",
                    CreatedAt = seedDate
                });
            
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();
            
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.UserId, ci.ProductId });

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.ProductId, r.UserId })
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique();
        }
    }
}
