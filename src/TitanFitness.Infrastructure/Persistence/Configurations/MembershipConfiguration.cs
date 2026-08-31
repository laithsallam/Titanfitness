using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the Membership aggregate, including its two child-entity collections
/// (Freeze, GuestPass) and its AgreedTerms snapshot. Freeze and GuestPass get
/// real tables with a shadow MembershipId FK - they have genuine identity, so
/// OwnsMany (which is meant for identity-less owned collections) is the wrong
/// tool here. The aggregate boundary they live inside is enforced by only
/// ever loading/saving them through IMembershipRepository, not by the schema.
/// </summary>
public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.PurchaseDateUtc).IsRequired();
        builder.Property(m => m.IsCancelled).IsRequired();
        builder.Property(m => m.CancelledOn);

        builder.OwnsOne(m => m.Period, p =>
        {
            p.Property(x => x.Start).HasColumnName("StartDate").IsRequired();
            p.Property(x => x.End).HasColumnName("EndDate").IsRequired();
        });
        builder.Navigation(m => m.Period).IsRequired();

        builder.OwnsOne(m => m.Terms, t =>
        {
            t.Property(x => x.PricePaid).HasColumnName("PricePaid").HasColumnType("decimal(10,2)").IsRequired();
            t.Property(x => x.DurationInMonths).HasColumnName("TermDurationInMonths").IsRequired();
            t.Property(x => x.MaxFreezeDays).HasColumnName("TermMaxFreezeDays").IsRequired();
            t.Property(x => x.MaxNumberOfFreezes).HasColumnName("TermMaxNumberOfFreezes").IsRequired();
            t.Property(x => x.GuestPassQuota).HasColumnName("TermGuestPassQuota").IsRequired();
            t.Property(x => x.AccessScope).HasColumnName("TermAccessScope").HasConversion<string>().HasMaxLength(20).IsRequired();
        });
        builder.Navigation(m => m.Terms).IsRequired();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(m => m.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(m => m.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.MemberId);
        builder.HasIndex(m => m.PlanId);

        builder.HasMany(m => m.Freezes)
            .WithOne()
            .HasForeignKey("MembershipId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.Freezes)
            .HasField("_freezes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(m => m.GuestPasses)
            .WithOne()
            .HasForeignKey("MembershipId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.GuestPasses)
            .HasField("_guestPasses")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class FreezeConfiguration : IEntityTypeConfiguration<Freeze>
{
    public void Configure(EntityTypeBuilder<Freeze> builder)
    {
        builder.ToTable("Freezes");
        builder.HasKey(f => f.Id);

        builder.OwnsOne(f => f.Period, p =>
        {
            p.Property(x => x.Start).HasColumnName("StartDate").IsRequired();
            p.Property(x => x.End).HasColumnName("EndDate").IsRequired();
        });
        builder.Navigation(f => f.Period).IsRequired();

        builder.Property(f => f.Reason).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(f => f.Notes).HasMaxLength(200);
        builder.Property(f => f.RequestedOnUtc).IsRequired();
    }
}

public class GuestPassConfiguration : IEntityTypeConfiguration<GuestPass>
{
    public void Configure(EntityTypeBuilder<GuestPass> builder)
    {
        builder.ToTable("GuestPasses");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.IssuedOn).IsRequired();
        builder.Property(g => g.UsedOn);
        builder.Property(g => g.GuestName).HasMaxLength(100);
    }
}
