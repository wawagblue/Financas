using Financas.MicroServices;
using Microsoft.Extensions.Configuration;

namespace Financas.Tests.Infra;

public class ServicosExternosHttpsTests
{
    private static readonly string[] ChavesObrigatorias =
    [
        "ServicosExternos:Auth:BaseUrl",
        "ServicosExternos:IaWhatsApp:BaseUrl",
        "ServicosExternos:OpenFinance:BaseUrl",
        "ReverseProxy:Clusters:auth:Destinations:principal:Address",
        "ReverseProxy:Clusters:ia-whatsapp:Destinations:principal:Address",
        "ReverseProxy:Clusters:open-finance:Destinations:principal:Address"
    ];

    [Fact]
    public void Production_aceita_apenas_urls_https_para_todos_os_servicos()
    {
        var configuracao = CriarConfiguracao("https://servico.test/");

        var acao = () => ServicosExternosOptions.ValidarUrlsHttps(configuracao, isDevelopment: false);

        acao.Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(Chaves))]
    public void Production_rejeita_url_http_ou_ausente(string chave, string? valor)
    {
        var valores = ChavesObrigatorias.ToDictionary(c => c, _ => "https://servico.test/");
        valores[chave] = valor ?? string.Empty;
        var configuracao = new ConfigurationBuilder().AddInMemoryCollection(valores!).Build();

        var acao = () => ServicosExternosOptions.ValidarUrlsHttps(configuracao, isDevelopment: false);

        acao.Should().Throw<InvalidOperationException>().WithMessage($"*{chave}*");
    }

    [Fact]
    public void Development_permite_url_http_local()
    {
        var configuracao = CriarConfiguracao("http://servico.local/");

        var acao = () => ServicosExternosOptions.ValidarUrlsHttps(configuracao, isDevelopment: true);

        acao.Should().NotThrow();
    }

    public static TheoryData<string, string?> Chaves => CriarDados();

    private static TheoryData<string, string?> CriarDados()
    {
        var dados = new TheoryData<string, string?>();
        foreach (var chave in ChavesObrigatorias)
        {
            dados.Add(chave, "http://servico.test/");
            dados.Add(chave, null);
        }
        return dados;
    }

    private static IConfigurationRoot CriarConfiguracao(string url) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(ChavesObrigatorias.ToDictionary(c => c, _ => url)!)
            .Build();
}
