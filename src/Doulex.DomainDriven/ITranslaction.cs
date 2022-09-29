namespace Doulex.DomainDriven;

/// <summary>
/// The interface of the transaction control
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 返回当前事务的Id
    /// </summary>
    public Guid TransactionId { get; }

    /// <summary>
    /// Commit the transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>
    /// Rollback the transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken));
}
