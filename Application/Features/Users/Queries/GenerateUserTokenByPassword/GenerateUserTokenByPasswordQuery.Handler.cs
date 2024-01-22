using Boostan.Application.Contracts;
using Boostan.Application.Contracts.Identity;
using Boostan.Application.Models.Common;
using Boostan.Application.Models.Jwt;
using Mediator;
using Microsoft.Extensions.Configuration;

namespace Boostan.Application.Features.Users.Queries.GenerateUserTokenByPassword;

internal class GenerateUserTokenByPasswordQueryHandler
    : IRequestHandler<GenerateUserTokenByPasswordQuery, OperationResult<AccessToken>>
{
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;
    private readonly IAppUserManager _userManager;


    public GenerateUserTokenByPasswordQueryHandler(IJwtService jwtService, IAppUserManager userManager,
        IConfiguration configuration)
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async ValueTask<OperationResult<AccessToken>> Handle(GenerateUserTokenByPasswordQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.GetByUserName(request.UserName);

        if (user is null)
            return OperationResult<AccessToken>.FailureResult("نام کاربری و یا رمز عبور اشتباه است");

        var result = await _userManager.Login(user, request.Password);


        if (!result.Succeeded)
            return OperationResult<AccessToken>.FailureResult("نام کاربری و یا رمز عبور اشتباه است");

        await _userManager.UpdateUserAsync(user);

        var token = await _jwtService.GenerateAsync(user);

        return OperationResult<AccessToken>.SuccessResult(token);
    }
}