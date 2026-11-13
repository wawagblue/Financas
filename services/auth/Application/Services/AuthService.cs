using Microsoft.EntityFrameworkCore;
using TccProway.Api.Application.Abstractions;
using TccProway.Api.Application.Contracts;
using TccProway.Api.Infrastructure.Persistence;

namespace TccProway.Api.Application.Services;

public sealed class AuthService(AppDbContext db, IPasswordHasher hasher, ITokenService tokens)
{
    public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Usuarios.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash)) return null;
        return await IssueAsync(user, ct);
    }

    public async Task<TokenResponse?> RefreshAsync(string plainToken, CancellationToken ct)
    {
        var hash = tokens.HashRefreshToken(plainToken);
        var current = await db.RefreshTokens.Include(x => x.Usuario).SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (current is null || !current.EstaAtivo) return null;
        var replacement = tokens.CreateRefreshToken(current.UsuarioId);
        current.Revogar(replacement.Entity.TokenHash);
        db.RefreshTokens.Add(replacement.Entity);
        var access = tokens.CreateAccessToken(current.Usuario);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return null; } // apenas uma rotação concorrente pode vencer
        return new TokenResponse(access.Token, replacement.PlainText, access.ExpiresAtUtc);
    }

    public async Task<bool> LogoutAsync(string plainToken, CancellationToken ct)
    {
        var hash = tokens.HashRefreshToken(plainToken);
        var token = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (token is null || !token.EstaAtivo) return false;
        token.Revogar();
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<TokenResponse> IssueAsync(Domain.Entities.Usuario user, CancellationToken ct)
    {
        var refresh = tokens.CreateRefreshToken(user.Id);
        db.RefreshTokens.Add(refresh.Entity);
        var access = tokens.CreateAccessToken(user);
        await db.SaveChangesAsync(ct);
        return new TokenResponse(access.Token, refresh.PlainText, access.ExpiresAtUtc);
    }
}
