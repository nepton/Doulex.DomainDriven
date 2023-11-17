using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Force.DeepCloner;

namespace Doulex.DomainDriven.Repo.FileSystem;

/// <summary>
/// 负责实体对象缓存
/// </summary>
public class EntityCaching
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, AggregateMeta>> _cache = new();

    private readonly EntityPersistence _entityFile;

    public EntityCaching(FileSystemOptions options)
    {
        _entityFile = new(options);
    }

    public void AddOrUpdateCache(IAggregateRoot aggregateRoot)
    {
        if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));
        if (aggregateRoot.Id == null) throw new InvalidOperationException("The id of entity is null");

        var type  = aggregateRoot.GetType();
        var key   = aggregateRoot.Id;
        var meta  = new AggregateMeta(aggregateRoot.DeepClone());
        var cache = _cache.GetOrAdd(type, LoadCache);
        cache.AddOrUpdate(key, meta, (_, _) => meta);
    }

    public void RemoveCache(Type type, object key)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (key == null) throw new ArgumentNullException(nameof(key));

        var cache = _cache.GetOrAdd(type, LoadCache);
        cache.TryRemove(key, out _);
    }

    public TAggregateRoot? Get<TAggregateRoot>(object key) where TAggregateRoot : class, IAggregateRoot, IEntity
    {
        var type  = typeof(TAggregateRoot);
        var cache = _cache.GetOrAdd(type, LoadCache);
        var agg   = cache.TryGetValue(key, out var meta) ? (TAggregateRoot) meta.AggregateRoot : default;

        return agg?.DeepClone();
    }

    public TAggregateRoot? Get<TAggregateRoot>(Func<TAggregateRoot, bool> predicate) where TAggregateRoot : class, IAggregateRoot, IEntity
    {
        var type  = typeof(TAggregateRoot);
        var cache = _cache.GetOrAdd(type, LoadCache);
        var agg   = cache.Values.Select(x => (TAggregateRoot) x.AggregateRoot).Where(predicate).FirstOrDefault();

        return agg?.DeepClone();
    }

    public TAggregateRoot[] GetAll<TAggregateRoot>(Func<TAggregateRoot, bool>? predicate, int? skip, int? take)
        where TAggregateRoot : class, IAggregateRoot, IEntity
    {
        var type  = typeof(TAggregateRoot);
        var cache = _cache.GetOrAdd(type, LoadCache);
        var q     = cache.Values.Select(x => (TAggregateRoot) x.AggregateRoot);

        if (predicate != null)
            q = q.Where(predicate);
        if (skip != null)
            q = q.Skip(skip.Value);
        if (take != null)
            q = q.Take(take.Value);

        return q.Select(x => x.DeepClone()).ToArray();
    }

    private ConcurrentDictionary<object, AggregateMeta> LoadCache(Type type)
    {
        var cache      = new ConcurrentDictionary<object, AggregateMeta>();
        var aggregates = _entityFile.GetAllAsync(type, CancellationToken.None).Result;
        foreach (var agg in aggregates)
        {
            var meta = new AggregateMeta(agg);
            cache.AddOrUpdate(agg.Id, meta, (_, _) => meta);
        }

        return cache;
    }

    public long Count<TAggregateRoot>(Func<TAggregateRoot, bool>? predicate) where TAggregateRoot : class, IAggregateRoot, IEntity
    {
        var type  = typeof(TAggregateRoot);
        var cache = _cache.GetOrAdd(type, LoadCache);
        var q     = cache.Values.Select(x => (TAggregateRoot) x.AggregateRoot);

        if (predicate != null)
            q = q.Where(predicate);

        return q.LongCount();
    }
}
