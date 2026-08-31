using FluentValidation;
using MediatR;
using TitanFitness.Domain.Classes;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.Classes;

/// <summary>
/// "Staff book a member onto a session on their behalf, after finding the
/// member and confirming they are eligible. A member without an active
/// membership cannot be booked at all."
/// </summary>
public record BookSessionCommand(Guid SessionId, Guid MemberId, string? NotesForTrainer) : IRequest<ClassSessionDto>;

public class BookSessionCommandValidator : AbstractValidator<BookSessionCommand>
{
    public BookSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.NotesForTrainer).MaximumLength(500);
    }
}

public class BookSessionCommandHandler : IRequestHandler<BookSessionCommand, ClassSessionDto>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IMembershipRepository _memberships;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;

    public BookSessionCommandHandler(IClassSessionRepository sessions, IMembershipRepository memberships,
        IDateTimeProvider clock, IUnitOfWork uow)
    {
        _sessions = sessions;
        _memberships = memberships;
        _clock = clock;
        _uow = uow;
    }

    public async Task<ClassSessionDto> Handle(BookSessionCommand request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct)
            ?? throw new DomainException("Session not found.");

        var memberships = await _memberships.GetByMemberAsync(request.MemberId, ct);
        var hasActiveMembership = memberships.Any(m => m.GetStatus(_clock.Today) == MembershipStatus.Active);
        if (!hasActiveMembership)
            throw new DomainException("A member without an active membership cannot be booked.");

        var otherActiveSessions = await _sessions.GetActiveSessionsForMemberAsync(request.MemberId, ct);
        SessionSchedulingService.EnsureNoMemberDoubleBooking(session, request.MemberId, otherActiveSessions);

        session.Book(request.MemberId, _clock.UtcNow, request.NotesForTrainer);

        _sessions.Update(session);
        await _uow.SaveChangesAsync(ct);

        return ClassSessionMapper.ToDto(session);
    }
}
