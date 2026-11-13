using Financas.Models;
using Financas.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financas.Data.Configurations;

public class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> builder)
    {
        builder.ToTable("transacoes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.UsuarioId).HasColumnType("char(36)").IsRequired();

        builder.Property(t => t.Descricao).IsRequired().HasMaxLength(TransacaoService.TamanhoMaximoDescricao);
        builder.Property(t => t.Valor).HasPrecision(18, 2);
        builder.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Categoria).HasMaxLength(TransacaoService.TamanhoMaximoCategoria);
        builder.Property(t => t.Data).IsRequired();
        builder.Property(t => t.CriadoEm).IsRequired();

        builder.HasIndex(t => t.Data);
        builder.HasIndex(t => new { t.UsuarioId, t.Data });
    }
}
