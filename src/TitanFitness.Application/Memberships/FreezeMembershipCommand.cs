using FluentValidation;
using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Memberships;

namespace TitanFitness.Application.Memberships;

public record FreezeMembershipCommand(
    Guid MembershipId, DateOnly StartDate, int DurationMonths, FreezeReason Reason, string? Notes) : IRequest<MembershipDto>;

public class FreezeMembershipCommandValidator : AbstractValidator<FreezeMembershipCommand>
{
    public FreezeMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.DurationMonths).GreaterThan(0).LessThanOrEqualTo(12);
        RuleFor(x => x.Notes).MaximumLength(200);
        RuleFor(x => x.Reason).IsInEnum();
    }
}

public class FreezeMembershipCommandHandler : IRequestHandler<FreezeMembershipCommand, MembershipDto>
{
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public FreezeMembershipCommandHandler(IMembershipRepository memberships, IDateTimeProvider clock, IUnitOfWork uow)
    {
        _memberships = memberships;
        _clock = clock;
        _uow = uow;
    }

    public async Task<MembershipDto> Handle(FreezeMembershipCommand request, CancellationToken ct)
    {
        var membership = await _memberships.GetByIdAsync(request.MembershipId, ct)
            ?? throw new DomainException("Membership not found.");

        membership.RequestFreeze(request.StartDate, request.DurationMonths, request.Reason,
            request.Notes, _clock.Today, _clock.UtcNow);

        _memberships.Update(membership);
        await _uow.SaveChangesAsync(ct);

        return MembershipMapper.ToDto(membership, _clock);
    }
}
