namespace TitanFitness.Domain.Common;

/// <summary>
/// An inclusive [Start, End] span of calendar dates. Used by Membership periods
/// and Freeze windows, both of which need the same "does this overlap that"
/// and "does this fully contain that" checks.
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new DomainException("End date cannot be before start date.");
        return new DateRange(start, end);
    }

    public int TotalDays => End.DayNumber - Start.DayNumber + 1;

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public bool Overlaps(DateRange other) => Start <= other.End && other.Start <= End;

    /// <summary>Returns a new range with the same start but an End pushed forward by the given days.</summary>
    public DateRange ExtendEndBy(int days) => new(Start, End.AddDays(days));

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:yyyy-MM-dd} → {End:yyyy-MM-dd}";
}
