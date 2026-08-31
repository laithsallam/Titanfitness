using MediatR;
using TitanFitness.Domain.Studios;

namespace TitanFitness.Application.Studios;

public record GetStudiosByBranchQuery(Guid BranchId) : IRequest<List<StudioDto>>;

public class GetStudiosByBranchQueryHandler : IRequestHandler<GetStudiosByBranchQuery, List<StudioDto>>
{
    private readonly IStudioRepository _studios;
    public GetStudiosByBranchQueryHandler(IStudioRepository studios) => _studios = studios;

    public async Task<List<StudioDto>> Handle(GetStudiosByBranchQuery request, CancellationToken ct)
    {
        var studios = await _studios.GetByBranchAsync(request.BranchId, ct);
        return studios.Select(s => new StudioDto(s.Id, s.Name, s.BranchId, s.Capacity)).ToList();
    }
}
