using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Boostan.SharedKernel.ValidationBase;

public class ApplicationBaseValidationModelProvider<TApplicationModel> : AbstractValidator<TApplicationModel>
{
    public ApplicationBaseValidationModelProvider(IServiceScope serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceScope ServiceProvider { get; }
}