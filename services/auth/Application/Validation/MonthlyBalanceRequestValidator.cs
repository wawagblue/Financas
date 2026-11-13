using FluentValidation;
using TccProway.Api.Application.Contracts;

namespace TccProway.Api.Application.Validation;

public sealed class MonthlyBalanceRequestValidator : AbstractValidator<MonthlyBalanceRequest>
{
    public MonthlyBalanceRequestValidator()
    {
        RuleFor(x => x.MonthlyIncome).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999999.99m);
        RuleFor(x => x.FixedExpenses).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999999.99m);
        RuleFor(x => x.VariableExpenses).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999999.99m);
        RuleFor(x => x).Must(x => x.FixedExpenses + x.VariableExpenses <= 999999999999.99m)
            .WithMessage("A soma das despesas excede o limite suportado.");
    }
}
