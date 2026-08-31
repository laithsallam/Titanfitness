namespace TitanFitness.Application.Branches;

public record BranchDto(Guid Id, string Name, string? Address, TimeOnly OpeningTime, TimeOnly ClosingTime);
