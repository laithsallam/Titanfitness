using FluentValidation;
using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Members;

namespace TitanFitness.Application.Members;

/// <summary>
/// "Creating a member does not grant access on its own." This handler only
/// ever produces a Member; a Plan must be purchased separately via
/// PurchaseMembershipCommand for anything to actually grant entry.
/// </summary>
public record CreateMemberCommand(
    string? MembershipNumber, string FullName, string? Email, string? Phone,
    string? Address, DateOnly JoinedDate, string? PhotoUrl, Guid HomeBranchId) : IRequest<MemberDto>;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.MembershipNumber).MaximumLength(10);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(200);
        RuleFor(x => x.HomeBranchId).NotEmpty();
    }
}

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, MemberDto>
{
    private readonly IMemberRepository _members;
    private readonly IBranchRepository _branches;
    private readonly IUnitOfWork _uow;

    public CreateMemberCommandHandler(IMemberRepository members, IBranchRepository branches, IUnitOfWork uow)
    {
        _members = members;
        _branches = branches;
        _uow = uow;
    }

    public async Task<MemberDto> Handle(CreateMemberCommand request, CancellationToken ct)
    {
        var branch = await _branches.GetByIdAsync(request.HomeBranchId, ct)
            ?? throw new DomainException("Home branch not found.");

        var numberText = string.IsNullOrWhiteSpace(request.MembershipNumber)
            ? await GenerateUniqueNumberAsync(ct)
            : request.MembershipNumber;

        if (await _members.MembershipNumberExistsAsync(numberText, ct))
            throw new DomainException($"Membership number '{numberText}' is already in use.");

        var member = Member.Register(
            MembershipNumber.Create(numberText),
            request.FullName, request.Email, request.Phone, request.Address,
            request.JoinedDate, request.PhotoUrl, branch.Id);

        await _members.AddAsync(member, ct);
        await _uow.SaveChangesAsync(ct);

        return ToDto(member);
    }

    private async Task<string> GenerateUniqueNumberAsync(CancellationToken ct)
    {
        string candidate;
        do
        {
            candidate = $"TF-{Random.Shared.Next(1000, 9999)}";
        } while (await _members.MembershipNumberExistsAsync(candidate, ct));
        return candidate;
    }

    internal static MemberDto ToDto(Member m) => new(m.Id, m.MembershipNumber.Value, m.FullName,
        m.Email, m.Phone, m.Address, m.JoinedDate, m.PhotoUrl, m.HomeBranchId);
}
