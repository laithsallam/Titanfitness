using FluentValidation;
using MediatR;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Application.Plans;

public record CreatePlanCommand(
    string Name, decimal Price, int DurationInMonths,
    int MaxFreezeDays, int MaxNumberOfFreezes, int GuestPassQuota,
    AccessScope AccessScope) : IRequest<PlanDto>;

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationInMonths).GreaterThan(0);
        RuleFor(x => x.MaxFreezeDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxNumberOfFreezes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GuestPassQuota).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AccessScope).IsInEnum();
    }
}

public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, PlanDto>
{
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;
    public CreatePlanCommandHandler(IPlanRepository plans, IUnitOfWork uow)
    {
        _plans = plans;
        _uow = uow;
    }

    public async Task<PlanDto> Handle(CreatePlanCommand request, CancellationToken ct)
    {
        var plan = Plan.Create(request.Name, request.Price, request.DurationInMonths,
            request.MaxFreezeDays, request.MaxNumberOfFreezes, request.GuestPassQuota, request.AccessScope);

        await _plans.AddAsync(plan, ct);
        await _uow.SaveChangesAsync(ct);
        return ToDto(plan);
    }

    internal static PlanDto ToDto(Plan p) => new(p.Id, p.Name, p.Price, p.DurationInMonths,
        p.MaxFreezeDays, p.MaxNumberOfFreezes, p.GuestPassQuota, p.AccessScope.ToString(), p.IsPublished);
}
