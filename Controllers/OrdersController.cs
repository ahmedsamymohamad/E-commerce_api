using E_commerce_API.Dtos;
using E_commerce_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce_API.Controllers;

[ApiController, Authorize, Route("api/orders")]
public sealed class OrdersController(IOrderService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Checkout(CancellationToken ct)
    {
        var order = await service.CheckoutAsync(User.UserId(), ct);
        return Created("/api/orders/my-orders", order);
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetMine(CancellationToken ct) => Ok(await service.GetMineAsync(User.UserId(), ct));

    [Authorize(Roles = "Admin"), HttpPut("{id:int}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(int id, UpdateOrderStatusRequest request, CancellationToken ct) => Ok(await service.UpdateStatusAsync(id, request.Status, ct));
}
