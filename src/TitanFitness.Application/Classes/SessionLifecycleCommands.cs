using FluentValidation;
using MediatR;
using TitanFitness.Domain.Classes;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Common;

namespace TitanFitness.Application.Classes;

public record StartSessionCommand(Guid SessionId) : IRequest<ClassSessionDto>;
public record CompleteSessionCommand(Guid SessionId) : IRequest<ClassSessionDto>;
public record CancelSessionCommand(Guid SessionId) : IRequest<ClassSessionDto>;
public record MarkAttendedCommand(Guid SessionId, Guid BookingId) : IRequest<ClassSessionDto>;
public record MarkNoShowCommand(Guid SessionId, Guid BookingId) : IRequest<ClassSessionDto>;

public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, ClassSessionDto>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    public StartSessionCommandHandler(IClassSessionRepository sessions, IUnitOfWork uow) { _sessions = sessions; _uow = uow; }
    public async Task<ClassSessionDto> Handle(StartSessionCommand request, CancellationToken ct)
    {
        var s = await _sessions.GetByIdAsync(request.SessionId, ct) ?? throw new DomainException("Session not found.");
        s.Start();
        _sessions.Update(s);
        await _uow.SaveChangesAsync(ct);
        return ClassSessionMapper.ToDto(s);
    }
}

public class CompleteSessionCommandHandler : IRequestHandler<CompleteSessionCommand, ClassSessionDto>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    public CompleteSessionCommandHandler(IClassSessionRepository sessions, IUnitOfWork uow) { _sessions = sessions; _uow = uow; }
    public async Task<ClassSessionDto> Handle(CompleteSessionCommand request, CancellationToken ct)
    {
        var s = await _sessions.GetByIdAsync(request.SessionId, ct) ?? throw new DomainException("Session not found.");
        s.Complete();
        _sessions.Update(s);
        await _uow.SaveChangesAsync(ct);
        return ClassSessionMapper.ToDto(s);
    }
}

public class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, ClassSessionDto>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    public CancelSessionCommandHandler(IClassSessionRepository sessions, IUnitOfWork uow) { _sessions = sessions; _uow = uow; }
    public async Task<ClassSessionDto> Handle(CancelSessionCommand request, CancellationToken ct)
    {
        var s = await _sessions.GetByIdAsync(request.SessionId, ct) ?? throw new DomainException("Session not found.");
        s.CancelSession();
        _sessions.Update(s);
        await _uow.SaveChangesAsync(ct);
        return ClassSessionMapper.ToDto(s);
    }
}

public class MarkAttendedCommandHandler : IRequestHandler<MarkAttendedCommand, ClassSessionDto>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    public MarkAttendedCommandHandler(IClassSessionRepository sessions, IUnitOfWork uow) { _sessions = sessions; _uow = uow; }
    public async Task<ClassSessionDto> Handle(MarkAttendedCommand request, CancellationToken ct)
    {
        var s = await _sessions.GetByIdAsync(request.SessionId, ct) ?? throw new DomainException("Session not found.");
        s.MarkAttended(request.BookingId);
        _sessions.Update(s);
        await _uow.SaveChangesAsync(ct);
        return ClassSessionMapper.ToDto(s);
    }
}

public class MarkNoShowCommandHandler : IRequestHandler<MarkNoShowCommand, ClassSessionDto>
{
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;
    public MarkNoShowCommandHandler(IClassSessionRepository sessions, IUnitOfWork uow) { _sessions = sessions; _uow = uow; }
    public async Task<ClassSessionDto> Handle(MarkNoShowCommand request, CancellationToken ct)
    {
        var s = await _sessions.GetByIdAsync(request.SessionId, ct) ?? throw new DomainException("Session not found.");
        s.MarkNoShow(request.BookingId);
        _sessions.Update(s);
        await _uow.SaveChangesAsync(ct);
        return ClassSessionMapper.ToDto(s);
    }
}
