namespace TitanFitness.Domain.Memberships;

/// <summary>
/// The status shown to staff. Only Cancelled is a fact stored forever; every
/// other value (Pending/Active/Frozen/Expired) is derived on demand from the
/// current date and the freeze list - see Membership.GetStatus. Storing those
/// as a mutable field would require a background job ticking every membership
/// over at midnight; deriving them means the truth is always correct the
/// instant you ask, with no job to forget to run.
/// </summary>
public enum MembershipStatus
{
    Pending = 0,
    Active = 1,
    Frozen = 2,
    Expired = 3,
    Cancelled = 4
}
