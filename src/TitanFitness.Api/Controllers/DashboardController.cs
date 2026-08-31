using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Dashboard;

namespace TitanFitness.Api.Controllers;

/// <summary>Backs Figure 1: Dashboard.</summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] Guid? branchId, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetDashboardStatsQuery(branchId), ct));

    [HttpGet("upcoming-sessions")]
    public async Task<ActionResult<List<UpcomingSessionDto>>> GetUpcomingSessions([FromQuery] Guid? branchId, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetUpcomingSessionsQuery(branchId), ct));
}
