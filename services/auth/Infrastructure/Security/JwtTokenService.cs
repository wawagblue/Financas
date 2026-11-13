using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TccProway.Api.Application.Abstractions;
using TccProway.Api.Domain.Entities;

namespace TccProway.Api.Infrastructure.Security;

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _secret = configuration["Jwt:Secret"] ?? configuration["JWT_SECRET"]
        ?? throw new InvalidOperationException("JWT_SECRET não configurado.");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "TccProway";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "TccProway.Client";
    private readonly int _accessMinutes = configuration.GetValue("Jwt:AccessTokenMinutes", 15);
    private readonly int _refreshDays = configuration.GetValue("Jwt:RefreshTokenDays", 7);

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Usuario usuario)
    {
        var expires = DateTime.UtcNow.AddMinutes(_accessMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_issuer, _audience, claims, DateTime.UtcNow, expires, credentials);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }

    public IssuedRefreshToken CreateRefreshToken(Guid usuarioId)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var plain = Convert.ToBase64String(bytes);
        var hash = HashRefreshToken(plain);
        var entity = RefreshToken.Criar(usuarioId, hash, DateTime.UtcNow.AddDays(_refreshDays));
        return new IssuedRefreshToken(plain, entity);
    }

    public string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
