using DominClass.Common.BaseEtity;
using Microsoft.AspNetCore.Identity;

namespace Boostan.Domain.Entities.User;

public class Role : IdentityRole<int>, IEntity
{
    public Role()
    {
        CreatedDate = DateTime.Now;
    }

    public string DisplayName { get; set; }
    public int? DefaultUserId { get; set; }
    public int? CreatorUserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DominClass.Entities.User.User DefaultUser { get; set; }
    public DominClass.Entities.User.User CreatorUser { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}