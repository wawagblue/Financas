using Financas.MicroServices;
using Financas.MicroServices.Auth;
using Financas.MicroServices.IaWhatsApp;
using Financas.MicroServices.OpenFinance;
using Microsoft.AspNetCore.Mvc;

namespace Financas.Controllers;

[ApiController]
[Route("api/servicos")]
[Produces("application/json")]
public class ServicosController(
    IAuthClient auth,
    IIaWhatsAppClient iaWhatsApp,
    IOpenFinanceClient openFinance) : ControllerBase
{
    /// <summary>Verifica se os microserviços dos outros devs estão no ar.</summary>
    /// <remarks>Sempre responde 200; cada serviço indica se está disponível ou não.</remarks>
    [HttpGet("status")]
    [ProducesResponseType<IEnumerable<StatusServico>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StatusServico>>> Status(CancellationToken ct)
    {
        var status = await Task.WhenAll(
            auth.VerificarDisponibilidadeAsync(ct),
            iaWhatsApp.VerificarDisponibilidadeAsync(ct),
            openFinance.VerificarDisponibilidadeAsync(ct));
        // Mantém o formato usado pelo frontend, mas não expõe hosts, portas ou mensagens de exceção.
        return Ok(status.Select(servico => new StatusServico(servico.Nome, servico.Disponivel)));
    }
}
