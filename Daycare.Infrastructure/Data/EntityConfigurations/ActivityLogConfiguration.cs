using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// EF Core configuration for the ActivityLog entity.
/// </summary>
public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs", "public");

        builder.HasKey(a => a.Id).HasName("PK_ActivityLogs");

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => ActivityLogId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(a => a.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(a => a.DailyReportId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? DailyReportId.Create(value.Value) : null)
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(a => a.Time)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.DurationMinutes)
            .IsRequired(false);

        builder.Property(a => a.ActivityType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.Participation)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.StaffId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Foreign Key
        builder.HasOne(a => a.Child)
            .WithMany()
            .HasForeignKey(a => a.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.DailyReport)
            .WithMany()
            .HasForeignKey(a => a.DailyReportId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(a => a.ChildId).HasDatabaseName("IX_ActivityLogs_ChildId");
        builder.HasIndex(a => a.DailyReportId).HasDatabaseName("IX_ActivityLogs_DailyReportId");
        builder.HasIndex(a => a.Time).HasDatabaseName("IX_ActivityLogs_Time");
        builder.HasIndex(a => new { a.ChildId, a.Time }).HasDatabaseName("IX_ActivityLogs_ChildId_Time");
    }
}
