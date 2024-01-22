using AutoMapper;
using Dotnet.fs.Application.Contracts.Identity;
using Dotnet.fs.Application.Models.Identity;
using Dotnet.fs.Domain.Entities.User;
using Dotnet.fs.Infrastructure.Identity.Identity.Manager;
using Dotnet.fs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Dotnet.fs.Infrastructure.Identity.Identity.PermissionManager;

internal class RoleManagerService : IRoleManagerService
{
    private static ConcurrentBag<ActionDescriptionDto> _permissions;
    private readonly IActionDescriptorCollectionProvider _actionDescriptor;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<RoleManagerService> _logger;
    private readonly IMapper _mapper;
    private readonly AppRoleManager _roleManger;
    private readonly AppUserManager _userManager;
    public object GetPermissionsLock = new();

    public RoleManagerService(AppRoleManager roleManger, IMapper mapper,
        IActionDescriptorCollectionProvider actionDescriptor, AppUserManager userManager,
        ILogger<RoleManagerService> logger, ApplicationDbContext db)
    {
        _roleManger = roleManger;
        _mapper = mapper;
        _actionDescriptor = actionDescriptor;
        _userManager = userManager;
        _logger = logger;
        _db = db;
    }

    public async Task<List<GetRolesDto>> GetRolesAsync()
    {
        List<GetRolesDto> result = await _roleManger.Roles
            .Include(obj => obj.CreatorUser)
            .ThenInclude(obj => obj.RealProfile)
            .Include(obj => obj.DefaultUser)
            .ThenInclude(obj => obj.RealProfile)
            .Select(r => _mapper.Map<Role, GetRolesDto>(r)).ToListAsync();
        return result;
    }

    public async Task<IdentityResult> CreateRoleAsync(CreateRoleDto model)
    {
        var role = new Role
        {
            Name = model.RoleName, DisplayName = model.DisplayName, CreatorUserId = model.CreatorUserId
        };

        IdentityResult result = await _roleManger.CreateAsync(role);

        return result;
    }

    public Task<ConcurrentBag<ActionDescriptionDto>> GetPermissionActionsAsync()
    {
        if (_permissions?.Any() ?? false)
            return Task.FromResult(_permissions);
        lock (GetPermissionsLock)
        {
            _permissions = new ConcurrentBag<ActionDescriptionDto>();
            var actions = new ConcurrentBag<ActionDescriptionDto>();
            IReadOnlyList<ActionDescriptor> actionDescriptors = _actionDescriptor.ActionDescriptors.Items;
            string controllerName = "";
            foreach (ActionDescriptor actionDescriptor in actionDescriptors)
                try
                {
                    var descriptor = (ControllerActionDescriptor)actionDescriptor;

                    bool hasPermissionOnController = descriptor.ControllerTypeInfo
                        .GetCustomAttributes<AuthorizeAttribute>()
                        .Any(obj => obj.Policy == ConstantPolicies.DynamicPermission);
                    bool hasPermissionOnAction = descriptor.MethodInfo.GetCustomAttributes<AuthorizeAttribute>()
                        .Any(obj => obj.Policy == ConstantPolicies.DynamicPermission);

                    if (hasPermissionOnController)
                        if (controllerName != descriptor.ControllerName)
                        {
                            actions.Add(new ActionDescriptionDto
                            {
                                AreaName =
                                    descriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue,
                                ControllerName = descriptor.ControllerName,
                                DisplayName = descriptor.ControllerTypeInfo.GetCustomAttribute<DisplayAttribute>()
                                    ?.Name,
                                Description = descriptor.ControllerTypeInfo.GetCustomAttribute<DisplayAttribute>()
                                    ?.Description
                            });

                            controllerName = descriptor.ControllerName;
                        }


                    if (!hasPermissionOnAction) continue;

                    actions.Add(GetActionDescription(actionDescriptor));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }

            foreach (ActionDescriptionDto actionDescriptionDto in actions) _permissions.Add(actionDescriptionDto);

            return Task.FromResult(actions);
        }
    }

