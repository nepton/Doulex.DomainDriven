namespace Doulex.DomainDriven.Repo.FileSystem;

internal class AggregateMeta
{
    public AggregateMeta(object aggregateRoot)
    {
        AggregateRoot = aggregateRoot;
    }

    public object AggregateRoot { get; set; }
}
