using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Studios;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public class StudioConfiguration : IEntityTypeConfiguration<Studio>
{
    public void Configure(EntityTypeBuilder<Studio> builder)
    {
        builder.ToTable("Studios");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Capacity).IsRequired();

        // Studio -> Branch: reference by FK only, no navigation back (Branch
        // doesn't need to know its studios to do its job - see Branch's doc comment).
        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.BranchId);
    }
}
