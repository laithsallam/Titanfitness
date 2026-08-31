using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Branches;

namespace TitanFitness.Api.Controllers;

[ApiController]
[Route("api/branches")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;
    public BranchesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Figure 1 branch selector / general branch list.</summary>
    [HttpGet]
    public async Task<ActionResult<List<BranchDto>>> GetAll(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetBranchesQuery(), ct));

    [HttpPost]
    public async Task<ActionResult<BranchDto>> Create(CreateBranchCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }
}
