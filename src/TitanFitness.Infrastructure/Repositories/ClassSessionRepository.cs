using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Classes;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class ClassSessionRepository : IClassSessionRepository
{
    private readonly TitanFitnessDbContext _db;
    public ClassSessionRepository(TitanFitnessDbContext db) => _db = db;

    private IQueryable<ClassSession> WithBookings() => _db.ClassSessions.Include(s => s.Bookings);

    public async Task<ClassSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await WithBookings().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<ClassSession>> GetByTrainerOnDateAsync(Guid trainerId, DateOnly date, CancellationToken ct = default) =>
        await WithBookings().Where(s => s.TrainerId == trainerId && s.SessionDate == date).ToListAsync(ct);

    public async Task<List<ClassSession>> GetByStudioOnDateAsync(Guid studioId, DateOnly date, CancellationToken ct = default) =>
        await WithBookings().Where(s => s.StudioId == studioId && s.SessionDate == date).ToListAsync(ct);

    public async Task<List<ClassSession>> GetActiveSessionsForMemberAsync(Guid memberId, CancellationToken ct = default) =>
        await WithBookings()
            .Where(s => s.Bookings.Any(b =>
                b.MemberId == memberId &&
                (b.Status == BookingStatus.Booked || b.Status == BookingStatus.Waitlisted)))
            .ToListAsync(ct);

    public async Task<List<ClassSession>> GetScheduleAsync(DateOnly date, Guid? branchId, CancellationToken ct = default)
    {
        var query = WithBookings().Where(s => s.SessionDate == date);
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId.Value);
        return await query.OrderBy(s => s.StartTime).ToListAsync(ct);
    }

    public Task AddAsync(ClassSession aggregate, CancellationToken ct = default)
    {
        _db.ClassSessions.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(ClassSession aggregate) => _db.ClassSessions.Update(aggregate);
    public void Remove(ClassSession aggregate) => _db.ClassSessions.Remove(aggregate);
}
