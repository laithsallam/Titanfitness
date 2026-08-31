using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Memberships;

namespace TitanFitness.Api.Controllers;

/// <summary>Backs Figures 4-7: Change Plan / Freeze Membership / guest passes.</summary>
[ApiController]
[Route("api/memberships")]
public class MembershipsController : ControllerBase
{
    private readonly IMediator _mediator;
    public MembershipsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MembershipDto>> GetById(Guid id, CancellationToken ct)
    {
        var membership = await _mediator.Send(new GetMembershipByIdQuery(id), ct);
        return membership is null ? NotFound() : Ok(membership);
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<MembershipDto>> Purchase(PurchaseMembershipCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Figure 7: Freeze Membership.</summary>
    [HttpPost("{id:guid}/freeze")]
    public async Task<ActionResult<MembershipDto>> Freeze(Guid id, [FromBody] FreezeRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new FreezeMembershipCommand(id, request.StartDate, request.DurationMonths, request.Reason, request.Notes), ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<MembershipDto>> Cancel(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new CancelMembershipCommand(id), ct));

    /// <summary>Figure 5 (Renew) and Figure 6 (Change Plan) - same operation, different plan/timing.</summary>
    [HttpPost("{id:guid}/renew-or-change-plan")]
    public async Task<ActionResult<MembershipDto>> RenewOrChangePlan(Guid id, [FromBody] RenewOrChangePlanRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new RenewOrChangePlanCommand(id, request.NewPlanId, request.EffectiveImmediately), ct));

    [HttpPost("{id:guid}/guest-passes")]
    public async Task<ActionResult<MembershipDto>> IssueGuestPass(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new IssueGuestPassCommand(id), ct));

    [HttpPost("{id:guid}/guest-passes/{guestPassId:guid}/redeem")]
    public async Task<ActionResult<MembershipDto>> RedeemGuestPass(Guid id, Guid guestPassId, [FromBody] RedeemGuestPassRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new RedeemGuestPassCommand(id, guestPassId, request.GuestName), ct));
}

public record FreezeRequest(DateOnly StartDate, int DurationMonths, TitanFitness.Domain.Memberships.FreezeReason Reason, string? Notes);
public record RenewOrChangePlanRequest(Guid NewPlanId, bool EffectiveImmediately);
public record RedeemGuestPassRequest(string? GuestName);
