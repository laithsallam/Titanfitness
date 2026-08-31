using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Price).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(p => p.DurationInMonths).IsRequired();
        builder.Property(p => p.MaxFreezeDays).IsRequired();
        builder.Property(p => p.MaxNumberOfFreezes).IsRequired();
        builder.Property(p => p.GuestPassQuota).IsRequired();
        builder.Property(p => p.AccessScope).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.IsPublished).IsRequired();
    }
}
