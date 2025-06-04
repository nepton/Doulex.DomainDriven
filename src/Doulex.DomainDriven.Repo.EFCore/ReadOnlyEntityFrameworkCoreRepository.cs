using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Doulex.DomainDriven.Repo.EFCore;

/// <summary>
/// The repository implementation for Entity Framework Core
/// This class is just a wrapper for the Entity Framework Core repository
/// To help users to create their own repository easily. This is NOT A CONTRACT
/// </summary>
/// <remarks>
/// 
/// User can inherit from this class to create their own repository
/// So that in interface IUserRepository, we recommend using standard function to define CRUD
/// 
/// class UserRepository: EntityFrameworkCoreRepository[User], IUserRepository
/// {
/// }
/// 
/// </remarks>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class ReadOnlyEntityFrameworkCoreRepository<TAggregateRoot, TKey> : IReadOnlyRepository<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot, IEntity<TKey>
    where TKey : notnull
{
    private readonly DbContext _dbContext;

    /// <summary>
    /// ctor
    /// </summary>
    protected ReadOnlyEntityFrameworkCoreRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #region Asynchronous Methods

    /// <summary>
    /// Find the entity by the given key 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual Task<TAggregateRoot?> GetAsync(TKey id, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().FindAsync(new object?[] { id }, cancel).AsTask();
    }

    /// <summary>
    /// Find the entity by precondition
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot?> GetAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().FirstOrDefaultAsync(predicate, cancel);
    }

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, int skip, int take, CancellationToken cancel)
    {
        var query = _dbContext.Set<TAggregateRoot>().Where(predicate);
        return query.Skip(skip).Take(take).ToArrayAsync(cancel);
    }

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().Where(predicate).ToArrayAsync(cancel);
    }

    /// <summary>
    /// Get All entities from the repository
    /// </summary>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(int skip, int take, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().Skip(skip).Take(take).ToArrayAsync(cancel);
    }

    /// <summary>
    /// Get all entities from the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().ToArrayAsync(cancel);
    }

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="id">The id to find in db</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    public Task<bool> ExistsAsync(TKey id, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().AnyAsync(x => x.Id.Equals(id), cancel);
    }

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="predicate">The finding predicate</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    public Task<bool> ExistsAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().AnyAsync(predicate, cancel);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns>Returns the number of entities</returns>
    public Task<int> CountAsync(CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().CountAsync(cancel);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns>Returns the number of entities</returns>
    public Task<int> CountAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().CountAsync(predicate, cancel);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<long> LongCountAsync(CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().LongCountAsync(cancel);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="predicate">The query condition</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<long> LongCountAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel)
    {
        return _dbContext.Set<TAggregateRoot>().LongCountAsync(predicate, cancel);
    }

    #endregion

    /// <summary>
    /// Get the queryable object of the repository
    /// </summary>
    /// <returns></returns>
    public IQueryable<TAggregateRoot> AsQueryable()
    {
        return _dbContext.Set<TAggregateRoot>().AsNoTracking().AsQueryable();
    }
}
