namespace TitanFitness.Application.Members;

public record MemberDto(
    Guid Id, string MembershipNumber, string FullName, string? Email, string? Phone,
    string? Address, DateOnly JoinedDate, string? PhotoUrl, Guid HomeBranchId);
