using System.Linq.Expressions;

namespace Doulex.DomainDriven;

/// <summary>
/// The interface of repository, user should define each invoke method for create, update, delete, query.
/// </summary>
public interface IReadOnlyRepository
{
}

/// <summary>
/// The interface of repository.
/// If you want to use the repository with basic functions in the domain, you should implement this interface.
/// </summary>
public interface IReadOnlyRepository<TAggregateRoot, in TKey> : IReadOnlyRepository
    where TAggregateRoot : IAggregateRoot, IEntity<TKey> where TKey : notnull
{
    /// <summary>
    /// Find the entity by the given key 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot?> GetAsync(TKey id, CancellationToken cancel);

    /// <summary>
    /// Find the entity by precondition
    /// The different with FindAsync is that FindAsync is search local cache in first, if not found, then search database. But GetAsync will search database directly. 
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot?> GetAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel);

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, int skip, int take, CancellationToken cancel);

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel);

    /// <summary>
    /// Get All entities from the repository
    /// </summary>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(int skip, int take, CancellationToken cancel);

    /// <summary>
    /// Get all entities from the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancel);

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="id">The id to find in db</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    Task<bool> ExistsAsync(TKey id, CancellationToken cancel);

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="predicate">The finding predicate</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    Task<bool> ExistsAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel);

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns>Returns the number of entities</returns>
    Task<int> CountAsync(CancellationToken cancel);

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns>Returns the number of entities</returns>
    Task<int> CountAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel);

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<long> LongCountAsync(CancellationToken cancel);

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="predicate">The query condition</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<long> LongCountAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel);

    /// <summary>
    /// Get the queryable of entities in the repository
    /// </summary>
    /// <returns></returns>
    IQueryable<TAggregateRoot> AsQueryable();
}
