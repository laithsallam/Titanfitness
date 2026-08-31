using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Classes;

/// <summary>
/// A member's place on a session. Child entity of ClassSession: whether a
/// booking lands as Booked or Waitlisted, and who gets promoted when a spot
/// frees up, are facts about the *session as a whole*, not about one booking
/// in isolation - so, like Freeze/GuestPass on Membership, this has identity
/// but no independent lifecycle or repository.
/// </summary>
public sealed class Booking : Entity<Guid>
{
    public Guid MemberId { get; private set; }
    public DateTime BookedOnUtc { get; private set; }
    public BookingStatus Status { get; private set; }
    public int? WaitlistPosition { get; private set; }
    public string? NotesForTrainer { get; private set; }

    private Booking() { }

    internal Booking(Guid id, Guid memberId, DateTime bookedOnUtc, BookingStatus status,
        int? waitlistPosition, string? notesForTrainer) : base(id)
    {
        MemberId = memberId;
        BookedOnUtc = bookedOnUtc;
        Status = status;
        WaitlistPosition = waitlistPosition;
        NotesForTrainer = notesForTrainer;
    }

    internal void PromoteFromWaitlist()
    {
        Status = BookingStatus.Booked;
        WaitlistPosition = null;
    }

    internal void ShiftWaitlistPositionUp()
    {
        if (WaitlistPosition.HasValue && WaitlistPosition.Value > 0)
            WaitlistPosition -= 1;
    }

    internal void Cancel() => Status = BookingStatus.Cancelled;
    internal void MarkAttended() => Status = BookingStatus.Attended;
    internal void MarkNoShow() => Status = BookingStatus.NoShow;

    public bool IsActive => Status is BookingStatus.Booked or BookingStatus.Waitlisted;
}
