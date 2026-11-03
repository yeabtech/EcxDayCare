using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// EF Core configuration for the Incident entity.
/// </summary>
public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents", "public");

        builder.HasKey(i => i.Id).HasName("PK_Incidents");

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => IncidentId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(i => i.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(i => i.IncidentTime)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(i => i.Location)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(i => i.InjuryType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.AffectedBodyPart)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.FirstAidGiven)
            .HasMaxLength(500);

        builder.Property(i => i.StaffId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(i => i.ParentNotified)
            .IsRequired();

        builder.Property(i => i.ParentSignature)
            .HasMaxLength(500);

        builder.Property(i => i.FollowUp)
            .HasMaxLength(1000);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(i => i.ChildId)
            .HasDatabaseName("IX_Incidents_ChildId");

        builder.HasIndex(i => new { i.ChildId, i.IncidentTime })
            .HasDatabaseName("IX_Incidents_ChildId_IncidentTime")
            .IsUnique(false);
    }
}
