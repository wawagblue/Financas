using FluentValidation;
using TccProway.Api.Application.Contracts;

namespace TccProway.Api.Application.Validation;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254); RuleFor(x => x.Password).NotEmpty(); }
}
public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(72)
            .Must(password => System.Text.Encoding.UTF8.GetByteCount(password) <= 72)
            .WithMessage("A palavra-passe deve ter entre 12 caracteres e no máximo 72 bytes em UTF-8 (limite do BCrypt).");
    }
}
public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator() { RuleFor(x => x.Nome).NotEmpty().MaximumLength(120); RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254); }
}
public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}
