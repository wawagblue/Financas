using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccProway.Api.Application.Contracts;
using TccProway.Api.Application.Services;

namespace TccProway.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(UserService users) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request, CancellationToken ct)
    {
        var result = await users.RegisterAsync(request, ct);
        return result is null ? Conflict(new { message = "Email já cadastrado." }) : Created("/api/v1/users/me", result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)) return Unauthorized();
        var user = await users.GetAsync(id, ct);
        return user is null ? NotFound() : Ok(UserService.ToResponse(user));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateMe(UpdateUserRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)) return Unauthorized();
        var result = await users.UpdateAsync(id, request, ct);
        return result is null ? Conflict(new { message = "Email já cadastrado ou utilizador inexistente." }) : Ok(result);
    }
}
