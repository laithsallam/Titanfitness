using MediatR;
using TitanFitness.Domain.CheckIns;

namespace TitanFitness.Application.CheckIns;

public record GetCheckInHistoryQuery(Guid MemberId) : IRequest<List<CheckInDto>>;

public class GetCheckInHistoryQueryHandler : IRequestHandler<GetCheckInHistoryQuery, List<CheckInDto>>
{
    private readonly ICheckInRepository _checkIns;
    public GetCheckInHistoryQueryHandler(ICheckInRepository checkIns) => _checkIns = checkIns;

    public async Task<List<CheckInDto>> Handle(GetCheckInHistoryQuery request, CancellationToken ct)
    {
        var history = await _checkIns.GetByMemberAsync(request.MemberId, ct);
        return history.Select(c => new CheckInDto(c.Id, c.MemberId, c.BranchId, c.CheckInDateTimeUtc,
            c.Result.ToString(), c.RefusalReason)).ToList();
    }
}
