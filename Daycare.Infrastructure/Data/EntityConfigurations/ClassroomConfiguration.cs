using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// Entity Framework configuration for Classroom entity
/// </summary>
public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.ToTable("Classrooms", "public");

        builder.HasKey(c => c.Id).HasName("PK_Classrooms");

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => ClassroomId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Ignore(c => c.ClassroomId);

        builder.Property(c => c.Name)
            .HasColumnType("character varying(100)")
            .HasMaxLength(100)
            .IsRequired();

        // AgeGroup owned type
        builder.OwnsOne(c => c.AgeGroup, ab =>
        {
            ab.Property(a => a.MinAgeMonths)
                .HasColumnName("MinAgeMonths")
                .HasColumnType("integer")
                .IsRequired();

            ab.Property(a => a.MaxAgeMonths)
                .HasColumnName("MaxAgeMonths")
                .HasColumnType("integer")
                .IsRequired();
        });

        // Capacity owned type
        builder.OwnsOne(c => c.Capacity, cb =>
        {
            cb.Property(cap => cap.MaxCapacity)
                .HasColumnName("MaxCapacity")
                .HasColumnType("integer")
                .IsRequired();
        });

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.LeadTeacherId)
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(c => c.Description)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Children collection relationship
        builder.HasMany(c => c.Children)
            .WithOne(ch => ch.Classroom)
            .HasForeignKey(ch => ch.ClassroomId)
            .OnDelete(DeleteBehavior.SetNull);

        // Staff collection relationship
        builder.HasMany(c => c.StaffMembers)
            .WithOne(s => s.Classroom)
            .HasForeignKey(s => s.ClassroomId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
