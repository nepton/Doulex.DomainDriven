using System.Linq.Expressions;

namespace Doulex.DomainDriven;

/// <summary>
/// The interface of repository, user should define each invoke method for create, update, delete, query.
/// </summary>
public interface IRepository
{
}

/// <summary>
/// The interface of repository.
/// If you want to use the repository with basic functions in the domain, you should implement this interface.
/// </summary>
public interface IRepository<TAggregateRoot, in TKey> : IRepository
    where TAggregateRoot : IAggregateRoot, IEntity<TKey> where TKey : notnull
{
    /// <summary>
    /// Add new entity to the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default);

    /// <summary>
    /// Add new entity to the repository or update the entity in the repository if the id of entity has existed 
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns>Return true if the entity has been added, or return false if the entity has been updated</returns>
    Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default);

    /// <summary>
    /// Update the exists entity in the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default);

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    Task RemoveAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default);

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="id">The id of entity</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if the entity has been removed, false if the entity cannot be found</returns>
    Task RemoveAsync(TKey id, CancellationToken cancel = default);

    /// <summary>
    /// Find the entity by the given key
    /// The different with FindAsync is that FindAsync is search local cache in first, if not found, then search database. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    [Obsolete("Use GetAsync instead")]
    Task<TAggregateRoot?> FindAsync(TKey id, CancellationToken cancel = default);

    /// <summary>
    /// Find the entity by the given key 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot?> GetAsync(TKey id, CancellationToken cancel = default);

    /// <summary>
    /// Find the entity by precondition
    /// The different with FindAsync is that FindAsync is search local cache in first, if not found, then search database. But GetAsync will search database directly. 
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot?> GetAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, int skip, int take, CancellationToken cancel = default);

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Get All entities from the repository
    /// </summary>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(int skip, int take, CancellationToken cancel = default);

    /// <summary>
    /// Get all entities from the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancel = default);

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="id">The id to find in db</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    Task<bool> ExistsAsync(TKey id, CancellationToken cancel = default);

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="predicate">The finding predicate</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    Task<bool> ExistsAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default);
}
