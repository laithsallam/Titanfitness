namespace TitanFitness.Domain.Common;

/// <summary>A same-day [Open, Close) time window. Used for branch operating hours.</summary>
public sealed class TimeRange : ValueObject
{
    public TimeOnly Open { get; }
    public TimeOnly Close { get; }

    private TimeRange(TimeOnly open, TimeOnly close)
    {
        Open = open;
        Close = close;
    }

    public static TimeRange Create(TimeOnly open, TimeOnly close)
    {
        if (close <= open)
            throw new DomainException("Closing time must be after opening time.");
        return new TimeRange(open, close);
    }

    public bool Contains(TimeOnly time) => time >= Open && time < Close;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Open;
        yield return Close;
    }
}
