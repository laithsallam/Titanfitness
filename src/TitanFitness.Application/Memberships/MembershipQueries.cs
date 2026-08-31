using MediatR;
using TitanFitness.Domain.Memberships;
using TitanFitness.Application.Common.Interfaces;

namespace TitanFitness.Application.Memberships;

public record GetMembershipsForMemberQuery(Guid MemberId) : IRequest<List<MembershipDto>>;

public class GetMembershipsForMemberQueryHandler : IRequestHandler<GetMembershipsForMemberQuery, List<MembershipDto>>
{
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    public GetMembershipsForMemberQueryHandler(IMembershipRepository memberships, IDateTimeProvider clock)
    {
        _memberships = memberships;
        _clock = clock;
    }

    public async Task<List<MembershipDto>> Handle(GetMembershipsForMemberQuery request, CancellationToken ct)
    {
        var memberships = await _memberships.GetByMemberAsync(request.MemberId, ct);
        return memberships
            .OrderByDescending(m => m.Period.End)
            .Select(m => MembershipMapper.ToDto(m, _clock))
            .ToList();
    }
}

public record GetMembershipByIdQuery(Guid Id) : IRequest<MembershipDto?>;

public class GetMembershipByIdQueryHandler : IRequestHandler<GetMembershipByIdQuery, MembershipDto?>
{
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    public GetMembershipByIdQueryHandler(IMembershipRepository memberships, IDateTimeProvider clock)
    {
        _memberships = memberships;
        _clock = clock;
    }

    public async Task<MembershipDto?> Handle(GetMembershipByIdQuery request, CancellationToken ct)
    {
        var m = await _memberships.GetByIdAsync(request.Id, ct);
        return m is null ? null : MembershipMapper.ToDto(m, _clock);
    }
}
