namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database connection exception
/// Thrown when unable to establish or maintain a database connection
/// </summary>
public class DbConnectionException : DomainDrivenException
{
    /// <summary>
    /// Initializes a new instance of the DbConnectionException class
    /// </summary>
    public DbConnectionException() : base("Database connection failed")
    {
        ErrorCode = "DB_CONNECTION_FAILED";
        Severity = ExceptionSeverity.Critical;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbConnectionException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbConnectionException(string message) : base(message)
    {
        ErrorCode = "DB_CONNECTION_FAILED";
        Severity = ExceptionSeverity.Critical;
        IsRetryable = true;
    }

    /// <summary>
    /// Initializes a new instance of the DbConnectionException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbConnectionException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_CONNECTION_FAILED";
        Severity = ExceptionSeverity.Critical;
        IsRetryable = true;
    }

    /// <summary>
    /// Gets or sets the database server address
    /// </summary>
    public string? ServerAddress { get; set; }

    /// <summary>
    /// Gets or sets the database name
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the connection string (sensitive information should be masked)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the reason type for connection failure
    /// </summary>
    public ConnectionFailureReason FailureReason { get; set; }
}

/// <summary>
/// Enumeration for connection failure reasons
/// </summary>
public enum ConnectionFailureReason
{
    /// <summary>
    /// Network unreachable
    /// </summary>
    NetworkUnreachable,

    /// <summary>
    /// Authentication failed
    /// </summary>
    AuthenticationFailed,

    /// <summary>
    /// Database not found
    /// </summary>
    DatabaseNotFound,

    /// <summary>
    /// Connection timeout
    /// </summary>
    ConnectionTimeout,

    /// <summary>
    /// Connection refused
    /// </summary>
    ConnectionRefused,

    /// <summary>
    /// Connection pool exhausted
    /// </summary>
    ConnectionPoolExhausted,

    /// <summary>
    /// Other reason
    /// </summary>
    Other
}
