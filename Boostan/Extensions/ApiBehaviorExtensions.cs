using Microsoft.AspNetCore.Mvc;

namespace Boostan.Extensions;

public static class ApiBehaviorExtensions
{
    public static void ConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
    }
}