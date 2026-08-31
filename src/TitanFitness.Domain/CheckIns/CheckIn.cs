using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.CheckIns;

/// <summary>
/// A single attempt to enter a branch, granted or refused, with a reason when
/// refused. Standalone aggregate: "every attempt is recorded... and an
/// established member accumulates a great many of these," so a CheckIn's
/// entire job is to exist as an immutable append-only fact - it has no
/// invariant that spans other CheckIns and nothing about it is ever edited
/// after creation. The eligibility decision itself is cross-aggregate
/// (Member + Membership + Branch) and is computed by CheckInEligibilityService
/// before this record is created; CheckIn just stores the verdict.
/// </summary>
public sealed class CheckIn : AggregateRoot<Guid>
{
    public Guid MemberId { get; private set; }
    public Guid BranchId { get; private set; }
    public DateTime CheckInDateTimeUtc { get; private set; }
    public CheckInResult Result { get; private set; }
    public string? RefusalReason { get; private set; }

    private CheckIn() { }

    private CheckIn(Guid id, Guid memberId, Guid branchId, DateTime checkInDateTimeUtc,
        CheckInResult result, string? refusalReason) : base(id)
    {
        MemberId = memberId;
        BranchId = branchId;
        CheckInDateTimeUtc = checkInDateTimeUtc;
        Result = result;
        RefusalReason = refusalReason;
    }

    public static CheckIn Admit(Guid memberId, Guid branchId, DateTime checkInDateTimeUtc) =>
        new(Guid.NewGuid(), memberId, branchId, checkInDateTimeUtc, CheckInResult.Admitted, null);

    public static CheckIn Refuse(Guid memberId, Guid branchId, DateTime checkInDateTimeUtc, string reason)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 100);
        return new CheckIn(Guid.NewGuid(), memberId, branchId, checkInDateTimeUtc, CheckInResult.Refused, reason);
    }
}
