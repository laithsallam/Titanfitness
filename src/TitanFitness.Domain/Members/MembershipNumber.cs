using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Members;

/// <summary>
/// The business key staff actually search by (e.g. "TF-8932"). Modeled as a
/// value object rather than a bare string so formatting/length rules live in
/// one place and can't drift between the API DTO and the entity.
/// </summary>
public sealed class MembershipNumber : ValueObject
{
    public string Value { get; }

    private MembershipNumber(string value) => Value = value;

    public static MembershipNumber Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, "Membership number", 10);
        return new MembershipNumber(value.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
