using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.CheckIns;

public interface ICheckInRepository : IRepository<CheckIn, Guid>
{
    Task<List<CheckIn>> GetByMemberAsync(Guid memberId, CancellationToken ct = default);
    Task<int> CountAdmittedTodayAsync(DateOnly today, CancellationToken ct = default);
}
