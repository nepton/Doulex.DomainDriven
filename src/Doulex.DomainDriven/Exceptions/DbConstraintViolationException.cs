namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database constraint violation exception
/// Thrown when a database operation violates constraints (such as primary key, foreign key, unique constraint, check constraint, etc.)
/// </summary>
public class DbConstraintViolationException : DbUpdateException
{
    /// <summary>
    /// Initializes a new instance of the DbConstraintViolationException class
    /// </summary>
    public DbConstraintViolationException() : base("Database constraint violation")
    {
        ErrorCode = "DB_CONSTRAINT_VIOLATION";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbConstraintViolationException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbConstraintViolationException(string message) : base(message)
    {
        ErrorCode = "DB_CONSTRAINT_VIOLATION";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbConstraintViolationException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbConstraintViolationException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_CONSTRAINT_VIOLATION";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Gets or sets the name of the violated constraint
    /// </summary>
    public string? ConstraintName { get; set; }

    /// <summary>
    /// Gets or sets the type of constraint
    /// </summary>
    public ConstraintType ConstraintType { get; set; }

    /// <summary>
    /// Gets or sets the name of the involved table
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the name of the involved column
    /// </summary>
    public string? ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the value that violated the constraint
    /// </summary>
    public object? ViolatingValue { get; set; }
}

/// <summary>
/// Enumeration for constraint types
/// </summary>
public enum ConstraintType
{
    /// <summary>
    /// Primary key constraint
    /// </summary>
    PrimaryKey,

    /// <summary>
    /// Foreign key constraint
    /// </summary>
    ForeignKey,

    /// <summary>
    /// Unique constraint
    /// </summary>
    Unique,

    /// <summary>
    /// Check constraint
    /// </summary>
    Check,

    /// <summary>
    /// Not null constraint
    /// </summary>
    NotNull,

    /// <summary>
    /// Other constraint
    /// </summary>
    Other
}
