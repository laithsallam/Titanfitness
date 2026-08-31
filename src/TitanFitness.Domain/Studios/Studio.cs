using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Studios;

/// <summary>
/// A bookable room inside a branch. Standalone aggregate (not a child of Branch)
/// so that ClassSession - which needs a studio's capacity to enforce
/// "capacity limit cannot exceed the studio's capacity" - can reference it by
/// ID and look it up independently, without pulling the whole Branch aggregate
/// through the back door.
/// </summary>
public sealed class Studio : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public Guid BranchId { get; private set; }
    public int Capacity { get; private set; }

    private Studio() { }

    private Studio(Guid id, string name, Guid branchId, int capacity) : base(id)
    {
        Name = name;
        BranchId = branchId;
        Capacity = capacity;
    }

    public static Studio Create(string name, Guid branchId, int capacity)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 50);
        Guard.AgainstNegativeOrZero(capacity, nameof(Capacity));
        return new Studio(Guid.NewGuid(), name, branchId, capacity);
    }

    public void UpdateDetails(string name, int capacity)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 50);
        Guard.AgainstNegativeOrZero(capacity, nameof(Capacity));
        Name = name;
        Capacity = capacity;
    }
}
