using E_commerce_API.Models;
using Microsoft.EntityFrameworkCore;

namespace E_commerce_API.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Product>().Property(x => x.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Order>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Cart>().HasIndex(x => x.UserId).IsUnique();
        modelBuilder.Entity<CartItem>().HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
        modelBuilder.Entity<Product>().HasOne(x => x.Category).WithMany(x => x.Products).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>().HasOne(x => x.Brand).WithMany(x => x.Products).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Category>().HasData(new Category { Id = 1, Name = "Electronics" }, new Category { Id = 2, Name = "Home" });
        modelBuilder.Entity<Brand>().HasData(new Brand { Id = 1, Name = "Contoso" }, new Brand { Id = 2, Name = "Fabrikam" });
    }
}
