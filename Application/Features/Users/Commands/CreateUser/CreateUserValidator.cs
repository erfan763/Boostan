using FluentValidation;

namespace Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(50).EmailAddress().WithMessage("your email is not valid");
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(50).WithMessage("your name is not valid");
    }
}