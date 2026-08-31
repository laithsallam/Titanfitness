using FluentValidation;
using MediatR;
using TitanFitness.Domain.Memberships;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;

namespace TitanFitness.Application.Memberships;

public record IssueGuestPassCommand(Guid MembershipId) : IRequest<MembershipDto>;

public class IssueGuestPassCommandValidator : AbstractValidator<IssueGuestPassCommand>
{
    public IssueGuestPassCommandValidator() => RuleFor(x => x.MembershipId).NotEmpty();
}

public class IssueGuestPassCommandHandler : IRequestHandler<IssueGuestPassCommand, MembershipDto>
{
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public IssueGuestPassCommandHandler(IMembershipRepository memberships, IDateTimeProvider clock, IUnitOfWork uow)
    {
        _memberships = memberships;
        _clock = clock;
        _uow = uow;
    }

    public async Task<MembershipDto> Handle(IssueGuestPassCommand request, CancellationToken ct)
    {
        var membership = await _memberships.GetByIdAsync(request.MembershipId, ct)
            ?? throw new DomainException("Membership not found.");

        membership.IssueGuestPass(_clock.Today);

        _memberships.Update(membership);
        await _uow.SaveChangesAsync(ct);
        return MembershipMapper.ToDto(membership, _clock);
    }
}

public record RedeemGuestPassCommand(Guid MembershipId, Guid GuestPassId, string? GuestName) : IRequest<MembershipDto>;

public class RedeemGuestPassCommandValidator : AbstractValidator<RedeemGuestPassCommand>
{
    public RedeemGuestPassCommandValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.GuestPassId).NotEmpty();
        RuleFor(x => x.GuestName).MaximumLength(100);
    }
}

public class RedeemGuestPassCommandHandler : IRequestHandler<RedeemGuestPassCommand, MembershipDto>
{
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public RedeemGuestPassCommandHandler(IMembershipRepository memberships, IDateTimeProvider clock, IUnitOfWork uow)
    {
        _memberships = memberships;
        _clock = clock;
        _uow = uow;
    }

    public async Task<MembershipDto> Handle(RedeemGuestPassCommand request, CancellationToken ct)
    {
        var membership = await _memberships.GetByIdAsync(request.MembershipId, ct)
            ?? throw new DomainException("Membership not found.");

        membership.RedeemGuestPass(request.GuestPassId, request.GuestName, _clock.Today);

        _memberships.Update(membership);
        await _uow.SaveChangesAsync(ct);
        return MembershipMapper.ToDto(membership, _clock);
    }
}
