using Microsoft.EntityFrameworkCore;
using TccProway.Api.Domain.Entities;

namespace TccProway.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios"); e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnType("char(36)");
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(254).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();
            e.Property(x => x.CriadoEmUtc).IsRequired(); e.Property(x => x.AtualizadoEmUtc).IsRequired();
            e.HasMany(x => x.RefreshTokens).WithOne(x => x.Usuario).HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens"); e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnType("char(36)");
            e.Property(x => x.UsuarioId).HasColumnType("char(36)");
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.Property(x => x.SubstituidoPorTokenHash).HasMaxLength(64);
            e.Property(x => x.Version).HasColumnType("char(36)").IsConcurrencyToken();
            e.Ignore(x => x.EstaAtivo);
        });
    }
}
