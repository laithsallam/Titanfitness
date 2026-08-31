using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TitanFitness.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add ...` work without spinning up the whole API
/// host. Only used by the EF Core CLI at design time - never by the running app.
/// </summary>
public class TitanFitnessDbContextFactory : IDesignTimeDbContextFactory<TitanFitnessDbContext>
{
    public TitanFitnessDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TitanFitnessDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=TitanFitness;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        return new TitanFitnessDbContext(optionsBuilder.Options);
    }
}
