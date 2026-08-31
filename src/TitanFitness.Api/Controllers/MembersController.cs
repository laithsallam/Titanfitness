using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Members;
using TitanFitness.Application.Memberships;
using TitanFitness.Application.CheckIns;

namespace TitanFitness.Api.Controllers;

/// <summary>Backs Figures 2-5: Member Directory / Add New Member / Member Profile.</summary>
[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;
    public MembersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<MemberDto>>> Search(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await _mediator.Send(new SearchMembersQuery(search, page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MemberDto>> GetById(Guid id, CancellationToken ct)
    {
        var member = await _mediator.Send(new GetMemberByIdQuery(id), ct);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MemberDto>> Update(Guid id, [FromBody] UpdateMemberRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateMemberCommand(id, request.FullName, request.Email, request.Phone, request.Address, request.PhotoUrl), ct);
        return Ok(result);
    }

    /// <summary>The member's membership history - powers the "Current Plan" card and past memberships.</summary>
    [HttpGet("{id:guid}/memberships")]
    public async Task<ActionResult<List<MembershipDto>>> GetMemberships(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetMembershipsForMemberQuery(id), ct));

    [HttpGet("{id:guid}/check-ins")]
    public async Task<ActionResult<List<CheckInDto>>> GetCheckInHistory(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetCheckInHistoryQuery(id), ct));
}

public record UpdateMemberRequest(string FullName, string? Email, string? Phone, string? Address, string? PhotoUrl);
