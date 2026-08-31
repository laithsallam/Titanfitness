using TitanFitness.Domain.Classes;
using TitanFitness.Domain.Common;
using Xunit;

namespace TitanFitness.Domain.Tests;

public class ClassSessionTests
{
    private static ClassSession NewSession(int capacity = 2) => ClassSession.Schedule(
        "HIIT Core Blast", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        new DateOnly(2026, 9, 1), new TimeOnly(9, 0), 45, capacity, studioCapacity: 30, description: null);

    [Fact]
    public void Capacity_cannot_exceed_studio_capacity()
    {
        Assert.Throws<DomainException>(() => ClassSession.Schedule(
            "HIIT", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2026, 9, 1), new TimeOnly(9, 0), 45, capacityLimit: 40, studioCapacity: 30, description: null));
    }

    [Fact]
    public void Bookings_beyond_capacity_are_waitlisted_in_order()
    {
        var session = NewSession(capacity: 1);
        var first = session.Book(Guid.NewGuid(), DateTime.UtcNow, null);
        var second = session.Book(Guid.NewGuid(), DateTime.UtcNow, null);

        Assert.Equal(BookingStatus.Booked, first.Status);
        Assert.Equal(BookingStatus.Waitlisted, second.Status);
        Assert.Equal(1, second.WaitlistPosition);
    }

    [Fact]
    public void Cancelling_a_booked_place_promotes_the_first_waitlisted_member()
    {
        var session = NewSession(capacity: 1);
        var first = session.Book(Guid.NewGuid(), DateTime.UtcNow, null);
        var second = session.Book(Guid.NewGuid(), DateTime.UtcNow, null);

        session.CancelBooking(first.Id);

        Assert.Equal(BookingStatus.Booked, second.Status);
        Assert.Null(second.WaitlistPosition);
    }

    [Fact]
    public void A_member_cannot_hold_two_places_on_the_same_session()
    {
        var session = NewSession(capacity: 5);
        var memberId = Guid.NewGuid();
        session.Book(memberId, DateTime.UtcNow, null);

        Assert.Throws<DomainException>(() => session.Book(memberId, DateTime.UtcNow, null));
    }

    [Fact]
    public void Session_never_exceeds_its_capacity_limit_of_booked_places()
    {
        var session = NewSession(capacity: 2);
        session.Book(Guid.NewGuid(), DateTime.UtcNow, null);
        session.Book(Guid.NewGuid(), DateTime.UtcNow, null);
        var third = session.Book(Guid.NewGuid(), DateTime.UtcNow, null);

        Assert.Equal(BookingStatus.Waitlisted, third.Status);
        Assert.Equal(2, session.Bookings.Count(b => b.Status == BookingStatus.Booked));
    }
}
