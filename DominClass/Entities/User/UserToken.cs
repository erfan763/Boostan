using DominClass.Common.BaseEtity;
using Microsoft.AspNetCore.Identity;

namespace DominClass.Entities.UserToken;

public class UserToken : IdentityUserToken<int>, IEntity
{
    public UserToken()
    {
        GeneratedTime = DateTime.Now;
    }

    public int UserId { get; set; }
    public int UserTokenId { get; set; }
    public User.User User { get; set; }
    public DateTime GeneratedTime { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}