using Microsoft.Extensions.Options;

namespace Financas.MicroServices.IaWhatsApp;

public class IaWhatsAppClient(HttpClient http, IOptions<ServicosExternosOptions> options)
    : ServicoExternoClientBase(http, "IA + WhatsApp", options.Value.IaWhatsApp.CaminhoHealth), IIaWhatsAppClient;
