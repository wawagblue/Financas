using Microsoft.Extensions.Options;

namespace Financas.MicroServices.Auth;

public class AuthClient(HttpClient http, IOptions<ServicosExternosOptions> options)
    : ServicoExternoClientBase(http, "Autenticação", options.Value.Auth.CaminhoHealth), IAuthClient;
