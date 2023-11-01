using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Doulex.DomainDriven.Repo.FS;

/// <summary>
/// 负责实体对象读写
/// </summary>
public class EntityPersistence
{
    private readonly FileSystemOptions _options;

    public EntityPersistence(FileSystemOptions options)
    {
        _options = options;
    }

    private string GetFullPath(Type type)
    {
        if (_options.RootPath == null)
            throw new InvalidOperationException("The root path of file system repository is not configured");

        return Path.Combine(_options.RootPath, type.Name);
    }

    private string GetFullPath(Type type, object key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        if (_options.RootPath == null)
            throw new InvalidOperationException("The root path of file system repository is not configured");

        return Path.Combine(_options.RootPath, type.Name, key.ToString());
    }

    public async Task AddAsync(IAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        var filePath = GetFullPath(aggregateRoot.GetType(), aggregateRoot.Id);
        PrepareDirectory(filePath);

        // Cannot replace the exists file in add mode
        if (File.Exists(filePath))
            throw new InvalidOperationException($"The entity {aggregateRoot.Id} already exists");

        var json = JsonConvert.SerializeObject(aggregateRoot, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task AddOrUpdateAsync(IAggregateRoot agg, CancellationToken cancel)
    {
        var filePath = GetFullPath(agg.GetType(), agg.Id);
        PrepareDirectory(filePath);

        var json = JsonConvert.SerializeObject(agg, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json, cancel);
    }

    public async Task UpdateAsync(IAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        var filePath = GetFullPath(aggregateRoot.GetType(), aggregateRoot.Id);
        PrepareDirectory(filePath);

        // Cannot update the not exists file in update mode
        if (!File.Exists(filePath))
            throw new InvalidOperationException($"The entity {aggregateRoot.Id} does not exists");

        var json = JsonConvert.SerializeObject(aggregateRoot, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public Task RemoveAsync(Type type, object key, CancellationToken cancellationToken)
    {
        var filePath = GetFullPath(type, key);
        PrepareDirectory(filePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private static void PrepareDirectory(string filePath)
    {
        // prepare the directory
        var dirPath = Path.GetDirectoryName(filePath);
        if (dirPath != null && !Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
    }

    public async Task<IAggregateRoot?> GetAsync(Type type, object key, CancellationToken cancellationToken)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        // check the type is from aggregate root
        if (typeof(IAggregateRoot).IsAssignableFrom(type) == false)
            throw new InvalidOperationException($"The type {type} is not from IAggregateRoot");

        var filePath = GetFullPath(type, key);

        // Cannot remove the not exists file in remove mode
        if (!File.Exists(filePath))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var agg  = JsonConvert.DeserializeObject(json, type);
            return (IAggregateRoot) agg!;
        }
        catch (Exception e)
        {
            // todo change the logger
            Console.WriteLine("Cannot deserialize the file {0}, error: {1}", filePath, e.Message);
            return null;
        }
    }

    public async Task<IAggregateRoot[]> GetAllAsync(Type type, CancellationToken cancellationToken)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        // check the type is from aggregate root
        if (typeof(IAggregateRoot).IsAssignableFrom(type) == false)
            throw new InvalidOperationException($"The type {type} is not from IAggregateRoot");

        var dirPath = GetFullPath(type);

        // Cannot remove the not exists file in remove mode
        if (!Directory.Exists(dirPath))
            return Array.Empty<IAggregateRoot>();

        var files = Directory.GetFiles(dirPath);
        var aggs  = new IAggregateRoot[files.Length];
        for (var i = 0; i < files.Length; i++)
        {
            try
            {
                var json = await File.ReadAllTextAsync(files[i], cancellationToken);
                var agg  = JsonConvert.DeserializeObject(json, type);
                aggs[i] = (IAggregateRoot) agg!;
            }
            catch (Exception e)
            {
                // todo change the logger
                Console.WriteLine("Cannot deserialize the file {0}, error: {1}", files[i], e.Message);
            }
        }

        return aggs;
    }
}
