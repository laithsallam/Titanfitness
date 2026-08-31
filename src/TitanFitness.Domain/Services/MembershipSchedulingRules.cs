using TitanFitness.Domain.Common;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Domain.Services;

/// <summary>
/// Rules that span more than one Membership (or a Membership plus a Plan) and
/// therefore cannot live inside the Membership aggregate itself - a domain
/// service operates on already-loaded aggregates handed to it by the
/// application layer, with no repository/DB access of its own, keeping the
/// domain layer free of infrastructure concerns.
/// </summary>
public static class MembershipSchedulingRules
{
    /// <summary>"A member must never hold two memberships covering the same day."</summary>
    public static void EnsureNoOverlap(IEnumerable<Membership> existingMemberships, DateRange candidatePeriod)
    {
        var conflict = existingMemberships
            .Where(m => !m.IsCancelled)
            .FirstOrDefault(m => m.Period.Overlaps(candidatePeriod));

        if (conflict is not null)
            throw new DomainException(
                $"Member already holds a membership covering {conflict.Period}; periods cannot overlap.");
    }

    public enum ChangeEffect { AtRenewal, Immediately }

    /// <summary>
    /// Produces the follow-on membership for a Renew or Change Plan action.
    /// This never edits the existing Membership row - "cancellation is final"
    /// and AgreedTerms is immutable, so the only way to move a member onto new
    /// terms is a brand new Membership aggregate, scheduled so it never
    /// overlaps the one it replaces.
    /// </summary>
    public static Membership CreateFollowOn(
        Membership current,
        Plan newPlan,
        ChangeEffect effect,
        DateOnly today,
        DateTime nowUtc,
        IEnumerable<Membership> allOtherMembershipsForMember /* must exclude `current` itself */)
    {
        if (current.IsCancelled)
            throw new DomainException("Cancelled memberships cannot be renewed.");

        DateOnly newStart;
        if (effect == ChangeEffect.Immediately)
        {
            current.Cancel(today);
            newStart = today;
        }
        else
        {
            newStart = current.Period.End.AddDays(1);
        }

        var candidateEnd = newStart.AddMonths(newPlan.DurationInMonths);
        EnsureNoOverlap(allOtherMembershipsForMember, DateRange.Create(newStart, candidateEnd));

        return Membership.PurchaseFollowOn(current.MemberId, newPlan, newStart, nowUtc);
    }
}
