using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

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
    /// 构造函数
    /// </summary>
    protected EntityFrameworkCoreRepository(DbContext context)
    {
        _dbSet = context.Set<TAggregateRoot>();
    }

    #region Asynchronous Methods

    protected virtual Task OnChangingAsync(TAggregateRoot aggregateRoot, ActionType actionType, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnChangedAsync(TAggregateRoot aggregateRoot, ActionType actionType, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Add new entity to the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public virtual async Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        await OnChangingAsync(aggregateRoot, ActionType.Add, cancel);
        await _dbSet.AddAsync(aggregateRoot, cancel);
        await OnChangedAsync(aggregateRoot, ActionType.Add, cancel);
    }

    /// <summary>
    /// Update the exists entity in the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        await OnChangingAsync(aggregateRoot, ActionType.Update, cancel);
        _dbSet.Update(aggregateRoot);
        await OnChangedAsync(aggregateRoot, ActionType.Update, cancel);
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public virtual async Task RemoveAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        await OnChangingAsync(aggregateRoot, ActionType.Remove, cancel);
        _dbSet.Remove(aggregateRoot);
        await OnChangedAsync(aggregateRoot, ActionType.Remove, cancel);
    }

    /// <summary>
    /// Find the entity by the given key
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual Task<TAggregateRoot?> FindAsync(TKey id, CancellationToken cancel = default)
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
    public Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, int? skip = null, int? take = null, CancellationToken cancel = default)
    {
        var query = _dbSet.Where(predicate);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return query.ToArrayAsync(cancel);
    }

    /// <summary>
    /// Get All entities from the repository
    /// </summary>
    /// <param name="skip">Indicate that how many records will be skipped</param>
    /// <param name="take">Indicate that how many records will be taken</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(int? skip = null, int? take = null, CancellationToken cancel = default)
    {
        var query = _dbSet.AsQueryable();

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return query.ToArrayAsync(cancel);
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
}
