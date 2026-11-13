namespace TccProway.Api.Domain.Entities;

/// <summary>Persiste somente SHA-256 do segredo apresentado pelo cliente.</summary>
public sealed class RefreshToken
{
    private RefreshToken() { }

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime ExpiraEmUtc { get; private set; }
    public DateTime? RevogadoEmUtc { get; private set; }
    public string? SubstituidoPorTokenHash { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();
    public bool EstaAtivo => RevogadoEmUtc is null && ExpiraEmUtc > DateTime.UtcNow;

    public static RefreshToken Criar(Guid usuarioId, string tokenHash, DateTime expiraEmUtc) => new()
    {
        Id = Guid.NewGuid(), UsuarioId = usuarioId, TokenHash = tokenHash,
        CriadoEmUtc = DateTime.UtcNow, ExpiraEmUtc = expiraEmUtc
    };

    public void Revogar(string? substituidoPorHash = null)
    {
        RevogadoEmUtc ??= DateTime.UtcNow;
        SubstituidoPorTokenHash = substituidoPorHash;
        Version = Guid.NewGuid();
    }
}
