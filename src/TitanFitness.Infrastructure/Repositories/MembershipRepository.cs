using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Memberships;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly TitanFitnessDbContext _db;
    public MembershipRepository(TitanFitnessDbContext db) => _db = db;

    private IQueryable<Membership> WithChildren() =>
        _db.Memberships.Include(m => m.Freezes).Include(m => m.GuestPasses);

    public async Task<Membership?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await WithChildren().FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<List<Membership>> GetByMemberAsync(Guid memberId, CancellationToken ct = default) =>
        await WithChildren().Where(m => m.MemberId == memberId).ToListAsync(ct);

    public async Task<int> CountActiveByPlanAsync(Guid planId, DateOnly asOf, CancellationToken ct = default)
    {
        // "412 active memberships keep the terms they were sold" - computed
        // from stored Period bounds since Status is derived, not stored.
        var candidates = await _db.Memberships
            .Where(m => m.PlanId == planId && !m.IsCancelled)
            .Include(m => m.Freezes)
            .ToListAsync(ct);

        return candidates.Count(m => m.GetStatus(asOf) is MembershipStatus.Active or MembershipStatus.Frozen);
    }

    public async Task<int> CountActiveTodayAsync(DateOnly today, CancellationToken ct = default)
    {
        var candidates = await _db.Memberships
            .Where(m => !m.IsCancelled)
            .Include(m => m.Freezes)
            .ToListAsync(ct);

        return candidates.Count(m => m.GetStatus(today) == MembershipStatus.Active);
    }

    public Task AddAsync(Membership aggregate, CancellationToken ct = default)
    {
        _db.Memberships.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(Membership aggregate) => _db.Memberships.Update(aggregate);
    public void Remove(Membership aggregate) => _db.Memberships.Remove(aggregate);
}
