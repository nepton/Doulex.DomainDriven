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
public class EntityFrameworkCoreRepository<TAggregateRoot, TKey> : ReadOnlyEntityFrameworkCoreRepository<TAggregateRoot, TKey>, IRepository<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot, IEntity<TKey>
    where TKey : notnull
{
    private readonly DbContext _dbContext;

    /// <summary>
    /// The validator, optional
    /// </summary>
    private readonly IAggregateRootValidator<TAggregateRoot>? _validator;

    /// <summary>
    /// ctor
    /// </summary>
    protected EntityFrameworkCoreRepository(DbContext dbContext, IAggregateRootValidator<TAggregateRoot> validator) : base(dbContext)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// ctor
    /// </summary>
    protected EntityFrameworkCoreRepository(DbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    #region Asynchronous Methods

    /// <summary>
    /// Add a new entity to the repository or update the entity in the repository if the id of entity has existed 
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="mode">The mode indicates that how to save the entity to the repository</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, SaveMode mode, CancellationToken cancel)
    {
        return mode switch
        {
            SaveMode.Update => UpdateAsync(aggregateRoot, cancel),
            SaveMode.Add    => AddAsync(aggregateRoot, cancel),
            _               => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public async Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel)
    {
        var entry = _dbContext.Entry(aggregateRoot);

        // Case 1: Entity is already tracked by the current DbContext
        if (entry.State != EntityState.Detached)
        {
            await UpdateAsync(aggregateRoot, cancel);
            return;
        }

        // Case 2: Entity is not tracked, we try to find it in the database using the primary key
        var entityType    = _dbContext.Model.FindEntityType(typeof(TAggregateRoot));
        var keyProperties = entityType?.FindPrimaryKey()?.Properties;

        if (keyProperties is { Count: > 0 })
        {
            var keyValues      = keyProperties.Select(p => p.PropertyInfo?.GetValue(aggregateRoot)).ToArray();
            var existingEntity = await _dbContext.Set<TAggregateRoot>().FindAsync(keyValues, cancel);

            if (existingEntity != null)
            {
                await UpdateAsync(aggregateRoot, cancel);
            }
            else
            {
                await AddAsync(aggregateRoot, cancel);
            }
        }
        else
        {
            // Use "add" if the entity does not have a primary key
            await AddAsync(aggregateRoot, cancel);
        }
    }

    /// <summary>
    /// Add new entity to the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public virtual async Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancel)
    {
        if (_validator != null)
        {
            await _validator.ValidateBeforeAddAsync(aggregateRoot, cancel);
        }

        await _dbContext.Set<TAggregateRoot>().AddAsync(aggregateRoot, cancel);
    }

    /// <summary>
    /// Update the existing entity in the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel)
    {
        if (_validator != null)
        {
            await _validator.ValidateBeforeUpdateAsync(aggregateRoot, cancel);
        }

        _dbContext.Set<TAggregateRoot>().Update(aggregateRoot);
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public virtual async Task RemoveAsync(TAggregateRoot aggregateRoot, CancellationToken cancel)
    {
        if (_validator != null)
        {
            await _validator.ValidateBeforeRemoveAsync(aggregateRoot, cancel);
        }

        _dbContext.Set<TAggregateRoot>().Remove(aggregateRoot);
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="id">The id of entity</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if the entity has been removed, false if the entity cannot be found</returns>
    public virtual async Task RemoveAsync(TKey id, CancellationToken cancel)
    {
        var entity = await GetAsync(id, cancel);
        if (entity is null)
            return;

        await RemoveAsync(entity, cancel);
    }

    #endregion
}
