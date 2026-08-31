using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Memberships;

namespace TitanFitness.Application.Memberships;

internal static class MembershipMapper
{
    public static MembershipDto ToDto(Membership m, IDateTimeProvider clock) => new(
        m.Id, m.MemberId, m.PlanId, m.Period.Start, m.Period.End,
        m.GetStatus(clock.Today).ToString(), m.IsCancelled,
        new AgreedTermsDto(m.Terms.PricePaid, m.Terms.DurationInMonths, m.Terms.MaxFreezeDays,
            m.Terms.MaxNumberOfFreezes, m.Terms.GuestPassQuota, m.Terms.AccessScope.ToString()),
        m.RemainingFreezeDays, m.RemainingFreezeCount, m.RemainingGuestPasses,
        m.Freezes.Select(f => new FreezeDto(f.Id, f.Period.Start, f.Period.End, f.Reason.ToString(), f.Notes)).ToList(),
        m.GuestPasses.Select(g => new GuestPassDto(g.Id, g.IssuedOn, g.UsedOn, g.GuestName)).ToList());
}
