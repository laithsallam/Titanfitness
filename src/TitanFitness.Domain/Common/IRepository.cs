namespace TitanFitness.Domain.Common;

/// <summary>Minimal repository contract - one per aggregate root, never per entity/VO inside it.</summary>
public interface IRepository<TAggregate, TId> where TAggregate : AggregateRoot<TId> where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task AddAsync(TAggregate aggregate, CancellationToken ct = default);
    void Update(TAggregate aggregate);
    void Remove(TAggregate aggregate);
}

/// <summary>Commits everything changed across repositories in this request as one transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
