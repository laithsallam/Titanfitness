using TitanFitness.Domain.Common;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Domain.Memberships;

/// <summary>
/// The thing that actually grants access. Owns its AgreedTerms snapshot plus
/// every Freeze and GuestPass taken against it, because the invariants that
/// matter here - "does this freeze fit inside the remaining budget", "has the
/// guest-pass quota been used up", "does the freeze push the end date out
/// correctly" - can only be checked correctly by looking at the whole set at
/// once inside a single transaction. That is the textbook reason to pull them
/// inside this aggregate's boundary instead of giving Freeze/GuestPass their
/// own repositories.
///
/// What this aggregate deliberately does NOT know: whether the member holds
/// another overlapping membership, or whether check-in/booking should be
/// allowed elsewhere. Those span multiple aggregates and live in domain
/// services / application handlers that load the relevant aggregates and
/// coordinate them - see Domain/Services/MembershipSchedulingRules.
/// </summary>
public sealed class Membership : AggregateRoot<Guid>
{
    private readonly List<Freeze> _freezes = new();
    private readonly List<GuestPass> _guestPasses = new();

    public Guid MemberId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateTime PurchaseDateUtc { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public AgreedTerms Terms { get; private set; } = null!;
    public bool IsCancelled { get; private set; }
    public DateOnly? CancelledOn { get; private set; }

    public IReadOnlyCollection<Freeze> Freezes => _freezes.AsReadOnly();
    public IReadOnlyCollection<GuestPass> GuestPasses => _guestPasses.AsReadOnly();

    private Membership() { }

    private Membership(Guid id, Guid memberId, Guid planId, DateTime purchaseDateUtc,
        DateRange period, AgreedTerms terms) : base(id)
    {
        MemberId = memberId;
        PlanId = planId;
        PurchaseDateUtc = purchaseDateUtc;
        Period = period;
        Terms = terms;
        IsCancelled = false;
    }

    /// <summary>
    /// Buy a plan. Start may be in the future - "a membership may be bought to
    /// begin on a future date, and until that date arrives it exists but
    /// grants nothing" - which GetStatus expresses by returning Pending.
    /// </summary>
    public static Membership Purchase(Guid memberId, Plan plan, DateOnly startDate, DateTime purchaseDateUtc)
    {
        if (!plan.IsPublished)
            throw new DomainException("Cannot purchase a plan that is not published.");

        var terms = AgreedTerms.CopyFrom(plan);
        var endDate = startDate.AddMonths(terms.DurationInMonths);
        var period = DateRange.Create(startDate, endDate);

        return new Membership(Guid.NewGuid(), memberId, plan.Id, purchaseDateUtc, period, terms);
    }

    /// <summary>
    /// Used by renewal/change-plan flows to create the follow-on membership
    /// (a brand new aggregate, never a mutation of the old one - see
    /// MembershipSchedulingRules.CreateFollowOn for why renewal never edits
    /// the existing row).
    /// </summary>
    internal static Membership PurchaseFollowOn(Guid memberId, Plan newPlan, DateOnly startDate, DateTime purchaseDateUtc)
        => Purchase(memberId, newPlan, startDate, purchaseDateUtc);

    // ---- Status -------------------------------------------------------

    public MembershipStatus GetStatus(DateOnly asOf)
    {
        if (IsCancelled) return MembershipStatus.Cancelled;
        if (IsFrozenOn(asOf)) return MembershipStatus.Frozen;
        if (asOf < Period.Start) return MembershipStatus.Pending;
        if (asOf > Period.End) return MembershipStatus.Expired;
        return MembershipStatus.Active;
    }

    public bool IsFrozenOn(DateOnly date) => _freezes.Any(f => f.Period.Contains(date));

    /// <summary>
    /// "Entry is granted only when they hold a membership that has begun, has
    /// not ended, is not frozen, and reaches the branch they are standing in."
    /// </summary>
    public bool GrantsAccessTo(Guid branchId, Guid memberHomeBranchId, DateOnly asOf)
    {
        if (GetStatus(asOf) != MembershipStatus.Active) return false;
        return Terms.AccessScope == Common.AccessScope.AllBranches || branchId == memberHomeBranchId;
    }

    // ---- Freezing -------------------------------------------------------

    /// <summary>
    /// "A freeze cannot begin in the past and cannot run past the end of the
    /// membership." "The agreed terms cap both the total days that may be
    /// frozen and how many separate freezes are allowed." "When it resumes,
    /// its end date moves forward by exactly the days it spent frozen, so
    /// nothing is lost" - applied immediately, since the new end date is a
    /// deterministic function of the freeze length, not of some later event.
    /// </summary>
    public Freeze RequestFreeze(DateOnly startDate, int durationMonths, FreezeReason reason,
        string? notes, DateOnly today, DateTime requestedOnUtc)
    {
        if (IsCancelled)
            throw new DomainException("A cancelled membership cannot be frozen.");
        if (startDate < today)
            throw new DomainException("A freeze cannot begin in the past.");
        if (durationMonths <= 0)
            throw new DomainException("Freeze duration must be at least one month.");
        Guard.AgainstTooLong(notes, nameof(notes), 200);

        var endDate = startDate.AddMonths(durationMonths);
        if (endDate > Period.End)
            throw new DomainException("A freeze cannot run past the end of the membership.");

        var candidatePeriod = DateRange.Create(startDate, endDate);
        if (_freezes.Any(f => f.Period.Overlaps(candidatePeriod)))
            throw new DomainException("This freeze overlaps an existing freeze on this membership.");

        if (_freezes.Count + 1 > Terms.MaxNumberOfFreezes)
            throw new DomainException(
                $"This membership already used its maximum of {Terms.MaxNumberOfFreezes} freeze(s).");

        var totalFrozenDays = _freezes.Sum(f => f.DurationDays) + candidatePeriod.TotalDays;
        if (totalFrozenDays > Terms.MaxFreezeDays)
            throw new DomainException(
                $"This freeze would use {totalFrozenDays} of the {Terms.MaxFreezeDays} freeze day(s) allowed.");

        var freeze = new Freeze(Guid.NewGuid(), candidatePeriod, reason, notes, requestedOnUtc);
        _freezes.Add(freeze);

        // The membership resumes having lost nothing: push the end date out
        // by exactly the days spent frozen.
        Period = Period.ExtendEndBy(candidatePeriod.TotalDays);

        return freeze;
    }

    public int RemainingFreezeDays => Terms.MaxFreezeDays - _freezes.Sum(f => f.DurationDays);
    public int RemainingFreezeCount => Terms.MaxNumberOfFreezes - _freezes.Count;

    // ---- Guest passes ---------------------------------------------------

    public GuestPass IssueGuestPass(DateOnly issuedOn)
    {
        if (IsCancelled)
            throw new DomainException("A cancelled membership cannot issue guest passes.");
        if (_guestPasses.Count >= Terms.GuestPassQuota)
            throw new DomainException(
                $"This membership's guest pass quota of {Terms.GuestPassQuota} has been used up.");

        var pass = new GuestPass(Guid.NewGuid(), issuedOn);
        _guestPasses.Add(pass);
        return pass;
    }

    public void RedeemGuestPass(Guid guestPassId, string? guestName, DateOnly usedOn)
    {
        var pass = _guestPasses.FirstOrDefault(p => p.Id == guestPassId)
            ?? throw new DomainException("Guest pass not found on this membership.");
        pass.Redeem(guestName, usedOn);
    }

    public int RemainingGuestPasses => Terms.GuestPassQuota - _guestPasses.Count;

    // ---- Ending -----------------------------------------------------------

    /// <summary>"Cancellation is final: it cannot be resumed, renewed from, or reversed."</summary>
    public void Cancel(DateOnly cancelledOn)
    {
        if (IsCancelled)
            throw new DomainException("This membership is already cancelled.");
        IsCancelled = true;
        CancelledOn = cancelledOn;
        // Shorten the period so GetStatus reports Cancelled/Expired consistently
        // for any date from here on, without needing IsCancelled everywhere.
        if (cancelledOn < Period.End)
            Period = DateRange.Create(Period.Start, cancelledOn);
    }
}
