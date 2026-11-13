using Financas.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Financas.Tests.Api;

/// <summary>
/// Sobe a API em memória trocando o MySQL por um banco InMemory isolado por instância.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nomeBanco = $"financas-testes-{Guid.NewGuid()}";

    public Action<IServiceCollection>? ConfigurarServicos { get; init; }
    public bool UsarJwtReal { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("JWT_SECRET", "local_test_secret_value_at_least_32_bytes_long");

        builder.ConfigureTestServices(services =>
        {
            if (!UsarJwtReal)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = UsuarioTesteHandler.Esquema;
                    options.DefaultChallengeScheme = UsuarioTesteHandler.Esquema;
                }).AddScheme<AuthenticationSchemeOptions, UsuarioTesteHandler>(UsuarioTesteHandler.Esquema, _ => { });
            }

            services.RemoveAll(typeof(DbContextOptions<FinancasDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<FinancasDbContext>));
            services.AddDbContext<FinancasDbContext>(o => o.UseInMemoryDatabase(_nomeBanco));

            ConfigurarServicos?.Invoke(services);
        });
    }
}

internal sealed class UsuarioTesteHandler(
    Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
    Microsoft.Extensions.Logging.ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Esquema = "UsuarioTeste";
    public static readonly Guid IdUsuario = Guid.Parse("11111111-1111-1111-1111-111111111111");

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identidade = new ClaimsIdentity([new Claim("sub", IdUsuario.ToString())], Esquema);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identidade), Esquema);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

internal static class ServiceCollectionTestExtensions
{
    public static void RemoveAll(this IServiceCollection services, Type tipo)
    {
        foreach (var descritor in services.Where(d => d.ServiceType == tipo).ToList())
            services.Remove(descritor);
    }
}
