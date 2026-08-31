using MediatR;
using TitanFitness.Domain.Branches;

namespace TitanFitness.Application.Branches;

public record GetBranchesQuery : IRequest<List<BranchDto>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, List<BranchDto>>
{
    private readonly IBranchRepository _branches;
    public GetBranchesQueryHandler(IBranchRepository branches) => _branches = branches;

    public async Task<List<BranchDto>> Handle(GetBranchesQuery request, CancellationToken ct)
    {
        var branches = await _branches.GetAllAsync(ct);
        return branches
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.OperatingHours.Open, b.OperatingHours.Close))
            .ToList();
    }
}
