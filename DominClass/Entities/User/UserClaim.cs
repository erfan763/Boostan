using DominClass.Common.BaseEtity;
using Microsoft.AspNetCore.Identity;

namespace Boostan.DomainClass.Entities.User;

public class UserClaim : IdentityUserClaim<int>, IEntity
{
    public DominClass.Entities.User.User User { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}