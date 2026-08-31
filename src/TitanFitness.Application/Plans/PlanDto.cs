namespace TitanFitness.Application.Plans;

public record PlanDto(
    Guid Id, string Name, decimal Price, int DurationInMonths,
    int MaxFreezeDays, int MaxNumberOfFreezes, int GuestPassQuota,
    string AccessScope, bool IsPublished);
