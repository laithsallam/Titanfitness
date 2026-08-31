using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Studios;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class StudioRepository : IStudioRepository
{
    private readonly TitanFitnessDbContext _db;
    public StudioRepository(TitanFitnessDbContext db) => _db = db;

    public async Task<Studio?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Studios.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<Studio>> GetByBranchAsync(Guid branchId, CancellationToken ct = default) =>
        await _db.Studios.Where(s => s.BranchId == branchId).OrderBy(s => s.Name).ToListAsync(ct);

    public Task AddAsync(Studio aggregate, CancellationToken ct = default)
    {
        _db.Studios.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(Studio aggregate) => _db.Studios.Update(aggregate);
    public void Remove(Studio aggregate) => _db.Studios.Remove(aggregate);
}
