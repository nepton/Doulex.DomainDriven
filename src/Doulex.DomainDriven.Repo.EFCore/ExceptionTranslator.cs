using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Doulex.DomainDriven.Exceptions;

namespace Doulex.DomainDriven.Repo.EFCore;

/// <summary>
/// Translates Entity Framework Core exceptions to domain-driven exceptions
/// </summary>
public static class ExceptionTranslator
{
    /// <summary>
    /// Translates EF Core exceptions to domain exceptions
    /// </summary>
    /// <param name="exception">The original exception</param>
    /// <param name="operationType">The type of operation that caused the exception</param>
    /// <returns>Translated domain exception</returns>
    public static Exception TranslateException(Exception exception, string? operationType = null)
    {
        return exception switch
        {
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException efConcurrencyEx => 
                TranslateConcurrencyException(efConcurrencyEx, operationType),
            
            Microsoft.EntityFrameworkCore.DbUpdateException efUpdateEx => 
                TranslateUpdateException(efUpdateEx, operationType),
            
            InvalidOperationException invalidOpEx when IsTransactionException(invalidOpEx) => 
                TranslateTransactionException(invalidOpEx),
            
            TimeoutException timeoutEx => 
                TranslateTimeoutException(timeoutEx, operationType),
            
            DbException dbEx => 
                TranslateDatabaseException(dbEx, operationType),
            
            _ => exception
        };
    }

    private static Exceptions.DbUpdateConcurrencyException TranslateConcurrencyException(
        Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException efException,
        string? operationType)
    {
        var domainException = new Exceptions.DbUpdateConcurrencyException(
            "Database concurrency conflict occurred during update operation",
            efException)
        {
            OperationType = operationType,
            AffectedEntitiesCount = efException.Entries.Count
        };

        // Extract entity information from the first entry if available
        var firstEntry = efException.Entries.FirstOrDefault();
        if (firstEntry != null)
        {
            domainException.EntityType = firstEntry.Entity.GetType();
            domainException.EntityId = GetEntityId(firstEntry);

            // Try to extract version information
            var currentValues = firstEntry.CurrentValues;
            var originalValues = firstEntry.OriginalValues;

            if (currentValues != null)
                domainException.Context["CurrentValues"] = currentValues.ToObject();
            if (originalValues != null)
                domainException.Context["OriginalValues"] = originalValues.ToObject();
        }

        return domainException;
    }

    private static Exceptions.DbUpdateException TranslateUpdateException(
        Microsoft.EntityFrameworkCore.DbUpdateException efException,
        string? operationType)
    {
        // Check if it's a constraint violation
        if (IsConstraintViolation(efException))
        {
            return TranslateConstraintViolationException(efException, operationType);
        }

        // Check if it's a deadlock
        if (IsDeadlock(efException))
        {
            return TranslateDeadlockException(efException, operationType);
        }

        // Check if it's a validation error
        if (IsValidationError(efException))
        {
            return TranslateValidationException(efException, operationType);
        }

        // Generic update exception
        var domainException = new Exceptions.DbUpdateException(
            "Database update operation failed",
            efException)
        {
            OperationType = operationType,
            AffectedEntitiesCount = efException.Entries?.Count ?? 0
        };

        return domainException;
    }

    private static DbConstraintViolationException TranslateConstraintViolationException(
        Microsoft.EntityFrameworkCore.DbUpdateException efException, 
        string? operationType)
    {
        var domainException = new DbConstraintViolationException(
            "Database constraint violation occurred", 
            efException)
        {
            OperationType = operationType,
            AffectedEntitiesCount = efException.Entries?.Count ?? 0
        };

        // Analyze the inner exception to determine constraint type
        var innerException = efException.InnerException;
        if (innerException != null)
        {
            var message = innerException.Message.ToLowerInvariant();
            
            if (message.Contains("primary key") || message.Contains("pk_"))
            {
                domainException.ConstraintType = ConstraintType.PrimaryKey;
            }
            else if (message.Contains("foreign key") || message.Contains("fk_"))
            {
                domainException.ConstraintType = ConstraintType.ForeignKey;
            }
            else if (message.Contains("unique") || message.Contains("uq_"))
            {
                domainException.ConstraintType = ConstraintType.Unique;
            }
            else if (message.Contains("check") || message.Contains("ck_"))
            {
                domainException.ConstraintType = ConstraintType.Check;
            }
            else if (message.Contains("not null") || message.Contains("null"))
            {
                domainException.ConstraintType = ConstraintType.NotNull;
            }

            // Extract constraint name using regex
            var constraintNameMatch = Regex.Match(message, @"constraint\s+['""]?([^'"".\s]+)['""]?", RegexOptions.IgnoreCase);
            if (constraintNameMatch.Success)
            {
                domainException.ConstraintName = constraintNameMatch.Groups[1].Value;
            }

            // Extract table name
            var tableNameMatch = Regex.Match(message, @"table\s+['""]?([^'"".\s]+)['""]?", RegexOptions.IgnoreCase);
            if (tableNameMatch.Success)
            {
                domainException.TableName = tableNameMatch.Groups[1].Value;
            }
        }

        return domainException;
    }

