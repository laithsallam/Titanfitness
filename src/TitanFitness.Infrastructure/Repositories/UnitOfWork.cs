using TitanFitness.Domain.Common;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TitanFitnessDbContext _db;
    public UnitOfWork(TitanFitnessDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
