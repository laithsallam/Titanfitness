using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;

namespace TitanFitness.Domain.Services;

/// <summary>
/// Decides whether a member walking up to a branch gets in. This spans Member
/// (home branch), Membership (status + scope) and the branch being entered -
/// three different aggregates - so it cannot live on any single one of them.
/// The application handler loads the member and their memberships and passes
/// them in; this service contains only the decision logic, no I/O.
/// </summary>
public static class CheckInEligibilityService
{
    public static CheckIn Evaluate(Member member, IEnumerable<Membership> memberships, Guid branchId, DateTime nowUtc, DateOnly today)
    {
        var granting = memberships.FirstOrDefault(m => m.GrantsAccessTo(branchId, member.HomeBranchId, today));

        return granting is not null
            ? CheckIn.Admit(member.Id, branchId, nowUtc)
            : CheckIn.Refuse(member.Id, branchId, nowUtc, DetermineRefusalReason(memberships, branchId, member.HomeBranchId, today));
    }

    private static string DetermineRefusalReason(IEnumerable<Membership> memberships, Guid branchId, Guid homeBranchId, DateOnly today)
    {
        var membershipList = memberships.ToList();
        if (membershipList.Count == 0)
            return "No membership on file.";

        var mostRelevant = membershipList
            .OrderByDescending(m => m.Period.End)
            .First();

        var status = mostRelevant.GetStatus(today);
        return status switch
        {
            MembershipStatus.Pending => "Membership has not started yet.",
            MembershipStatus.Expired => "Membership has expired.",
            MembershipStatus.Cancelled => "Membership was cancelled.",
            MembershipStatus.Frozen => "Membership is currently frozen.",
            MembershipStatus.Active => "Membership does not grant access to this branch.",
            _ => "Not eligible for entry."
        };
    }
}
