using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Plans;

namespace TitanFitness.Api.Controllers;

/// <summary>Backs Figures 16-19: Plan Catalogue / View / Add / Update.</summary>
[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly IMediator _mediator;
    public PlansController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<PlanDto>>> GetAll([FromQuery] bool publishedOnly, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetPlansQuery(publishedOnly), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlanDto>> GetById(Guid id, CancellationToken ct)
    {
        var plan = await _mediator.Send(new GetPlanByIdQuery(id), ct);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<PlanDto>> Create(CreatePlanCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// "Changes to this plan apply to new purchases only. 412 active
    /// memberships keep the terms they were sold" (Figure 19 banner) - the
    /// count comes back in the response so the UI can render exactly that.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlanUpdateResultDto>> Update(Guid id, [FromBody] UpdatePlanRequest request, CancellationToken ct)
    {
        var command = new UpdatePlanCommand(id, request.Name, request.Price, request.DurationInMonths,
            request.MaxFreezeDays, request.MaxNumberOfFreezes, request.GuestPassQuota, request.AccessScope);
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<PlanDto>> Publish(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new PublishPlanCommand(id), ct));

    [HttpPost("{id:guid}/retire")]
    public async Task<ActionResult<PlanDto>> Retire(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new RetirePlanCommand(id), ct));
}

public record UpdatePlanRequest(string Name, decimal Price, int DurationInMonths,
    int MaxFreezeDays, int MaxNumberOfFreezes, int GuestPassQuota,
    TitanFitness.Domain.Common.AccessScope AccessScope);
