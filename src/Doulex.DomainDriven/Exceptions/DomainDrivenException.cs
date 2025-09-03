namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Base exception class for domain-driven design framework
/// All framework-related exceptions should inherit from this class
/// </summary>
public abstract class DomainDrivenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DomainDrivenException class
    /// </summary>
    protected DomainDrivenException() : base()
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the DomainDrivenException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    protected DomainDrivenException(string message) : base(message)
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the DomainDrivenException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    protected DomainDrivenException(string message, Exception innerException) : base(message, innerException)
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the timestamp when the exception occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets or sets the error code for programmatic handling
    /// </summary>
    public virtual string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the severity level of the exception
    /// </summary>
    public virtual ExceptionSeverity Severity { get; set; } = ExceptionSeverity.Error;



    /// <summary>
    /// Gets additional context information
    /// </summary>
    public Dictionary<string, object> Context { get; } = new();
}

/// <summary>
/// Enumeration for exception severity levels
/// </summary>
public enum ExceptionSeverity
{
    /// <summary>
    /// Information level
    /// </summary>
    Information,

    /// <summary>
    /// Warning level
    /// </summary>
    Warning,

    /// <summary>
    /// Error level
    /// </summary>
    Error,

    /// <summary>
    /// Critical error level
    /// </summary>
    Critical,

    /// <summary>
    /// Fatal error level
    /// </summary>
    Fatal
}
