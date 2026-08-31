namespace TitanFitness.Application.Classes;

public record BookingDto(Guid Id, Guid MemberId, DateTime BookedOnUtc, string Status,
    int? WaitlistPosition, string? NotesForTrainer);

public record ClassSessionDto(
    Guid Id, string ClassName, Guid BranchId, Guid StudioId, Guid TrainerId,
    DateOnly SessionDate, TimeOnly StartTime, int DurationMinutes, int CapacityLimit,
    string Status, string? Description, int BookedCount, double FillRate,
    List<BookingDto> Bookings);
