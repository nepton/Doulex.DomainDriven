# Doulex.DomainDriven.Repo.EFCore - Exception Handling

This package provides Entity Framework Core implementation for the Doulex.DomainDriven framework with comprehensive exception handling and translation.

## Features

- **Automatic Exception Translation**: Converts EF Core exceptions to domain-driven exceptions
- **Comprehensive Error Handling**: Covers all common database operation scenarios
- **Retry Logic Support**: Built-in support for retryable operations
- **Detailed Context Information**: Rich exception context for debugging and monitoring
- **Structured Logging**: Exception properties designed for structured logging

## Exception Translation

The `ExceptionTranslator` automatically converts Entity Framework Core exceptions to domain exceptions:

### EF Core Exception → Domain Exception Mapping

| EF Core Exception                | Domain Exception                 | Description                                                 |
|----------------------------------|----------------------------------|-------------------------------------------------------------|
| `RepoUpdateConcurrencyException` | `RepoUpdateConcurrencyException` | Optimistic concurrency conflicts                            |
| `RepoUpdateException`            | `RepoUpdateException`            | General database update failures                            |
| `TimeoutException`               | `RepoTimeoutException`           | Operation timeouts                                          |
| `RepoException`                  | `RepoUpdateException`            | All database-specific errors (reliable, no message parsing) |
| `InvalidOperationException`      | `RepoTransactionException`       | Transaction failures                                        |

## Usage Examples

### Basic Repository Usage

```csharp
public class UserService
{
    private readonly IRepository<User, int> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public async Task<bool> CreateUserAsync(User user)
    {
        try
        {
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (RepoUpdateException ex)
        {
            _logger.LogError("Database update failed: {Message}", ex.Message);
            throw;
        }
        catch (RepoValidationException ex)
        {
            _logger.LogError("User validation failed: {Errors}", 
                string.Join(", ", ex.ValidationErrors.Select(e => e.ErrorMessage)));
            throw;
        }
    }
}
```

### Retry Logic for Transient Failures

```csharp
public async Task<bool> UpdateUserWithRetryAsync(User user, int maxRetries = 3)
{
    var retryCount = 0;
    
    while (retryCount < maxRetries)
    {
        try
        {
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (RepoUpdateConcurrencyException ex)
        {
            retryCount++;
            _logger.LogWarning("Concurrency conflict (attempt {Attempt}): {Message}",
                retryCount, ex.Message);

            if (retryCount >= maxRetries)
                throw;

            // Exponential backoff
            await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount - 1)));
        }
        catch (RepoTimeoutException ex)
        {
            retryCount++;
            _logger.LogWarning("Timeout detected (attempt {Attempt}): {Message}",
                retryCount, ex.Message);

            if (retryCount >= maxRetries)
                throw;

            // Wait before retry
            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
    }
    
    return false;
}
```

### Transaction Handling

```csharp
public async Task<bool> TransferDataAsync(int fromId, int toId, decimal amount)
{
    ITransaction? transaction = null;
    
    try
    {
        transaction = await _unitOfWork.BeginTransactionAsync();
        
        // Perform operations
        await DebitAccountAsync(fromId, amount);
        await CreditAccountAsync(toId, amount);
        
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return true;
    }
    catch (RepoTransactionException ex)
    {
        _logger.LogError("Transaction failed: {Operation} - {Message}", 
            ex.FailedOperation, ex.Message);
        
        if (transaction != null)
        {
            await transaction.RollbackAsync();
        }
        
        throw;
    }
    finally
    {
        transaction?.Dispose();
    }
}
```

### Query Exception Handling

```csharp
public async Task<User?> GetUserSafelyAsync(int userId)
{
    try
    {
        return await _userRepository.GetAsync(userId);
    }
    catch (RepoTimeoutException ex)
    {
        _logger.LogWarning("Query timed out after {Timeout}s", ex.TimeoutSeconds);
        throw;
    }
    catch (RepoUpdateException ex)
    {
        _logger.LogError("Database operation failed: {Message}", ex.Message);
        throw;
    }
}
```

## Exception Properties

Each domain exception includes rich context information:

### RepoUpdateException

- `OperationType`: Type of operation that failed
- `AffectedEntitiesCount`: Number of entities affected

### RepoUpdateConcurrencyException

- `EntityType`: Type of conflicting entity
- `EntityId`: ID of conflicting entity
- `CurrentVersion`: Current database version
- `AttemptedVersion`: Version that was attempted

## Best Practices

1. **Implement retry logic** for appropriate exceptions (concurrency, timeout)
2. **Log exception context** for debugging and monitoring
3. **Use structured logging** with exception properties
4. **Monitor exception patterns** to identify systemic issues
5. **Handle database errors gracefully** with user-friendly messages
6. **Use transactions appropriately** and always handle rollbacks
7. **Avoid message-based exception detection** - rely on exception types only

## Configuration

The exception translation is automatic and requires no configuration. However, you can customize the translation logic by extending the `ExceptionTranslator` class.

## Monitoring and Alerting

Set up monitoring for:

- `Critical` and `Fatal` severity exceptions
- High frequency of retryable exceptions
- Connection failure patterns
- Storage space issues
- Permission denied attempts

## Dependencies

- Microsoft.EntityFrameworkCore (6.0+ or 8.0+)
- Doulex.DomainDriven (contains exception definitions)
