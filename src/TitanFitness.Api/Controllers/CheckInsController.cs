using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.CheckIns;

namespace TitanFitness.Api.Controllers;

/// <summary>Backs the "New Check-in" / "Manual Check-in" quick actions on the Dashboard.</summary>
[ApiController]
[Route("api/check-ins")]
public class CheckInsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CheckInsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<CheckInDto>> Create(PerformCheckInCommand command, CancellationToken ct) =>
        Ok(await _mediator.Send(command, ct));
}
