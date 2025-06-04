namespace Doulex.DomainDriven;

/// <summary>
/// The mode of attachment
/// </summary>
public enum SaveMode
{
    /// <summary>
    /// The aggregate root will add to the repository
    /// </summary>
    Add,

    /// <summary>
    /// The aggregate root will update to the repository
    /// </summary>
    Update,
}
