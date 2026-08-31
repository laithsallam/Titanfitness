using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Address).HasMaxLength(200);

        builder.OwnsOne(b => b.OperatingHours, oh =>
        {
            oh.Property(x => x.Open).HasColumnName("OpeningTime").IsRequired();
            oh.Property(x => x.Close).HasColumnName("ClosingTime").IsRequired();
        });

        builder.Navigation(b => b.OperatingHours).IsRequired();
    }
}
