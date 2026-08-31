using FluentValidation;
using MediatR;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Application.Plans;

public record PublishPlanCommand(Guid Id) : IRequest<PlanDto>;

public class PublishPlanCommandValidator : AbstractValidator<PublishPlanCommand>
{
    public PublishPlanCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class PublishPlanCommandHandler : IRequestHandler<PublishPlanCommand, PlanDto>
{
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;
    public PublishPlanCommandHandler(IPlanRepository plans, IUnitOfWork uow) { _plans = plans; _uow = uow; }

    public async Task<PlanDto> Handle(PublishPlanCommand request, CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(request.Id, ct) ?? throw new DomainException("Plan not found.");
        plan.Publish();
        _plans.Update(plan);
        await _uow.SaveChangesAsync(ct);
        return CreatePlanCommandHandler.ToDto(plan);
    }
}

public record RetirePlanCommand(Guid Id) : IRequest<PlanDto>;

public class RetirePlanCommandValidator : AbstractValidator<RetirePlanCommand>
{
    public RetirePlanCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class RetirePlanCommandHandler : IRequestHandler<RetirePlanCommand, PlanDto>
{
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;
    public RetirePlanCommandHandler(IPlanRepository plans, IUnitOfWork uow) { _plans = plans; _uow = uow; }

    public async Task<PlanDto> Handle(RetirePlanCommand request, CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(request.Id, ct) ?? throw new DomainException("Plan not found.");
        plan.Retire();
        _plans.Update(plan);
        await _uow.SaveChangesAsync(ct);
        return CreatePlanCommandHandler.ToDto(plan);
    }
}
