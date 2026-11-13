using TccProway.Api.Domain.Entities;

namespace TccProway.Api.Application.Abstractions;

public sealed record IssuedRefreshToken(string PlainText, RefreshToken Entity);
public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Usuario usuario);
    IssuedRefreshToken CreateRefreshToken(Guid usuarioId);
    string HashRefreshToken(string token);
}
