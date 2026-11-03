using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Daycare.Infrastructure.Data;
using Daycare.Infrastructure.Identity;
using ECX_Daycare.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECX_Daycare.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Policy = "RequireAdmin")]
public sealed class AdminSettingsController : ControllerBase
{
    private readonly DaycareDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSettingsController> _logger;

    public AdminSettingsController(
        DaycareDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AdminSettingsController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var database = await ReadDatabaseMetricsAsync(cancellationToken);
            stopwatch.Stop();

            var counts = new SystemRecordCounts(
                Children: await _dbContext.Children.CountAsync(cancellationToken),
                Staff: await _dbContext.Staff.CountAsync(cancellationToken),
                Parents: await _dbContext.Parents.CountAsync(cancellationToken),
                Accounts: await _userManager.Users.CountAsync(cancellationToken),
                ActiveAccounts: await _userManager.Users.CountAsync(user => user.IsActive, cancellationToken),
                AttendanceRecords: await _dbContext.Attendances.CountAsync(cancellationToken));

            var uploadThing = await ReadUploadThingUsageAsync(cancellationToken);
            var processStartedAt = Process.GetCurrentProcess().StartTime.ToUniversalTime();
            var response = new AdminSystemMetrics(
                Database: database with { MeasuredQueryMilliseconds = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 1) },
                UploadThing: uploadThing,
                Records: counts,
                Application: new ApplicationRuntimeMetrics(
                    Environment: _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production",
                    Runtime: System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                    StartedAt: processStartedAt,
                    UptimeSeconds: Math.Max(0, (long)(DateTime.UtcNow - processStartedAt).TotalSeconds)));

            return Ok(ApiResponse<AdminSystemMetrics>.Success(response));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not read admin system metrics");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail("System metrics are temporarily unavailable."));
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeAdminPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(ApiResponse<object>.Fail("Enter your current and new passwords."));
        if (request.NewPassword.Length < 8)
            return BadRequest(ApiResponse<object>.Fail("The new password must be at least 8 characters long."));
        if (request.CurrentPassword == request.NewPassword)
            return BadRequest(ApiResponse<object>.Fail("Choose a new password that differs from your current password."));

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized(ApiResponse<object>.Fail("Your account could not be identified. Please sign in again."));

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            var message = changeResult.Errors.Any(error => error.Code == "PasswordMismatch")
                ? "The current password is incorrect."
                : string.Join(" ", changeResult.Errors.Select(error => error.Description));
            return BadRequest(ApiResponse<object>.Fail(message));
        }

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == user.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in activeRefreshTokens)
            token.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<object>.Success(true,
            "Password changed. Sign in again on other devices to continue."));
    }

    private async Task<DatabaseMetrics> ReadDatabaseMetricsAsync(CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT current_database(),
                       pg_database_size(current_database()),
                       (SELECT count(*) FROM pg_stat_activity WHERE datname = current_database()),
                       current_setting('max_connections')::integer,
                       COALESCE((
                           SELECT round(100.0 * blks_hit / NULLIF(blks_hit + blks_read, 0), 1)
                           FROM pg_stat_database WHERE datname = current_database()
                       ), 0),
                       pg_postmaster_start_time()
                """;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException("The database metrics query returned no data.");

            return new DatabaseMetrics(
                Name: reader.GetString(0),
                SizeBytes: reader.GetInt64(1),
                ActiveConnections: checked((int)reader.GetInt64(2)),
                MaxConnections: reader.GetInt32(3),
                CacheHitPercent: Convert.ToDouble(reader.GetValue(4)),
                ServerStartedAt: reader.GetDateTime(5),
                MeasuredQueryMilliseconds: 0);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private async Task<UploadThingUsageMetrics> ReadUploadThingUsageAsync(CancellationToken cancellationToken)
    {
        const long localUploadLimitBytes = StorageController.MaxFileSizeBytes;
        var rawToken = _configuration["UPLOADTHING_TOKEN"]
            ?? Environment.GetEnvironmentVariable("UPLOADTHING_TOKEN");
        if (string.IsNullOrWhiteSpace(rawToken))
            return new UploadThingUsageMetrics(false, "UploadThing token is not configured.", null, null, null, null, localUploadLimitBytes);

        try
        {
            var token = rawToken.Trim().Trim('"', '\'');
            using var tokenDocument = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(token)));
            if (!tokenDocument.RootElement.TryGetProperty("apiKey", out var apiKeyElement) ||
                string.IsNullOrWhiteSpace(apiKeyElement.GetString()))
                return new UploadThingUsageMetrics(false, "UploadThing token is missing its API key.", null, null, null, null, localUploadLimitBytes);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.uploadthing.com/v6/getUsageInfo")
            {
                Content = JsonContent.Create(new { })
            };
            request.Headers.Add("x-uploadthing-api-key", apiKeyElement.GetString());

            using var response = await _httpClientFactory.CreateClient().SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("UploadThing usage request returned HTTP {StatusCode}", (int)response.StatusCode);
                return new UploadThingUsageMetrics(false, "UploadThing did not return usage data.", null, null, null, null, localUploadLimitBytes);
            }

            await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var payload = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
            var usage = payload.RootElement.TryGetProperty("data", out var data) ? data : payload.RootElement;
            return new UploadThingUsageMetrics(
                true,
                null,
                GetInt64(usage, "totalBytes"),
                GetInt64(usage, "appTotalBytes"),
                GetInt64(usage, "filesUploaded"),
                GetInt64(usage, "limitBytes"),
                localUploadLimitBytes);
        }
        catch (Exception exception) when (exception is FormatException or JsonException or HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "UploadThing usage information is unavailable");
            return new UploadThingUsageMetrics(false, "UploadThing usage could not be loaded. Check the API key and network connection.", null, null, null, null, localUploadLimitBytes);
        }
    }

    private static long? GetInt64(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.TryGetInt64(out var value) ? value : null;
}

public sealed record ChangeAdminPasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AdminSystemMetrics(
    DatabaseMetrics Database,
    UploadThingUsageMetrics UploadThing,
    SystemRecordCounts Records,
    ApplicationRuntimeMetrics Application);

public sealed record DatabaseMetrics(
    string Name,
    long SizeBytes,
    int ActiveConnections,
    int MaxConnections,
    double CacheHitPercent,
    DateTime ServerStartedAt,
    double MeasuredQueryMilliseconds);

public sealed record UploadThingUsageMetrics(
    bool Connected,
    string? Message,
    long? TotalBytes,
    long? AppTotalBytes,
    long? FilesUploaded,
    long? LimitBytes,
    long LocalUploadLimitBytes);

public sealed record SystemRecordCounts(
    int Children,
    int Staff,
    int Parents,
    int Accounts,
    int ActiveAccounts,
    int AttendanceRecords);

public sealed record ApplicationRuntimeMetrics(
    string Environment,
    string Runtime,
    DateTime StartedAt,
    long UptimeSeconds);
