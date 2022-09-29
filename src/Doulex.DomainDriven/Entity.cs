namespace Doulex.DomainDriven;

/// <summary>
/// 实体基类对象
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class Entity<TKey> : IEntity<TKey>
    where TKey : notnull
{
    /// <summary>
    /// 对象的Id
    /// </summary>
    public abstract TKey Id { get; }

    #region Domain event

    private List<IDomainEvent>? _domainEvents;

    /// <summary>
    /// Retrieve all domain events that have been applied to this entity.
    /// </summary>
    /// <returns></returns>
    public IDomainEvent[] GetDomainEvents() => _domainEvents?.ToArray() ?? Array.Empty<IDomainEvent>();

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents ??= new();
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    #endregion

    #region Equality members

    /// <summary>确定指定的对象是否等于当前对象。</summary>
    /// <returns>如果指定的对象等于当前对象，则为 true，否则为 false。</returns>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;

        var other = (Entity<TKey>) obj;
        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    /// <summary>作为默认哈希函数。</summary>
    /// <returns>当前对象的哈希代码。</returns>
    public override int GetHashCode()
    {
        return EqualityComparer<TKey>.Default.GetHashCode(Id ?? throw new InvalidOperationException());
    }

    public static bool operator ==(Entity<TKey>? opl, Entity<TKey>? opr)
    {
        if ((object?) opl == null)
        {
            return (object?) opr == null;
        }

        return opl.Equals(opr);
    }

    public static bool operator !=(Entity<TKey>? opl, Entity<TKey>? opr)
    {
        return !(opl == opr);
    }

    #endregion
};
