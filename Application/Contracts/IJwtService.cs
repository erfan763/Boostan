using System.Security.Claims;
using Boostan.Application.Models.Jwt;
using DominClass.Entities.User;

namespace Boostan.Application.Contracts;

public interface IJwtService
{
    Task<AccessToken> GenerateAsync(User user);
    Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    Task<AccessToken> GenerateByPhoneNumberAsync(string phoneNumber);
}