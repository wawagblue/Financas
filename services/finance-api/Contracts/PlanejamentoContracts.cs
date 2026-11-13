namespace Financas.Contracts;

public record OrcamentoRequest(string Categoria, decimal LimiteMensal);
public record OrcamentoResponse(Guid Id, string Categoria, decimal LimiteMensal, decimal GastoNoMes, decimal PercentualUsado);
public record RecorrenciaRequest(string Descricao, decimal Valor, string Categoria, DateTime ProximoVencimento);
public record RecorrenciaResponse(Guid Id, string Descricao, decimal Valor, string Categoria, DateTime ProximoVencimento);
public record MetaRequest(string Nome, decimal ValorAlvo, DateTime? Prazo);
public record DepositoRequest(decimal Valor);
public record MetaResponse(Guid Id, string Nome, decimal ValorAlvo, decimal ValorGuardado, DateTime? Prazo);
public record CategoriaRelatorio(string Categoria, decimal Receitas, decimal Despesas);
public record MesRelatorio(string Mes, decimal Receitas, decimal Despesas);
public record RelatorioResponse(string Mes, decimal Receitas, decimal Despesas, decimal Saldo, IReadOnlyList<CategoriaRelatorio> Categorias, IReadOnlyList<MesRelatorio> Historico);
