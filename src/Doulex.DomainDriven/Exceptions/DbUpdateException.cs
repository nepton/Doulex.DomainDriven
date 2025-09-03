namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database update exception
/// Thrown when a database update operation fails
/// </summary>
public class DbUpdateException : DomainDrivenException
{
    /// <summary>
    /// Initializes a new instance of the DbUpdateException class
    /// </summary>
    public DbUpdateException() : base("Database update operation failed")
    {
        ErrorCode = "DB_UPDATE_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbUpdateException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbUpdateException(string message) : base(message)
    {
        ErrorCode = "DB_UPDATE_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbUpdateException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbUpdateException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_UPDATE_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = true;
    }

    /// <summary>
    /// Gets or sets the number of affected entities
    /// </summary>
    public int AffectedEntitiesCount { get; set; }

    /// <summary>
    /// Gets or sets the type of operation that failed
    /// </summary>
    public string? OperationType { get; set; }
}
