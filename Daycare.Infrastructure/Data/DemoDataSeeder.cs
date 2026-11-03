using Daycare.Domain.Entities;
using Daycare.Domain.ValueObjects;
using Daycare.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Daycare.Infrastructure.Data;

/// <summary>
/// Destructively resets application data and creates a complete Ethiopian demo roster.
/// Run only through the explicit --reset-demo-data command-line option.
/// </summary>
public static class DemoDataSeeder
{
    private const string AdminEmail = "admin@daycare.com";

    private sealed record RoomSeed(string Name, int MinAgeMonths, int MaxAgeMonths, ProgramType ProgramType);
    private sealed record TeacherSeed(string FirstName, string LastName, int RoomIndex, string Password);
    private sealed record FamilySeed(string ChildFirstName, string ChildLastName, int AgeMonths, Gender Gender,
        string ParentFirstName, string ParentLastName, ParentRole ParentRole);

    private static readonly RoomSeed[] Rooms =
    [
        new("Infant Room", 0, 12, ProgramType.InfantCare),
        new("Toddler Room", 13, 24, ProgramType.Toddler),
        new("Early Learners", 25, 36, ProgramType.Preschool),
        new("Preschool Room", 37, 48, ProgramType.Preschool),
        new("Pre-K Room", 49, 60, ProgramType.Preschool)
    ];

    private static readonly TeacherSeed[] Teachers =
    [
        new("Hana", "Getachew", 0, "HanaDaycare2026!"),
        new("Dawit", "Bekele", 0, "DawitDaycare2026!"),
        new("Selamawit", "Tadesse", 1, "SelamawitDaycare2026!"),
        new("Yonatan", "Abebe", 1, "YonatanDaycare2026!"),
        new("Bethlehem", "Girma", 2, "BethlehemDaycare2026!"),
        new("Natnael", "Fikru", 2, "NatnaelDaycare2026!"),
        new("Meseret", "Alemu", 3, "MeseretDaycare2026!"),
        new("Samuel", "Tesfaye", 3, "SamuelDaycare2026!"),
        new("Rahel", "Worku", 4, "RahelDaycare2026!"),
        new("Biruk", "Kebede", 4, "BirukDaycare2026!")
    ];

    private static readonly FamilySeed[] Families =
    [
        new("Lulit", "Abebe", 6, Gender.Female, "Mekdes", "Abebe", ParentRole.Mother),
        new("Henok", "Bekele", 8, Gender.Male, "Tigist", "Bekele", ParentRole.Mother),
        new("Makda", "Girma", 10, Gender.Female, "Amanuel", "Girma", ParentRole.Father),
        new("Nahom", "Tadesse", 11, Gender.Male, "Rahel", "Tadesse", ParentRole.Mother),
        new("Frehiwot", "Kebede", 13, Gender.Female, "Yonas", "Kebede", ParentRole.Father),
        new("Dawit", "Alemu", 16, Gender.Male, "Saba", "Alemu", ParentRole.Mother),
        new("Eyerusalem", "Worku", 19, Gender.Female, "Mulugeta", "Worku", ParentRole.Father),
        new("Natnael", "Fikru", 22, Gender.Male, "Hirut", "Fikru", ParentRole.Mother),
        new("Mihret", "Tesfaye", 25, Gender.Female, "Abebe", "Tesfaye", ParentRole.Father),
        new("Yared", "Getachew", 28, Gender.Male, "Selam", "Getachew", ParentRole.Mother),
        new("Hawi", "Abebe", 31, Gender.Female, "Biniyam", "Abebe", ParentRole.Father),
        new("Robel", "Bekele", 34, Gender.Male, "Genet", "Bekele", ParentRole.Mother),
        new("Meron", "Girma", 37, Gender.Female, "Fitsum", "Girma", ParentRole.Father),
        new("Kaleb", "Tadesse", 40, Gender.Male, "Bethlehem", "Tadesse", ParentRole.Mother),
        new("Selam", "Kebede", 43, Gender.Female, "Desta", "Kebede", ParentRole.Father),
        new("Yonatan", "Alemu", 46, Gender.Male, "Kidist", "Alemu", ParentRole.Mother),
        new("Ruth", "Worku", 49, Gender.Female, "Aster", "Worku", ParentRole.Mother),
        new("Meklit", "Fikru", 52, Gender.Female, "Tewodros", "Fikru", ParentRole.Father),
        new("Biruk", "Tesfaye", 55, Gender.Male, "Marta", "Tesfaye", ParentRole.Mother),
        new("Saron", "Getachew", 58, Gender.Female, "Girma", "Getachew", ParentRole.Father)
    ];

    public static async Task ResetAndSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DaycareDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
        await using var transaction = await db.Database.BeginTransactionAsync();

