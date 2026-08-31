using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Classes;
using TitanFitness.Domain.Studios;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClassName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SessionDate).IsRequired();
        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.DurationMinutes).IsRequired();
        builder.Property(s => s.CapacityLimit).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasOne<Branch>().WithMany().HasForeignKey(s => s.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Studio>().WithMany().HasForeignKey(s => s.StudioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Trainer>().WithMany().HasForeignKey(s => s.TrainerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.TrainerId, s.SessionDate });
        builder.HasIndex(s => new { s.StudioId, s.SessionDate });
        builder.HasIndex(s => new { s.BranchId, s.SessionDate });

        builder.HasMany(s => s.Bookings)
            .WithOne()
            .HasForeignKey("ClassSessionId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Bookings)
            .HasField("_bookings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookedOnUtc).IsRequired();
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(b => b.WaitlistPosition);
        builder.Property(b => b.NotesForTrainer).HasMaxLength(500);

        // MemberId is a plain FK-shaped column (not an EF nav to Member) - a
        // Booking references the member who reserved the spot but does not
        // need to load the Member aggregate to do anything.
        builder.Property(b => b.MemberId).IsRequired();
        builder.HasIndex(b => b.MemberId);
    }
}
