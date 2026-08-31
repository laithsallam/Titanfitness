namespace TitanFitness.Domain.Common;

/// <summary>
/// Thrown when a caller tries to push an aggregate into a state that violates
/// one of its invariants (e.g. freezing past the plan's cap, double-booking a
/// session). The API layer maps this to HTTP 422 / 409, never 500.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
