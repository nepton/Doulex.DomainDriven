namespace Doulex.DomainDriven;

public interface IStrategy
{
    Task ExecuteAsync(Func<Task> operation);
}
