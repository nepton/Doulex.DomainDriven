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

| EF Core Exception | Domain Exception | Retryable | Description |
|-------------------|------------------|-----------|-------------|
| `DbUpdateConcurrencyException` | `DbUpdateConcurrencyException` | ✅ | Optimistic concurrency conflicts |
| `DbUpdateException` (constraint) | `DbConstraintViolationException` | ❌ | Primary key, foreign key, unique constraints |
| `DbUpdateException` (deadlock) | `DbDeadlockException` | ✅ | Database deadlocks |
| `DbUpdateException` (validation) | `DbValidationException` | ❌ | Data validation failures |
| `DbUpdateException` (generic) | `DbUpdateException` | ✅ | General update failures |
| `TimeoutException` | `DbTimeoutException` | ✅ | Operation timeouts |
| `DbException` (connection) | `DbConnectionException` | ✅ | Connection failures |
| `DbException` (permission) | `DbPermissionException` | ❌ | Permission denied |
| `DbException` (storage) | `DbStorageException` | ❌ | Storage issues |
| `InvalidOperationException` (transaction) | `DbTransactionException` | ❌ | Transaction failures |

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
        catch (DbConstraintViolationException ex) when (ex.ConstraintType == ConstraintType.Unique)
        {
            _logger.LogWarning("User with email {Email} already exists", user.Email);
            throw new ExistsException($"User with email {user.Email} already exists");
        }
        catch (DbValidationException ex)
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
        catch (DbUpdateConcurrencyException ex) when (ex.IsRetryable)
        {
            retryCount++;
            _logger.LogWarning("Concurrency conflict (attempt {Attempt}): {Message}", 
                retryCount, ex.Message);
            
            if (retryCount >= maxRetries)
                throw;
                
            // Exponential backoff
            await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount - 1)));
        }
        catch (DbDeadlockException ex) when (ex.ShouldRetry)
        {
            retryCount++;
            _logger.LogWarning("Deadlock detected (attempt {Attempt}): {Message}", 
                retryCount, ex.Message);
            
            if (retryCount >= maxRetries)
                throw;
                
            // Random delay to avoid thundering herd
            var random = new Random();
            await Task.Delay(TimeSpan.FromMilliseconds(random.Next(50, 200)));
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
    catch (DbTransactionException ex)
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
    catch (DbConnectionException ex)
    {
        _logger.LogError("Database connection failed: {Reason}", ex.FailureReason);
        throw;
    }
    catch (DbTimeoutException ex)
    {
        _logger.LogWarning("Query timed out after {Timeout}s", ex.TimeoutSeconds);
        throw;
    }
    catch (DbPermissionException ex)
    {
        _logger.LogError("Permission denied for operation: {Operation}", ex.Operation);
        throw;
    }
}
```

## Exception Properties

Each domain exception includes rich context information:

### DbUpdateConcurrencyException
- `EntityType`: Type of conflicting entity
- `EntityId`: ID of conflicting entity  
- `CurrentVersion`: Current database version
- `AttemptedVersion`: Version that was attempted

### DbConstraintViolationException
- `ConstraintType`: Type of violated constraint
- `ConstraintName`: Name of violated constraint
- `TableName`: Affected table name
- `ColumnName`: Affected column name
- `ViolatingValue`: Value that caused violation

### DbDeadlockException
- `VictimTransactionId`: ID of deadlock victim transaction
- `InvolvedResources`: List of resources involved in deadlock
- `DetectionTime`: When deadlock was detected
- `DeadlockType`: Type of deadlock
- `ShouldRetry`: Whether retry is recommended

### DbConnectionException
- `ServerAddress`: Database server address
- `DatabaseName`: Database name
- `FailureReason`: Specific reason for connection failure
- `ConnectionString`: Connection string (sensitive data masked)

## Best Practices

1. **Always handle retryable exceptions** with appropriate retry logic
2. **Log exception context** for debugging and monitoring
3. **Use structured logging** with exception properties
4. **Implement circuit breaker patterns** for connection failures
5. **Monitor exception patterns** to identify systemic issues
6. **Handle constraint violations gracefully** with user-friendly messages
7. **Use transactions appropriately** and always handle rollbacks

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
