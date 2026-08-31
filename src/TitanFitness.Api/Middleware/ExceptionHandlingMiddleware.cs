using System.Net;
using System.Text.Json;
using FluentValidation;
using TitanFitness.Domain.Common;

namespace TitanFitness.Api.Middleware;

/// <summary>
/// Translates domain/application failures into HTTP responses so controllers
/// stay free of try/catch: a DomainException (a broken business rule, e.g.
/// "freeze exceeds the allowance") becomes 422, a FluentValidation failure
/// (malformed request shape) becomes 400, "not found" reads become 404 via
/// null DTOs handled in the controller, and anything unexpected is a 500 with
/// no internal details leaked to the client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, "Validation failed",
                ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (DomainException ex)
        {
            await WriteProblem(context, HttpStatusCode.UnprocessableEntity, "Business rule violation", new[] { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", Array.Empty<string>());
        }
    }

    private static async Task WriteProblem(HttpContext context, HttpStatusCode status, string title, IEnumerable<string> errors)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;
        var body = JsonSerializer.Serialize(new { title, status = (int)status, errors });
        await context.Response.WriteAsync(body);
    }
}