    private static DbDeadlockException TranslateDeadlockException(
        Microsoft.EntityFrameworkCore.DbUpdateException efException, 
        string? operationType)
    {
        var domainException = new DbDeadlockException(
            "Database deadlock detected", 
            efException)
        {
            OperationType = operationType,
            AffectedEntitiesCount = efException.Entries?.Count ?? 0,
            DeadlockType = DeadlockType.Resource,
            DetectionTime = DateTime.UtcNow
        };

        return domainException;
    }

    private static DbValidationException TranslateValidationException(
        Microsoft.EntityFrameworkCore.DbUpdateException efException, 
        string? operationType)
    {
        var domainException = new DbValidationException(
            "Data validation failed during database operation", 
            efException)
        {
            OperationType = operationType,
            AffectedEntitiesCount = efException.Entries?.Count ?? 0
        };

        // Extract validation errors from entries if available
        if (efException.Entries != null)
        {
            foreach (var entry in efException.Entries)
            {
                domainException.EntityType = entry.Entity.GetType();
                domainException.EntityId = GetEntityId(entry);
                break; // Use first entry for now
            }
        }

        return domainException;
    }

    private static DbTransactionException TranslateTransactionException(InvalidOperationException exception)
    {
        var domainException = new DbTransactionException(
            "Database transaction operation failed", 
            exception);

        var message = exception.Message.ToLowerInvariant();
        if (message.Contains("transaction already started"))
        {
            domainException.FailedOperation = TransactionOperation.Begin;
            domainException.TransactionState = TransactionState.Active;
        }
        else if (message.Contains("transaction not supported"))
        {
            domainException.FailedOperation = TransactionOperation.Begin;
            domainException.TransactionState = TransactionState.NotStarted;
        }

        return domainException;
    }

    private static DbTimeoutException TranslateTimeoutException(TimeoutException exception, string? operationType)
    {
        var domainException = new DbTimeoutException(
            "Database operation timed out", 
            exception)
        {
            Command = operationType,
            TimeoutType = TimeoutType.CommandTimeout
        };

        return domainException;
    }

    private static Exception TranslateDatabaseException(DbException dbException, string? operationType)
    {
        // Check for specific database error codes
        var message = dbException.Message.ToLowerInvariant();
        
        if (message.Contains("timeout") || message.Contains("timed out"))
        {
            return new DbTimeoutException("Database operation timed out", dbException)
            {
                Command = operationType,
                TimeoutType = TimeoutType.CommandTimeout
            };
        }

        if (message.Contains("connection") || message.Contains("network"))
        {
            return new DbConnectionException("Database connection failed", dbException)
            {
                FailureReason = ConnectionFailureReason.NetworkUnreachable
            };
        }

        if (message.Contains("permission") || message.Contains("access denied"))
        {
            return new DbPermissionException("Insufficient database permissions", dbException)
            {
                Operation = GetDatabaseOperation(operationType)
            };
        }

        if (message.Contains("disk") || message.Contains("space"))
        {
            return new DbStorageException("Database storage error", dbException)
            {
                IssueType = StorageIssueType.InsufficientDiskSpace
            };
        }

        // Return generic database exception
        return new Exceptions.DbUpdateException("Database operation failed", dbException)
        {
            OperationType = operationType
        };
    }

    private static bool IsConstraintViolation(Microsoft.EntityFrameworkCore.DbUpdateException exception)
    {
        var message = exception.InnerException?.Message?.ToLowerInvariant() ?? string.Empty;
        return message.Contains("constraint") || 
               message.Contains("unique") || 
               message.Contains("primary key") || 
               message.Contains("foreign key") ||
               message.Contains("duplicate");
    }

    private static bool IsDeadlock(Microsoft.EntityFrameworkCore.DbUpdateException exception)
    {
        var message = exception.InnerException?.Message?.ToLowerInvariant() ?? string.Empty;
        return message.Contains("deadlock") || message.Contains("was deadlocked");
    }

    private static bool IsValidationError(Microsoft.EntityFrameworkCore.DbUpdateException exception)
    {
        var message = exception.InnerException?.Message?.ToLowerInvariant() ?? string.Empty;
        return message.Contains("validation") || message.Contains("invalid");
    }

    private static bool IsTransactionException(InvalidOperationException exception)
    {
        var message = exception.Message.ToLowerInvariant();
        return message.Contains("transaction");
    }

    private static object? GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        try
        {
            var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
            if (keyProperties?.Count == 1)
            {
                var keyProperty = keyProperties.First();
                return entry.Property(keyProperty.Name).CurrentValue;
            }
        }
        catch
        {
            // Ignore errors when extracting entity ID
        }
        return null;
    }

    private static DatabaseOperation GetDatabaseOperation(string? operationType)
    {
        return operationType?.ToLowerInvariant() switch
        {
            "insert" or "add" => DatabaseOperation.Insert,
            "update" => DatabaseOperation.Update,
            "delete" or "remove" => DatabaseOperation.Delete,
            "select" or "get" or "find" => DatabaseOperation.Select,
            _ => DatabaseOperation.Other
        };
    }
}
