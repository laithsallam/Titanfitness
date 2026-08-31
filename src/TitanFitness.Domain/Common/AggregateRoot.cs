namespace TitanFitness.Domain.Common;

/// <summary>
/// Marks the entry point of a consistency boundary. Only aggregate roots are
/// fetched from repositories directly; everything inside (child entities,
/// value objects) is reached only by navigating through the root, and every
/// invariant that spans objects inside the boundary is enforced here.
///
/// Domain events are collected but not dispatched by this template - wiring
/// a dispatcher (e.g. via SaveChanges interceptor + MediatR notifications) is
/// straightforward to add later without touching this base class.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
