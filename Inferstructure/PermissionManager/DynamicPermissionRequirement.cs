using Dotnet.fs.Application.Contracts.Identity;
using Dotnet.fs.Application.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Dotnet.fs.Infrastructure.Identity.Identity.PermissionManager;

public class DynamicPermissionRequirement : IAuthorizationRequirement
{
}

public class DynamicPermissionHandler : AuthorizationHandler<DynamicPermissionRequirement>
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IDynamicPermissionService _dynamicPermissionService;
    private readonly IRoleManagerService _roleManagerService;

    public DynamicPermissionHandler(IDynamicPermissionService dynamicPermissionService,
        IHttpContextAccessor contextAccessor, IRoleManagerService roleManagerService)
    {
        _dynamicPermissionService = dynamicPermissionService;
        _contextAccessor = contextAccessor;
        _roleManagerService = roleManagerService;
    }


    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DynamicPermissionRequirement requirement)
    {
        ClaimsPrincipal user = _contextAccessor.HttpContext.User;

        RouteValueDictionary routeData = _contextAccessor.HttpContext.GetRouteData().Values;

        string controller = routeData["controller"]?.ToString();
        string action = routeData["action"]?.ToString();
        string area = routeData["area"]?.ToString();
        string method = _contextAccessor.HttpContext.Request.Method.ToLowerInvariant();
        List<ActionDescriptionDto> permissions = (await _roleManagerService.GetPermissionActionsAsync()).ToList();
        ActionDescriptionDto currentPermission = permissions.FirstOrDefault(obj =>
            (area?.Equals(obj.AreaName, StringComparison.InvariantCultureIgnoreCase) ?? true) &&
            (controller?.Equals(obj.ControllerName, StringComparison.InvariantCultureIgnoreCase) ?? true) &&
            (action?.Equals(obj.ActionName, StringComparison.InvariantCultureIgnoreCase) ?? true) &&
            method.Equals(obj.Method, StringComparison.InvariantCultureIgnoreCase));

        if (currentPermission is null)
        {
            context.Succeed(requirement);
            return;
        }

        if (_dynamicPermissionService.CanAccess(user, currentPermission.Key))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}