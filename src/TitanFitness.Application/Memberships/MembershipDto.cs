namespace TitanFitness.Application.Memberships;

public record AgreedTermsDto(decimal PricePaid, int DurationInMonths, int MaxFreezeDays,
    int MaxNumberOfFreezes, int GuestPassQuota, string AccessScope);

public record FreezeDto(Guid Id, DateOnly StartDate, DateOnly EndDate, string Reason, string? Notes);

public record GuestPassDto(Guid Id, DateOnly IssuedOn, DateOnly? UsedOn, string? GuestName);

public record MembershipDto(
    Guid Id, Guid MemberId, Guid PlanId, DateOnly StartDate, DateOnly EndDate,
    string Status, bool IsCancelled, AgreedTermsDto Terms,
    int RemainingFreezeDays, int RemainingFreezeCount, int RemainingGuestPasses,
    List<FreezeDto> Freezes, List<GuestPassDto> GuestPasses);
