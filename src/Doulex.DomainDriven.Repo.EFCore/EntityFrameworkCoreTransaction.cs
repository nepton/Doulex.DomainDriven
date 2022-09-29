using Microsoft.EntityFrameworkCore.Storage;

namespace Doulex.DomainDriven.Repo.EFCore;

/// <summary>
/// The transaction implement for ITransaction
/// </summary>
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class EntityFrameworkCoreTransaction : ITransaction
{
    public EntityFrameworkCoreTransaction(IDbContextTransaction transaction)
    {
        CurrentTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    /// <summary>
    /// The current transaction
    /// </summary>
    public IDbContextTransaction CurrentTransaction { get; }

    /// <summary>
    /// 返回当前事务的Id
    /// </summary>
    public Guid TransactionId => CurrentTransaction.TransactionId;

    /// <summary>
    /// Commit the transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return CurrentTransaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Rollback the transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return CurrentTransaction.RollbackAsync(cancellationToken);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public virtual void Dispose()
    {
        CurrentTransaction.Dispose();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public virtual ValueTask DisposeAsync()
    {
        return CurrentTransaction.DisposeAsync();
    }
}
