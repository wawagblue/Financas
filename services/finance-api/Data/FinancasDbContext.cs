using Financas.Models;
using Microsoft.EntityFrameworkCore;

namespace Financas.Data;

public class FinancasDbContext(DbContextOptions<FinancasDbContext> options) : DbContext(options)
{
    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<DespesaRecorrente> DespesasRecorrentes => Set<DespesaRecorrente>();
    public DbSet<MetaEconomia> MetasEconomia => Set<MetaEconomia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancasDbContext).Assembly);
    }
}
