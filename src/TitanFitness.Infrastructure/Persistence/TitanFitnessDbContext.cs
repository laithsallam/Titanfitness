using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Classes;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Domain.Studios;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Infrastructure.Persistence;

public sealed class TitanFitnessDbContext : DbContext
{
    public TitanFitnessDbContext(DbContextOptions<TitanFitnessDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TitanFitnessDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
