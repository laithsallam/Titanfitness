using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Branches;

public interface IBranchRepository : IRepository<Branch, Guid>
{
    Task<List<Branch>> GetAllAsync(CancellationToken ct = default);
}
