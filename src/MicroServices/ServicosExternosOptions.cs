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
}

public class ServicoExternoOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string CaminhoHealth { get; set; } = "/health";
    public int TimeoutSegundos { get; set; } = 10;
}
