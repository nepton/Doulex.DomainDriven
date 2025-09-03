using Microsoft.EntityFrameworkCore.Storage;
using Doulex.DomainDriven.Exceptions;

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
    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await CurrentTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var domainEx = new DbTransactionException("Transaction commit failed", ex)
            {
                TransactionId = TransactionId,
                FailedOperation = TransactionOperation.Commit,
                TransactionState = TransactionState.RolledBack
            };
            throw domainEx;
        }
    }

    /// <summary>
    /// Rollback the transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await CurrentTransaction.RollbackAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var domainEx = new DbTransactionException("Transaction rollback failed", ex)
            {
                TransactionId = TransactionId,
                FailedOperation = TransactionOperation.Rollback,
                TransactionState = TransactionState.Aborted
            };
            throw domainEx;
        }
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
