using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// Entity Framework configuration for Enrollment entity.
/// </summary>
public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments", "public");

        builder.HasKey(e => e.Id).HasName("PK_Enrollments");

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => EnrollmentId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Ignore(e => e.EnrollmentId);

        builder.Property(e => e.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.ClassroomId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? ClassroomId.Create(value.Value) : null)
            .HasColumnType("uuid");

        builder.Property(e => e.ProgramType)
            .HasConversion<string>()
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DaysOfWeek)
            .HasColumnType("character varying(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.StartDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.EndDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.ContractSigned)
            .IsRequired();

        builder.Property(e => e.ImmunizationRecord)
            .IsRequired();

        builder.Property(e => e.BirthCertificate)
            .IsRequired();

        builder.Property(e => e.MedicalForm)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EndReason)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Foreign keys
        builder.HasOne(e => e.Child)
            .WithMany()
            .HasForeignKey(e => e.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Classroom)
            .WithMany()
            .HasForeignKey(e => e.ClassroomId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.ChildId).HasDatabaseName("IX_Enrollments_ChildId");
        builder.HasIndex(e => e.Status).HasDatabaseName("IX_Enrollments_Status");
        builder.HasIndex(e => e.ClassroomId).HasDatabaseName("IX_Enrollments_ClassroomId");
    }
}
