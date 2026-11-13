using Financas.Repositories;
using Financas.Services;
using Financas.MicroServices;
using Financas.MicroServices.Auth;
using Financas.MicroServices.IaWhatsApp;
using Financas.MicroServices.OpenFinance; 
using Financas.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Financas;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Financas") ?? string.Empty;
        var versaoMySql = Version.Parse(configuration["Database:VersaoMySql"] ?? "8.4.0");

        services.AddDbContext<FinancasDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(versaoMySql),
                mysql => mysql.EnableRetryOnFailure()));

        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<TransacaoService>();
        services.AddSingleton(TimeProvider.System);

        AddClientesServicosExternos(services, configuration);

        return services;
    }

    private static void AddClientesServicosExternos(IServiceCollection services, IConfiguration configuration)
    {
        var secao = configuration.GetSection(ServicosExternosOptions.Secao);
        services.Configure<ServicosExternosOptions>(secao);
        var opcoes = secao.Get<ServicosExternosOptions>() ?? new ServicosExternosOptions();

        services.AddHttpClient<IAuthClient, AuthClient>(http => Configurar(http, opcoes.Auth));
        services.AddHttpClient<IIaWhatsAppClient, IaWhatsAppClient>(http => Configurar(http, opcoes.IaWhatsApp));
        services.AddHttpClient<IOpenFinanceClient, OpenFinanceClient>(http => Configurar(http, opcoes.OpenFinance));
    }

    private static void Configurar(HttpClient http, ServicoExternoOptions opcoes)
    {
        if (Uri.TryCreate(opcoes.BaseUrl, UriKind.Absolute, out var baseUrl))
            http.BaseAddress = baseUrl;
        http.Timeout = TimeSpan.FromSeconds(opcoes.TimeoutSegundos);
    }
}
