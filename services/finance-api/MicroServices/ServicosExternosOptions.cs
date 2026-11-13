namespace Financas.MicroServices;

/// <summary>
/// Endereços dos microserviços dos outros devs. Vem da seção "ServicosExternos" do appsettings
/// ou de variáveis de ambiente (ex.: ServicosExternos__Auth__BaseUrl).
/// </summary>
public class ServicosExternosOptions
{
    public const string Secao = "ServicosExternos";

    public ServicoExternoOptions Auth { get; set; } = new();
    public ServicoExternoOptions IaWhatsApp { get; set; } = new();
    public ServicoExternoOptions OpenFinance { get; set; } = new();

    public static void ValidarUrlsHttps(IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
            return;

        string[] caminhos =
        [
            "ServicosExternos:Auth:BaseUrl",
            "ServicosExternos:IaWhatsApp:BaseUrl",
            "ServicosExternos:OpenFinance:BaseUrl",
            "ReverseProxy:Clusters:auth:Destinations:principal:Address",
            "ReverseProxy:Clusters:ia-whatsapp:Destinations:principal:Address",
            "ReverseProxy:Clusters:open-finance:Destinations:principal:Address"
        ];

        foreach (var caminho in caminhos)
        {
            var valor = configuration[caminho];
            if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException($"A configuração '{caminho}' deve conter uma URL HTTPS válida fora de Development.");
        }
    }
}

public class ServicoExternoOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string CaminhoHealth { get; set; } = "/health";
    public int TimeoutSegundos { get; set; } = 10;
}
