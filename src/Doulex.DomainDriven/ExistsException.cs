using Doulex.DomainDriven.Exceptions;

namespace Doulex.DomainDriven;

/// <summary>
/// Entity already exists exception
/// Thrown when attempting to create an entity that already exists
/// </summary>
public class ExistsException : DomainDrivenException
{
    /// <summary>
    /// Initializes a new instance of the ExistsException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ExistsException(string message) : base(message)
    {
        ErrorCode = "ENTITY_EXISTS";
        Severity = ExceptionSeverity.Warning;
    }

    /// <summary>
    /// Initializes a new instance of the ExistsException class with a specified error message and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ExistsException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "ENTITY_EXISTS";
        Severity = ExceptionSeverity.Warning;
    }

    /// <summary>
    /// Gets or sets the type of entity that already exists
    /// </summary>
    public Type? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the ID of the entity that already exists
    /// </summary>
    public object? EntityId { get; set; }
}
