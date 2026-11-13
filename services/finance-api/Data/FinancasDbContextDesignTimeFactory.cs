using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Financas.Data;

/// <summary>
/// Usada só pelo `dotnet ef` (migrations). Não conecta no banco para gerar migrações;
/// para `dotnet ef database update`, defina a variável de ambiente ConnectionStrings__Financas.
/// </summary>
public class FinancasDbContextDesignTimeFactory : IDesignTimeDbContextFactory<FinancasDbContext>
{
    public FinancasDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Financas")
            ?? "Server=localhost;Port=3306;Database=financas;User=financas;Password=design-time";

        var options = new DbContextOptionsBuilder<FinancasDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

        return new FinancasDbContext(options);
    }
}
