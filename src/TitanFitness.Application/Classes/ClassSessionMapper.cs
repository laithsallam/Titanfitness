using TitanFitness.Domain.Classes;

namespace TitanFitness.Application.Classes;

internal static class ClassSessionMapper
{
    public static ClassSessionDto ToDto(ClassSession s) => new(
        s.Id, s.ClassName, s.BranchId, s.StudioId, s.TrainerId, s.SessionDate, s.StartTime,
        s.DurationMinutes, s.CapacityLimit, s.Status.ToString(), s.Description,
        s.Bookings.Count(b => b.Status == BookingStatus.Booked), s.FillRate,
        s.Bookings.Select(b => new BookingDto(b.Id, b.MemberId, b.BookedOnUtc, b.Status.ToString(),
            b.WaitlistPosition, b.NotesForTrainer)).ToList());
}
