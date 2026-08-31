using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Trainers;

public interface ITrainerRepository : IRepository<Trainer, Guid>
{
    Task<List<Trainer>> GetAllAsync(CancellationToken ct = default);
}
