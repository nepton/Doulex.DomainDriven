namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database storage exception
/// Thrown when there are database storage space issues or storage-related problems
/// </summary>
public class DbStorageException : DomainDrivenException
{
    /// <summary>
    /// Initializes a new instance of the DbStorageException class
    /// </summary>
    public DbStorageException() : base("Database storage error")
    {
        ErrorCode = "DB_STORAGE_ERROR";
        Severity = ExceptionSeverity.Critical;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbStorageException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbStorageException(string message) : base(message)
    {
        ErrorCode = "DB_STORAGE_ERROR";
        Severity = ExceptionSeverity.Critical;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbStorageException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbStorageException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_STORAGE_ERROR";
        Severity = ExceptionSeverity.Critical;
        IsRetryable = false;
    }

    /// <summary>
    /// Gets or sets the type of storage issue
    /// </summary>
    public StorageIssueType IssueType { get; set; }

    /// <summary>
    /// Gets or sets the available storage space in bytes
    /// </summary>
    public long? AvailableSpace { get; set; }

    /// <summary>
    /// Gets or sets the required storage space in bytes
    /// </summary>
    public long? RequiredSpace { get; set; }

    /// <summary>
    /// Gets or sets the storage device or file path
    /// </summary>
    public string? StoragePath { get; set; }

    /// <summary>
    /// Gets or sets the database name
    /// </summary>
    public string? DatabaseName { get; set; }
}

/// <summary>
/// Enumeration for storage issue types
/// </summary>
public enum StorageIssueType
{
    /// <summary>
    /// Insufficient disk space
    /// </summary>
    InsufficientDiskSpace,

    /// <summary>
    /// Database file size limit
    /// </summary>
    DatabaseSizeLimit,

    /// <summary>
    /// Insufficient log space
    /// </summary>
    InsufficientLogSpace,

    /// <summary>
    /// Insufficient temporary space
    /// </summary>
    InsufficientTempSpace,

    /// <summary>
    /// Storage device failure
    /// </summary>
    StorageDeviceFailure,

    /// <summary>
    /// File system error
    /// </summary>
    FileSystemError,

    /// <summary>
    /// Other storage issue
    /// </summary>
    Other
}
