using MediatR;
using TitanFitness.Domain.Members;

namespace TitanFitness.Application.Members;

public record GetMemberByIdQuery(Guid Id) : IRequest<MemberDto?>;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto?>
{
    private readonly IMemberRepository _members;
    public GetMemberByIdQueryHandler(IMemberRepository members) => _members = members;

    public async Task<MemberDto?> Handle(GetMemberByIdQuery request, CancellationToken ct)
    {
        var m = await _members.GetByIdAsync(request.Id, ct);
        return m is null ? null : CreateMemberCommandHandler.ToDto(m);
    }
}

public record SearchMembersQuery(string? SearchTerm, int Page = 1, int PageSize = 20) : IRequest<List<MemberDto>>;

public class SearchMembersQueryHandler : IRequestHandler<SearchMembersQuery, List<MemberDto>>
{
    private readonly IMemberRepository _members;
    public SearchMembersQueryHandler(IMemberRepository members) => _members = members;

    public async Task<List<MemberDto>> Handle(SearchMembersQuery request, CancellationToken ct)
    {
        var members = await _members.SearchAsync(request.SearchTerm, request.Page, request.PageSize, ct);
        return members.Select(CreateMemberCommandHandler.ToDto).ToList();
    }
}