    public async Task<RolePermissionDto> GetRolePermissionsAsync(int roleId)
    {
        Role role = await _roleManger.Roles
            .Include(x => x.Claims)
            .SingleOrDefaultAsync(x => x.Id == roleId);

        if (role == null)
            return null;

        List<RoleClaim> rolePermissions = role.Claims
            .Where(claim => claim.ClaimType is nameof(ConstantPolicies.DynamicPermission)).ToList();
        ActionDescriptionDto[] allPermissions = (await GetPermissionActionsAsync()).ToArray();
        IEnumerable<ActionDescriptionDto> dynamicActions = role.Name is RolesConstant.AdminRole
            ? allPermissions
            : allPermissions.Where(permissions => rolePermissions.Any(rolePerm =>
                rolePerm.ClaimValue != null && rolePerm.ClaimValue.Equals(permissions.Key)));
        return new RolePermissionDto { Role = role, Actions = dynamicActions.ToList() };
    }

    public async Task<bool> ChangeRolePermissionsAsync(EditRolePermissionsDto model)
    {
        Role role = await _roleManger.Roles
            .Include(x => x.Claims)
            .SingleOrDefaultAsync(x => x.Id == model.RoleId);

        if (role == null) return false;

        List<string> selectedPermissions = model.Permissions;

        List<string> roleClaims = role.Claims
            .Where(x => x.ClaimType == ConstantPolicies.DynamicPermission)
            .Select(x => x.ClaimValue)
            .ToList();


        // add new permissions 
        List<string> newPermissions = selectedPermissions.Except(roleClaims).ToList();
        foreach (string permission in newPermissions)
            role.Claims.Add(new RoleClaim
            {
                ClaimType = ConstantPolicies.DynamicPermission,
                ClaimValue = permission,
                CreatedClaim = DateTime.Now,
                RoleId = role.Id
            });

        // remove deleted permissions
        List<string> removedPermissions = roleClaims.Except(selectedPermissions).ToList();
        foreach (string permission in removedPermissions)
        {
            RoleClaim roleClaim = role.Claims
                .SingleOrDefault(x =>
                    x.ClaimType == ConstantPolicies.DynamicPermission &&
                    x.ClaimValue == permission);

            if (roleClaim != null) _db.RemoveRange(roleClaim);
        }

        await _db.SaveChangesAsync();

        role.DisplayName = model.DisplayName;
        role.DefaultUserId = model.DefaultUserId;
        IdentityResult result = await _roleManger.UpdateAsync(role);

        if (!result.Succeeded) return false;
        IList<User> users = await _userManager.GetUsersInRoleAsync(role.Name);

        foreach (User user in users) await _userManager.UpdateSecurityStampAsync(user);

        return true;
    }

    public async Task<Role> GetRoleByIdAsync(int roleId)
    {
        return await _roleManger.Roles.Where(c => c.Id == roleId)
            .Include(obj => obj.CreatorUser)
            .ThenInclude(obj => obj.RealProfile)
            .Include(obj => obj.DefaultUser)
            .ThenInclude(obj => obj.RealProfile).FirstOrDefaultAsync();
    }

    public async Task<Role> GetAdminRole()
    {
        return await _roleManger.Roles.FirstAsync(obj => obj.Name.Equals("admin"));
    }

    public async Task<IdentityResult> UpdateUserRoles(UpdateUserRolesDto model)
    {
        ArgumentNullException.ThrowIfNull(model.Roles, nameof(model.Roles));

        IList<string> currentRoles = await _userManager.GetRolesAsync(model.User);

        IEnumerable<string> deletedRoles = currentRoles.Except(model.Roles);
        IdentityResult deleteResult = await _userManager.RemoveFromRolesAsync(model.User, deletedRoles);
        if (deleteResult != IdentityResult.Success)
            return deleteResult;

        IEnumerable<string> addedRoles = model.Roles.Except(currentRoles);
        return await _userManager.AddToRolesAsync(model.User, addedRoles);
    }

