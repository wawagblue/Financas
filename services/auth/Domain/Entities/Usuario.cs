namespace TccProway.Api.Domain.Entities;

public sealed class Usuario
{
    private Usuario() { } // EF Core

    public Usuario(string nome, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        CriadoEmUtc = DateTime.UtcNow;
        AtualizadoEmUtc = CriadoEmUtc;
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime AtualizadoEmUtc { get; private set; }
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public void AtualizarPerfil(string nome, string email)
    {
        Nome = nome;
        Email = email.Trim().ToLowerInvariant();
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}
