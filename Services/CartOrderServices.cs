using System.Security.Claims;
using E_commerce_API.Data;
using E_commerce_API.Dtos;
using E_commerce_API.Models;
using Microsoft.EntityFrameworkCore;

namespace E_commerce_API.Services;

public interface ICartService { Task<CartResponse> GetAsync(int userId, CancellationToken ct); Task<CartResponse> AddAsync(int userId, AddCartItemRequest request, CancellationToken ct); }
public interface IOrderService { Task<OrderResponse> CheckoutAsync(int userId, CancellationToken ct); Task<IReadOnlyList<OrderResponse>> GetMineAsync(int userId, CancellationToken ct); Task<OrderResponse> UpdateStatusAsync(int id, OrderStatus status, CancellationToken ct); }

public sealed class CartService(AppDbContext db) : ICartService
{
    public async Task<CartResponse> GetAsync(int userId, CancellationToken ct) => Map(await LoadAsync(userId, ct));

    public async Task<CartResponse> AddAsync(int userId, AddCartItemRequest request, CancellationToken ct)
    {
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == request.ProductId && x.IsActive, ct) ?? throw new KeyNotFoundException("Product was not found.");
        var cart = await LoadAsync(userId, ct);
        var item = cart.Items.SingleOrDefault(x => x.ProductId == request.ProductId);
        if ((item?.Quantity ?? 0) + request.Quantity > product.StockQuantity) throw new InvalidOperationException("Requested quantity exceeds available stock.");
        if (item is null) cart.Items.Add(new CartItem { Product = product, ProductId = product.Id, Quantity = request.Quantity });
        else item.Quantity += request.Quantity;
        await db.SaveChangesAsync(ct);
        return Map(cart);
    }

    private async Task<Cart> LoadAsync(int userId, CancellationToken ct)
    {
        var cart = await db.Carts.Include(x => x.Items).ThenInclude(x => x.Product).SingleOrDefaultAsync(x => x.UserId == userId, ct);
        if (cart is not null) return cart;
        cart = new Cart { UserId = userId };
        db.Carts.Add(cart);
        await db.SaveChangesAsync(ct);
        return cart;
    }

    private static CartResponse Map(Cart cart) => new(cart.Items.Select(x => new CartItemResponse(x.ProductId, x.Product.Name, x.Product.Price, x.Quantity, x.Product.Price * x.Quantity)).ToList(), cart.Items.Sum(x => x.Product.Price * x.Quantity));
}

public sealed class OrderService(AppDbContext db, ILogger<OrderService> logger) : IOrderService
{
    public async Task<OrderResponse> CheckoutAsync(int userId, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var cart = await db.Carts.Include(x => x.Items).ThenInclude(x => x.Product).SingleOrDefaultAsync(x => x.UserId == userId, ct) ?? throw new InvalidOperationException("Cart is empty.");
        if (cart.Items.Count == 0) throw new InvalidOperationException("Cart is empty.");
        foreach (var item in cart.Items)
            if (item.Product.StockQuantity < item.Quantity) throw new InvalidOperationException($"Insufficient stock for {item.Product.Name}.");
        var order = new Order { UserId = userId, Status = OrderStatus.Pending };
        foreach (var item in cart.Items)
        {
            item.Product.StockQuantity -= item.Quantity;
            order.Items.Add(new OrderItem { ProductId = item.ProductId, ProductName = item.Product.Name, UnitPrice = item.Product.Price, Quantity = item.Quantity });
        }
        order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        db.Orders.Add(order);
        db.CartItems.RemoveRange(cart.Items);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        logger.LogInformation("Created order {OrderId} for user {UserId} with total {Total}", order.Id, userId, order.Total);
        return Map(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetMineAsync(int userId, CancellationToken ct)
        => (await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct)).Select(Map).ToList();

    public async Task<OrderResponse> UpdateStatusAsync(int id, OrderStatus status, CancellationToken ct)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Order was not found.");
        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered) throw new InvalidOperationException("This order can no longer change status.");
        order.Status = status;
        await db.SaveChangesAsync(ct);
        return Map(order);
    }

    private static OrderResponse Map(Order x) => new(x.Id, x.Status, x.Total, x.CreatedAt, x.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList());
}

public static class ClaimsPrincipalExtensions
{
    public static int UserId(this ClaimsPrincipal user) => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(ClaimTypes.Name) ?? throw new UnauthorizedAccessException());
}
