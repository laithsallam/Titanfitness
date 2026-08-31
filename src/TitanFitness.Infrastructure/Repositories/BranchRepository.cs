using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Branches;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly TitanFitnessDbContext _db;
    public BranchRepository(TitanFitnessDbContext db) => _db = db;

    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Branches.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<List<Branch>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Branches.OrderBy(b => b.Name).ToListAsync(ct);

    public Task AddAsync(Branch aggregate, CancellationToken ct = default)
    {
        _db.Branches.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(Branch aggregate) => _db.Branches.Update(aggregate);
    public void Remove(Branch aggregate) => _db.Branches.Remove(aggregate);
}
