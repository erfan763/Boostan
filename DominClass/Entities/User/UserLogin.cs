using DominClass.Common.BaseEtity;
using Microsoft.AspNetCore.Identity;

namespace DominClass.Entities.UserLogin;

public class UserLogin : IdentityUserLogin<int>, IEntity
{
    public UserLogin()
    {
        LoggedOn = DateTime.Now;
    }

    public int UserId { get; set; }
    public int UserLoginId { get; set; }
    public User.User User { get; set; }
    public DateTime LoggedOn { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}