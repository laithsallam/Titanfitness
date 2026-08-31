using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Classes;

/// <summary>
/// One scheduled class: a name, a trainer, a studio, a start time and a
/// capacity. Owns its Bookings because "a session never takes more bookings
/// than its capacity... the first waiting is promoted automatically when a
/// place is freed" is a single atomic invariant over the whole booking list -
/// exactly the kind of rule an aggregate boundary exists to protect. Trainer,
/// Branch and Studio are referenced by ID only: this aggregate does not need
/// their full state to do its job, it only needs their identity plus a couple
/// of facts (studio capacity, trainer/studio double-booking) that are checked
/// by a domain service using data the application layer fetches for it - see
/// SessionSchedulingService.
/// </summary>
public sealed class ClassSession : AggregateRoot<Guid>
{
    private readonly List<Booking> _bookings = new();

    public string ClassName { get; private set; } = null!;
    public Guid BranchId { get; private set; }
    public Guid StudioId { get; private set; }
    public Guid TrainerId { get; private set; }
    public DateOnly SessionDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public int CapacityLimit { get; private set; }
    public SessionStatus Status { get; private set; }
    public string? Description { get; private set; }

    public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();

    private ClassSession() { }

    private ClassSession(Guid id, string className, Guid branchId, Guid studioId, Guid trainerId,
        DateOnly sessionDate, TimeOnly startTime, int durationMinutes, int capacityLimit, string? description)
        : base(id)
    {
        ClassName = className;
        BranchId = branchId;
        StudioId = studioId;
        TrainerId = trainerId;
        SessionDate = sessionDate;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        CapacityLimit = capacityLimit;
        Description = description;
        Status = SessionStatus.Open;
    }

    /// <summary>
    /// Studio capacity is passed in (not looked up) because ClassSession does
    /// not hold a reference to the Studio aggregate - the caller (a domain
    /// service backed by the application layer's repositories) already fetched
    /// it to check "a session cannot be given a capacity larger than the
    /// studio that holds it."
    /// </summary>
    public static ClassSession Schedule(string className, Guid branchId, Guid studioId, Guid trainerId,
        DateOnly sessionDate, TimeOnly startTime, int durationMinutes, int capacityLimit,
        int studioCapacity, string? description)
    {
        Guard.AgainstNullOrWhiteSpace(className, nameof(ClassName), 100);
        Guard.AgainstTooLong(description, nameof(Description), 500);
        Guard.AgainstNegativeOrZero(durationMinutes, nameof(DurationMinutes));
        Guard.AgainstNegativeOrZero(capacityLimit, nameof(CapacityLimit));

        if (capacityLimit > studioCapacity)
            throw new DomainException(
                $"Capacity limit ({capacityLimit}) cannot exceed the studio's capacity ({studioCapacity}).");

        return new ClassSession(Guid.NewGuid(), className, branchId, studioId, trainerId,
            sessionDate, startTime, durationMinutes, capacityLimit, description);
    }

    public TimeOnly EndTime => StartTime.Add(TimeSpan.FromMinutes(DurationMinutes));

    public bool OverlapsTimeWith(ClassSession other) =>
        SessionDate == other.SessionDate &&
        StartTime < other.EndTime &&
        other.StartTime < EndTime;

    public bool HasStartedAsOf(DateTime nowUtc, TimeSpan utcOffset)
    {
        var localNow = nowUtc + utcOffset;
        var sessionStart = SessionDate.ToDateTime(StartTime);
        return localNow >= sessionStart;
    }

    // ---- Booking --------------------------------------------------------

    private int BookedCount => _bookings.Count(b => b.Status == BookingStatus.Booked);

    /// <summary>
    /// "Staff book a member onto a session... A session never takes more
    /// bookings than its capacity. Once full, further members join a waiting
    /// list in the order they applied... A member cannot hold two places on
    /// one session." Overlap with the member's other sessions is a
    /// cross-aggregate check the caller must perform beforehand (see
    /// SessionSchedulingService.EnsureNoMemberDoubleBooking) since it needs
    /// every other session that member is booked on.
    /// </summary>
    public Booking Book(Guid memberId, DateTime bookedOnUtc, string? notesForTrainer)
    {
        if (Status is SessionStatus.Cancelled or SessionStatus.Completed or SessionStatus.InProgress)
            throw new DomainException("This session no longer accepts bookings.");

        if (_bookings.Any(b => b.MemberId == memberId && b.IsActive))
            throw new DomainException("This member already holds a place on this session.");

        Guard.AgainstTooLong(notesForTrainer, nameof(notesForTrainer), 500);

        Booking booking;
        if (BookedCount < CapacityLimit)
        {
            booking = new Booking(Guid.NewGuid(), memberId, bookedOnUtc, BookingStatus.Booked, null, notesForTrainer);
        }
        else
        {
            var nextPosition = _bookings.Count(b => b.Status == BookingStatus.Waitlisted) + 1;
            booking = new Booking(Guid.NewGuid(), memberId, bookedOnUtc, BookingStatus.Waitlisted, nextPosition, notesForTrainer);
        }

        _bookings.Add(booking);
        return booking;
    }

    /// <summary>"the first waiting is promoted automatically when a place is freed."</summary>
    public void CancelBooking(Guid bookingId)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId)
            ?? throw new DomainException("Booking not found on this session.");
        if (!booking.IsActive)
            throw new DomainException("This booking is no longer active.");

        var wasBooked = booking.Status == BookingStatus.Booked;
        booking.Cancel();

        if (wasBooked)
        {
            var next = _bookings
                .Where(b => b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefault();
            next?.PromoteFromWaitlist();
        }

        foreach (var wl in _bookings.Where(b => b.Status == BookingStatus.Waitlisted))
            wl.ShiftWaitlistPositionUp();
    }

    public void MarkAttended(Guid bookingId)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId && b.Status == BookingStatus.Booked)
            ?? throw new DomainException("No active booking found to mark attended.");
        booking.MarkAttended();
    }

    public void MarkNoShow(Guid bookingId)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId && b.Status == BookingStatus.Booked)
            ?? throw new DomainException("No active booking found to mark as no-show.");
        booking.MarkNoShow();
    }

    // ---- Session lifecycle -----------------------------------------------

    public void Start()
    {
        if (Status != SessionStatus.Open)
            throw new DomainException("Only an open session can start.");
        Status = SessionStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != SessionStatus.InProgress)
            throw new DomainException("Only a session in progress can be completed.");
        Status = SessionStatus.Completed;
    }

    public void CancelSession()
    {
        if (Status is SessionStatus.Completed or SessionStatus.Cancelled)
            throw new DomainException("This session cannot be cancelled from its current status.");
        Status = SessionStatus.Cancelled;
        foreach (var b in _bookings.Where(b => b.IsActive))
            b.Cancel();
    }

    public double FillRate => CapacityLimit == 0 ? 0 : (double)BookedCount / CapacityLimit;
}
