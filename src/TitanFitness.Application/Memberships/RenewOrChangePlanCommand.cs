using FluentValidation;
using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.Memberships;

/// <summary>
/// Covers both "Renew" (Figure 5: same plan, membership has expired) and
/// "Change Plan" (Figure 6: a different plan, "At renewal" or "Immediately").
/// Both go through MembershipSchedulingRules.CreateFollowOn, which always
/// produces a brand new Membership rather than mutating the old one - see the
/// design note on that method for why.
/// </summary>
public record RenewOrChangePlanCommand(
    Guid CurrentMembershipId, Guid NewPlanId, bool EffectiveImmediately) : IRequest<MembershipDto>;

public class RenewOrChangePlanCommandValidator : AbstractValidator<RenewOrChangePlanCommand>
{
    public RenewOrChangePlanCommandValidator()
    {
        RuleFor(x => x.CurrentMembershipId).NotEmpty();
        RuleFor(x => x.NewPlanId).NotEmpty();
    }
}

public class RenewOrChangePlanCommandHandler : IRequestHandler<RenewOrChangePlanCommand, MembershipDto>
{
    private readonly IMembershipRepository _memberships;
    private readonly IPlanRepository _plans;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public RenewOrChangePlanCommandHandler(IMembershipRepository memberships, IPlanRepository plans,
        IDateTimeProvider clock, IUnitOfWork uow)
    {
        _memberships = memberships;
        _plans = plans;
        _clock = clock;
        _uow = uow;
    }

    public async Task<MembershipDto> Handle(RenewOrChangePlanCommand request, CancellationToken ct)
    {
        var current = await _memberships.GetByIdAsync(request.CurrentMembershipId, ct)
            ?? throw new DomainException("Membership not found.");
        var newPlan = await _plans.GetByIdAsync(request.NewPlanId, ct)
            ?? throw new DomainException("Plan not found.");
        if (!newPlan.IsPublished)
            throw new DomainException("Cannot move a member onto a plan that is not published.");

        var otherMemberships = (await _memberships.GetByMemberAsync(current.MemberId, ct))
            .Where(m => m.Id != current.Id)
            .ToList();

        var effect = request.EffectiveImmediately
            ? MembershipSchedulingRules.ChangeEffect.Immediately
            : MembershipSchedulingRules.ChangeEffect.AtRenewal;

        var followOn = MembershipSchedulingRules.CreateFollowOn(
            current, newPlan, effect, _clock.Today, _clock.UtcNow, otherMemberships);

        _memberships.Update(current); // may have been cancelled by CreateFollowOn (Immediately case)
        await _memberships.AddAsync(followOn, ct);
        await _uow.SaveChangesAsync(ct);

        return MembershipMapper.ToDto(followOn, _clock);
    }
}
