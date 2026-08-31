using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Classes;
using TitanFitness.Domain.Memberships;

namespace TitanFitness.Application.Dashboard;

/// <summary>
/// "Staff follow the state of the business as it runs" (Figure 1: Dashboard).
/// Purely a read model - it fans out to several repositories and folds the
/// results together; there is no aggregate called "Dashboard", because none
/// of these numbers are ever written to as a unit, only read.
/// </summary>
public record GetDashboardStatsQuery(Guid? BranchId) : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly ICheckInRepository _checkIns;
    private readonly IMembershipRepository _memberships;
    private readonly IClassSessionRepository _sessions;
    private readonly IDateTimeProvider _clock;

    public GetDashboardStatsQueryHandler(ICheckInRepository checkIns, IMembershipRepository memberships,
        IClassSessionRepository sessions, IDateTimeProvider clock)
    {
        _checkIns = checkIns;
        _memberships = memberships;
        _sessions = sessions;
        _clock = clock;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var today = _clock.Today;

        var checkInsToday = await _checkIns.CountAdmittedTodayAsync(today, ct);
        var activeMembers = await _memberships.CountActiveTodayAsync(today, ct);
        var todaysSessions = await _sessions.GetScheduleAsync(today, request.BranchId, ct);

        var liveSessions = todaysSessions.Where(s => s.Status != SessionStatus.Cancelled).ToList();
        var upcoming = liveSessions.Count(s => s.Status == SessionStatus.Open || s.Status == SessionStatus.InProgress);
        var bookingsToday = liveSessions.Sum(s => s.Bookings.Count(b => b.Status != BookingStatus.Cancelled));
        var avgFillRate = liveSessions.Count == 0 ? 0 : liveSessions.Average(s => s.FillRate);

        return new DashboardStatsDto(checkInsToday, activeMembers, upcoming, bookingsToday, avgFillRate);
    }
}

public record GetUpcomingSessionsQuery(Guid? BranchId) : IRequest<List<UpcomingSessionDto>>;

public class GetUpcomingSessionsQueryHandler : IRequestHandler<GetUpcomingSessionsQuery, List<UpcomingSessionDto>>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IDateTimeProvider _clock;
    public GetUpcomingSessionsQueryHandler(IClassSessionRepository sessions, IDateTimeProvider clock)
    {
        _sessions = sessions;
        _clock = clock;
    }

    public async Task<List<UpcomingSessionDto>> Handle(GetUpcomingSessionsQuery request, CancellationToken ct)
    {
        var sessions = await _sessions.GetScheduleAsync(_clock.Today, request.BranchId, ct);
        return sessions
            .Where(s => s.Status != SessionStatus.Cancelled && s.Status != SessionStatus.Completed)
            .OrderBy(s => s.StartTime)
            .Select(s => new UpcomingSessionDto(s.Id, s.ClassName, s.BranchId, s.StartTime,
                s.Bookings.Count(b => b.Status == BookingStatus.Booked), s.CapacityLimit, s.Status.ToString()))
            .ToList();
    }
}
