namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database operation timeout exception
/// Thrown when a database operation execution time exceeds the preset timeout
/// </summary>
public class DbTimeoutException : DomainDrivenException
{
    /// <summary>
    /// Initializes a new instance of the DbTimeoutException class
    /// </summary>
    public DbTimeoutException() : base("Database operation timeout")
    {
        ErrorCode = "DB_TIMEOUT";
        Severity = ExceptionSeverity.Error;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbTimeoutException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbTimeoutException(string message) : base(message)
    {
        ErrorCode = "DB_TIMEOUT";
        Severity = ExceptionSeverity.Error;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbTimeoutException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_TIMEOUT";
        Severity = ExceptionSeverity.Error;
        IsRetryable = true;
    }

    /// <summary>
    /// Gets or sets the timeout duration in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the actual execution time in seconds
    /// </summary>
    public double ActualExecutionSeconds { get; set; }

    /// <summary>
    /// Gets or sets the SQL command or operation type that was executed
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// Gets or sets the type of timeout
    /// </summary>
    public TimeoutType TimeoutType { get; set; }
}

/// <summary>
/// Enumeration for timeout types
/// </summary>
public enum TimeoutType
{
    /// <summary>
    /// Command execution timeout
    /// </summary>
    CommandTimeout,

    /// <summary>
    /// Connection timeout
    /// </summary>
    ConnectionTimeout,

    /// <summary>
    /// Transaction timeout
    /// </summary>
    TransactionTimeout,

    /// <summary>
    /// Lock wait timeout
    /// </summary>
    LockTimeout
}
