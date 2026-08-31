using MediatR;
using TitanFitness.Domain.Classes;

namespace TitanFitness.Application.Classes;

public record GetClassSessionByIdQuery(Guid Id) : IRequest<ClassSessionDto?>;

public class GetClassSessionByIdQueryHandler : IRequestHandler<GetClassSessionByIdQuery, ClassSessionDto?>
{
    private readonly IClassSessionRepository _sessions;
    public GetClassSessionByIdQueryHandler(IClassSessionRepository sessions) => _sessions = sessions;

    public async Task<ClassSessionDto?> Handle(GetClassSessionByIdQuery request, CancellationToken ct)
    {
        var s = await _sessions.GetByIdAsync(request.Id, ct);
        return s is null ? null : ClassSessionMapper.ToDto(s);
    }
}

/// <summary>Backs Figure 8 (Class Schedule): a branch's sessions for one day.</summary>
public record GetClassScheduleQuery(DateOnly Date, Guid? BranchId) : IRequest<List<ClassSessionDto>>;

public class GetClassScheduleQueryHandler : IRequestHandler<GetClassScheduleQuery, List<ClassSessionDto>>
{
    private readonly IClassSessionRepository _sessions;
    public GetClassScheduleQueryHandler(IClassSessionRepository sessions) => _sessions = sessions;

    public async Task<List<ClassSessionDto>> Handle(GetClassScheduleQuery request, CancellationToken ct)
    {
        var sessions = await _sessions.GetScheduleAsync(request.Date, request.BranchId, ct);
        return sessions.Select(ClassSessionMapper.ToDto).ToList();
    }
}
