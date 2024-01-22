using DominClass.Entities.User;
using Microsoft.AspNetCore.Identity;

namespace Boostan.Application.Contracts.Identity;

public interface IAppUserManager
{
    Task<IdentityResult> CreateUser(User user);

    Task<IdentityResult> Login(User user, string password);

    Task<User> GetByUserName(string userName);
}