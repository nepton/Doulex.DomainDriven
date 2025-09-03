namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database deadlock exception
/// Thrown when a database operation encounters a deadlock
/// </summary>
public class DbDeadlockException : DbUpdateException
{
    /// <summary>
    /// Initializes a new instance of the DbDeadlockException class
    /// </summary>
    public DbDeadlockException() : base("Database deadlock")
    {
        ErrorCode = "DB_DEADLOCK";
        Severity = ExceptionSeverity.Warning;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbDeadlockException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbDeadlockException(string message) : base(message)
    {
        ErrorCode = "DB_DEADLOCK";
        Severity = ExceptionSeverity.Warning;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbDeadlockException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbDeadlockException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_DEADLOCK";
        Severity = ExceptionSeverity.Warning;
        IsRetryable = true;
    }

    /// <summary>
    /// Gets or sets the transaction ID of the deadlock victim
    /// </summary>
    public Guid? VictimTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the list of resources involved in the deadlock
    /// </summary>
    public List<string> InvolvedResources { get; set; } = new();

    /// <summary>
    /// Gets or sets the time when the deadlock was detected
    /// </summary>
    public DateTime DetectionTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the type of deadlock
    /// </summary>
    public DeadlockType DeadlockType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether retry is recommended
    /// </summary>
    public bool ShouldRetry { get; set; } = true;
}

/// <summary>
/// Enumeration for deadlock types
/// </summary>
public enum DeadlockType
{
    /// <summary>
    /// Resource deadlock
    /// </summary>
    Resource,

    /// <summary>
    /// Conversion deadlock
    /// </summary>
    Conversion,

    /// <summary>
    /// Cycle deadlock
    /// </summary>
    Cycle,

    /// <summary>
    /// Other type of deadlock
    /// </summary>
    Other
}
