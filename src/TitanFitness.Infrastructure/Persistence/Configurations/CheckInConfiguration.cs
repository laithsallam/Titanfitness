using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Members;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("CheckIns");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CheckInDateTimeUtc).IsRequired();
        builder.Property(c => c.Result).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.RefusalReason).HasMaxLength(100);

        builder.HasOne<Member>().WithMany().HasForeignKey(c => c.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Branch>().WithMany().HasForeignKey(c => c.BranchId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.MemberId, c.CheckInDateTimeUtc });
        builder.HasIndex(c => new { c.BranchId, c.CheckInDateTimeUtc });
    }
}
