using DominClass.Common.BaseEtity;

namespace DominClass.Entities.User;

public sealed class User : BaseEntity
{
    public string UserName { get; set; }
    public string Email { get; set; }
}