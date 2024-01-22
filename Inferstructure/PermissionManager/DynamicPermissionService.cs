using System.Security.Claims;

namespace Dotnet.fs.Infrastructure.Identity.Identity.PermissionManager;

public class DynamicPermissionService : IDynamicPermissionService
{
    public bool CanAccess(ClaimsPrincipal user, string permissionKey)
    {
        return true;
        if (user.IsInRole("admin")) return true;

        List<Claim> userClaims = user.FindAll(ConstantPolicies.DynamicPermission).ToList();
        return userClaims.Any(item => item.Value.Equals(permissionKey, StringComparison.OrdinalIgnoreCase));
    }
}