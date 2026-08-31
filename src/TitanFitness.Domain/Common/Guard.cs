namespace TitanFitness.Domain.Common;

/// <summary>Small guard-clause helpers so aggregates read as business rules, not if/throw noise.</summary>
public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string fieldName, int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} is required.");
        if (maxLength.HasValue && value.Length > maxLength.Value)
            throw new DomainException($"{fieldName} must be at most {maxLength.Value} characters.");
        return value;
    }

    public static string? AgainstTooLong(string? value, string fieldName, int maxLength)
    {
        if (value is not null && value.Length > maxLength)
            throw new DomainException($"{fieldName} must be at most {maxLength} characters.");
        return value;
    }

    public static int AgainstNegativeOrZero(int value, string fieldName)
    {
        if (value <= 0) throw new DomainException($"{fieldName} must be greater than zero.");
        return value;
    }

    public static int AgainstNegative(int value, string fieldName)
    {
        if (value < 0) throw new DomainException($"{fieldName} cannot be negative.");
        return value;
    }

    public static decimal AgainstNegative(decimal value, string fieldName)
    {
        if (value < 0) throw new DomainException($"{fieldName} cannot be negative.");
        return value;
    }
}
