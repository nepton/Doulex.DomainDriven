namespace Doulex.DomainDriven;

/// <summary>
/// Aggregate root validator used to validate the aggregate root before performing add, update, or delete operations
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root</typeparam>
public interface IAggregateRootValidator<in TAggregate>
{
    /// <summary>
    /// Validates the aggregate root before adding it
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to be validated</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>An asynchronous task</returns>
    Task ValidateBeforeAddAsync(TAggregate aggregate, CancellationToken cancellation);

    /// <summary>
    /// Validates the aggregate root before updating it
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to be validated</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>An asynchronous task</returns>
    Task ValidateBeforeUpdateAsync(TAggregate aggregate, CancellationToken cancellation);

    /// <summary>
    /// Validates the aggregate root before removing it
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to be validated</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>An asynchronous task</returns>
    Task ValidateBeforeRemoveAsync(TAggregate aggregate, CancellationToken cancellation);
}
