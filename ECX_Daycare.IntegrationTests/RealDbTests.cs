using System;
using System.Threading.Tasks;
using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;
using Daycare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace ECX_Daycare.IntegrationTests;

public class RealDbTests
{
    private static string GetTestConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("DAYCARE_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Set DAYCARE_TEST_CONNECTION_STRING to run database integration tests.");

        var parsedConnection = new NpgsqlConnectionStringBuilder(connectionString);
        if (!parsedConnection.Database.Contains("test", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Database integration tests require a database name containing 'test'.");

        return connectionString;
    }

    [Fact]
    public async Task TestSavingChildToRealDb()
    {
        var options = new DbContextOptionsBuilder<DaycareDbContext>()
            .UseNpgsql(GetTestConnectionString())
            .Options;

        using var context = new DaycareDbContext(options);

        // Date specified with UTC
        var birthDate = DateTime.SpecifyKind(DateTime.Parse("2020-01-01T00:00:00"), DateTimeKind.Utc);
        var enrollDate = DateTime.SpecifyKind(DateTime.Parse("2024-01-01T00:00:00"), DateTimeKind.Utc);

        var child = new Child(
            ChildId.New(),
            new PersonName("Test", "Child"),
            new DateOfBirth(birthDate),
            Gender.Male,
            enrollDate);

        context.Children.Add(child);
        await context.SaveChangesAsync();

        // Clean up test entity
        context.Children.Remove(child);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task TestSavingStaffToRealDb()
    {
        var options = new DbContextOptionsBuilder<DaycareDbContext>()
            .UseNpgsql(GetTestConnectionString())
            .Options;

        using var context = new DaycareDbContext(options);

        var hireDate = DateTime.SpecifyKind(DateTime.Parse("2024-01-15T00:00:00"), DateTimeKind.Utc);
        var staff = new Staff(
            StaffId.New(),
            "IntegrationTest",
            "StaffMember",
            StaffRole.LeadTeacher,
            "555-432-1098",
            $"integration.{Guid.NewGuid():N}@daycare.com",
            hireDate,
            new TimeOnly(8, 30),
            new TimeOnly(17, 30),
            null,
            "CPR Certified");

        context.Staff.Add(staff);
        await context.SaveChangesAsync();

        // Verify saved
        var retrieved = await context.Staff.FirstOrDefaultAsync(s => s.Id == staff.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("IntegrationTest", retrieved.FirstName);
        Assert.Equal(StaffRole.LeadTeacher, retrieved.Role);

        // Clean up test entity
        context.Staff.Remove(staff);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task TestSavingEnrollmentToRealDb()
    {
        var options = new DbContextOptionsBuilder<DaycareDbContext>()
            .UseNpgsql(GetTestConnectionString())
            .Options;

        using var context = new DaycareDbContext(options);

        // Create temporary child
        var child = new Child(
            ChildId.New(),
            new PersonName("Enrollment", "Child"),
            new DateOfBirth(DateTime.SpecifyKind(DateTime.Parse("2021-05-10T00:00:00"), DateTimeKind.Utc)),
            Gender.Female,
            DateTime.SpecifyKind(DateTime.Parse("2024-02-01T00:00:00"), DateTimeKind.Utc));

        context.Children.Add(child);
        await context.SaveChangesAsync();

        var startDate = DateTime.SpecifyKind(DateTime.Parse("2024-03-01T00:00:00"), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(DateTime.Parse("2024-09-01T00:00:00"), DateTimeKind.Utc);

        var enrollmentResult = Enrollment.Create(
            child.Id,
            ProgramType.FullTime,
            "Monday, Tuesday, Wednesday, Thursday, Friday",
            startDate,
            endDate,
            contractSigned: true,
            immunizationRecord: true,
            birthCertificate: true,
            medicalForm: true);

        Assert.True(enrollmentResult.IsSuccess);
        var enrollment = enrollmentResult.Value!;

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        // Verify retrieval
        var retrieved = await context.Enrollments
            .Include(e => e.Child)
            .FirstOrDefaultAsync(e => e.Id == enrollment.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(child.Id, retrieved.ChildId);
        Assert.Equal(ProgramType.FullTime, retrieved.ProgramType);
        Assert.True(retrieved.ContractSigned);
        Assert.True(retrieved.ImmunizationRecord);
        Assert.Equal(EnrollmentStatus.Active, retrieved.Status);

        // Clean up
        context.Enrollments.Remove(enrollment);
        context.Children.Remove(child);
        await context.SaveChangesAsync();
    }
}
