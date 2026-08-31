using FluentValidation;
using MediatR;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Application.Trainers;

public record UpdateTrainerCommand(Guid Id, string Name, string? Email, string? Phone, bool IsActive) : IRequest<TrainerDto>;

public class UpdateTrainerCommandValidator : AbstractValidator<UpdateTrainerCommand>
{
    public UpdateTrainerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}

public class UpdateTrainerCommandHandler : IRequestHandler<UpdateTrainerCommand, TrainerDto>
{
    private readonly ITrainerRepository _trainers;
    private readonly IUnitOfWork _uow;
    public UpdateTrainerCommandHandler(ITrainerRepository trainers, IUnitOfWork uow)
    {
        _trainers = trainers;
        _uow = uow;
    }

    public async Task<TrainerDto> Handle(UpdateTrainerCommand request, CancellationToken ct)
    {
        var trainer = await _trainers.GetByIdAsync(request.Id, ct)
            ?? throw new DomainException("Trainer not found.");

        trainer.UpdateDetails(request.Name, request.Email, request.Phone);
        if (request.IsActive) trainer.Activate(); else trainer.Deactivate();

        _trainers.Update(trainer);
        await _uow.SaveChangesAsync(ct);
        return new TrainerDto(trainer.Id, trainer.Name, trainer.Email, trainer.Phone, trainer.IsActive);
    }
}
