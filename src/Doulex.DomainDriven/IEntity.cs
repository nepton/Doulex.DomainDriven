namespace Doulex.DomainDriven;

/// <summary>
/// 实体类接口
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 对象 Id
    /// </summary>
    object Id { get; }
}

/// <summary>
/// 实体对象接口
/// </summary>
public interface IEntity<out TKey> : IEntity
    where TKey : notnull
{
    /// <summary>
    /// 对象的Id
    /// </summary>
    new TKey Id { get; }

    /// <summary>
    /// 实现基类对象Id
    /// </summary>
    object IEntity.Id => Id!;
}
