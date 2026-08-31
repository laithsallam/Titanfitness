using FluentValidation;
using MediatR;

namespace TitanFitness.Application.Common.Behaviors;

/// <summary>
/// Runs every registered FluentValidation validator for a request before the
/// handler executes. This is the CQRS pipeline's one cross-cutting concern:
/// handlers stay focused on orchestration (load aggregate -> call domain
/// method -> save) and never write their own null/length checks - those are
/// either FluentValidation rules (shape of the request) or DomainException
/// thrown by the aggregate (business rules).
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
