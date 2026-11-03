using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Daycare.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Daycare.Infrastructure.Storage;

/// <summary>
/// Uploads files to UploadThing without a Node runtime.
///
/// The <c>UPLOADTHING_TOKEN</c> value is a base64-encoded JSON payload containing
/// <c>apiKey</c>, <c>appId</c> and <c>regions</c>. The upload uses the documented
/// presigned-URL flow:
///   1. POST /v7/prepareUpload to obtain a presigned upload <c>url</c> and file <c>key</c>.
///   2. PUT the file bytes to that presigned URL.
/// The resulting public URL is <c>https://{appId}.ufs.sh/f/{key}</c>.
/// </summary>
public sealed class UploadThingStorageService : IStorageService
{
    private const string PrepareUploadEndpoint = "https://api.uploadthing.com/v7/prepareUpload";

    /// <summary>Version header sent alongside UploadThing API requests.</summary>
    private const string UploadThingVersion = "7.7.4";

    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadThingStorageService> _logger;

    private readonly string? _apiKey;
    private readonly string? _appId;

    public UploadThingStorageService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<UploadThingStorageService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var rawToken = configuration["UPLOADTHING_TOKEN"]
            ?? Environment.GetEnvironmentVariable("UPLOADTHING_TOKEN");

        var payload = ParseToken(rawToken);
        _apiKey = payload?.ApiKey;
        _appId = payload?.AppId;

        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_appId))
        {
            _logger.LogWarning(
                "UPLOADTHING_TOKEN is not configured or could not be parsed. File uploads will fail.");
        }
    }

    /// <summary>
    /// Decodes the base64 JSON token into its apiKey/appId/regions parts.
    /// </summary>
    private static UploadThingTokenPayload? ParseToken(string? rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return null;

        var token = rawToken.Trim().Trim('"').Trim('\'');

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return JsonSerializer.Deserialize<UploadThingTokenPayload>(json, JsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_appId))
            throw new InvalidOperationException(
                "UPLOADTHING_TOKEN is not configured or is missing 'apiKey'/'appId'. " +
                "Add it to the .env file or environment variables.");

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        var effectiveContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType;

        // 1. Ask UploadThing for a presigned URL and file key.
        var preparePayload = new
        {
            fileName,
            fileSize = bytes.Length,
            fileType = effectiveContentType,
            acl = "public-read",
            contentDisposition = "inline"
        };

        using var prepareRequest = new HttpRequestMessage(HttpMethod.Post, PrepareUploadEndpoint)
        {
            Content = JsonContent.Create(preparePayload, options: JsonOptions)
        };
        prepareRequest.Headers.Add("x-uploadthing-api-key", _apiKey);
        prepareRequest.Headers.Add("x-uploadthing-version", UploadThingVersion);

        using var prepareResponse = await _httpClient.SendAsync(prepareRequest, cancellationToken);
        var prepareBody = await prepareResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!prepareResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "UploadThing prepareUpload failed with {StatusCode}: {Body}",
                (int)prepareResponse.StatusCode,
                prepareBody);
            throw new InvalidOperationException(
                $"UploadThing prepareUpload failed ({(int)prepareResponse.StatusCode}). {prepareBody}");
        }

        var prepared = JsonSerializer.Deserialize<PrepareUploadResponse>(prepareBody, JsonOptions);
        if (string.IsNullOrWhiteSpace(prepared?.Url) || string.IsNullOrWhiteSpace(prepared.Key))
        {
            _logger.LogError("UploadThing prepareUpload returned an incomplete response: {Body}", prepareBody);
            throw new InvalidOperationException("UploadThing did not return an upload URL.");
        }

        // 2. PUT the file bytes to the presigned URL (multipart form-data, as the SDK does).
        using var uploadRequest = new HttpRequestMessage(HttpMethod.Put, prepared.Url);
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(effectiveContentType);
        form.Add(fileContent, "file", fileName);
        uploadRequest.Content = form;
        uploadRequest.Headers.Add("Range", "bytes=0-");

        using var uploadResponse = await _httpClient.SendAsync(uploadRequest, cancellationToken);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!uploadResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "UploadThing ingest failed with {StatusCode}: {Body}",
                (int)uploadResponse.StatusCode,
                uploadBody);
            throw new InvalidOperationException(
                $"UploadThing upload failed ({(int)uploadResponse.StatusCode}). {uploadBody}");
        }

        // Public files are served from the app's ufs.sh domain.
        return $"https://{_appId}.ufs.sh/f/{prepared.Key}";
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Shape of the base64-decoded UPLOADTHING_TOKEN value.
    /// </summary>
    private sealed record UploadThingTokenPayload(
        [property: JsonPropertyName("apiKey")] string? ApiKey,
        [property: JsonPropertyName("appId")] string? AppId,
        [property: JsonPropertyName("regions")] List<string>? Regions);

    /// <summary>
    /// Response of <c>POST /v7/prepareUpload</c>.
    /// </summary>
    private sealed record PrepareUploadResponse(
        [property: JsonPropertyName("key")] string? Key,
        [property: JsonPropertyName("url")] string? Url);
}
