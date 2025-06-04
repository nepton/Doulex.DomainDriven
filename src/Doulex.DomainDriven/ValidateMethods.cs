namespace Doulex.DomainDriven;

/// <summary>
/// Validation method enum, indicates the operation type during aggregate root validation
/// </summary>
public enum ValidateMethods
{
    /// <summary>
    /// Add operation
    /// </summary>
    Add,

    /// <summary>
    /// Update operation
    /// </summary>
    Update,

    /// <summary>
    /// Remove operation
    /// </summary>
    Remove,
}
