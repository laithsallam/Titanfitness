namespace TitanFitness.Application.Trainers;

public record TrainerDto(Guid Id, string Name, string? Email, string? Phone, bool IsActive);
