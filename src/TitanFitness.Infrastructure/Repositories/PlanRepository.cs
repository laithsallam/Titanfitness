using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Plans;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly TitanFitnessDbContext _db;
    public PlanRepository(TitanFitnessDbContext db) => _db = db;

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Plans.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Plan>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Plans.OrderBy(p => p.Name).ToListAsync(ct);

    public async Task<List<Plan>> GetPublishedAsync(CancellationToken ct = default) =>
        await _db.Plans.Where(p => p.IsPublished).OrderBy(p => p.Name).ToListAsync(ct);

    public Task AddAsync(Plan aggregate, CancellationToken ct = default)
    {
        _db.Plans.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(Plan aggregate) => _db.Plans.Update(aggregate);
    public void Remove(Plan aggregate) => _db.Plans.Remove(aggregate);
}
