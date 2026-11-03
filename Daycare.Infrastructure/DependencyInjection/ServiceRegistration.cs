using Daycare.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Daycare.Infrastructure.Data;
using Daycare.Infrastructure.Identity;
using Daycare.Infrastructure.Repositories;
using Daycare.Infrastructure.Storage;
using Daycare.Domain.Repositories;

namespace Daycare.Infrastructure.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Infrastructure layer.
/// Registers database context, repositories, and other infrastructure services.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Adds Infrastructure services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<DaycareDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("Daycare.Infrastructure");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
        });

        // Register repositories
        services.AddScoped<IChildRepository, ChildRepository>();
        services.AddScoped<IParentRepository, ParentRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IDailyReportRepository, DailyReportRepository>();
        services.AddScoped<IParentNotificationRepository, ParentNotificationRepository>();
        services.AddScoped<IParentIncidentNotificationRepository, ParentIncidentNotificationRepository>();
        services.AddScoped<IFeedingLogRepository, FeedingLogRepository>();
        services.AddScoped<INapLogRepository, NapLogRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IHealthLogRepository, HealthLogRepository>();

        // Register Identity
        services.AddIdentityCore<Daycare.Infrastructure.Identity.ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>()
        .AddEntityFrameworkStores<DaycareDbContext>()
        .AddDefaultTokenProviders();

        // Register JWT services
        services.Configure<Daycare.Infrastructure.Identity.JwtSettings>(
            configuration.GetSection(Daycare.Infrastructure.Identity.JwtSettings.SectionName));
        services.AddScoped<Daycare.Application.Common.Interfaces.IJwtTokenService, Daycare.Infrastructure.Identity.JwtTokenService>();

        // Register remote file storage (UploadThing)
        services.AddHttpClient<IStorageService, UploadThingStorageService>();

        return services;
    }
}
