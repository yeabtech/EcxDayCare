using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// Entity Framework configuration for Attendance aggregate entity.
/// Maps table, constraints, typed ID conversions, and relationships.
/// </summary>
public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances", "public");

        builder.HasKey(a => a.Id).HasName("PK_Attendances");

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => AttendanceId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Ignore(a => a.AttendanceId);

        builder.Property(a => a.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(a => a.ClassroomId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? ClassroomId.Create(value.Value) : null)
            .HasColumnType("uuid");

        builder.Property(a => a.Date)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.CheckInTime)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.CheckOutTime)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.DroppedOffBy)
            .HasColumnType("character varying(150)")
            .HasMaxLength(150);

        builder.Property(a => a.PickedUpBy)
            .HasColumnType("character varying(150)")
            .HasMaxLength(150);

        builder.Property(a => a.Notes)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500);

        builder.Property(a => a.StaffId)
            .HasColumnType("uuid");

        builder.Property(a => a.CheckOutStaffId)
            .HasColumnType("uuid");

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Foreign keys
        builder.HasOne(a => a.Child)
            .WithMany()
            .HasForeignKey(a => a.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Classroom)
            .WithMany()
            .HasForeignKey(a => a.ClassroomId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for high-frequency queries
        builder.HasIndex(a => a.ChildId).HasDatabaseName("IX_Attendances_ChildId");
        builder.HasIndex(a => a.Date).HasDatabaseName("IX_Attendances_Date");
        builder.HasIndex(a => a.Status).HasDatabaseName("IX_Attendances_Status");
        builder.HasIndex(a => a.ClassroomId).HasDatabaseName("IX_Attendances_ClassroomId");
        builder.HasIndex(a => new { a.ChildId, a.Date }).HasDatabaseName("IX_Attendances_ChildId_Date");
    }
}
