using FluentValidation;
using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.CheckIns;

/// <summary>
/// "Every attempt is recorded, whether granted or refused." The eligibility
/// decision itself is delegated to CheckInEligibilityService, a pure domain
/// service - this handler's only job is to gather the Member, their
/// Memberships and the Branch, hand them to the service, and persist whatever
/// CheckIn record comes back.
/// </summary>
public record PerformCheckInCommand(Guid MemberId, Guid BranchId) : IRequest<CheckInDto>;

public class PerformCheckInCommandValidator : AbstractValidator<PerformCheckInCommand>
{
    public PerformCheckInCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class PerformCheckInCommandHandler : IRequestHandler<PerformCheckInCommand, CheckInDto>
{
    private readonly IMemberRepository _members;
    private readonly IBranchRepository _branches;
    private readonly IMembershipRepository _memberships;
    private readonly ICheckInRepository _checkIns;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public PerformCheckInCommandHandler(IMemberRepository members, IBranchRepository branches,
        IMembershipRepository memberships, ICheckInRepository checkIns, IDateTimeProvider clock, IUnitOfWork uow)
    {
        _members = members;
        _branches = branches;
        _memberships = memberships;
        _checkIns = checkIns;
        _clock = clock;
        _uow = uow;
    }

    public async Task<CheckInDto> Handle(PerformCheckInCommand request, CancellationToken ct)
    {
        var member = await _members.GetByIdAsync(request.MemberId, ct)
            ?? throw new DomainException("Member not found.");
        var branch = await _branches.GetByIdAsync(request.BranchId, ct)
            ?? throw new DomainException("Branch not found.");
        var memberships = await _memberships.GetByMemberAsync(member.Id, ct);

        var checkIn = CheckInEligibilityService.Evaluate(member, memberships, branch.Id, _clock.UtcNow, _clock.Today);

        await _checkIns.AddAsync(checkIn, ct);
        await _uow.SaveChangesAsync(ct);

        return new CheckInDto(checkIn.Id, checkIn.MemberId, checkIn.BranchId, checkIn.CheckInDateTimeUtc,
            checkIn.Result.ToString(), checkIn.RefusalReason);
    }
}
