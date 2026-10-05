using E_commerce_API.Dtos;
using E_commerce_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce_API.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct) => Ok(await service.RegisterAsync(request, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct) => Ok(await service.LoginAsync(request, ct));
}
