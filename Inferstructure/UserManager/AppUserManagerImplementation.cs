using Boostan.Application.Contracts.Identity;
using Boostan.Infrastructure.Manager;
using DominClass.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.fs.Infrastructure.Identity.UserManager;

public class AppUserManagerImplementation : IAppUserManager
{
    private readonly AppUserManager _userManager;

    public AppUserManagerImplementation(AppUserManager userManager)
    {
        _userManager = userManager;
    }

    public Task<IdentityResult> CreateUser(User user)
    {
        return _userManager.CreateAsync(user);
    }


    public async Task<IdentityResult> Login(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password)
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = "Password is not correct" });
    }

    public Task<User> GetByUserName(string userName)
    {
        return _userManager.FindByNameAsync(userName);
    }

    Task<IdentityResult> IAppUserManager.UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }


    public async Task<List<User>> GetAllUsersAsync(int pageSize, int page)
    {
        return await _userManager.Users.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();
    }

    public async Task<IdentityResult> CreateUserWithPasswordAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }


    public async Task<IdentityResult> IncrementAccessFailedCountAsync(User user)
    {
        return await _userManager.AccessFailedAsync(user);
    }

    public async Task<bool> IsUserLockedOutAsync(User user)
    {
        var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(user);

        return lockoutEndDate.HasValue && lockoutEndDate.Value > DateTimeOffset.Now;
    }

    public async Task<bool> IsExistPassword(User user)
    {
        return await _userManager.HasPasswordAsync(user);
    }
}