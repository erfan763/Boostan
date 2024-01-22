using System.Security.Claims;

namespace Dotnet.fs.Infrastructure.Identity.Identity.PermissionManager;

public interface IDynamicPermissionService
{
    bool CanAccess(ClaimsPrincipal user, string permissionKey);
}