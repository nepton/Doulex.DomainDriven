using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Doulex.DomainDriven.Repo.EFCore;

/// <summary>
/// EF Core implementation of unit of work
/// </summary>
public class EntityFrameworkCoreUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    protected EntityFrameworkCoreUnitOfWork(DbContext dbContext)
    {
        _context = dbContext;
    }

    /// <summary>
    /// Commit the transaction
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual Task<int> SaveChangesAsync(CancellationToken cancel = default)
    {
        return _context.SaveChangesAsync(cancel);
    }

    /// <summary>
    /// Indicate whether the transaction is active
    /// </summary>
    public virtual bool HasActiveTransaction => _context.Database.CurrentTransaction != null;


    /// <summary>
    /// Indicate whether the unit of work support nested transaction
    /// </summary>
    /// <remarks>
    /// Set true if your database support nested transaction, otherwise set false.
    /// The default value is false.
    /// </remarks>
    public virtual bool SupportNestedTransaction => false;

    /// <summary>
    /// Indicate whether the unit of work support transaction
    /// </summary>
    /// <returns></returns>
    public virtual bool SupportTransaction => true;

    /// <summary>
    /// Begin a transaction
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!SupportTransaction)
            throw new InvalidOperationException("Transaction not supported");

        if (HasActiveTransaction && !SupportNestedTransaction)
            throw new InvalidOperationException("Transaction already started");

        var tran = await _context.Database.BeginTransactionAsync(cancellationToken);

        return new EntityFrameworkCoreTransaction(tran);
    }

    /// <summary>
    /// Union the transaction with input transaction.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task UseTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!SupportTransaction)
            throw new InvalidOperationException("Transaction not supported");

        if (transaction is not EntityFrameworkCoreTransaction tran)
            throw new ArgumentException("The transaction is not EntityFrameworkCoreTransaction");

        return _context.Database.UseTransactionAsync(tran.CurrentTransaction?.GetDbTransaction(), cancellationToken);
    }
}
