using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Doulex.DomainDriven.Repo.EFCore;

/// <summary>
/// The repository implementation for Entity Framework Core
/// This class is just a wrapper for the Entity Framework Core repository
/// To help users to create their own repository easily, This is NOT A CONTRACT
/// </summary>
/// <remarks>
/// 
/// User can inherit from this class to create their own repository
/// So that in interface IUserRepository, we recommend to use standard function to define CRUD
/// 
/// class UserRepository : EntityFrameworkCoreRepository[User], IUserRepository
/// {
/// }
/// 
/// </remarks>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class EntityFrameworkCoreRepository<TAggregateRoot, TKey> : IRepository<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot, IEntity<TKey>
    where TKey : notnull
{
    private readonly DbSet<TAggregateRoot> _dbSet;

    /// <summary>
    /// ctor
    /// </summary>
    protected EntityFrameworkCoreRepository(DbContext context)
    {
        _dbSet = context.Set<TAggregateRoot>();
    }

    #region Asynchronous Methods

    /// <summary>
    /// Add new entity to the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public virtual async Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        await _dbSet.AddAsync(aggregateRoot, cancel);
    }

    /// <summary>
    /// Add new entity to the repository or update the entity in the repository if the id of entity has existed 
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="mode">The mode indicates that how to save the entity to the repository</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, SaveMode mode, CancellationToken cancel = default)
    {
        return mode switch
        {
            SaveMode.Update => UpdateAsync(aggregateRoot, cancel),
            SaveMode.Add    => AddAsync(aggregateRoot, cancel),
            _               => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    /// <summary>
    /// Update the exists entity in the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        return Task.Run(() => _dbSet.Update(aggregateRoot), cancel);
    }

    public virtual async Task UpdateAsync(
        Expression<Func<TAggregateRoot, bool>>           predicate,
        Expression<Func<TAggregateRoot, TAggregateRoot>> updateFactory,
        CancellationToken                                cancel = default)
    {
        await _dbSet.Where(predicate).UpdateAsync(updateFactory, cancel);
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public virtual Task RemoveAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        return Task.Run(() => _dbSet.Remove(aggregateRoot), cancel);
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="id">The id of entity</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if the entity has been removed, false if the entity cannot be found</returns>
    public virtual Task RemoveAsync(TKey id, CancellationToken cancel = default)
    {
        return _dbSet.Where(x => x.Id.Equals(id)).DeleteAsync(cancel);
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task RemoveAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        return _dbSet.Where(predicate).DeleteAsync(cancel);
    }

    /// <summary>
    /// Find the entity by the given key
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    [Obsolete("Use GetAsync instead")]
    public virtual Task<TAggregateRoot?> FindAsync(TKey id, CancellationToken cancel = default)
    {
        return _dbSet.FindAsync(new object?[] {id}, cancel).AsTask();
    }

    /// <summary>
    /// Find the entity by the given key 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual Task<TAggregateRoot?> GetAsync(TKey id, CancellationToken cancel = default)
    {
        return _dbSet.FindAsync(new object?[] {id}, cancel).AsTask();
    }

    /// <summary>
    /// Find the entity by precondition
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot?> GetAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        return _dbSet.FirstOrDefaultAsync(predicate, cancel);
    }

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, int skip, int take, CancellationToken cancel = default)
    {
        var query = _dbSet.Where(predicate);
        return query.Skip(skip).Take(take).ToArrayAsync(cancel);
    }

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        return _dbSet.Where(predicate).ToArrayAsync(cancel);
    }

    /// <summary>
    /// Get All entities from the repository
    /// </summary>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(int skip, int take, CancellationToken cancel = default)
    {
        return _dbSet.Skip(skip).Take(take).ToArrayAsync(cancel);
    }

    /// <summary>
    /// Get all entities from the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancel = default)
    {
        return _dbSet.ToArrayAsync(cancel);
    }

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="id">The id to find in db</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    public Task<bool> ExistsAsync(TKey id, CancellationToken cancel = default)
    {
        return _dbSet.AnyAsync(x => x.Id.Equals(id), cancel);
    }

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="predicate">The finding predicate</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    public Task<bool> ExistsAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        return _dbSet.AnyAsync(predicate, cancel);
    }

    #endregion

    /// <summary>
    /// Get the queryable object of the repository
    /// </summary>
    /// <returns></returns>
    public IQueryable<TAggregateRoot> Queryable()
    {
        return _dbSet.AsNoTracking().AsQueryable();
    }
}
