using FluentValidation;
using MediatR;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Classes;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Services;
using TitanFitness.Domain.Studios;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Application.Classes;

public record ScheduleClassSessionCommand(
    string ClassName, Guid BranchId, Guid StudioId, Guid TrainerId,
    DateOnly SessionDate, TimeOnly StartTime, int DurationMinutes, int CapacityLimit,
    string? Description) : IRequest<ClassSessionDto>;

public class ScheduleClassSessionCommandValidator : AbstractValidator<ScheduleClassSessionCommand>
{
    public ScheduleClassSessionCommandValidator()
    {
        RuleFor(x => x.ClassName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.StudioId).NotEmpty();
        RuleFor(x => x.TrainerId).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.CapacityLimit).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class ScheduleClassSessionCommandHandler : IRequestHandler<ScheduleClassSessionCommand, ClassSessionDto>
{
    private readonly IStudioRepository _studios;
    private readonly ITrainerRepository _trainers;
    private readonly IClassSessionRepository _sessions;
    private readonly IUnitOfWork _uow;

    public ScheduleClassSessionCommandHandler(IStudioRepository studios, ITrainerRepository trainers,
        IClassSessionRepository sessions, IUnitOfWork uow)
    {
        _studios = studios;
        _trainers = trainers;
        _sessions = sessions;
        _uow = uow;
    }

    public async Task<ClassSessionDto> Handle(ScheduleClassSessionCommand request, CancellationToken ct)
    {
        var studio = await _studios.GetByIdAsync(request.StudioId, ct)
            ?? throw new DomainException("Studio not found.");
        var trainer = await _trainers.GetByIdAsync(request.TrainerId, ct)
            ?? throw new DomainException("Trainer not found.");
        if (!trainer.IsActive)
            throw new DomainException("Cannot schedule a session with an inactive trainer.");
        if (studio.BranchId != request.BranchId)
            throw new DomainException("The selected studio does not belong to the selected branch.");

        var session = ClassSession.Schedule(request.ClassName, request.BranchId, studio.Id, trainer.Id,
            request.SessionDate, request.StartTime, request.DurationMinutes, request.CapacityLimit,
            studio.Capacity, request.Description);

        var trainerSessions = await _sessions.GetByTrainerOnDateAsync(trainer.Id, request.SessionDate, ct);
        var studioSessions = await _sessions.GetByStudioOnDateAsync(studio.Id, request.SessionDate, ct);
        SessionSchedulingService.EnsureNoTrainerOrStudioConflict(session, trainerSessions.Concat(studioSessions));

        await _sessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        return ClassSessionMapper.ToDto(session);
    }
}
