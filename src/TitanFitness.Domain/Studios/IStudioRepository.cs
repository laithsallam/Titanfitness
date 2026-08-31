using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Studios;

public interface IStudioRepository : IRepository<Studio, Guid>
{
    Task<List<Studio>> GetByBranchAsync(Guid branchId, CancellationToken ct = default);
}
