using Boostan.Application.Models.Common;
using Boostan.Application.Models.Jwt;
using Boostan.SharedKernel.ValidationBase;
using Boostan.SharedKernel.ValidationBase.Contracts;
using FluentValidation;
using Mediator;

namespace Boostan.Application.Features.Users.Queries.GenerateUserTokenByPassword;

public record GenerateUserTokenByPasswordQuery
    (string UserName, string Password) : IRequest<OperationResult<AccessToken>>,
        IValidatableModel<GenerateUserTokenByPasswordQuery>
{
    public IValidator<GenerateUserTokenByPasswordQuery> ValidateApplicationModel(
        ApplicationBaseValidationModelProvider<GenerateUserTokenByPasswordQuery> validator)
    {
        validator.RuleFor(c => c.UserName)
            .NotEmpty()
            .NotNull()
            .WithMessage("Please enter username");

        validator.RuleFor(c => c.Password)
            .NotEmpty()
            .NotNull()
            .WithMessage("Please enter password");

        return validator;
    }
}