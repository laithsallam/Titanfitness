using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Memberships;

/// <summary>
/// A single guest visit slot carried by a Membership. Like Freeze, this is a
/// child entity: its "has the quota been used up" rule can only be enforced by
/// counting siblings inside the same Membership, so it is never created or
/// mutated except through the parent aggregate.
/// </summary>
public sealed class GuestPass : Entity<Guid>
{
    public DateOnly IssuedOn { get; private set; }
    public DateOnly? UsedOn { get; private set; }
    public string? GuestName { get; private set; }

    public bool IsUsed => UsedOn.HasValue;

    private GuestPass() { }

    internal GuestPass(Guid id, DateOnly issuedOn) : base(id)
    {
        IssuedOn = issuedOn;
    }

    internal void Redeem(string? guestName, DateOnly usedOn)
    {
        if (IsUsed)
            throw new DomainException("This guest pass has already been used.");
        Guard.AgainstTooLong(guestName, nameof(GuestName), 100);
        GuestName = guestName;
        UsedOn = usedOn;
    }
}
