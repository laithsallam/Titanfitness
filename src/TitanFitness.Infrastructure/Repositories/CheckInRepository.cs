using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class CheckInRepository : ICheckInRepository
{
    private readonly TitanFitnessDbContext _db;
    public CheckInRepository(TitanFitnessDbContext db) => _db = db;

    public async Task<CheckIn?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.CheckIns.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<CheckIn>> GetByMemberAsync(Guid memberId, CancellationToken ct = default) =>
        await _db.CheckIns
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.CheckInDateTimeUtc)
            .ToListAsync(ct);

    public async Task<int> CountAdmittedTodayAsync(DateOnly today, CancellationToken ct = default)
    {
        var start = today.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);
        return await _db.CheckIns.CountAsync(c =>
            c.Result == CheckInResult.Admitted &&
            c.CheckInDateTimeUtc >= start && c.CheckInDateTimeUtc < end, ct);
    }

    public Task AddAsync(CheckIn aggregate, CancellationToken ct = default)
    {
        _db.CheckIns.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(CheckIn aggregate) => _db.CheckIns.Update(aggregate);
    public void Remove(CheckIn aggregate) => _db.CheckIns.Remove(aggregate);
}
