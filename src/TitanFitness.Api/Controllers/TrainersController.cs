using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Trainers;

namespace TitanFitness.Api.Controllers;

/// <summary>Backs Figures 12-15: Trainer Directory / View / Add / Update.</summary>
[ApiController]
[Route("api/trainers")]
public class TrainersController : ControllerBase
{
    private readonly IMediator _mediator;
    public TrainersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<TrainerDto>>> GetAll(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetTrainersQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainerDto>> GetById(Guid id, CancellationToken ct)
    {
        var trainer = await _mediator.Send(new GetTrainerByIdQuery(id), ct);
        return trainer is null ? NotFound() : Ok(trainer);
    }

    [HttpPost]
    public async Task<ActionResult<TrainerDto>> Create(CreateTrainerCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TrainerDto>> Update(Guid id, [FromBody] UpdateTrainerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateTrainerCommand(id, request.Name, request.Email, request.Phone, request.IsActive), ct);
        return Ok(result);
    }
}

public record UpdateTrainerRequest(string Name, string? Email, string? Phone, bool IsActive);
