using Boostan.Domain.Entities.User;
using DominClass.Common.BaseEtity;
using Microsoft.AspNetCore.Identity;

namespace Boostan.DomainClass.Entities.User;

public class RoleClaim : IdentityRoleClaim<int>, IEntity
{
    public RoleClaim()
    {
        CreatedClaim = DateTime.Now;
    }

    public DateTime CreatedClaim { get; set; }
    public Role Role { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}