using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Daycare.Infrastructure.Identity;

namespace Daycare.Infrastructure.Data.EntityConfigurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "public");

        builder.HasKey(rt => rt.Id).HasName("PK_RefreshTokens");

        builder.Property(rt => rt.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(rt => rt.UserId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnType("character varying(256)")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .HasDatabaseName("IX_RefreshTokens_Token")
            .IsUnique();

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rt => rt.RevokedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(rt => rt.ReplacedByToken)
            .HasColumnType("character varying(256)")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
