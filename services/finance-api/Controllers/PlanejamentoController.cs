using Financas.Contracts;
using Financas.Data;
using Financas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Financas.Controllers;

[ApiController]
[Authorize(Policy = "UsuarioAutenticado")]
[Route("api")]
[Produces("application/json")]
public class PlanejamentoController(FinancasDbContext db, TimeProvider clock) : ControllerBase
{
    private Guid UsuarioId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("orcamentos")]
    public async Task<ActionResult<IReadOnlyList<OrcamentoResponse>>> Orcamentos([FromQuery] string? mes, CancellationToken ct)
    {
        if (!TryMes(mes, out var inicio, out var fim)) return BadRequest("Informe o mês no formato AAAA-MM.");
        var gastos = await db.Transacoes.AsNoTracking().Where(t => t.UsuarioId == UsuarioId && t.Tipo == TipoTransacao.Despesa && t.Data >= inicio && t.Data < fim)
            .GroupBy(t => t.Categoria ?? "Sem categoria").Select(g => new { Categoria = g.Key, Total = g.Sum(t => t.Valor) }).ToDictionaryAsync(x => x.Categoria, x => x.Total, ct);
        var items = await db.Orcamentos.AsNoTracking().Where(x => x.UsuarioId == UsuarioId).OrderBy(x => x.Categoria).ToListAsync(ct);
        return Ok(items.Select(x => { var gasto = gastos.GetValueOrDefault(x.Categoria); return new OrcamentoResponse(x.Id, x.Categoria, x.LimiteMensal, gasto, x.LimiteMensal == 0 ? 0 : Math.Round(gasto / x.LimiteMensal * 100, 1)); }).ToList());
    }

    [HttpPost("orcamentos")]
    public async Task<ActionResult<OrcamentoResponse>> CriarOrcamento(OrcamentoRequest request, CancellationToken ct)
    {
        var categoria = request.Categoria?.Trim();
        if (string.IsNullOrWhiteSpace(categoria) || categoria.Length > 100 || request.LimiteMensal <= 0) return BadRequest("Informe uma categoria e um limite mensal maior que zero.");
        if (await db.Orcamentos.AnyAsync(x => x.UsuarioId == UsuarioId && x.Categoria == categoria, ct)) return Conflict("Já existe um orçamento para essa categoria.");
        var item = new Orcamento { Id = Guid.NewGuid(), UsuarioId = UsuarioId, Categoria = categoria, LimiteMensal = request.LimiteMensal, CriadoEm = clock.GetUtcNow().UtcDateTime };
        db.Orcamentos.Add(item); await db.SaveChangesAsync(ct);
        return StatusCode(StatusCodes.Status201Created, new OrcamentoResponse(item.Id, item.Categoria, item.LimiteMensal, 0, 0));
    }

    [HttpDelete("orcamentos/{id:guid}")]
    public async Task<IActionResult> RemoverOrcamento(Guid id, CancellationToken ct)
    {
        var item = await db.Orcamentos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId, ct);
        if (item is null) return NotFound(); db.Orcamentos.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("recorrencias")]
    public async Task<ActionResult<IReadOnlyList<RecorrenciaResponse>>> Recorrencias(CancellationToken ct) => Ok(await db.DespesasRecorrentes.AsNoTracking().Where(x => x.UsuarioId == UsuarioId).OrderBy(x => x.ProximoVencimento).Select(x => new RecorrenciaResponse(x.Id, x.Descricao, x.Valor, x.Categoria, x.ProximoVencimento)).ToListAsync(ct));

    [HttpPost("recorrencias")]
    public async Task<ActionResult<RecorrenciaResponse>> CriarRecorrencia(RecorrenciaRequest request, CancellationToken ct)
    {
        var descricao = request.Descricao?.Trim(); var categoria = request.Categoria?.Trim();
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length > 200 || string.IsNullOrWhiteSpace(categoria) || categoria.Length > 100 || request.Valor <= 0 || request.ProximoVencimento == default) return BadRequest("Preencha descrição, categoria, valor e próximo vencimento.");
        var item = new DespesaRecorrente { Id = Guid.NewGuid(), UsuarioId = UsuarioId, Descricao = descricao, Categoria = categoria, Valor = request.Valor, DiaVencimento = request.ProximoVencimento.Day, ProximoVencimento = request.ProximoVencimento.Date, CriadoEm = clock.GetUtcNow().UtcDateTime };
        db.DespesasRecorrentes.Add(item); await db.SaveChangesAsync(ct);
        return StatusCode(StatusCodes.Status201Created, new RecorrenciaResponse(item.Id, item.Descricao, item.Valor, item.Categoria, item.ProximoVencimento));
    }

    [HttpPost("recorrencias/{id:guid}/pagar")]
    public async Task<ActionResult<RecorrenciaResponse>> PagarRecorrencia(Guid id, CancellationToken ct)
    {
        var item = await db.DespesasRecorrentes.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId, ct);
        if (item is null) return NotFound();
        var vencimento = item.ProximoVencimento;
        var pagoEm = clock.GetUtcNow().UtcDateTime;
        db.Transacoes.Add(new Transacao { Id = Guid.NewGuid(), UsuarioId = UsuarioId, Descricao = item.Descricao, Valor = item.Valor, Tipo = TipoTransacao.Despesa, Categoria = item.Categoria, Data = pagoEm, CriadoEm = pagoEm });
        var proximoMes = vencimento.AddMonths(1); item.ProximoVencimento = new DateTime(proximoMes.Year, proximoMes.Month, Math.Min(item.DiaVencimento, DateTime.DaysInMonth(proximoMes.Year, proximoMes.Month)));
        await db.SaveChangesAsync(ct); return Ok(new RecorrenciaResponse(item.Id, item.Descricao, item.Valor, item.Categoria, item.ProximoVencimento));
    }

    [HttpDelete("recorrencias/{id:guid}")]
    public async Task<IActionResult> RemoverRecorrencia(Guid id, CancellationToken ct)
    {
        var item = await db.DespesasRecorrentes.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId, ct);
        if (item is null) return NotFound(); db.DespesasRecorrentes.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("metas")]
    public async Task<ActionResult<IReadOnlyList<MetaResponse>>> Metas(CancellationToken ct) => Ok(await db.MetasEconomia.AsNoTracking().Where(x => x.UsuarioId == UsuarioId).OrderBy(x => x.Prazo).Select(x => new MetaResponse(x.Id, x.Nome, x.ValorAlvo, x.ValorGuardado, x.Prazo)).ToListAsync(ct));

    [HttpPost("metas")]
    public async Task<ActionResult<MetaResponse>> CriarMeta(MetaRequest request, CancellationToken ct)
    {
        var nome = request.Nome?.Trim(); if (string.IsNullOrWhiteSpace(nome) || nome.Length > 120 || request.ValorAlvo <= 0) return BadRequest("Informe um nome e um valor-alvo maior que zero.");
        var item = new MetaEconomia { Id = Guid.NewGuid(), UsuarioId = UsuarioId, Nome = nome, ValorAlvo = request.ValorAlvo, Prazo = request.Prazo?.Date, CriadoEm = clock.GetUtcNow().UtcDateTime };
        db.MetasEconomia.Add(item); await db.SaveChangesAsync(ct); return StatusCode(StatusCodes.Status201Created, new MetaResponse(item.Id, item.Nome, item.ValorAlvo, item.ValorGuardado, item.Prazo));
    }

    [HttpPost("metas/{id:guid}/depositos")]
    public async Task<ActionResult<MetaResponse>> Depositar(Guid id, DepositoRequest request, CancellationToken ct)
    {
        if (request.Valor <= 0) return BadRequest("O depósito deve ser maior que zero.");
        var item = await db.MetasEconomia.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId, ct);
        if (item is null) return NotFound(); item.ValorGuardado += request.Valor; await db.SaveChangesAsync(ct);
        return Ok(new MetaResponse(item.Id, item.Nome, item.ValorAlvo, item.ValorGuardado, item.Prazo));
    }

    [HttpDelete("metas/{id:guid}")]
    public async Task<IActionResult> RemoverMeta(Guid id, CancellationToken ct)
    {
        var item = await db.MetasEconomia.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId, ct);
        if (item is null) return NotFound(); db.MetasEconomia.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("relatorios")]
    public async Task<ActionResult<RelatorioResponse>> Relatorio([FromQuery] string? mes, CancellationToken ct)
    {
        if (!TryMes(mes, out var inicio, out var fim)) return BadRequest("Informe o mês no formato AAAA-MM.");
        var transacoes = await db.Transacoes.AsNoTracking().Where(t => t.UsuarioId == UsuarioId && t.Data >= inicio.AddMonths(-5) && t.Data < fim).ToListAsync(ct);
        var atual = transacoes.Where(t => t.Data >= inicio).ToList();
        decimal Receitas(IEnumerable<Transacao> itens) => itens.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor);
        decimal Despesas(IEnumerable<Transacao> itens) => itens.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor);
        var categorias = atual.GroupBy(t => t.Categoria ?? "Sem categoria").Select(g => new CategoriaRelatorio(g.Key, Receitas(g), Despesas(g))).OrderByDescending(g => g.Despesas + g.Receitas).ToList();
        var historico = Enumerable.Range(0, 6).Select(i => inicio.AddMonths(i - 5)).Select(d => { var grupo = transacoes.Where(t => t.Data.Year == d.Year && t.Data.Month == d.Month).ToList(); return new MesRelatorio(d.ToString("yyyy-MM"), Receitas(grupo), Despesas(grupo)); }).ToList();
        var receitas = Receitas(atual); var despesas = Despesas(atual);
        return Ok(new RelatorioResponse(inicio.ToString("yyyy-MM"), receitas, despesas, receitas - despesas, categorias, historico));
    }

    private static bool TryMes(string? mes, out DateTime inicio, out DateTime fim)
    {
        if (DateTime.TryParseExact(mes, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out inicio)) { inicio = new DateTime(inicio.Year, inicio.Month, 1); fim = inicio.AddMonths(1); return true; }
        inicio = default; fim = default; return false;
    }
}
