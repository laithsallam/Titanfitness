using Microsoft.EntityFrameworkCore;
using TitanFitness.Api.Middleware;
using TitanFitness.Application;
using TitanFitness.Infrastructure;
using TitanFitness.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // DateOnly/TimeOnly serialize natively in System.Text.Json on .NET 8.
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Titan Fitness - Staff Portal API",
        Version = "v1",
        Description = "REST API for the Titan Fitness staff portal: branches, studios, members, plans, " +
                      "memberships, trainers, class sessions and check-ins. Built with DDD, CQRS (MediatR), " +
                      "EF Core Code First (Fluent API) and FluentValidation."
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Applies any pending migrations on startup in Development so `dotnet run`
// gives a ready-to-use database with no extra steps. Remove/guard this for
// production, where migrations should run as a deliberate release step.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TitanFitnessDbContext>();
    db.Database.Migrate();
}

app.Run();
