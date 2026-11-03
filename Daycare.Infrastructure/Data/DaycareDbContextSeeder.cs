using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;

namespace Daycare.Infrastructure.Data;

/// <summary>
/// Provides seed data for the DaycareDbContext.
/// This seeder is intended for development and testing purposes only.
/// Production environments should not use this seeder.
/// </summary>
public static class DaycareDbContextSeeder
{
    /// <summary>
    /// Seeds development data to the database if it is empty.
    /// This method should only be called in development environments.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>A task that completes when seeding is done.</returns>
    public static async Task SeedDevelopmentDataAsync(this DaycareDbContext context)
    {
        // Only seed if database is empty
        if (context.Children.Any())
        {
            return;
        }

        var children = new List<Child>
        {
            new Child(
                id: ChildId.New(),
                name: new PersonName("Emma", "Johnson"),
                dateOfBirth: new DateOfBirth(new DateTime(2021, 3, 15)),
                gender: Gender.Female,
                enrollmentDate: new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                photoUrl: null,
                allergies: "Peanuts",
                medicalNotes: "None"
            ),
            new Child(
                id: ChildId.New(),
                name: new PersonName("Liam", "Smith"),
                dateOfBirth: new DateOfBirth(new DateTime(2020, 7, 22)),
                gender: Gender.Male,
                enrollmentDate: new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                photoUrl: null,
                allergies: null,
                medicalNotes: "Mild asthma"
            ),
            new Child(
                id: ChildId.New(),
                name: new PersonName("Sophia", "Williams"),
                dateOfBirth: new DateOfBirth(new DateTime(2021, 11, 5)),
                gender: Gender.Female,
                enrollmentDate: new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                photoUrl: null,
                allergies: "Dairy",
                medicalNotes: null
            ),
            new Child(
                id: ChildId.New(),
                name: new PersonName("Noah", "Brown"),
                dateOfBirth: new DateOfBirth(new DateTime(2019, 5, 10)),
                gender: Gender.Male,
                enrollmentDate: new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                photoUrl: null,
                allergies: null,
                medicalNotes: "Scheduled for withdrawal"
            ),
        };

        // Mark Noah as inactive for variety
        children[3].MarkInactive();

        await context.Children.AddRangeAsync(children);
        await context.SaveChangesAsync();
    }
}
