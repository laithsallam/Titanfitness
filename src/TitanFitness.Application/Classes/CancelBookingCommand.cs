using FluentValidation;
using MediatR;
using TitanFitness.Domain.Classes;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;

namespace TitanFitness.Application.Classes;

public record CancelBookingCommand(Guid SessionId, Guid BookingId) : IRequest<ClassSessionDto>;

public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.BookingId).NotEmpty();
    }
}

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, ClassSessionDto>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    public CancelBookingCommandHandler(IClassSessionRepository sessions, IUnitOfWork uow) { _sessions = sessions; _uow = uow; }

    public async Task<ClassSessionDto> Handle(CancelBookingCommand request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct)
            ?? throw new DomainException("Session not found.");

        session.CancelBooking(request.BookingId);

        _sessions.Update(session);
        await _uow.SaveChangesAsync(ct);

        return ClassSessionMapper.ToDto(session);
    }
}
