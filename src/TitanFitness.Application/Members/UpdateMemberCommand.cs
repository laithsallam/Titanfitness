using FluentValidation;
using MediatR;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Members;

namespace TitanFitness.Application.Members;

public record UpdateMemberCommand(Guid Id, string FullName, string? Email, string? Phone,
    string? Address, string? PhotoUrl) : IRequest<MemberDto>;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(200);
    }
}

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, MemberDto>
{
    private readonly IMemberRepository _members;
    private readonly IUnitOfWork _uow;
    public UpdateMemberCommandHandler(IMemberRepository members, IUnitOfWork uow) { _members = members; _uow = uow; }

    public async Task<MemberDto> Handle(UpdateMemberCommand request, CancellationToken ct)
    {
        var member = await _members.GetByIdAsync(request.Id, ct) ?? throw new DomainException("Member not found.");
        member.UpdateProfile(request.FullName, request.Email, request.Phone, request.Address, request.PhotoUrl);
        _members.Update(member);
        await _uow.SaveChangesAsync(ct);
        return CreateMemberCommandHandler.ToDto(member);
    }
}
