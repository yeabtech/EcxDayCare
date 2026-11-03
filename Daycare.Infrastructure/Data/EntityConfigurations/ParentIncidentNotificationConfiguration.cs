using Daycare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// Entity Framework configuration for parent incident notifications.
/// </summary>
public sealed class ParentIncidentNotificationConfiguration : IEntityTypeConfiguration<ParentIncidentNotification>
{
    public void Configure(EntityTypeBuilder<ParentIncidentNotification> builder)
    {
        builder.ToTable("ParentIncidentNotifications", "public");

        builder.HasKey(notification => notification.Id)
            .HasName("PK_ParentIncidentNotifications");

        builder.Property(notification => notification.Id)
            .HasConversion(id => id.Value, value => ParentIncidentNotificationId.Create(value))
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(notification => notification.ParentId)
            .HasConversion(id => id.Value, value => ParentId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(notification => notification.IncidentId)
            .HasConversion(id => id.Value, value => IncidentId.Create(value))
            .HasColumnType("uuid")
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

        builder.HasOne(notification => notification.Incident)
            .WithMany()
            .HasForeignKey(notification => notification.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(notification => new { notification.ParentId, notification.IncidentId })
            .IsUnique()
            .HasDatabaseName("IX_ParentIncidentNotifications_ParentId_IncidentId_Unique");

        builder.HasIndex(notification => new { notification.ParentId, notification.IsRead, notification.CreatedAt })
            .HasDatabaseName("IX_ParentIncidentNotifications_ParentId_IsRead_CreatedAt");
    }
}
