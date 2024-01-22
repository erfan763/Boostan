using DominClass.Common.BaseEtity;
using Microsoft.AspNetCore.Identity;

namespace DominClass.Entities.User;

public sealed class User : IdentityUser<int>, IEntity
{
    public User()
    {
        GeneratedCode = Guid.NewGuid().ToString();
    }

    public string GeneratedCode { get; set; }

    //public string UserName { get; set; }
    public string Email { get; set; }

    public ICollection<UserToken.UserToken> Tokens { get; set; }

    //public ICollection<UserLogin.UserLogin> Logins { get; set; }

    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}