namespace Doulex.DomainDriven.Repo.FS;

internal class AggregateMeta
{
    public AggregateMeta(object aggregateRoot)
    {
        AggregateRoot = aggregateRoot;
    }

    public object AggregateRoot { get; set; }
}
