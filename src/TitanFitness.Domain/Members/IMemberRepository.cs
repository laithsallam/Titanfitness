using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Members;

public interface IMemberRepository : IRepository<Member, Guid>
{
    Task<Member?> GetByMembershipNumberAsync(string membershipNumber, CancellationToken ct = default);
    Task<bool> MembershipNumberExistsAsync(string membershipNumber, CancellationToken ct = default);
    Task<List<Member>> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken ct = default);
}
