using TccProway.Api.Application.Abstractions;

namespace TccProway.Api.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    // BCrypt gera salt aleatório automaticamente. Cost 12 é ajustável após medição no ambiente real.
    private const int WorkFactor = 12;
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    public bool Verify(string password, string passwordHash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, passwordHash); }
        catch (BCrypt.Net.SaltParseException) { return false; }
    }
}
