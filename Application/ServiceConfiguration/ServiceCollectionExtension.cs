using System.Reflection;
using Boostan.Application.Common;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Boostan.Application.ServiceConfiguration;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //TODO: check add mediator
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "Dotnet.fs.Application.Mediator";
        });

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidateCommandBehavior<,>));
        //services.AddMediator(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());


        return services;
    }
}