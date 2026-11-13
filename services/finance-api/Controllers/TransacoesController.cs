using Financas.Contracts;
using Financas.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Financas.Controllers;

[ApiController]
[Authorize(Policy = "UsuarioAutenticado")]
[Route("api/transacoes")]
[Produces("application/json")]
public class TransacoesController(TransacaoService service) : ControllerBase
{
    /// <summary>Lista todas as transações (mais recentes primeiro).</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<TransacaoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TransacaoResponse>>> Listar(CancellationToken ct)
    {
        var transacoes = await service.ListarAsync(UsuarioId, ct);
        return Ok(transacoes.Select(TransacaoResponse.De));
    }

    /// <summary>Busca uma transação pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TransacaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransacaoResponse>> Obter(Guid id, CancellationToken ct)
    {
        var resultado = await service.ObterAsync(id, UsuarioId, ct);
        return resultado.Sucesso ? Ok(TransacaoResponse.De(resultado.Valor!)) : this.ParaErro(resultado);
    }

    /// <summary>Cria uma nova transação.</summary>
    [HttpPost]
    [ProducesResponseType<TransacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransacaoResponse>> Criar(TransacaoRequest request, CancellationToken ct)
    {
        var resultado = await service.CriarAsync(UsuarioId, request.ParaDominio(), ct);
        if (!resultado.Sucesso)
            return this.ParaErro(resultado);

        var resposta = TransacaoResponse.De(resultado.Valor!);
        return CreatedAtAction(nameof(Obter), new { id = resposta.Id }, resposta);
    }

    /// <summary>Atualiza uma transação existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TransacaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransacaoResponse>> Atualizar(Guid id, TransacaoRequest request, CancellationToken ct)
    {
        var resultado = await service.AtualizarAsync(id, UsuarioId, request.ParaDominio(), ct);
        return resultado.Sucesso ? Ok(TransacaoResponse.De(resultado.Valor!)) : this.ParaErro(resultado);
    }

    /// <summary>Remove uma transação.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        var resultado = await service.RemoverAsync(id, UsuarioId, ct);
        return resultado.Sucesso ? NoContent() : this.ParaErro(resultado);
    }
    private Guid UsuarioId => Guid.Parse(User.FindFirst("sub")!.Value);
}
