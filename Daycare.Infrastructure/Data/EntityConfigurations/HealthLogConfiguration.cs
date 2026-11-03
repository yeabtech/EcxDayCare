using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// EF Core configuration for the HealthLog entity.
/// </summary>
public class HealthLogConfiguration : IEntityTypeConfiguration<HealthLog>
{
    public void Configure(EntityTypeBuilder<HealthLog> builder)
    {
        builder.ToTable("HealthLogs", "public");

        builder.HasKey(h => h.Id).HasName("PK_HealthLogs");

        builder.Property(h => h.Id)
            .HasConversion(id => id.Value, value => HealthLogId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(h => h.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(h => h.Time)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(h => h.EventType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.Description)
            .HasMaxLength(500);

        builder.Property(h => h.StaffId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(h => h.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(h => h.ChildId)
            .HasDatabaseName("IX_HealthLogs_ChildId");

        builder.HasIndex(h => new { h.ChildId, h.Time })
            .HasDatabaseName("IX_HealthLogs_ChildId_Time")
            .IsUnique(false);
    }
}
