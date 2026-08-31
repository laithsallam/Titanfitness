using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Members;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");
        builder.HasKey(m => m.Id);

        builder.OwnsOne(m => m.MembershipNumber, mn =>
        {
            mn.Property(x => x.Value)
                .HasColumnName("MembershipNumber")
                .HasMaxLength(10)
                .IsRequired();
            mn.HasIndex(x => x.Value).IsUnique();
        });
        builder.Navigation(m => m.MembershipNumber).IsRequired();

        builder.Property(m => m.FullName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(100);
        builder.Property(m => m.Phone).HasMaxLength(20);
        builder.Property(m => m.Address).HasMaxLength(200);
        builder.Property(m => m.JoinedDate).IsRequired();
        builder.Property(m => m.PhotoUrl);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(m => m.HomeBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.HomeBranchId);
    }
}
