namespace TccProway.Api.Application.Contracts;

/// <summary>Valores mensais usados para simular o orçamento, sem os persistir.</summary>
public sealed record MonthlyBalanceRequest(decimal MonthlyIncome, decimal FixedExpenses, decimal VariableExpenses);

/// <summary>Resultado calculado para o orçamento mensal.</summary>
public sealed record MonthlyBalanceResponse(
    decimal MonthlyIncome,
    decimal TotalExpenses,
    decimal Balance,
    decimal SavingsRatePercentage);
