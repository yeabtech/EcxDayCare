using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// EF Core fluent configuration for the DailyReport aggregate.
/// </summary>
public class DailyReportConfiguration : IEntityTypeConfiguration<DailyReport>
{
    public void Configure(EntityTypeBuilder<DailyReport> builder)
    {
        builder.ToTable("DailyReports", "public");

        builder.HasKey(r => r.Id).HasName("PK_DailyReports");

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => DailyReportId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        // Suppress EF-generated alias property
        builder.Ignore(r => r.DailyReportId);

        builder.Property(r => r.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(r => r.Date)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(r => r.Mood)
            .HasColumnType("character varying(100)")
            .HasMaxLength(100);

        builder.Property(r => r.MealsSummary)
            .HasColumnType("character varying(1000)")
            .HasMaxLength(1000);

        builder.Property(r => r.NapSummary)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500);

        builder.Property(r => r.ActivitiesSummary)
            .HasColumnType("character varying(1000)")
            .HasMaxLength(1000);

        builder.Property(r => r.DiaperSummary)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500);

        builder.Property(r => r.Milestones)
            .HasColumnType("character varying(1000)")
            .HasMaxLength(1000);

        builder.Property(r => r.Concerns)
            .HasColumnType("character varying(1000)")
            .HasMaxLength(1000);

        builder.Property(r => r.StaffId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(r => r.SentToParent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.SentAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(r => r.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(r => r.Child)
            .WithMany()
            .HasForeignKey(r => r.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one report per child per day
        builder.HasIndex(r => new { r.ChildId, r.Date })
            .IsUnique()
            .HasDatabaseName("IX_DailyReports_ChildId_Date_Unique");

        // Common query indexes
        builder.HasIndex(r => r.ChildId).HasDatabaseName("IX_DailyReports_ChildId");
        builder.HasIndex(r => r.Date).HasDatabaseName("IX_DailyReports_Date");
        builder.HasIndex(r => r.StaffId).HasDatabaseName("IX_DailyReports_StaffId");
        builder.HasIndex(r => r.SentToParent).HasDatabaseName("IX_DailyReports_SentToParent");
    }
}
