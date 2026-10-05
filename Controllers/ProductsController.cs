using E_commerce_API.Dtos;
using E_commerce_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce_API.Controllers;

[ApiController, Route("api/products")]
public sealed class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProductListResponse>> Search([FromQuery] string? search, [FromQuery] string? category, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await service.SearchAsync(search, category, sort, page, pageSize, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
        => (await service.GetByIdAsync(id, ct)) is { } product ? Ok(product) : NotFound();

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var product = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
