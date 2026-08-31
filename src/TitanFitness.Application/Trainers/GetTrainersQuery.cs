using MediatR;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Application.Trainers;

public record GetTrainersQuery : IRequest<List<TrainerDto>>;

public class GetTrainersQueryHandler : IRequestHandler<GetTrainersQuery, List<TrainerDto>>
{
    private readonly ITrainerRepository _trainers;
    public GetTrainersQueryHandler(ITrainerRepository trainers) => _trainers = trainers;

    public async Task<List<TrainerDto>> Handle(GetTrainersQuery request, CancellationToken ct)
    {
        var trainers = await _trainers.GetAllAsync(ct);
        return trainers.Select(t => new TrainerDto(t.Id, t.Name, t.Email, t.Phone, t.IsActive)).ToList();
    }
}

public record GetTrainerByIdQuery(Guid Id) : IRequest<TrainerDto?>;

public class GetTrainerByIdQueryHandler : IRequestHandler<GetTrainerByIdQuery, TrainerDto?>
{
    private readonly ITrainerRepository _trainers;
    public GetTrainerByIdQueryHandler(ITrainerRepository trainers) => _trainers = trainers;

    public async Task<TrainerDto?> Handle(GetTrainerByIdQuery request, CancellationToken ct)
    {
        var t = await _trainers.GetByIdAsync(request.Id, ct);
        return t is null ? null : new TrainerDto(t.Id, t.Name, t.Email, t.Phone, t.IsActive);
    }
}
