using System.Security.Claims;
using System.Text;
using Boostan.Application.Contracts.Identity;
using Boostan.Application.Models.ApiResult;
using Boostan.Domain.Entities.User;
using Boostan.Infrastructure.Dtos;
using Boostan.Infrastructure.Manager;
using Boostan.SharedKernel.Extensions;
using DominClass.Entities.User;

using Dotnet.fs.Domain.Entities.User;
using Dotnet.fs.Infrastructure.Identity.Identity.Manager;
using Dotnet.fs.Infrastructure.Identity.Identity.validator;
using Dotnet.fs.Infrastructure.Identity.UserManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Boostan.Infrastructure.ServiceConfiguration;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterIdentityServices(this IServiceCollection services,
        IdentitySettings identitySettings)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAppUserManager, AppUserManagerImplementation>();


        services.AddScoped<IUserValidator<User>, AppUserValidator>();
        services.AddScoped<UserValidator<User>, AppUserValidator>();

        //services.AddScoped<IUserClaimsPrincipalFactory<User>, AppUserClaimsPrincipleFactory>();

        services.AddScoped<IRoleValidator<Role>, AppRoleValidator>();
        services.AddScoped<RoleValidator<Role>, AppRoleValidator>();

        //services.AddScoped<IAuthorizationHandler, CurrentUserResourceHandler>();
        //services.AddScoped<IRoleStore<Role>, RoleStore<>>();
        //services.AddScoped<IUserStore<User>, AppUserStore>();
        //services.AddScoped<IRoleManagerService, RoleManagerService>();


        services.AddIdentity<User, Role>(options =>
        {
            options.Stores.ProtectPersonalData = false;

            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequireUppercase = false;

            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = true;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = false;
            options.User.RequireUniqueEmail = false;
        }).AddUserStore<AppUserStore>();
            //.AddRoleStore<RoleStore>().
           
            //AddUserManager<AppUserManager>().AddRoleManager<AppRoleManager>().AddErrorDescriber<AppErrorDescriber>()
            //.AddDefaultTokenProviders().AddSignInManager<AppSignInManager>()
            //.AddDefaultTokenProviders()
            //.AddPasswordlessLoginTotpTokenProvider();


        //For [ProtectPersonalData] Attribute In Identity

        //services.AddScoped<ILookupProtectorKeyRing, KeyRing>();

        //services.AddScoped<ILookupProtector, LookupProtector>();

        //services.AddScoped<IPersonalDataProtector, PersonalDataProtector>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(ConstantPolicies.DynamicPermission, policy =>
            {
                policy.RequireAuthenticatedUser();
                //policy.Requirements.Add(new DynamicPermissionRequirement());
            });
            options.AddPolicy(ConstantPolicies.CurrentUserResource, policy =>
            {
                policy.RequireAuthenticatedUser();
                //policy.Requirements.Add(new CurrentUserResourceRequirement());
            });
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var secretkey = Encoding.UTF8.GetBytes(identitySettings.SecretKey);
            //var encryptionkey = Encoding.UTF8.GetBytes(identitySettings.Encryptkey);

            var validationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero, // default: 5 min
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretkey),
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateAudience = true, //default : false
                ValidAudience = identitySettings.Audience,
                ValidateIssuer = true, //default : false
                ValidIssuer = identitySettings.Issuer,

                //TokenDecryptionKey = new SymmetricSecurityKey(encryptionkey),
            };

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = validationParameters;
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                    //logger.LogError("Authentication failed.", context.Exception);

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var signInManager = context.HttpContext.RequestServices.GetRequiredService<AppSignInManager>();

                    var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                    if (claimsIdentity.Claims?.Any() != true)
                        context.Fail("This token has no claims.");

                    var securityStamp =
                        claimsIdentity.FindFirstValue(new ClaimsIdentityOptions().SecurityStampClaimType);
                    if (!securityStamp.HasValue())
                        context.Fail("This token has no secuirty stamp");

                    //Find user and token from database and perform your custom validation
                    var userId = claimsIdentity.GetUserId<int>();
                    // var user = await userRepository.GetByIdAsync(context.HttpContext.RequestAborted, userId);

                    //if (user.SecurityStamp != Guid.Parse(securityStamp))
                    //    context.Fail("Token secuirty stamp is not valid.");

                    var validatedUser = await signInManager.ValidateSecurityStampAsync(context.Principal);
                    if (validatedUser == null)
                        context.Fail("Token secuirty stamp is not valid.");
                },
                OnChallenge = async context =>
                {
                    //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                    //logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
                    if (context.AuthenticateFailure is SecurityTokenExpiredException)
                    {
                        context.HandleResponse();

                        var response = new ApiResult("Token is expired. refresh your token");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(response);
                    }

                    else if (context.AuthenticateFailure != null)
                    {
                        context.HandleResponse();

                        var response = new ApiResult("Token is Not Valid");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(response);
                    }

                    else
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = (int)StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new ApiResult("Invalid access token. Please login"));
                    }
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = (int)StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new ApiResult(ApiResultStatusCode.Forbidden.ToDisplay()));
                }
            };
        });

        return services;
    }
}