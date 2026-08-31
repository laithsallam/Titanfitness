using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TitanFitness.Application.Common.Interfaces;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Classes;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Domain.Studios;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Common;
using TitanFitness.Infrastructure.Persistence;
using TitanFitness.Infrastructure.Repositories;

namespace TitanFitness.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TitanFitnessDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TitanFitness")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IStudioRepository, StudioRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<IClassSessionRepository, ClassSessionRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();

        return services;
    }
}
