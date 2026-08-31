namespace TitanFitness.Application.Common.Interfaces;

/// <summary>
/// Marker used only so Application can express "I need a unit of work" without
/// referencing EF Core directly. Actual persistence contracts are the
/// per-aggregate repository interfaces declared in the Domain layer.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}
