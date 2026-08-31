using TitanFitness.Domain.Common;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Domain.Memberships;

/// <summary>
/// A frozen snapshot of a Plan's terms at the moment of purchase: price,
/// duration, freeze allowance, guest passes, and scope.
///
/// This is the single most important type in the whole model, because it is
/// the direct answer to the business rule that breaks the "obvious" design:
/// "when a plan changes, members who already hold a membership keep the terms
/// they were sold." If Membership simply held a PlanId and read limits off the
/// live Plan, every past member's rights would silently mutate whenever
/// management edited a price or a freeze cap. Making AgreedTerms an immutable
/// value object - copied field-by-field at purchase and never touched again -
/// means a Membership's rights are fixed the instant it exists, independent of
/// whatever happens to the Plan catalogue afterwards. The Membership keeps
/// PlanId only as a reporting/traceability pointer; every rule the aggregate
/// actually enforces (freeze caps, guest pass quota, access scope) reads from
/// AgreedTerms, never from Plan.
/// </summary>
public sealed class AgreedTerms : ValueObject
{
    public decimal PricePaid { get; }
    public int DurationInMonths { get; }
    public int MaxFreezeDays { get; }
    public int MaxNumberOfFreezes { get; }
    public int GuestPassQuota { get; }
    public AccessScope AccessScope { get; }

    private AgreedTerms(decimal pricePaid, int durationInMonths, int maxFreezeDays,
        int maxNumberOfFreezes, int guestPassQuota, AccessScope accessScope)
    {
        PricePaid = pricePaid;
        DurationInMonths = durationInMonths;
        MaxFreezeDays = maxFreezeDays;
        MaxNumberOfFreezes = maxNumberOfFreezes;
        GuestPassQuota = guestPassQuota;
        AccessScope = accessScope;
    }

    /// <summary>The only way to obtain AgreedTerms: copy a Plan's current fields, permanently.</summary>
    public static AgreedTerms CopyFrom(Plan plan) => new(
        plan.Price,
        plan.DurationInMonths,
        plan.MaxFreezeDays,
        plan.MaxNumberOfFreezes,
        plan.GuestPassQuota,
        plan.AccessScope);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PricePaid;
        yield return DurationInMonths;
        yield return MaxFreezeDays;
        yield return MaxNumberOfFreezes;
        yield return GuestPassQuota;
        yield return AccessScope;
    }
}
