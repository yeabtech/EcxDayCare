using Daycare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// Entity Framework configuration for parent in-app notifications.
/// </summary>
public sealed class ParentNotificationConfiguration : IEntityTypeConfiguration<ParentNotification>
{
    public void Configure(EntityTypeBuilder<ParentNotification> builder)
    {
        builder.ToTable("ParentNotifications", "public");

        builder.HasKey(notification => notification.Id)
            .HasName("PK_ParentNotifications");

        builder.Property(notification => notification.Id)
            .HasConversion(id => id.Value, value => ParentNotificationId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(notification => notification.ParentId)
            .HasConversion(id => id.Value, value => ParentId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(notification => notification.DailyReportId)
            .HasConversion(id => id.Value, value => DailyReportId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(notification => notification.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(notification => notification.Message)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(notification => notification.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(notification => notification.ReadAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(notification => notification.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(notification => notification.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(notification => notification.Parent)
            .WithMany()
            .HasForeignKey(notification => notification.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(notification => notification.DailyReport)
            .WithMany()
            .HasForeignKey(notification => notification.DailyReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(notification => new { notification.ParentId, notification.DailyReportId })
            .IsUnique()
            .HasDatabaseName("IX_ParentNotifications_ParentId_DailyReportId_Unique");

        builder.HasIndex(notification => new { notification.ParentId, notification.IsRead, notification.CreatedAt })
            .HasDatabaseName("IX_ParentNotifications_ParentId_IsRead_CreatedAt");
    }
}
