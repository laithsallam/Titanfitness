using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Plans;

/// <summary>
/// A catalogue offering: price, duration, freeze/guest-pass allowances, and
/// scope. Plan is mutable over time (management changes prices, retires plans),
/// but that mutability is exactly why a Membership never references a Plan's
/// live fields for its own terms - see AgreedTerms in the Memberships module.
/// This aggregate only has to protect its own fields and its Published/Retired
/// lifecycle; it knows nothing about who has purchased it.
/// </summary>
public sealed class Plan : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int DurationInMonths { get; private set; }
    public int MaxFreezeDays { get; private set; }
    public int MaxNumberOfFreezes { get; private set; }
    public int GuestPassQuota { get; private set; }
    public AccessScope AccessScope { get; private set; }
    public bool IsPublished { get; private set; }

    private Plan() { }

    private Plan(Guid id, string name, decimal price, int durationInMonths, int maxFreezeDays,
        int maxNumberOfFreezes, int guestPassQuota, AccessScope accessScope) : base(id)
    {
        Name = name;
        Price = price;
        DurationInMonths = durationInMonths;
        MaxFreezeDays = maxFreezeDays;
        MaxNumberOfFreezes = maxNumberOfFreezes;
        GuestPassQuota = guestPassQuota;
        AccessScope = accessScope;
        IsPublished = false;
    }

    public static Plan Create(string name, decimal price, int durationInMonths, int maxFreezeDays,
        int maxNumberOfFreezes, int guestPassQuota, AccessScope accessScope)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 50);
        Guard.AgainstNegative(price, nameof(Price));
        if (decimal.Round(price, 2) != price)
            throw new DomainException("Price may have at most 2 decimal places.");
        Guard.AgainstNegativeOrZero(durationInMonths, nameof(DurationInMonths));
        Guard.AgainstNegative(maxFreezeDays, nameof(MaxFreezeDays));
        Guard.AgainstNegative(maxNumberOfFreezes, nameof(MaxNumberOfFreezes));
        Guard.AgainstNegative(guestPassQuota, nameof(GuestPassQuota));

        return new Plan(Guid.NewGuid(), name, price, durationInMonths, maxFreezeDays,
            maxNumberOfFreezes, guestPassQuota, accessScope);
    }

    /// <summary>
    /// Updates the catalogue entry going forward. This intentionally has no
    /// effect on AgreedTerms already copied into existing Memberships - see
    /// the note on that type. Callers surface a warning like
    /// "412 active memberships keep the terms they were sold" using a count
    /// obtained from the Membership repository, not from this aggregate.
    /// </summary>
    public void UpdateDetails(string name, decimal price, int durationInMonths, int maxFreezeDays,
        int maxNumberOfFreezes, int guestPassQuota, AccessScope accessScope)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 50);
        Guard.AgainstNegative(price, nameof(Price));
        if (decimal.Round(price, 2) != price)
            throw new DomainException("Price may have at most 2 decimal places.");
        Guard.AgainstNegativeOrZero(durationInMonths, nameof(DurationInMonths));
        Guard.AgainstNegative(maxFreezeDays, nameof(MaxFreezeDays));
        Guard.AgainstNegative(maxNumberOfFreezes, nameof(MaxNumberOfFreezes));
        Guard.AgainstNegative(guestPassQuota, nameof(GuestPassQuota));

        Name = name;
        Price = price;
        DurationInMonths = durationInMonths;
        MaxFreezeDays = maxFreezeDays;
        MaxNumberOfFreezes = maxNumberOfFreezes;
        GuestPassQuota = guestPassQuota;
        AccessScope = accessScope;
    }

    public void Publish() => IsPublished = true;

    public void Retire() => IsPublished = false;
}
