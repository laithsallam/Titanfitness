using FluentValidation;
using MediatR;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Application.Trainers;

public record CreateTrainerCommand(string Name, string? Email, string? Phone) : IRequest<TrainerDto>;

public class CreateTrainerCommandValidator : AbstractValidator<CreateTrainerCommand>
{
    public CreateTrainerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}

public class CreateTrainerCommandHandler : IRequestHandler<CreateTrainerCommand, TrainerDto>
{
    private readonly ITrainerRepository _trainers;
    private readonly IUnitOfWork _uow;
    public CreateTrainerCommandHandler(ITrainerRepository trainers, IUnitOfWork uow)
    {
        _trainers = trainers;
        _uow = uow;
    }

    public async Task<TrainerDto> Handle(CreateTrainerCommand request, CancellationToken ct)
    {
        var trainer = Trainer.Create(request.Name, request.Email, request.Phone);
        await _trainers.AddAsync(trainer, ct);
        await _uow.SaveChangesAsync(ct);
        return new TrainerDto(trainer.Id, trainer.Name, trainer.Email, trainer.Phone, trainer.IsActive);
    }
}
