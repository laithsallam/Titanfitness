using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Members;
using TitanFitness.Infrastructure.Persistence;

namespace TitanFitness.Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly TitanFitnessDbContext _db;
    public MemberRepository(TitanFitnessDbContext db) => _db = db;

    public async Task<Member?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Member?> GetByMembershipNumberAsync(string membershipNumber, CancellationToken ct = default)
    {
        var normalized = membershipNumber.Trim().ToUpperInvariant();
        return await _db.Members.FirstOrDefaultAsync(m => m.MembershipNumber.Value == normalized, ct);
    }

    public async Task<bool> MembershipNumberExistsAsync(string membershipNumber, CancellationToken ct = default)
    {
        var normalized = membershipNumber.Trim().ToUpperInvariant();
        return await _db.Members.AnyAsync(m => m.MembershipNumber.Value == normalized, ct);
    }

    public async Task<List<Member>> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Members.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(m =>
                m.FullName.Contains(term) ||
                m.MembershipNumber.Value.Contains(term));
        }

        return await query
            .OrderBy(m => m.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task AddAsync(Member aggregate, CancellationToken ct = default)
    {
        _db.Members.Add(aggregate);
        return Task.CompletedTask;
    }

    public void Update(Member aggregate) => _db.Members.Update(aggregate);
    public void Remove(Member aggregate) => _db.Members.Remove(aggregate);
}
