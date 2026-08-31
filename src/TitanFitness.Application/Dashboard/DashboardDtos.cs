namespace TitanFitness.Application.Dashboard;

public record DashboardStatsDto(
    int CheckInsToday,
    int ActiveMembersToday,
    int UpcomingSessionsToday,
    int BookingsToday,
    double AverageFillRateToday);

public record UpcomingSessionDto(
    Guid Id, string ClassName, Guid BranchId, TimeOnly StartTime,
    int BookedCount, int CapacityLimit, string Status);
