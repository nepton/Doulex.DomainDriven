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

| Package                             | Version                                                                                                                                         | Downloads                                                                                                                                        | Description                                |
|-------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------|
| **Doulex.DomainDriven**             | [![nuget](https://img.shields.io/nuget/v/Doulex.DomainDriven.svg)](https://www.nuget.org/packages/Doulex.DomainDriven/)                         | [![stats](https://img.shields.io/nuget/dt/Doulex.DomainDriven.svg)](https://www.nuget.org/packages/Doulex.DomainDriven/)                         | Core DDD abstractions and exception system |
| **Doulex.DomainDriven.Repo.EFCore** | [![nuget](https://img.shields.io/nuget/v/Doulex.DomainDriven.Repo.EFCore.svg)](https://www.nuget.org/packages/Doulex.DomainDriven.Repo.EFCore/) | [![stats](https://img.shields.io/nuget/dt/Doulex.DomainDriven.Repo.EFCore.svg)](https://www.nuget.org/packages/Doulex.DomainDriven.Repo.EFCore/) | Entity Framework Core implementation       |

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
        catch (RepoUpdateException ex)
        {
            _logger.LogError("Database update failed: {Message}", ex.Message);
            throw;
        }
    }
}
```

## 🛡️ Exception Handling

The framework provides comprehensive exception handling with automatic EF Core exception translation. All database exceptions are automatically converted to domain exceptions with rich context information:

### Exception Types

| Exception                        | Description                      | Use Case                           |
|----------------------------------|----------------------------------|------------------------------------|
| `RepoUpdateException`            | Database update failures         | General database update operations |
| `RepoUpdateConcurrencyException` | Optimistic concurrency conflicts | Multiple users editing same data   |
| `RepoTimeoutException`           | Operation timeouts               | Long-running queries               |
| `RepoTransactionException`       | Transaction failures             | Commit/rollback issues             |
| `ExistsException`                | Entity already exists            | Duplicate entity creation          |

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

## 🏗️ Architecture Overview

The framework follows Domain-Driven Design principles with clean architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │  Application    │    │   Domain        │                │
│  │  Services       │    │   Services      │                │
│  └─────────────────┘    └─────────────────┘                │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │  Aggregate      │    │   Domain        │                │
│  │  Roots          │    │   Events        │                │
│  └─────────────────┘    └─────────────────┘                │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │  Entities       │    │   Value         │                │
│  │                 │    │   Objects       │                │
│  └─────────────────┘    └─────────────────┘                │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │  EF Core        │    │   Exception     │                │
│  │  Repositories   │    │   Translation   │                │
│  └─────────────────┘    └─────────────────┘                │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │  Unit of Work   │    │   Database      │                │
│  │                 │    │   Context       │                │
│  └─────────────────┘    └─────────────────┘                │
└─────────────────────────────────────────────────────────────┘
```

## 🔧 Advanced Configuration

### Custom Repository Implementation

```csharp
public class CustomUserRepository : EntityFrameworkCoreRepository<User, int>, IUserRepository
{
    public CustomUserRepository(DbContext context) : base(context)
    {
    }

    public async Task<User[]> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .ToArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Automatic exception translation
            var translatedEx = ExceptionTranslator.TranslateException(ex, "GetActiveUsers");
            throw translatedEx;
        }
    }
}
```

### Dependency Injection Setup

```csharp
// Program.cs (ASP.NET Core)
var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add DDD services
builder.Services.AddScoped<IUnitOfWork, EntityFrameworkCoreUnitOfWork<ApplicationDbContext>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();
```

### Exception Monitoring

```csharp
public class ExceptionMonitoringService
{
    private readonly ILogger<ExceptionMonitoringService> _logger;

    public void LogDomainException(DomainDrivenException ex)
    {
        _logger.LogError("Domain Exception: {ErrorCode} - {Message} | Severity: {Severity} | Timestamp: {Timestamp}",
            ex.ErrorCode, ex.Message, ex.Severity, ex.Timestamp);

        // Send alerts for critical exceptions
        if (ex.Severity == ExceptionSeverity.Critical || ex.Severity == ExceptionSeverity.Fatal)
        {
            // Send notification to administrators
            SendAlert(ex);
        }
    }
}
```

## 📊 Performance Considerations

- **Minimal Overhead**: Exception translation only occurs when exceptions are thrown
- **Efficient Queries**: Repository methods use optimized EF Core queries
- **Connection Pooling**: Leverage EF Core's built-in connection pooling
- **Async Operations**: All database operations are asynchronous
- **Memory Efficient**: Context information uses existing EF Core data structures

## 🧪 Testing

### Unit Testing Repositories

```csharp
[Test]
public async Task GetAsync_WithValidId_ReturnsUser()
{
    // Arrange
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    await using var context = new TestDbContext(options);
    var repository = new UserRepository(context);

    var user = new User("test@example.com", "Test User");
    await repository.AddAsync(user);
    await context.SaveChangesAsync();

    // Act
    var result = await repository.GetAsync(user.Id);

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Email, Is.EqualTo("test@example.com"));
}
```

### Integration Testing with Exception Handling

```csharp
[Test]
public async Task SaveChangesAsync_WithConcurrencyConflict_ThrowsConcurrencyException()
{
    // Arrange
    var user1 = await _repository.GetAsync(1);
    var user2 = await _repository.GetAsync(1);

    // Act & Assert
    user1.UpdateName("New Name 1");
    user2.UpdateName("New Name 2");

    await _repository.UpdateAsync(user1);
    await _unitOfWork.SaveChangesAsync(); // First update succeeds

    await _repository.UpdateAsync(user2);

    // Second update should throw concurrency exception
    Assert.ThrowsAsync<RepoUpdateConcurrencyException>(
        async () => await _unitOfWork.SaveChangesAsync());
}
```

## 🚀 Migration Guide

### From Raw EF Core

1. **Replace DbContext direct usage** with Unit of Work pattern
2. **Implement repository interfaces** for your aggregate roots
3. **Add exception handling** around database operations
4. **Update dependency injection** configuration

### From Other DDD Frameworks

1. **Map existing entities** to inherit from `AggregateRoot<TKey>`
2. **Implement repository interfaces** using provided base classes
3. **Replace existing exception handling** with domain exceptions
4. **Update service layer** to use new repository interfaces

## 📚 Additional Resources

- [Domain-Driven Design Fundamentals](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture Principles](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📝 Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

## ❓ Support

- 📖 **Documentation**: Check the README and inline documentation
- 🐛 **Issues**: Report bugs on [GitHub Issues](https://github.com/nepton/Doulex.DomainDriven/issues)
- 💬 **Discussions**: Join discussions on [GitHub Discussions](https://github.com/nepton/Doulex.DomainDriven/discussions)
- 📧 **Email**: Contact the maintainers for enterprise support

## 🏆 Acknowledgments

- Inspired by Domain-Driven Design principles by Eric Evans
- Built on top of Entity Framework Core
- Thanks to all contributors and the .NET community

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

⭐ **Star this repository** if you find it helpful!

🔔 **Watch** for updates and new releases.
