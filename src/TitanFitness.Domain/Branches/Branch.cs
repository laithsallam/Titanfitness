using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Branches;

/// <summary>
/// A physical location. Kept deliberately thin: Studios, Trainers, Members and
/// CheckIns all reference a BranchId rather than living inside this aggregate,
/// because each of those has its own independent lifecycle and is looked up on
/// its own (e.g. "check this member into branch X" never needs to load every
/// studio and trainer at that branch). Modeling Branch as a small, standalone
/// root keeps those cross-references cheap (ID only) instead of forcing every
/// write anywhere in the chain through one giant Branch aggregate.
/// </summary>
public sealed class Branch : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Address { get; private set; }
    public TimeRange OperatingHours { get; private set; } = null!;

    private Branch() { }

    private Branch(Guid id, string name, string? address, TimeRange operatingHours) : base(id)
    {
        Name = name;
        Address = address;
        OperatingHours = operatingHours;
    }

    public static Branch Create(string name, string? address, TimeRange operatingHours)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 50);
        Guard.AgainstTooLong(address, nameof(Address), 200);
        return new Branch(Guid.NewGuid(), name, address, operatingHours);
    }

    public void UpdateDetails(string name, string? address, TimeRange operatingHours)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 50);
        Guard.AgainstTooLong(address, nameof(Address), 200);
        Name = name;
        Address = address;
        OperatingHours = operatingHours;
    }

    public bool IsOpenAt(TimeOnly time) => OperatingHours.Contains(time);
}
