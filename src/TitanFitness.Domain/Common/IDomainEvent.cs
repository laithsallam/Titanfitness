namespace TitanFitness.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
