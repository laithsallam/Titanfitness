using FluentValidation;
using MediatR;
using TitanFitness.Domain.Memberships;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;

namespace TitanFitness.Application.Memberships;

public record CancelMembershipCommand(Guid MembershipId) : IRequest<MembershipDto>;

public class CancelMembershipCommandValidator : AbstractValidator<CancelMembershipCommand>
{
    public CancelMembershipCommandValidator() => RuleFor(x => x.MembershipId).NotEmpty();
}

public class CancelMembershipCommandHandler : IRequestHandler<CancelMembershipCommand, MembershipDto>
{
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public CancelMembershipCommandHandler(IMembershipRepository memberships, IDateTimeProvider clock, IUnitOfWork uow)
    {
        _memberships = memberships;
        _clock = clock;
        _uow = uow;
    }

    public async Task<MembershipDto> Handle(CancelMembershipCommand request, CancellationToken ct)
    {
        var membership = await _memberships.GetByIdAsync(request.MembershipId, ct)
            ?? throw new DomainException("Membership not found.");

        membership.Cancel(_clock.Today);

        _memberships.Update(membership);
        await _uow.SaveChangesAsync(ct);

        return MembershipMapper.ToDto(membership, _clock);
    }
}
