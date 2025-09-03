namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database transaction exception
/// Thrown when a transaction operation fails
/// </summary>
public class DbTransactionException : DomainDrivenException
{
    /// <summary>
    /// Initializes a new instance of the DbTransactionException class
    /// </summary>
    public DbTransactionException() : base("Database transaction operation failed")
    {
        ErrorCode = "DB_TRANSACTION_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbTransactionException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbTransactionException(string message) : base(message)
    {
        ErrorCode = "DB_TRANSACTION_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbTransactionException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbTransactionException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_TRANSACTION_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Gets or sets the transaction ID
    /// </summary>
    public Guid? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the transaction state
    /// </summary>
    public TransactionState TransactionState { get; set; }

    /// <summary>
    /// Gets or sets the type of transaction operation that failed
    /// </summary>
    public TransactionOperation FailedOperation { get; set; }

    /// <summary>
    /// Gets or sets the transaction isolation level
    /// </summary>
    public string? IsolationLevel { get; set; }
}

/// <summary>
/// Enumeration for transaction states
/// </summary>
public enum TransactionState
{
    /// <summary>
    /// Not started
    /// </summary>
    NotStarted,

    /// <summary>
    /// Active
    /// </summary>
    Active,

    /// <summary>
    /// Committed
    /// </summary>
    Committed,

    /// <summary>
    /// Rolled back
    /// </summary>
    RolledBack,

    /// <summary>
    /// Aborted
    /// </summary>
    Aborted
}

/// <summary>
/// Enumeration for transaction operations
/// </summary>
public enum TransactionOperation
{
    /// <summary>
    /// Begin transaction
    /// </summary>
    Begin,

    /// <summary>
    /// Commit transaction
    /// </summary>
    Commit,

    /// <summary>
    /// Rollback transaction
    /// </summary>
    Rollback,

    /// <summary>
    /// Savepoint
    /// </summary>
    Savepoint,

    /// <summary>
    /// Release savepoint
    /// </summary>
    ReleaseSavepoint
}
