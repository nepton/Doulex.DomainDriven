using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Doulex.DomainDriven.Repo.FileSystem;

public class FileSystemRepository<TAggregateRoot, TKey> : IRepository<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot, IEntity<TKey>
    where TKey : notnull
{
    private readonly FileSystemUnitOfWork _unitOfWork;
    private readonly EntityCaching        _caching;

    public FileSystemRepository(FileSystemUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _caching    = new(unitOfWork.Options);
    }

    public Task ApplyChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add new entity to the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        _unitOfWork.Enqueue(typeof(TAggregateRoot), PendingAction.Add, aggregateRoot);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Add a new entity to the repository or update the entity in the repository if the id of entity has existed 
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="mode"></param>
    /// <param name="cancel"></param>
    /// <returns>Return true if the entity has been added, or return false if the entity has been updated</returns>
    public Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, SaveMode mode, CancellationToken cancel = default)
    {
        var pendingAction = mode switch
        {
            SaveMode.Update => PendingAction.Update,
            SaveMode.Add    => PendingAction.Add,
            _               => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
        _unitOfWork.Enqueue(typeof(TAggregateRoot), pendingAction, aggregateRoot);
        return Task.CompletedTask;
    }

    public Task AddOrUpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Update the existed entity in the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        _unitOfWork.Enqueue(typeof(TAggregateRoot), PendingAction.Update, aggregateRoot);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="aggregateRoot"></param>
    /// <param name="cancel"></param>
    public Task RemoveAsync(TAggregateRoot aggregateRoot, CancellationToken cancel = default)
    {
        _unitOfWork.Enqueue(typeof(TAggregateRoot), PendingAction.Remove, aggregateRoot.Id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="id">The id of entity</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if the entity has been removed, false if the entity cannot be found</returns>
    public Task RemoveAsync(TKey id, CancellationToken cancel = default)
    {
        _unitOfWork.Enqueue(typeof(TAggregateRoot), PendingAction.Remove, id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove the entity from the repository
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task RemoveAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        var func = predicate.Compile();
        var aggs = _caching.GetAll(func, null, null);
        foreach (var agg in aggs)
        {
            _unitOfWork.Enqueue(typeof(TAggregateRoot), PendingAction.Remove, agg.Id);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Find the entity by the given key
    /// The different with FindAsync is that FindAsync is search local cache in first, if not found, then search database. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot?> FindAsync(TKey id, CancellationToken cancel = default)
    {
        return GetAsync(id, cancel);
    }

    /// <summary>
    /// Find the entity by the given key 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot?> GetAsync(TKey id, CancellationToken cancel = default)
    {
        var agg = _caching.Get<TAggregateRoot>(id);
        return Task.FromResult(agg);
    }

    /// <summary>
    /// Find the entity by precondition
    /// The different with FindAsync is that FindAsync is search local cache in first, if not found, then search database, But GetAsync will search a database directly. 
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot?> GetAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        // convert lambda expression to func
        var func = predicate.Compile();
        var agg  = _caching.Get(func);

        return Task.FromResult(agg);
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
        var func = predicate.Compile();
        var q    = _caching.GetAll(func, skip, take);
        return Task.FromResult(q);
    }

    /// <summary>
    /// Get All entities from the repository that match the given predicate
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        var func = predicate.Compile();
        var q    = _caching.GetAll(func, null, null);
        return Task.FromResult(q);
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
        var q = _caching.GetAll<TAggregateRoot>(null, skip, take);
        return Task.FromResult(q);
    }

    /// <summary>
    /// Get all entities from the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancel = default)
    {
        var q = _caching.GetAll<TAggregateRoot>(null, null, null);
        return Task.FromResult(q);
    }

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="id">The id to find in db</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    public Task<bool> ExistsAsync(TKey id, CancellationToken cancel = default)
    {
        var agg = _caching.Get<TAggregateRoot>(id);
        return Task.FromResult(agg != null);
    }

    /// <summary>
    /// Determine whether the entity exists in the repository
    /// </summary>
    /// <param name="predicate">The finding predicate</param>
    /// <param name="cancel">The cancellation token</param>
    /// <returns>Return true if entity has existed</returns>
    public Task<bool> ExistsAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        var func = predicate.Compile();
        var agg  = _caching.Get(func);
        return Task.FromResult(agg != null);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns>Returns the number of entities</returns>
    public Task<int> CountAsync(CancellationToken cancel = default)
    {
        var count = _caching.Count<TAggregateRoot>(null);
        return Task.FromResult((int)count);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="predicate">The condition of query</param>
    /// <param name="cancel"></param>
    /// <returns>Returns the number of entities</returns>
    public Task<int> CountAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        var func  = predicate.Compile();
        var count = _caching.Count(func);
        return Task.FromResult((int)count);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<long> LongCountAsync(CancellationToken cancel = default)
    {
        var count = _caching.Count<TAggregateRoot>(null);
        return Task.FromResult(count);
    }

    /// <summary>
    /// Count the entities in the repository
    /// </summary>
    /// <param name="predicate">The query condition</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public Task<long> LongCountAsync(Expression<Func<TAggregateRoot, bool>> predicate, CancellationToken cancel = default)
    {
        var func  = predicate.Compile();
        var count = _caching.Count(func);
        return Task.FromResult(count);
    }

    /// <summary>
    /// Get the queryable of entities in the repository
    /// </summary>
    /// <returns></returns>
    public IQueryable<TAggregateRoot> AsQueryable()
    {
        throw new NotImplementedException();
    }
}
