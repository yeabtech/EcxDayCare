using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// EF Core configuration for the FeedingLog entity.
/// </summary>
public class FeedingLogConfiguration : IEntityTypeConfiguration<FeedingLog>
{
    public void Configure(EntityTypeBuilder<FeedingLog> builder)
    {
        builder.ToTable("FeedingLogs", "public");

        builder.HasKey(f => f.Id).HasName("PK_FeedingLogs");

        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => FeedingLogId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(f => f.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(f => f.DailyReportId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? DailyReportId.Create(value.Value) : null)
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(f => f.Time)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(f => f.FeedingType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.FoodItems)
            .HasMaxLength(500);

        builder.Property(f => f.Amount)
            .HasMaxLength(100);

        builder.Property(f => f.PortionConsumed)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.StaffId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(f => f.Notes)
            .HasMaxLength(1000);

        builder.Property(f => f.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Foreign Key
        builder.HasOne(f => f.Child)
            .WithMany()
            .HasForeignKey(f => f.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.DailyReport)
            .WithMany()
            .HasForeignKey(f => f.DailyReportId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(f => f.ChildId).HasDatabaseName("IX_FeedingLogs_ChildId");
        builder.HasIndex(f => f.DailyReportId).HasDatabaseName("IX_FeedingLogs_DailyReportId");
        builder.HasIndex(f => f.Time).HasDatabaseName("IX_FeedingLogs_Time");
        builder.HasIndex(f => new { f.ChildId, f.Time }).HasDatabaseName("IX_FeedingLogs_ChildId_Time");
    }
}
