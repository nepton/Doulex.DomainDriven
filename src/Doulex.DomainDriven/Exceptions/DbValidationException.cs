namespace Doulex.DomainDriven.Exceptions;

/// <summary>
/// Database validation exception
/// Thrown when data does not conform to business rules or data format requirements
/// </summary>
public class DbValidationException : DbUpdateException
{
    /// <summary>
    /// Initializes a new instance of the DbValidationException class
    /// </summary>
    public DbValidationException() : base("Data validation failed")
    {
        ErrorCode = "DB_VALIDATION_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbValidationException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DbValidationException(string message) : base(message)
    {
        ErrorCode = "DB_VALIDATION_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the DbValidationException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DbValidationException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "DB_VALIDATION_FAILED";
        Severity = ExceptionSeverity.Error;
        IsRetryable = false;
    }

    /// <summary>
    /// Gets or sets the list of validation errors
    /// </summary>
    public List<ValidationError> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Gets or sets the type of entity that failed validation
    /// </summary>
    public Type? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the ID of the entity that failed validation
    /// </summary>
    public object? EntityId { get; set; }
}

/// <summary>
/// Validation error information
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Gets or sets the property name
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value that was attempted to be set
    /// </summary>
    public object? AttemptedValue { get; set; }

    /// <summary>
    /// Gets or sets the type of validation rule
    /// </summary>
    public string? ValidationType { get; set; }
}
