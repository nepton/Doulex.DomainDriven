namespace Doulex.DomainDriven;

/// <summary>
/// The interface of repository, user should define each invoke method for create, update, delete, query.
/// </summary>
public interface IRepository : IReadOnlyRepository
{
}

/// <summary>
/// The interface of repository.
/// If you want to use the repository with basic functions in the domain, you should implement this interface.
/// </summary>
public interface IRepository<TAggregateRoot, in TKey> : IRepository, IReadOnlyRepository<TAggregateRoot, TKey>
    where TAggregateRoot : IAggregateRoot, IEntity<TKey> where TKey : notnull
{
    /// <summary>
    /// Add new entity to the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancel);

    /// <summary>
    /// Add a new entity to the repository or update the entity in the repository if the id of entity has existed 
    /// </summary>
    /// <param name="aggregateRoot">The entity to add or update</param>
    /// <param name="mode">The mode of attachment</param>
    /// <param name="cancel"></param>
    /// <returns>Return true if the entity has been added, or return false if the entity has been updated</returns>
    Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, SaveMode mode, CancellationToken cancel);

    /// <summary>
    /// Add a new entity to the repository or update the entity in the repository if the object is retrieved from this repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel);

    /// <summary>
    /// Update the existing entity in the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel);

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    Task RemoveAsync(TAggregateRoot aggregateRoot, CancellationToken cancel);

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="id">The id of entity</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if the entity has been removed, false if the entity cannot be found</returns>
    Task RemoveAsync(TKey id, CancellationToken cancel);
}
