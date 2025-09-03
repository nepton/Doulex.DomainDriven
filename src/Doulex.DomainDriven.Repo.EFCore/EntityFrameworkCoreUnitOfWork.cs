using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Doulex.DomainDriven.Exceptions;

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
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancel = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancel);
        }
        catch (Exception ex)
        {
            var translatedEx = ExceptionTranslator.TranslateException(ex, "SaveChanges");
            throw translatedEx;
        }
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
        try
        {
            if (!SupportTransaction)
                throw new RepoTransactionException("Transaction not supported")
                {
                    FailedOperation = TransactionOperation.Begin,
                    TransactionState = TransactionState.NotStarted
                };

            if (HasActiveTransaction && !SupportNestedTransaction)
                throw new RepoTransactionException("Transaction already started")
                {
                    FailedOperation = TransactionOperation.Begin,
                    TransactionState = TransactionState.Active
                };

            var tran = await _context.Database.BeginTransactionAsync(cancellationToken);
            return new EntityFrameworkCoreTransaction(tran);
        }
        catch (RepoTransactionException)
        {
            throw; // Re-throw our domain exceptions as-is
        }
        catch (Exception ex)
        {
            var translatedEx = ExceptionTranslator.TranslateException(ex, "BeginTransaction");
            throw translatedEx;
        }
    }

    /// <summary>
    /// Union the transaction with input transaction.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task UseTransactionAsync(ITransaction transaction, CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            if (!SupportTransaction)
                throw new RepoTransactionException("Transaction not supported")
                {
                    FailedOperation = TransactionOperation.Begin,
                    TransactionState = TransactionState.NotStarted
                };

            if (transaction is not EntityFrameworkCoreTransaction tran)
                throw new RepoTransactionException("The transaction is not EntityFrameworkCoreTransaction")
                {
                    FailedOperation = TransactionOperation.Begin,
                    TransactionState = TransactionState.NotStarted
                };

            await _context.Database.UseTransactionAsync(tran.CurrentTransaction?.GetDbTransaction(), cancellationToken);
        }
        catch (RepoTransactionException)
        {
            throw; // Re-throw our domain exceptions as-is
        }
        catch (Exception ex)
        {
            var translatedEx = ExceptionTranslator.TranslateException(ex, "UseTransaction");
            throw translatedEx;
        }
    }
}
