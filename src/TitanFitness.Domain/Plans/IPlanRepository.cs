using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Plans;

public interface IPlanRepository : IRepository<Plan, Guid>
{
    Task<List<Plan>> GetAllAsync(CancellationToken ct = default);
    Task<List<Plan>> GetPublishedAsync(CancellationToken ct = default);
}
