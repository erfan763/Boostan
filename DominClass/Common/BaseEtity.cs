using System.ComponentModel.DataAnnotations;

namespace DominClass.Common.BaseEtity;

public abstract class BaseEntity
{
    public DateTime? CreateAt { get; }

    [Key] public Guid Id { get; protected set; }

    public DateTime? DeletedAt { get; }

    public DateTime? ModifiedAt { get; set; }
}