using E_commerce_API.Dtos;
using E_commerce_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce_API.Controllers;

[ApiController, Authorize, Route("api/cart")]
public sealed class CartController(ICartService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartResponse>> Get(CancellationToken ct) => Ok(await service.GetAsync(User.UserId(), ct));

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> Add(AddCartItemRequest request, CancellationToken ct) => Ok(await service.AddAsync(User.UserId(), request, ct));
}
