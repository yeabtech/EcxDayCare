using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;
using Daycare.Infrastructure.Identity;

namespace Daycare.Infrastructure.Data;

/// <summary>
/// The main database context for the Daycare Management System.
/// Provides access to all database entities and ASP.NET Core Identity tables.
/// </summary>
public class DaycareDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    /// <summary>
    /// Gets or sets the Children DbSet.
    /// </summary>
    public DbSet<Child> Children { get; set; }

    /// <summary>
    /// Gets or sets the Parents DbSet.
    /// </summary>
    public DbSet<Parent> Parents { get; set; }

    /// <summary>
    /// Gets or sets the Classrooms DbSet.
    /// </summary>
    public DbSet<Classroom> Classrooms { get; set; }

    /// <summary>
    /// Gets or sets the ParentChildren join DbSet.
    /// </summary>
    public DbSet<ParentChild> ParentChildren { get; set; }

    /// <summary>
    /// Gets or sets the Staff DbSet.
    /// </summary>
    public DbSet<Staff> Staff { get; set; }

    /// <summary>
    /// Gets or sets the RefreshTokens DbSet.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Gets or sets the Enrollments DbSet.
    /// </summary>
    public DbSet<Enrollment> Enrollments { get; set; }

    /// <summary>
    /// Gets or sets the Attendances DbSet.
    /// </summary>
    public DbSet<Attendance> Attendances { get; set; }

    /// <summary>
    /// Gets or sets the DailyReports DbSet.
    /// </summary>
    public DbSet<DailyReport> DailyReports { get; set; }

    /// <summary>
    /// Gets or sets the FeedingLogs DbSet.
    /// </summary>
    public DbSet<FeedingLog> FeedingLogs { get; set; }

    /// <summary>
    /// Gets or sets the NapLogs DbSet.
    /// </summary>
    public DbSet<NapLog> NapLogs { get; set; }

    /// <summary>
    /// Gets or sets the ActivityLogs DbSet.
    /// </summary>
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    /// <summary>
    /// Gets or sets the Incidents DbSet.
    /// </summary>
    public DbSet<Incident> Incidents { get; set; }

    /// <summary>
    /// Gets or sets the HealthLogs DbSet.
    /// </summary>
    public DbSet<HealthLog> HealthLogs { get; set; }

    /// <summary>
    /// Gets or sets parent in-app notifications.
    /// </summary>
    public DbSet<ParentNotification> ParentNotifications { get; set; }

    public DbSet<ParentIncidentNotification> ParentIncidentNotifications { get; set; }

    public DaycareDbContext(DbContextOptions<DaycareDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore value object types that should not be entities
        modelBuilder.Ignore<Daycare.Domain.Entities.ChildId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.AttendanceId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.ActivityLogId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.DailyReportId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.EnrollmentId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.FeedingLogId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.HealthLogId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.IncidentId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.NapLogId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.StaffId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.ParentNotificationId>();
        modelBuilder.Ignore<Daycare.Domain.Entities.ParentIncidentNotificationId>();

        ConfigureChildEntity(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DaycareDbContext).Assembly);

        // Configure all DateTime properties to UTC for PostgreSQL timestamp with time zone compatibility
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.ToUniversalTime(),
                        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Local).ToUniversalTime()));
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                        v => v.HasValue ? v.Value.ToUniversalTime() : v,
                        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : DateTime.SpecifyKind(v.Value, DateTimeKind.Local).ToUniversalTime()) : v));
                }
            }
        }
    }

    /// <summary>
    /// Configures the Child entity with proper database mappings and constraints.
    /// </summary>
    private static void ConfigureChildEntity(ModelBuilder modelBuilder)
    {
        var childBuilder = modelBuilder.Entity<Child>();

        // Table configuration with check constraints
        childBuilder.ToTable("Children", schema: "public", t =>
        {
            t.HasCheckConstraint("CK_Children_Gender",
                "\"Gender\" IN ('Male', 'Female', 'Other', 'PreferNotToSay')");
            t.HasCheckConstraint("CK_Children_Status",
                "\"Status\" IN ('Enrolled', 'Inactive', 'Withdrawn')");
        });

        // Primary key with UUID type
        childBuilder.HasKey(c => c.Id)
            .HasName("PK_Children");

        // ID conversion - Store as UUID in PostgreSQL
        childBuilder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => new ChildId(value))
            .HasColumnType("uuid")
            .HasColumnName("Id")
            .ValueGeneratedNever();

        // PersonName as owned type
        childBuilder.OwnsOne(c => c.Name, nb =>
        {
            nb.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasColumnType("character varying(100)")
                .HasMaxLength(100)
                .IsRequired();

            nb.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasColumnType("character varying(100)")
                .HasMaxLength(100)
                .IsRequired();
        });

        // DateOfBirth as owned type
        childBuilder.OwnsOne(c => c.DateOfBirth, db =>
        {
            db.Property(d => d.Date)
                .HasColumnName("DateOfBirth")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });

        // Gender enum - Store as text with check constraint
        childBuilder.Property(c => c.Gender)
            .HasConversion<string>()
            .HasColumnType("character varying(20)")
            .HasMaxLength(20)
            .IsRequired();

        // PhotoUrl
        childBuilder.Property(c => c.PhotoUrl)
            .HasColumnType("character varying(500)")
            .HasMaxLength(500)
            .IsRequired(false);

        // EnrollmentDate - timestamp with time zone for consistency
        childBuilder.Property(c => c.EnrollmentDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Allergies
        childBuilder.Property(c => c.Allergies)
            .HasColumnType("character varying(1000)")
            .HasMaxLength(1000)
            .IsRequired(false);

        // MedicalNotes
        childBuilder.Property(c => c.MedicalNotes)
            .HasColumnType("character varying(2000)")
            .HasMaxLength(2000)
            .IsRequired(false);

        // Status enum - Store as text with check constraint
        childBuilder.Property(c => c.Status)
            .HasConversion<string>()
            .HasColumnType("character varying(20)")
            .HasMaxLength(20)
            .IsRequired();

        // Audit fields with timezone awareness
        childBuilder.Property(c => c.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        childBuilder.Property(c => c.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for common queries - Optimized for typical operations
        childBuilder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Children_Status")
            .IsUnique(false);

        childBuilder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Children_CreatedAt")
            .IsUnique(false);

        childBuilder.HasIndex(c => c.EnrollmentDate)
            .HasDatabaseName("IX_Children_EnrollmentDate")
            .IsUnique(false);

        // Compound index for pagination/filtering common queries
        childBuilder.HasIndex(c => new { c.Status, c.CreatedAt })
            .HasDatabaseName("IX_Children_Status_CreatedAt")
            .IsUnique(false);

        // Classroom relationship
        childBuilder.Property(c => c.ClassroomId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? ClassroomId.Create(value.Value) : null)
            .HasColumnType("uuid")
            .HasColumnName("ClassroomId")
            .IsRequired(false);

        childBuilder.HasOne(c => c.Classroom)
            .WithMany(cl => cl.Children)
            .HasForeignKey(c => c.ClassroomId)
            .OnDelete(DeleteBehavior.SetNull);

        childBuilder.HasMany(c => c.ParentChildren)
            .WithOne(pc => pc.Child)
            .HasForeignKey(pc => pc.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
