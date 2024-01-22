using Boostan.Domain.Entities.User;
using DominClass.Common.BaseEtity;
using Microsoft.AspNetCore.Identity;

namespace Boostan.DomainClass.Entities.User;

public class UserRole : IdentityUserRole<int>, IEntity
{
    public DominClass.Entities.User.User User { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedUserRoleDate { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}