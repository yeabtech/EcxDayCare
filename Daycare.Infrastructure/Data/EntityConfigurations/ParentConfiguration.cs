using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

/// <summary>
/// Entity Framework configuration for Parent entity
/// </summary>
public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Parents", "public");

        builder.HasKey(p => p.Id).HasName("PK_Parents");

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => ParentId.Create(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Ignore(p => p.ParentId);

        // Name value object
        builder.OwnsOne(p => p.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnType("character varying(100)")
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("FirstName");

            nameBuilder.Property(n => n.LastName)
                .HasColumnType("character varying(100)")
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("LastName");
        });

        // ContactInfo value object
        builder.OwnsOne(p => p.ContactInfo, contactBuilder =>
        {
            contactBuilder.Property(c => c.Email)
                .HasColumnType("character varying(256)")
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("Email");

            contactBuilder.Property(c => c.PhoneNumber)
                .HasColumnType("character varying(20)")
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("PhoneNumber");
        });

        builder.Property(p => p.Address)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasColumnType("character varying(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasColumnType("character varying(1000)")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(p => p.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasMany(p => p.ParentChildren)
            .WithOne(pc => pc.Parent)
            .HasForeignKey(pc => pc.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity Framework configuration for ParentChild join entity
/// </summary>
public class ParentChildConfiguration : IEntityTypeConfiguration<ParentChild>
{
    public void Configure(EntityTypeBuilder<ParentChild> builder)
    {
        builder.ToTable("ParentChildren", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_ParentChildren_Role",
                "\"Role\" IN ('Guardian', 'Mother', 'Father')");
        });

        builder.HasKey(pc => new { pc.ParentId, pc.ChildId });

        builder.Property(pc => pc.ParentId)
            .HasConversion(id => id.Value, value => ParentId.Create(value))
            .HasColumnType("uuid");

        builder.Property(pc => pc.ChildId)
            .HasConversion(id => id.Value, value => ChildId.Create(value))
            .HasColumnType("uuid");

        builder.Property(pc => pc.Role)
            .HasConversion<string>()
            .HasColumnType("character varying(20)")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(ParentRole.Guardian);

        builder.Property(pc => pc.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(pc => pc.Parent)
            .WithMany(p => p.ParentChildren)
            .HasForeignKey(pc => pc.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Child)
            .WithMany(c => c.ParentChildren)
            .HasForeignKey(pc => pc.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
