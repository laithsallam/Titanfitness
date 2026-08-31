namespace TitanFitness.Application.CheckIns;

public record CheckInDto(Guid Id, Guid MemberId, Guid BranchId, DateTime CheckInDateTimeUtc,
    string Result, string? RefusalReason);
