namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database permission exception
/// Thrown when a user does not have sufficient permissions to perform a database operation
/// </summary>
public class DbPermissionException : DomainDrivenException
{
    /// <summary>
    /// Initializes a new instance of the DbPermissionException class
    /// </summary>
    public DbPermissionException() : base("Insufficient database permissions")
    {
        ErrorCode = "DB_PERMISSION_DENIED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbPermissionException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbPermissionException(string message) : base(message)
    {
        ErrorCode = "DB_PERMISSION_DENIED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbPermissionException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbPermissionException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_PERMISSION_DENIED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Gets or sets the username or user ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the resource being accessed
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the type of operation being attempted
    /// </summary>
    public DatabaseOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the required permission
    /// </summary>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// Gets or sets the permissions currently held by the user
    /// </summary>
    public List<string> CurrentPermissions { get; set; } = new();
}

/// <summary>
/// Enumeration for database operation types
/// </summary>
public enum DatabaseOperation
{
    /// <summary>
    /// Select/Query
    /// </summary>
    Select,

    /// <summary>
    /// Insert
    /// </summary>
    Insert,

    /// <summary>
    /// Update
    /// </summary>
    Update,

    /// <summary>
    /// Delete
    /// </summary>
    Delete,

    /// <summary>
    /// Create table
    /// </summary>
    CreateTable,

    /// <summary>
    /// Alter table structure
    /// </summary>
    AlterTable,

    /// <summary>
    /// Drop table
    /// </summary>
    DropTable,

    /// <summary>
    /// Create index
    /// </summary>
    CreateIndex,

    /// <summary>
    /// Drop index
    /// </summary>
    DropIndex,

    /// <summary>
    /// Execute stored procedure
    /// </summary>
    ExecuteProcedure,

    /// <summary>
    /// Other operation
    /// </summary>
    Other
}
