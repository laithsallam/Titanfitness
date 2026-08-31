using TitanFitness.Domain.Common;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using Xunit;

namespace TitanFitness.Domain.Tests;

public class MembershipTests
{
    private static Plan PublishedPlan(int durationMonths = 12, int maxFreezeDays = 60, int maxFreezes = 3, int guestPasses = 5) =>
        CreatePlan(durationMonths, maxFreezeDays, maxFreezes, guestPasses);

    private static Plan CreatePlan(int durationMonths, int maxFreezeDays, int maxFreezes, int guestPasses)
    {
        var plan = Plan.Create("Annual Pro", 899.00m, durationMonths, maxFreezeDays, maxFreezes, guestPasses, AccessScope.AllBranches);
        plan.Publish();
        return plan;
    }

    [Fact]
    public void Purchase_copies_plan_terms_into_agreed_terms_snapshot()
    {
        var plan = PublishedPlan();
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        Assert.Equal(plan.Price, membership.Terms.PricePaid);
        Assert.Equal(plan.MaxFreezeDays, membership.Terms.MaxFreezeDays);
        Assert.Equal(new DateOnly(2027, 1, 1), membership.Period.End);
    }

    [Fact]
    public void Updating_the_plan_after_purchase_does_not_change_existing_membership_terms()
    {
        var plan = PublishedPlan(maxFreezeDays: 60);
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        // Management cuts the plan's freeze allowance from 60 to 30.
        plan.UpdateDetails("Annual Pro", plan.Price, plan.DurationInMonths, 30,
            plan.MaxNumberOfFreezes, plan.GuestPassQuota, plan.AccessScope);

        Assert.Equal(60, membership.Terms.MaxFreezeDays); // unaffected - the whole point of AgreedTerms
    }

    [Fact]
    public void Freeze_extends_end_date_by_exactly_the_frozen_days_so_nothing_is_lost()
    {
        var plan = PublishedPlan();
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 1, 1), DateTime.UtcNow);
        var originalEnd = membership.Period.End;

        membership.RequestFreeze(new DateOnly(2026, 3, 1), 2, FreezeReason.ExtendedTravel, null,
            today: new DateOnly(2026, 2, 1), requestedOnUtc: DateTime.UtcNow);

        var frozenDays = membership.Freezes.Single().DurationDays;
        Assert.Equal(originalEnd.AddDays(frozenDays), membership.Period.End);
    }

    [Fact]
    public void Freeze_cannot_exceed_the_agreed_terms_max_freeze_days()
    {
        var plan = PublishedPlan(maxFreezeDays: 10, maxFreezes: 5);
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        Assert.Throws<DomainException>(() =>
            membership.RequestFreeze(new DateOnly(2026, 2, 1), 1, FreezeReason.Injury, null,
                today: new DateOnly(2026, 1, 15), requestedOnUtc: DateTime.UtcNow));
    }

    [Fact]
    public void Freeze_cannot_begin_in_the_past()
    {
        var plan = PublishedPlan();
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        Assert.Throws<DomainException>(() =>
            membership.RequestFreeze(new DateOnly(2026, 1, 1), 1, FreezeReason.Injury, null,
                today: new DateOnly(2026, 1, 15), requestedOnUtc: DateTime.UtcNow));
    }

    [Fact]
    public void Guest_pass_quota_cannot_be_exceeded()
    {
        var plan = PublishedPlan(guestPasses: 1);
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        membership.IssueGuestPass(new DateOnly(2026, 1, 10));

        Assert.Throws<DomainException>(() => membership.IssueGuestPass(new DateOnly(2026, 1, 11)));
    }

    [Fact]
    public void Cancelling_twice_is_rejected_because_cancellation_is_final()
    {
        var plan = PublishedPlan();
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 1, 1), DateTime.UtcNow);
        membership.Cancel(new DateOnly(2026, 3, 1));

        Assert.Throws<DomainException>(() => membership.Cancel(new DateOnly(2026, 3, 5)));
    }

    [Fact]
    public void GetStatus_reports_pending_before_start_and_active_within_period()
    {
        var plan = PublishedPlan();
        var membership = Membership.Purchase(Guid.NewGuid(), plan, new DateOnly(2026, 6, 1), DateTime.UtcNow);

        Assert.Equal(MembershipStatus.Pending, membership.GetStatus(new DateOnly(2026, 5, 1)));
        Assert.Equal(MembershipStatus.Active, membership.GetStatus(new DateOnly(2026, 6, 15)));
        Assert.Equal(MembershipStatus.Expired, membership.GetStatus(new DateOnly(2027, 7, 1)));
    }
}
