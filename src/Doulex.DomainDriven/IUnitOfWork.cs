namespace Doulex.DomainDriven;

/// <summary>
/// Unit of work, represents a transaction
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Save the changes
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancel = default);

    /// <summary>
    /// Begin a transaction
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>
    /// Union the transaction with the current unit of work, if the unit of work is not a transaction, it will throw an exception
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UseTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>
    /// Indicate whether the transaction is active
    /// </summary>
    bool HasActiveTransaction { get; }
}
