using System.ComponentModel.DataAnnotations;
using E_commerce_API.Models;

namespace E_commerce_API.Dtos;

/// <summary>Credentials used to register a customer.</summary>
public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(200)] public required string Email { get; init; }
    [Required, MinLength(8), MaxLength(100)] public required string Password { get; init; }
    [Required, MaxLength(100)] public required string Name { get; init; }
}

/// <summary>Credentials used to authenticate a user.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress] public required string Email { get; init; }
    [Required] public required string Password { get; init; }
}

/// <summary>JWT authentication response.</summary>
public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, UserRole Role);

/// <summary>Product returned by the API.</summary>
public sealed record ProductResponse(int Id, string Name, string? Description, decimal Price, int StockQuantity, string Category, string Brand, IReadOnlyList<string> Images);

/// <summary>Paginated product response.</summary>
public sealed record ProductListResponse(IReadOnlyList<ProductResponse> Items, int Page, int PageSize, int TotalCount);

/// <summary>Payload used to create a product.</summary>
public sealed record CreateProductRequest
{
    [Required, MaxLength(200)] public required string Name { get; init; }
    [MaxLength(2000)] public string? Description { get; init; }
    [Range(0.01, 1000000)] public decimal Price { get; init; }
    [Range(0, int.MaxValue)] public int StockQuantity { get; init; }
    [Range(1, int.MaxValue)] public int CategoryId { get; init; }
    [Range(1, int.MaxValue)] public int BrandId { get; init; }
    public List<string> Images { get; init; } = [];
}

/// <summary>Cart item payload.</summary>
public sealed record AddCartItemRequest
{
    [Range(1, int.MaxValue)] public int ProductId { get; init; }
    [Range(1, 1000)] public int Quantity { get; init; }
}

/// <summary>Cart contents and calculated total.</summary>
public sealed record CartResponse(IReadOnlyList<CartItemResponse> Items, decimal Total);

/// <summary>Cart line item.</summary>
public sealed record CartItemResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

/// <summary>Order returned by the API.</summary>
public sealed record OrderResponse(int Id, OrderStatus Status, decimal Total, DateTimeOffset CreatedAt, IReadOnlyList<OrderItemResponse> Items);

/// <summary>Order line item.</summary>
public sealed record OrderItemResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity);

/// <summary>Order status update payload.</summary>
public sealed record UpdateOrderStatusRequest { public required OrderStatus Status { get; init; } }
