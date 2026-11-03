using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// EF Core configuration for the NapLog entity.
/// </summary>
public class NapLogConfiguration : IEntityTypeConfiguration<NapLog>
{
    public void Configure(EntityTypeBuilder<NapLog> builder)
    {
        builder.ToTable("NapLogs", "public");

        builder.HasKey(n => n.Id).HasName("PK_NapLogs");

        builder.Property(n => n.Id)
            .HasConversion(id => id.Value, value => NapLogId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(n => n.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(n => n.DailyReportId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value.HasValue ? DailyReportId.Create(value.Value) : null)
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(n => n.StartTime)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(n => n.EndTime)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(n => n.Quality)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.StaffId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(n => n.Notes)
            .HasMaxLength(1000);

        builder.Property(n => n.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Ignore computed property
        builder.Ignore(n => n.DurationMinutes);

        // Foreign Key
        builder.HasOne(n => n.Child)
            .WithMany()
            .HasForeignKey(n => n.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.DailyReport)
            .WithMany()
            .HasForeignKey(n => n.DailyReportId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(n => n.ChildId).HasDatabaseName("IX_NapLogs_ChildId");
        builder.HasIndex(n => n.DailyReportId).HasDatabaseName("IX_NapLogs_DailyReportId");
        builder.HasIndex(n => n.StartTime).HasDatabaseName("IX_NapLogs_StartTime");
        builder.HasIndex(n => new { n.ChildId, n.StartTime }).HasDatabaseName("IX_NapLogs_ChildId_StartTime");
    }
}
