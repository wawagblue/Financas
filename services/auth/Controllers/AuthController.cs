using Microsoft.AspNetCore.Mvc;
using TccProway.Api.Application.Contracts;
using TccProway.Api.Application.Services;

namespace TccProway.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        return result is null ? Unauthorized(new { message = "Credenciais inválidas." }) : Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await auth.RefreshAsync(request.RefreshToken, ct);
        return result is null ? Unauthorized(new { message = "Refresh token inválido, expirado ou revogado." }) : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request.RefreshToken, ct); // idempotente; não revela se o token existia
        return NoContent();
    }
}
