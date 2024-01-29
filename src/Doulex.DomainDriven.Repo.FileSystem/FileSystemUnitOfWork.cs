using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Doulex.DomainDriven.Repo.FileSystem;

public class FileSystemUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// The pending queue
    /// </summary>
    private readonly ConcurrentQueue<Pending> _pending = new();

    private readonly FileSystemOptions _options;
    private readonly EntityPersistence _entityFile;
    private readonly EntityCaching     _cache;

    public FileSystemUnitOfWork(FileSystemOptions options)
    {
        _options    = options;
        _entityFile = new(options);
        _cache      = new(options);
    }

    /// <summary>
    /// Put the entity to the pending queue
    /// </summary>
    /// <param name="type"></param>
    /// <param name="action"></param>
    /// <param name="obj"></param>
    internal void Enqueue(Type type, PendingAction action, object? obj)
    {
        _pending.Enqueue(new Pending(type, action, obj));
    }

    private object? GetEntityId(Pending working)
    {
        var key = working.Action switch
        {
            PendingAction.Add    => (working.Object as IAggregateRoot)?.Id,
            PendingAction.Update => (working.Object as IAggregateRoot)?.Id,
            PendingAction.Remove => working.Object,
            _                    => throw new ArgumentOutOfRangeException()
        };
        return key;
    }

    /// <summary>
    /// Save the changes
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancel = default)
    {
        var count = 0;
        while (_pending.TryDequeue(out var working))
        {
            switch (working.Action)
            {
                case PendingAction.Add:
                {
                    if (working.Object is not IAggregateRoot agg)
                        throw new InvalidOperationException("The object is not IAggregateRoot");

                    await _entityFile.AddAsync(agg, cancel);
                    _cache.AddOrUpdateCache(agg);

                    break;
                }
                case PendingAction.Update:
                {
                    if (working.Object is not IAggregateRoot agg)
                        throw new InvalidOperationException("The object is not IAggregateRoot");

                    await _entityFile.UpdateAsync(agg, cancel);
                    _cache.AddOrUpdateCache(agg);

                    break;
                }
                case PendingAction.Remove:
                {
                    // Check the file exists
                    var entityId = GetEntityId(working) ?? throw new InvalidOperationException("The key of entity is null");

                    await _entityFile.RemoveAsync(working.Type, entityId, cancel);
                    _cache.RemoveCache(working.Type, entityId);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            count += 1;
        }

        return count;
    }

    /// <summary>
    /// Begin a transaction
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Union the transaction with the current unit of work, if the unit of work is not a transaction, it will throw an exception
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UseTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Indicate whether the transaction is active
    /// </summary>
    public bool HasActiveTransaction => false;


    /// <summary>
    /// Indicate whether the unit of work support nested transaction
    /// </summary>
    public bool SupportNestedTransaction => false;

    /// <summary>
    /// Indicate whether the unit of work support transaction
    /// </summary>
    /// <returns></returns>
    public bool SupportTransaction => false;

    public FileSystemOptions Options => _options;
}
