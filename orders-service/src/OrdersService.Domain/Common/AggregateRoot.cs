namespace OrdersService.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
{
    protected AggregateRoot() : base() { }

    protected AggregateRoot(TId id) : base(id) { }
}