        // Clear operational records before the entities they reference.
        await db.RefreshTokens.ExecuteDeleteAsync();
        await db.Attendances.ExecuteDeleteAsync();
        await db.DailyReports.ExecuteDeleteAsync();
        await db.FeedingLogs.ExecuteDeleteAsync();
        await db.NapLogs.ExecuteDeleteAsync();
        await db.ActivityLogs.ExecuteDeleteAsync();
        await db.Incidents.ExecuteDeleteAsync();
        await db.HealthLogs.ExecuteDeleteAsync();
        await db.Enrollments.ExecuteDeleteAsync();
        await db.ParentChildren.ExecuteDeleteAsync();
        await db.Children.ExecuteDeleteAsync();
        await db.Staff.ExecuteDeleteAsync();
        await db.Classrooms.ExecuteDeleteAsync();
        await db.Parents.ExecuteDeleteAsync();

        await db.Users
            .Where(user => user.Email != AdminEmail)
            .ExecuteDeleteAsync();

        var today = DateTime.UtcNow.Date;
        var roomIds = Rooms.Select(_ => ClassroomId.New()).ToArray();
        var teacherIds = Teachers.Select(_ => Guid.NewGuid()).ToArray();

        var classrooms = Rooms.Select((room, index) =>
        {
            var ageGroup = AgeGroup.Create(room.MinAgeMonths, room.MaxAgeMonths);
            var capacity = ClassroomCapacity.Create(12);
            if (ageGroup.IsFailure || capacity.IsFailure)
                throw new InvalidOperationException("The sample classroom age range or capacity is invalid.");

            return new Classroom(
                roomIds[index],
                room.Name,
                ageGroup.Value!,
                capacity.Value!,
                ClassroomStatus.Active,
                teacherIds[index * 2],
                $"Children aged {room.MinAgeMonths} to {room.MaxAgeMonths} months.");
        }).ToArray();

        var staff = Teachers.Select((teacher, index) => new Staff(
            StaffId.Create(teacherIds[index]),
            teacher.FirstName,
            teacher.LastName,
            index % 2 == 0 ? StaffRole.LeadTeacher : StaffRole.Teacher,
            $"+251911000{201 + index:000}",
            GetTeacherEmail(teacher),
            today,
            new TimeOnly(8, 0),
            new TimeOnly(17, 0),
            roomIds[teacher.RoomIndex],
            index % 2 == 0 ? "Early childhood education" : "Childcare and first aid")
        ).ToArray();

        var parents = new List<Parent>(Families.Length);
        var children = new List<Child>(Families.Length);
        var enrollments = new List<Enrollment>(Families.Length);

        for (var index = 0; index < Families.Length; index++)
        {
            var family = Families[index];
            var classroomIndex = index / 4;
            var dateOfBirth = today.AddMonths(-family.AgeMonths);
            var child = new Child(
                ChildId.New(),
                new PersonName(family.ChildFirstName, family.ChildLastName),
                new DateOfBirth(dateOfBirth),
                family.Gender,
                today,
                photoUrl: null,
                allergies: null,
                medicalNotes: null);
            child.AssignToClassroom(roomIds[classroomIndex]);

            var contact = ContactInfo.Create(
                $"parent{index + 1:00}@families.ecxdaycare.test",
                $"+251911000{index + 1:000}");
            if (contact.IsFailure)
                throw new InvalidOperationException(contact.Error);

            var parent = new Parent(
                ParentId.New(),
                new PersonName(family.ParentFirstName, family.ParentLastName),
                contact.Value!,
                "Addis Ababa, Ethiopia");
            var linkResult = parent.AddChild(child.Id, family.ParentRole);
            if (linkResult.IsFailure)
                throw new InvalidOperationException(linkResult.Error);

            parents.Add(parent);
            children.Add(child);
            enrollments.Add(new Enrollment(
                EnrollmentId.New(),
                child.Id,
                Rooms[classroomIndex].ProgramType,
                "Monday, Tuesday, Wednesday, Thursday, Friday",
                today,
                classroomId: roomIds[classroomIndex],
                status: EnrollmentStatus.Active));
        }

        await db.Classrooms.AddRangeAsync(classrooms);
        await db.Staff.AddRangeAsync(staff);
        await db.Parents.AddRangeAsync(parents);
        await db.Children.AddRangeAsync(children);
        await db.Enrollments.AddRangeAsync(enrollments);
        await db.SaveChangesAsync();

        foreach (var (teacher, index) in Teachers.Select((teacher, index) => (teacher, index)))
        {
            var email = GetTeacherEmail(teacher);
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Role = "Staff",
                StaffId = teacherIds[index],
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, teacher.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create staff login {email}: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, "Staff");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not assign the Staff role to {email}: {errors}");
            }
        }

        await transaction.CommitAsync();

        Console.WriteLine(
            $"Demo data ready: {await db.Children.CountAsync()} children, " +
            $"{await db.Parents.CountAsync()} parents, {await db.Classrooms.CountAsync()} classrooms, " +
            $"{await db.Staff.CountAsync()} staff, " +
            $"{await db.Users.CountAsync(user => user.Role == "Staff")} staff logins; " +
            $"{await db.HealthLogs.CountAsync()} health logs, {await db.Incidents.CountAsync()} incidents.");
        });
    }

    private static string GetTeacherEmail(TeacherSeed teacher) =>
        $"{teacher.FirstName}.{teacher.LastName}@ecxdaycare.test".ToLowerInvariant();
}
