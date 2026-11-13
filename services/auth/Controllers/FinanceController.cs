using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccProway.Api.Application.Contracts;

namespace TccProway.Api.Controllers;

/// <summary>Operações e simulações financeiras.</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance")]
public sealed class FinanceController : ControllerBase
{
    /// <summary>Simula o saldo mensal a partir dos rendimentos e despesas informados.</summary>
    /// <remarks>Este endpoint é um exemplo funcional e não persiste os valores.</remarks>
    /// <response code="200">Simulação calculada.</response>
    /// <response code="400">Valores negativos ou fora dos limites permitidos.</response>
    /// <response code="401">JWT ausente ou inválido.</response>
    [HttpPost("simulations/monthly-balance")]
    [ProducesResponseType<MonthlyBalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<MonthlyBalanceResponse> SimulateMonthlyBalance([FromBody] MonthlyBalanceRequest request)
    {
        var expenses = request.FixedExpenses + request.VariableExpenses;
        var balance = request.MonthlyIncome - expenses;
        var savingsRate = request.MonthlyIncome == 0
            ? 0
            : Math.Round(balance / request.MonthlyIncome * 100m, 2, MidpointRounding.AwayFromZero);

        return Ok(new MonthlyBalanceResponse(request.MonthlyIncome, expenses, balance, savingsRate));
    }
}
