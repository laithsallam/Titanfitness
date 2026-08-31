using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Classes;

public interface IClassSessionRepository : IRepository<ClassSession, Guid>
{
    Task<List<ClassSession>> GetByTrainerOnDateAsync(Guid trainerId, DateOnly date, CancellationToken ct = default);
    Task<List<ClassSession>> GetByStudioOnDateAsync(Guid studioId, DateOnly date, CancellationToken ct = default);
    Task<List<ClassSession>> GetActiveSessionsForMemberAsync(Guid memberId, CancellationToken ct = default);
    Task<List<ClassSession>> GetScheduleAsync(DateOnly date, Guid? branchId, CancellationToken ct = default);
}
