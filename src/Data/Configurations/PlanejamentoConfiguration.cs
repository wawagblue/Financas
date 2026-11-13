using Financas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financas.Data.Configurations;

public class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> b)
    {
        b.ToTable("orcamentos"); b.HasKey(x => x.Id);
        b.Property(x => x.UsuarioId).HasColumnType("char(36)");
        b.Property(x => x.Categoria).HasMaxLength(100).IsRequired();
        b.Property(x => x.LimiteMensal).HasPrecision(18, 2);
        b.Property(x => x.CriadoEm).IsRequired();
        b.HasIndex(x => new { x.UsuarioId, x.Categoria }).IsUnique();
    }
}

public class DespesaRecorrenteConfiguration : IEntityTypeConfiguration<DespesaRecorrente>
{
    public void Configure(EntityTypeBuilder<DespesaRecorrente> b)
    {
        b.ToTable("despesas_recorrentes"); b.HasKey(x => x.Id);
        b.Property(x => x.UsuarioId).HasColumnType("char(36)");
        b.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        b.Property(x => x.Valor).HasPrecision(18, 2);
        b.Property(x => x.Categoria).HasMaxLength(100).IsRequired();
        b.Property(x => x.ProximoVencimento).IsRequired(); b.Property(x => x.CriadoEm).IsRequired();
        b.HasIndex(x => new { x.UsuarioId, x.ProximoVencimento });
    }
}

public class MetaEconomiaConfiguration : IEntityTypeConfiguration<MetaEconomia>
{
    public void Configure(EntityTypeBuilder<MetaEconomia> b)
    {
        b.ToTable("metas_economia"); b.HasKey(x => x.Id);
        b.Property(x => x.UsuarioId).HasColumnType("char(36)");
        b.Property(x => x.Nome).HasMaxLength(120).IsRequired();
        b.Property(x => x.ValorAlvo).HasPrecision(18, 2);
        b.Property(x => x.ValorGuardado).HasPrecision(18, 2);
        b.HasIndex(x => new { x.UsuarioId, x.Prazo });
    }
}
