using Microsoft.Extensions.Options;

namespace Financas.MicroServices.OpenFinance;

public class OpenFinanceClient(HttpClient http, IOptions<ServicosExternosOptions> options)
    : ServicoExternoClientBase(http, "Open Finance", options.Value.OpenFinance.CaminhoHealth), IOpenFinanceClient;
