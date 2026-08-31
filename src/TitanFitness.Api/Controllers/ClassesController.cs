using MediatR;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Classes;

namespace TitanFitness.Api.Controllers;

/// <summary>Backs Figures 8-11: Class Schedule / Add New Class / Book Session.</summary>
[ApiController]
[Route("api/class-sessions")]
public class ClassesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ClassesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ClassSessionDto>>> GetSchedule(
        [FromQuery] DateOnly date, [FromQuery] Guid? branchId, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetClassScheduleQuery(date, branchId), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassSessionDto>> GetById(Guid id, CancellationToken ct)
    {
        var session = await _mediator.Send(new GetClassSessionByIdQuery(id), ct);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<ClassSessionDto>> Schedule(ScheduleClassSessionCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/bookings")]
    public async Task<ActionResult<ClassSessionDto>> Book(Guid id, [FromBody] BookSessionRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new BookSessionCommand(id, request.MemberId, request.NotesForTrainer), ct));

    [HttpDelete("{id:guid}/bookings/{bookingId:guid}")]
    public async Task<ActionResult<ClassSessionDto>> CancelBooking(Guid id, Guid bookingId, CancellationToken ct) =>
        Ok(await _mediator.Send(new CancelBookingCommand(id, bookingId), ct));

    [HttpPost("{id:guid}/bookings/{bookingId:guid}/attended")]
    public async Task<ActionResult<ClassSessionDto>> MarkAttended(Guid id, Guid bookingId, CancellationToken ct) =>
        Ok(await _mediator.Send(new MarkAttendedCommand(id, bookingId), ct));

    [HttpPost("{id:guid}/bookings/{bookingId:guid}/no-show")]
    public async Task<ActionResult<ClassSessionDto>> MarkNoShow(Guid id, Guid bookingId, CancellationToken ct) =>
        Ok(await _mediator.Send(new MarkNoShowCommand(id, bookingId), ct));

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<ClassSessionDto>> Start(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new StartSessionCommand(id), ct));

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ClassSessionDto>> Complete(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new CompleteSessionCommand(id), ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ClassSessionDto>> Cancel(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new CancelSessionCommand(id), ct));
}

public record BookSessionRequest(Guid MemberId, string? NotesForTrainer);