    public async Task<User> GetDefaultUserOfRoleById(int roleId)
    {
        Role role = await _roleManger.Roles
            .Where(x => x.Id == roleId)
            .Include(x => x.DefaultUser)
            .FirstOrDefaultAsync();

        return role.DefaultUser;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        List<Role> roles = await _roleManger.Roles
            .Include(x => x.DefaultUser).ToListAsync();
        return roles;
    }

    public async Task<int> GetRoleId(string roleName)
    {
        return (await _roleManger.Roles.FirstAsync(obj => obj.Name.Equals(roleName))).Id;
    }

    public async Task<List<GetRolesDto>> GetRoleByUserId(int userId)
    {
        User user = await _userManager.Users
            .Where(u => u.Id == userId)
            .Include(obj => obj.UserRoles)
            .ThenInclude(obj => obj.Role)
            .FirstAsync();
        return user.UserRoles.Select(ur => ur.Role).Select(r => _mapper.Map<Role, GetRolesDto>(r)).ToList();
    }

    public async Task<bool> DeleteRoleAsync(int roleId, bool? deleteRoleByCascadeDeleteUsers)
    {
        Role role = await _roleManger.Roles.Include(r => r.Claims)
            .Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == roleId);

        if (deleteRoleByCascadeDeleteUsers is false && role.Users.Count > 0) return false;

        if (role == null)
            return false;

        IList<User> users = await _userManager.GetUsersInRoleAsync(role.Name);

        foreach (User user in users)
        {
            await _userManager.RemoveFromRoleAsync(user, role.Name);
            await _userManager.UpdateSecurityStampAsync(user);
        }

        _db.RemoveRange(role.Claims);
        _db.RemoveRange(role.Users);
        _db.Remove(role);
        await _db.SaveChangesAsync();

        return true;
    }

    public static ActionDescriptionDto GetActionDescription(ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor.EndpointMetadata.FirstOrDefault(obj =>
                obj.GetType().Name.Equals(nameof(HttpMethodMetadata))) is not HttpMethodMetadata method)
            return null;
        var descriptor = (ControllerActionDescriptor)actionDescriptor;
        string template = "string.Empty";
        string httpMethod = method.HttpMethods[0]?.ToLowerInvariant();
        switch (httpMethod)
        {
            case "get":
                {
                    if (actionDescriptor.EndpointMetadata.FirstOrDefault(obj =>
                            obj.GetType().Name.Equals(nameof(HttpGetAttribute))) is
                        HttpGetAttribute methodAttribute)
                        template = methodAttribute.Template;

                    break;
                }
            case "post":
                {
                    if (actionDescriptor.EndpointMetadata.FirstOrDefault(obj =>
                            obj.GetType().Name.Equals(nameof(HttpPostAttribute))) is
                        HttpPostAttribute methodAttribute)
                        template = methodAttribute.Template;

                    break;
                }
            case "patch":
                {
                    if (actionDescriptor.EndpointMetadata.FirstOrDefault(obj =>
                            obj.GetType().Name.Equals(nameof(HttpPatchAttribute))) is
                        HttpPatchAttribute methodAttribute)
                        template = methodAttribute.Template;

                    break;
                }
            case "put":
                {
                    if (actionDescriptor.EndpointMetadata.FirstOrDefault(obj =>
                            obj.GetType().Name.Equals(nameof(HttpPutAttribute))) is
                        HttpPutAttribute methodAttribute)
                        template = methodAttribute.Template;

                    break;
                }
            case "delete":
                {
                    if (actionDescriptor.EndpointMetadata.FirstOrDefault(obj =>
                            obj.GetType().Name.Equals(nameof(HttpDeleteAttribute))) is
                        HttpDeleteAttribute methodAttribute)
                        template = methodAttribute.Template;

                    break;
                }
        }

        return new ActionDescriptionDto
        {
            AreaName =
                descriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue,
            ControllerName = descriptor.ControllerName,
            ActionName = descriptor.ActionName,
            Method = httpMethod,
            Route = template?.Split('/')[0],
            DisplayName = descriptor.MethodInfo.GetCustomAttribute<DisplayAttribute>()?.Name,
            Description = descriptor.MethodInfo.GetCustomAttribute<DisplayAttribute>()?.Description
        };
    }
}