using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Memberships;

public interface IMembershipRepository : IRepository<Membership, Guid>
{
    Task<List<Membership>> GetByMemberAsync(Guid memberId, CancellationToken ct = default);
    Task<int> CountActiveByPlanAsync(Guid planId, DateOnly asOf, CancellationToken ct = default);
    Task<int> CountActiveTodayAsync(DateOnly today, CancellationToken ct = default);
}
