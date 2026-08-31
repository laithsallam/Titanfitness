using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Studios;

namespace TitanFitness.Api.Controllers;

[ApiController]
[Route("api/studios")]
public class StudiosController : ControllerBase
{
    private readonly IMediator _mediator;
    public StudiosController(IMediator mediator) => _mediator = mediator;

    [HttpGet("by-branch/{branchId:guid}")]
    public async Task<ActionResult<List<StudioDto>>> GetByBranch(Guid branchId, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetStudiosByBranchQuery(branchId), ct));

    [HttpPost]
    public async Task<ActionResult<StudioDto>> Create(CreateStudioCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetByBranch), new { branchId = result.BranchId }, result);
    }
}
