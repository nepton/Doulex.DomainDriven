using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Doulex.DomainDriven.Repo.EFCore;

public class EntityFrameworkCoreStrategy : IStrategy
{
    private readonly IExecutionStrategy _createExecutionStrategy;

    public EntityFrameworkCoreStrategy(IExecutionStrategy createExecutionStrategy)
    {
        _createExecutionStrategy = createExecutionStrategy;
    }

    public Task ExecuteAsync(Func<Task> operation)
    {
        return _createExecutionStrategy.ExecuteAsync(operation);
    }
}
