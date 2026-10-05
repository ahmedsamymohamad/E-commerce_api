namespace E_commerce_API.Models;

public enum UserRole { Customer, Admin }
public enum OrderStatus { Pending, Paid, Processing, Shipped, Delivered, Cancelled }

public sealed class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = [];
}

public sealed class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}

public sealed class Brand
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = [];
}

public sealed class ProductImage
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public bool IsPrimary { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

public sealed class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<CartItem> Items { get; set; } = [];
}

public sealed class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
}

public sealed class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = [];
}

public sealed class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
