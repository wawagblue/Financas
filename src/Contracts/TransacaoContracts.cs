using System.ComponentModel.DataAnnotations;
using Financas.Models;
using Financas.Services;

namespace Financas.Contracts;

/// <summary>Dados para criar ou atualizar uma transação.</summary>
public record TransacaoRequest
{
    /// <example>Salário de novembro</example>
    public string? Descricao { get; init; }

    /// <example>5000.00</example>
    public decimal Valor { get; init; }

    /// <summary>Receita ou Despesa.</summary>
    /// <example>Receita</example>
    [Required]
    public TipoTransacao? Tipo { get; init; }

    /// <example>Trabalho</example>
    public string? Categoria { get; init; }

    /// <example>2026-11-05T00:00:00</example>
    public DateTime Data { get; init; }

    public DadosTransacao ParaDominio() => new(Descricao, Valor, Tipo!.Value, Categoria, Data);
}

public record TransacaoResponse(
    Guid Id,
    string Descricao,
    decimal Valor,
    TipoTransacao Tipo,
    string? Categoria,
    DateTime Data,
    DateTime CriadoEm,
    DateTime? AtualizadoEm)
{
    public static TransacaoResponse De(Transacao t) =>
        new(t.Id, t.Descricao, t.Valor, t.Tipo, t.Categoria, t.Data, t.CriadoEm, t.AtualizadoEm);
}
