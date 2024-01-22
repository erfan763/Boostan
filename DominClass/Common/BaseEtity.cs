namespace DominClass.Common.BaseEtity;

public interface IEntity : ITimeModification
{
}

public interface ITimeModification
{
    DateTime? CreatedTime { get; set; }
    DateTime? ModifiedDate { get; set; }
    DateTime? DeletedDate { get; set; }
}

public abstract class BaseEntity<TKey> : IEntity
{
    public TKey Id { get; protected set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not BaseEntity<TKey> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetType() == other.GetType() && Id.Equals(other.Id);
    }

    public static bool operator ==(BaseEntity<TKey> a, BaseEntity<TKey> b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(BaseEntity<TKey> a, BaseEntity<TKey> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }
}

public abstract class BaseEntity : BaseEntity<int>
{
}

public abstract class BaseEntityLong : BaseEntity<long>
{
}