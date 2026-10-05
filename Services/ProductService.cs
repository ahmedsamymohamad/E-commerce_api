using E_commerce_API.Data;
using E_commerce_API.Dtos;
using E_commerce_API.Models;
using Microsoft.EntityFrameworkCore;

namespace E_commerce_API.Services;

public interface IProductService
{
    Task<ProductListResponse> SearchAsync(string? search, string? category, string? sort, int page, int pageSize, CancellationToken ct);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct);
}

public sealed class ProductService(AppDbContext db) : IProductService
{
    public async Task<ProductListResponse> SearchAsync(string? search, string? category, string? sort, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Brand).Include(x => x.Images).Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category.Name == category);
        query = sort?.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(x => x.Price),
            "price_desc" => query.OrderByDescending(x => x.Price),
            "name" => query.OrderBy(x => x.Name),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
        var total = await query.CountAsync(ct);
        var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new ProductListResponse(products.Select(Map).ToList(), page, pageSize, total);
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct)
        => (await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Brand).Include(x => x.Images).SingleOrDefaultAsync(x => x.Id == id && x.IsActive, ct)) is { } p ? Map(p) : null;

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId, ct) || !await db.Brands.AnyAsync(x => x.Id == request.BrandId, ct))
            throw new KeyNotFoundException("Category or brand was not found.");
        var product = new Product { Name = request.Name, Description = request.Description, Price = request.Price, StockQuantity = request.StockQuantity, CategoryId = request.CategoryId, BrandId = request.BrandId };
        product.Images = request.Images.Distinct().Select((url, index) => new ProductImage { Url = url, IsPrimary = index == 0, Product = product }).ToList();
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        await db.Entry(product).Reference(x => x.Category).LoadAsync(ct);
        await db.Entry(product).Reference(x => x.Brand).LoadAsync(ct);
        return Map(product);
    }

    private static ProductResponse Map(Product p) => new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category.Name, p.Brand.Name, p.Images.Select(i => i.Url).ToList());
}
