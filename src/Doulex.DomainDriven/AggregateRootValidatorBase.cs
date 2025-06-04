namespace Doulex.DomainDriven;

/// <summary>
/// Base class for aggregate root validators, providing default virtual method implementations 
/// that allow subclasses to override only the methods they need.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root</typeparam>
public abstract class AggregateRootValidatorBase<TAggregate> : IAggregateRootValidator<TAggregate>
{
    /// <summary>
    /// Validates the aggregate root before it is added.
    /// </summary>
    /// <remarks>If overriding this method, there's no need to call the base class implementation.</remarks>
    /// <param name="aggregate">The aggregate root instance to validate</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public virtual Task ValidateBeforeAddAsync(TAggregate aggregate, CancellationToken cancellation)
    {
        return ValidateAsync(aggregate, ValidateMethods.Add, cancellation);
    }

    /// <summary>
    /// Validates the aggregate root before it is updated.
    /// </summary>
    /// <remarks>If overriding this method, there's no need to call the base class implementation.</remarks>
    /// <param name="aggregate">The aggregate root instance to validate</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public virtual Task ValidateBeforeUpdateAsync(TAggregate aggregate, CancellationToken cancellation)
    {
        return ValidateAsync(aggregate, ValidateMethods.Update, cancellation);
    }

    /// <summary>
    /// Validates the aggregate root before it is removed.
    /// </summary>
    /// <remarks>If overriding this method, there's no need to call the base class implementation.</remarks>
    /// <param name="aggregate">The aggregate root instance to validate</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public virtual Task ValidateBeforeRemoveAsync(TAggregate aggregate, CancellationToken cancellation)
    {
        // Call the general validation method with the remove operation flag
        return ValidateAsync(aggregate, ValidateMethods.Remove, cancellation);
    }

    /// <summary>
    /// Executes validation logic based on the specified validation method.
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to validate</param>
    /// <param name="method">The validation method (Add/Update/Remove)</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public virtual Task ValidateAsync(TAggregate aggregate, ValidateMethods method, CancellationToken cancellation)
    {
        // Default implementation does nothing. Subclasses can override this method to provide specific validation logic.
        return Task.CompletedTask;
    }
}
