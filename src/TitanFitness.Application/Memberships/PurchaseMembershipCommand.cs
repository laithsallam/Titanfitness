using FluentValidation;
using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Plans;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.Memberships;

public record PurchaseMembershipCommand(Guid MemberId, Guid PlanId, DateOnly StartDate) : IRequest<MembershipDto>;

public class PurchaseMembershipCommandValidator : AbstractValidator<PurchaseMembershipCommand>
{
    public PurchaseMembershipCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();
    }
}

public class PurchaseMembershipCommandHandler : IRequestHandler<PurchaseMembershipCommand, MembershipDto>
{
    private readonly IMemberRepository _members;
    private readonly IPlanRepository _plans;
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public PurchaseMembershipCommandHandler(IMemberRepository members, IPlanRepository plans,
        IMembershipRepository memberships, IDateTimeProvider clock, IUnitOfWork uow)
    {
        _members = members;
        _plans = plans;
        _memberships = memberships;
        _clock = clock;
        _uow = uow;
    }

    public async Task<MembershipDto> Handle(PurchaseMembershipCommand request, CancellationToken ct)
    {
        var member = await _members.GetByIdAsync(request.MemberId, ct)
            ?? throw new DomainException("Member not found.");
        var plan = await _plans.GetByIdAsync(request.PlanId, ct)
            ?? throw new DomainException("Plan not found.");

        var membership = Membership.Purchase(member.Id, plan, request.StartDate, _clock.UtcNow);

        var existing = await _memberships.GetByMemberAsync(member.Id, ct);
        MembershipSchedulingRules.EnsureNoOverlap(existing, membership.Period);

        await _memberships.AddAsync(membership, ct);
        await _uow.SaveChangesAsync(ct);

        return MembershipMapper.ToDto(membership, _clock);
    }
}
