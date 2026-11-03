using Daycare.Application;
using Daycare.Infrastructure.DependencyInjection;
using Daycare.Infrastructure.Data;
using ECX_Daycare.Common;
using ECX_Daycare.Extensions;
using ECX_Daycare.Middleware;
using ECX_Daycare.Services;
using Microsoft.EntityFrameworkCore;
// Enable Npgsql legacy timestamp behavior so DateTimes with Kind=Unspecified can be saved to timestamptz columns
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the frontend and API .env files.
LoadDotEnvFile(builder);

// Add CORS – allow Vite dev server origins
const string CorsPolicyName = "ViteDevServer";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "https://localhost:5173",
                "https://localhost:5174")
            .WithHeaders("Authorization", "Content-Type")
            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
            .AllowCredentials();
    });
});

// Add services to the container
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddSwaggerConfiguration()
    .AddHealthChecks();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["JWT_SECRET_KEY"];
if (string.IsNullOrWhiteSpace(jwtSecretKey)
    || System.Text.Encoding.UTF8.GetByteCount(jwtSecretKey) < 32)
{
    throw new InvalidOperationException(
        "Configure JWT_SECRET_KEY with a unique, high-entropy secret of at least 32 bytes.");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ECX_Daycare_API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ECX_Daycare_Client";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireStaffOrAdmin", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("RequireParent", policy => policy.RequireRole("Parent"));
    options.AddPolicy("RequireAuthenticated", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddScoped<IMessageBus, MessageBus>();
builder.Services.AddScoped<IIncidentEmailNotificationService, IncidentEmailNotificationService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Apply database migrations and seed identity data
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DaycareDbContext>();

    // Apply any pending migrations
    await dbContext.Database.MigrateAsync();

    // Seed identity roles and admin user
    await Daycare.Infrastructure.Identity.IdentityDataSeeder.SeedIdentityAsync(app.Services);

    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    if (args.Contains("--reset-demo-data", StringComparer.OrdinalIgnoreCase))
    {
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException("--reset-demo-data is available only in the Development environment.");
        }

        await Daycare.Infrastructure.Data.DemoDataSeeder.ResetAndSeedAsync(app.Services);
        logger.LogInformation("Database reset completed. Created 20 children, 20 parents, 5 classrooms, and 10 staff logins.");
        return;
    }

    logger.LogInformation("Database migrations and identity seeding completed successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration or identity seeding failed. This must be resolved before the application can continue.");
    throw;
}

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        if (!app.Environment.IsDevelopment())
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }
        return Task.CompletedTask;
    });
    await next();
});

// Configure Swagger
app.UseSwaggerConfiguration();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors(CorsPolicyName);

var frontendDistPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "ECX_Daycare.Frontend", "dist"));
if (Directory.Exists(frontendDistPath))
{
    var fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendDistPath);
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = fileProvider
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (Directory.Exists(frontendDistPath))
{
    app.MapFallback(async context =>
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var indexPath = Path.Combine(frontendDistPath, "index.html");
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(indexPath);
    });
}

app.Run();

// Loads simple KEY=VALUE pairs from the frontend and API .env files.
// Existing process environment variables take precedence over file values.
static void LoadDotEnvFile(WebApplicationBuilder builder)
{
    var candidates = new[]
    {
        Path.Combine(builder.Environment.ContentRootPath, "..", "ECX_Daycare.Frontend", ".env"),
        Path.Combine(builder.Environment.ContentRootPath, ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), ".env")
    };

    var fileConfiguration = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    foreach (var envPath in candidates.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
    {
        if (!File.Exists(envPath))
            continue;

        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"').Trim('\'');
            if (key.Length == 0 || Environment.GetEnvironmentVariable(key) is not null)
                continue;

            Environment.SetEnvironmentVariable(key, value);
            fileConfiguration.TryAdd(key, value);
        }
    }

    builder.Configuration.AddInMemoryCollection(fileConfiguration);
}
