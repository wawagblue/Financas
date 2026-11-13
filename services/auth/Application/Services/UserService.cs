using Microsoft.EntityFrameworkCore;
using TccProway.Api.Application.Abstractions;
using TccProway.Api.Application.Contracts;
using TccProway.Api.Domain.Entities;
using TccProway.Api.Infrastructure.Persistence;

namespace TccProway.Api.Application.Services;

public sealed class UserService(AppDbContext db, IPasswordHasher hasher)
{
    public async Task<UserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.Email == email, ct)) return null;
        var user = new Usuario(request.Nome.Trim(), email, hasher.Hash(request.Password));
        db.Usuarios.Add(user);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return null; } // protege também contra corridas de email único
        return ToResponse(user);
    }

    public Task<Usuario?> GetAsync(Guid id, CancellationToken ct) => db.Usuarios.SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var user = await db.Usuarios.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return null;
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.Id != id && x.Email == email, ct)) return null;
        user.AtualizarPerfil(request.Nome.Trim(), email);
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return null; }
        return ToResponse(user);
    }

    public static UserResponse ToResponse(Usuario user) => new(user.Id, user.Nome, user.Email, user.CriadoEmUtc, user.AtualizadoEmUtc);
}
