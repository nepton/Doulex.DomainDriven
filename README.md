# Doulex.DomainDriven

[![Build status](https://ci.appveyor.com/api/projects/status/np7c7landwamcwf4?svg=true)](https://ci.appveyor.com/project/nepton/Doulex.DomainDriven)
[![CodeQL](https://github.com/nepton/Doulex.DomainDriven/actions/workflows/codeql.yml/badge.svg)](https://github.com/nepton/Doulex.DomainDriven/actions/workflows/codeql.yml)
![GitHub issues](https://img.shields.io/github/issues/nepton/Doulex.DomainDriven.svg)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/nepton/Doulex.DomainDriven/blob/master/LICENSE)

A comprehensive Domain-Driven Design (DDD) framework for .NET that provides essential building blocks for implementing clean architecture patterns with robust exception handling and Entity Framework Core integration.

## ✨ Features

- 🏗️ **Complete DDD Building Blocks**: Aggregate roots, entities, value objects, domain events, and repositories
- 🔄 **Unit of Work Pattern**: Transaction management and change tracking
- 🛡️ **Comprehensive Exception Handling**: Rich domain exceptions with automatic EF Core translation
- 📊 **Entity Framework Core Integration**: Ready-to-use repository implementations
- 🔄 **Intelligent Retry Logic**: Automatic detection of retryable database operations
- 📝 **Structured Logging Support**: Exception properties designed for monitoring and debugging
- 🎯 **Clean Architecture**: Separation of concerns with clear boundaries
- ⚡ **High Performance**: Minimal overhead with efficient implementations

## 📦 NuGet Packages

| Package | Version | Downloads | Description |
|---------|---------|-----------|-------------|
| **Doulex.DomainDriven** | [![nuget](https://img.shields.io/nuget/v/Doulex.DomainDriven.svg)](https://www.nuget.org/packages/Doulex.DomainDriven/) | [![stats](https://img.shields.io/nuget/dt/Doulex.DomainDriven.svg)](https://www.nuget.org/packages/Doulex.DomainDriven/) | Core DDD abstractions and exception system |
| **Doulex.DomainDriven.Repo.EFCore** | [![nuget](https://img.shields.io/nuget/v/Doulex.DomainDriven.Repo.EFCore.svg)](https://www.nuget.org/packages/Doulex.DomainDriven.Repo.EFCore/) | [![stats](https://img.shields.io/nuget/dt/Doulex.DomainDriven.Repo.EFCore.svg)](https://www.nuget.org/packages/Doulex.DomainDriven.Repo.EFCore/) | Entity Framework Core implementation |

## 🚀 Quick Start

### Installation

Install the core package:
```bash
dotnet add package Doulex.DomainDriven
```

For Entity Framework Core support:
```bash
dotnet add package Doulex.DomainDriven.Repo.EFCore
```

Or via Package Manager Console:
```powershell
PM> Install-Package Doulex.DomainDriven
PM> Install-Package Doulex.DomainDriven.Repo.EFCore
```

### Basic Usage

#### 1. Define Your Domain Entities

```csharp
// Define an aggregate root
public class User : AggregateRoot<int>
{
    public string Email { get; private set; }
    public string Name { get; private set; }

    public User(string email, string name)
    {
        Email = email;
        Name = name;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty");

        Name = newName;
    }
}
```

#### 2. Create Repository Interface

```csharp
public interface IUserRepository : IRepository<User, int>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
```

#### 3. Implement Repository with EF Core

```csharp
public class UserRepository : EntityFrameworkCoreRepository<User, int>, IUserRepository
{
    public UserRepository(DbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await GetAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExistsAsync(u => u.Email == email, cancellationToken);
    }
}
```

#### 4. Configure Services

```csharp
// In Program.cs or Startup.cs
services.AddDbContext<YourDbContext>(options =>
    options.UseSqlServer(connectionString));

services.AddScoped<IUnitOfWork, EntityFrameworkCoreUnitOfWork<YourDbContext>>();
services.AddScoped<IUserRepository, UserRepository>();
```

#### 5. Use in Application Services

```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(string email, string name)
    {
        try
        {
            // Check if user already exists
            if (await _userRepository.EmailExistsAsync(email))
            {
                throw new ExistsException($"User with email {email} already exists");
            }

            // Create and save user
            var user = new User(email, name);
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User created successfully: {Email}", email);
            return user;
        }
        catch (DbConstraintViolationException ex) when (ex.ConstraintType == ConstraintType.Unique)
        {
            _logger.LogWarning("Unique constraint violation for email: {Email}", email);
            throw new ExistsException($"User with email {email} already exists");
        }
        catch (DbValidationException ex)
        {
            _logger.LogError("Validation failed: {Errors}",
                string.Join(", ", ex.ValidationErrors.Select(e => e.ErrorMessage)));
            throw;
        }
    }
}
```

## 🛡️ Exception Handling

The framework provides comprehensive exception handling with automatic EF Core exception translation:

### Exception Types

| Exception | Description | Retryable | Use Case |
|-----------|-------------|-----------|----------|
| `DbUpdateConcurrencyException` | Optimistic concurrency conflicts | ✅ | Multiple users editing same data |
| `DbConstraintViolationException` | Database constraint violations | ❌ | Unique key, foreign key violations |
| `DbValidationException` | Data validation failures | ❌ | Invalid data format or business rules |
| `DbDeadlockException` | Database deadlocks | ✅ | Resource contention |
| `DbConnectionException` | Connection failures | ✅ | Network issues, server downtime |
| `DbTimeoutException` | Operation timeouts | ✅ | Long-running queries |
| `DbTransactionException` | Transaction failures | ❌ | Commit/rollback issues |
| `DbPermissionException` | Permission denied | ❌ | Insufficient database permissions |
| `DbStorageException` | Storage issues | ❌ | Disk space, file system errors |
| `ExistsException` | Entity already exists | ❌ | Duplicate entity creation |

### Retry Logic Example

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

## 🔄 Transaction Management

### Basic Transaction Usage

```csharp
public async Task TransferDataAsync(int fromUserId, int toUserId, decimal amount)
{
    ITransaction? transaction = null;

    try
    {
        transaction = await _unitOfWork.BeginTransactionAsync();

        // Perform multiple operations
        var fromUser = await _userRepository.GetAsync(fromUserId);
        var toUser = await _userRepository.GetAsync(toUserId);

        fromUser.DebitAmount(amount);
        toUser.CreditAmount(amount);

        await _userRepository.UpdateAsync(fromUser);
        await _userRepository.UpdateAsync(toUser);

        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        _logger.LogInformation("Transfer completed: {Amount} from {FromUser} to {ToUser}",
            amount, fromUserId, toUserId);
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
