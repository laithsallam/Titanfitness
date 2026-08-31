using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Memberships;

/// <summary>
/// A child entity of Membership - it has an identity (a member can have several
/// freezes over time and staff need to reference a specific one) but no
/// lifecycle or repository of its own. It only ever exists, is created, and is
/// validated inside a Membership aggregate, because every rule about it
/// (does it fit the remaining freeze-day budget, does it overlap another
/// freeze, does it run past the membership) can only be checked by looking at
/// the whole membership at once.
/// </summary>
public sealed class Freeze : Entity<Guid>
{
    public DateRange Period { get; private set; } = null!;
    public FreezeReason Reason { get; private set; }
    public string? Notes { get; private set; }
    public DateTime RequestedOnUtc { get; private set; }

    private Freeze() { }

    internal Freeze(Guid id, DateRange period, FreezeReason reason, string? notes, DateTime requestedOnUtc)
        : base(id)
    {
        Period = period;
        Reason = reason;
        Notes = notes;
        RequestedOnUtc = requestedOnUtc;
    }

    public int DurationDays => Period.TotalDays;
}
