using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using E_commerce_API.Data;
using E_commerce_API.Dtos;
using E_commerce_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace E_commerce_API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
}

public sealed class AuthService(AppDbContext db, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(x => x.Email == request.Email.ToLowerInvariant(), ct))
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            Name = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return CreateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.ToLowerInvariant(), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");
        return CreateToken(user);
    }

    private AuthResponse CreateToken(User user)
    {
        var jwt = configuration.GetSection("Jwt");
        var expires = DateTimeOffset.UtcNow.AddMinutes(jwt.GetValue<int>("ExpirationMinutes", 60));
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Role, user.Role.ToString()), new Claim(ClaimTypes.Email, user.Email) };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT key is missing."))), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims, expires: expires.UtcDateTime, signingCredentials: credentials);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, user.Role);
    }
}
