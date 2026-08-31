using FluentValidation;
using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Application.Plans;

/// <summary>
/// "Changes to this plan apply to new purchases only." This handler edits the
/// Plan aggregate's own fields and nothing else - it never touches any
/// Membership, because AgreedTerms already froze those terms at purchase time.
/// ActiveMembershipsOnThisPlan is returned purely as an informational count
/// for the UI banner ("412 active memberships keep the terms they were sold"),
/// not as something the update logic reacts to.
/// </summary>
public record UpdatePlanCommand(
    Guid Id, string Name, decimal Price, int DurationInMonths,
    int MaxFreezeDays, int MaxNumberOfFreezes, int GuestPassQuota,
    AccessScope AccessScope) : IRequest<PlanUpdateResultDto>;

public record PlanUpdateResultDto(PlanDto Plan, int ActiveMembershipsOnThisPlan);

public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationInMonths).GreaterThan(0);
        RuleFor(x => x.MaxFreezeDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxNumberOfFreezes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GuestPassQuota).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, PlanUpdateResultDto>
{
    private readonly IPlanRepository _plans;
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public UpdatePlanCommandHandler(IPlanRepository plans, IMembershipRepository memberships,
        IDateTimeProvider clock, IUnitOfWork uow)
    {
        _plans = plans;
        _memberships = memberships;
        _clock = clock;
        _uow = uow;
    }

    public async Task<PlanUpdateResultDto> Handle(UpdatePlanCommand request, CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(request.Id, ct) ?? throw new DomainException("Plan not found.");

        plan.UpdateDetails(request.Name, request.Price, request.DurationInMonths,
            request.MaxFreezeDays, request.MaxNumberOfFreezes, request.GuestPassQuota, request.AccessScope);

        _plans.Update(plan);
        await _uow.SaveChangesAsync(ct);

        var activeCount = await _memberships.CountActiveByPlanAsync(plan.Id, _clock.Today, ct);
        return new PlanUpdateResultDto(CreatePlanCommandHandler.ToDto(plan), activeCount);
    }
}
