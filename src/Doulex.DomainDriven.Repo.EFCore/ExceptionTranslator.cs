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
            
            InvalidOperationException invalidOpEx =>
                TranslateTransactionException(invalidOpEx),
            
            TimeoutException timeoutEx => 
                TranslateTimeoutException(timeoutEx, operationType),
            
            DbException dbEx => 
                TranslateDatabaseException(dbEx, operationType),
            
            _ => exception
        };
    }

    private static Exceptions.RepoUpdateConcurrencyException TranslateConcurrencyException(
        Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException efException,
        string? operationType)
    {
        var domainException = new Exceptions.RepoUpdateConcurrencyException(
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

    private static Exceptions.RepoUpdateException TranslateUpdateException(
        Microsoft.EntityFrameworkCore.DbUpdateException efException,
        string? operationType)
    {
        // Generic update exception - no unreliable string-based detection
        var domainException = new Exceptions.RepoUpdateException(
            "Database update operation failed",
            efException)
        {
            OperationType = operationType,
            AffectedEntitiesCount = efException.Entries?.Count ?? 0
        };

        return domainException;
    }



    private static RepoTransactionException TranslateTransactionException(InvalidOperationException exception)
    {
        var domainException = new RepoTransactionException(
            "Database transaction operation failed",
            exception);

        // No unreliable message-based detection - keep it simple and reliable
        return domainException;
    }

    private static RepoTimeoutException TranslateTimeoutException(TimeoutException exception, string? operationType)
    {
        var domainException = new RepoTimeoutException(
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
        // No unreliable message-based detection - all DbExceptions become generic database exceptions
        // This is more reliable and works across all database providers and languages
        return new Exceptions.RepoUpdateException("Database operation failed", dbException)
        {
            OperationType = operationType
        };
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


}
