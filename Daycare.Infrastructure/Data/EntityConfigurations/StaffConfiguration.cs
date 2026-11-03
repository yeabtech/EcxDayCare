using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// Entity Framework configuration for Staff entity
/// </summary>
public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff", "public");

        builder.HasKey(s => s.Id).HasName("PK_Staff");

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => StaffId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Ignore(s => s.StaffId);

        builder.Property(s => s.FirstName)
            .HasColumnType("character varying(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.LastName)
            .HasColumnType("character varying(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Role)
            .HasConversion<string>()
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Phone)
            .HasColumnType("character varying(30)")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.Email)
            .HasColumnType("character varying(256)")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(s => s.Email)
            .HasDatabaseName("IX_Staff_Email")
            .IsUnique();

        builder.Property(s => s.HireDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(s => s.Certifications)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(s => s.ShiftStart)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(s => s.ShiftEnd)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(s => s.ClassroomId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? ClassroomId.Create(value.Value) : null)
            .HasColumnType("uuid")
            .HasColumnName("ClassroomId")
            .IsRequired(false);

        builder.Property(s => s.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationship with Classroom
        builder.HasOne(s => s.Classroom)
            .WithMany(c => c.StaffMembers)
            .HasForeignKey(s => s.ClassroomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.ClassroomId)
            .HasDatabaseName("IX_Staff_ClassroomId");
    }
}
