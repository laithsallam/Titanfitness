using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class TrainerRepository : ITrainerRepository
{
    private readonly TitanFitnessDbContext _db;
    public TrainerRepository(TitanFitnessDbContext db) => _db = db;

    public async Task<Trainer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Trainers.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<List<Trainer>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Trainers.OrderBy(t => t.Name).ToListAsync(ct);

    public Task AddAsync(Trainer aggregate, CancellationToken ct = default)
    {
        _db.Trainers.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(Trainer aggregate) => _db.Trainers.Update(aggregate);
    public void Remove(Trainer aggregate) => _db.Trainers.Remove(aggregate);
}
