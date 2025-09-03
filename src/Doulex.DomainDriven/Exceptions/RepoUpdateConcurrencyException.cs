namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database update concurrency exception
/// Thrown when multiple users simultaneously update the same data causing a concurrency conflict
/// </summary>
public class RepoUpdateConcurrencyException : RepoUpdateException
{
    /// <summary>
    /// Initializes a new instance of the DbUpdateConcurrencyException class
    /// </summary>
    public RepoUpdateConcurrencyException() : base("Database concurrency update conflict")
    {
        ErrorCode = "DB_CONCURRENCY_CONFLICT";
        Severity = ExceptionSeverity.Warning;
    }

    /// <summary>
    /// Initializes a new instance of the DbUpdateConcurrencyException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public RepoUpdateConcurrencyException(string message) : base(message)
    {
        ErrorCode = "DB_CONCURRENCY_CONFLICT";
        Severity = ExceptionSeverity.Warning;
    }

    /// <summary>
    /// Initializes a new instance of the DbUpdateConcurrencyException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public RepoUpdateConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_CONCURRENCY_CONFLICT";
        Severity = ExceptionSeverity.Warning;
    }

    /// <summary>
    /// Gets or sets the type of entity that had the conflict
    /// </summary>
    public Type? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the ID of the entity that had the conflict
    /// </summary>
    public object? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the current version number or timestamp in the database
    /// </summary>
    public object? CurrentVersion { get; set; }

    /// <summary>
    /// Gets or sets the version number or timestamp that was attempted to be updated
    /// </summary>
    public object? AttemptedVersion { get; set; }
}
