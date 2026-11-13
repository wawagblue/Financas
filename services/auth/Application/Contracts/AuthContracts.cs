namespace TccProway.Api.Application.Contracts;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc);
public sealed record RegisterUserRequest(string Nome, string Email, string Password);
public sealed record UpdateUserRequest(string Nome, string Email);
public sealed record UserResponse(Guid Id, string Nome, string Email, DateTime CriadoEmUtc, DateTime AtualizadoEmUtc);
