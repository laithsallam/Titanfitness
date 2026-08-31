using MediatR;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Application.Plans;

public record GetPlansQuery(bool PublishedOnly = false) : IRequest<List<PlanDto>>;

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, List<PlanDto>>
{
    private readonly IPlanRepository _plans;
    public GetPlansQueryHandler(IPlanRepository plans) => _plans = plans;

    public async Task<List<PlanDto>> Handle(GetPlansQuery request, CancellationToken ct)
    {
        var plans = request.PublishedOnly ? await _plans.GetPublishedAsync(ct) : await _plans.GetAllAsync(ct);
        return plans.Select(CreatePlanCommandHandler.ToDto).ToList();
    }
}

public record GetPlanByIdQuery(Guid Id) : IRequest<PlanDto?>;

public class GetPlanByIdQueryHandler : IRequestHandler<GetPlanByIdQuery, PlanDto?>
{
    private readonly IPlanRepository _plans;
    public GetPlanByIdQueryHandler(IPlanRepository plans) => _plans = plans;

    public async Task<PlanDto?> Handle(GetPlanByIdQuery request, CancellationToken ct)
    {
        var plan = await _plans.GetByIdAsync(request.Id, ct);
        return plan is null ? null : CreatePlanCommandHandler.ToDto(plan);
    }
}
