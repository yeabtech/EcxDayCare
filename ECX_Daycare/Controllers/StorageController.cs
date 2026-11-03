using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Daycare.Application.Common.Interfaces;
using ECX_Daycare.Common;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Handles file uploads (currently child/staff photos).
/// Files are uploaded to UploadThing and the returned public URL is persisted
/// on the owning entity (for example <c>Child.PhotoUrl</c>).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StorageController : ControllerBase
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp"
        };

    private readonly IStorageService _storageService;
    private readonly ILogger<StorageController> _logger;

    public StorageController(IStorageService storageService, ILogger<StorageController> logger)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Uploads an image to UploadThing and returns its public URL.
    /// </summary>
    /// <param name="file">The image file (jpg, jpeg, png, gif or webp, max 5 MB).</param>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Upload(
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("No file was provided."));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ApiResponse<object>.Fail("File exceeds the 5 MB size limit."));

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<object>.Fail("Only jpg, jpeg, png, gif and webp images are allowed."));

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse<object>.Fail("The uploaded file is not a supported image type."));

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await _storageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                cancellationToken);

            _logger.LogInformation(
                "Uploaded file {FileName} ({Size} bytes) to UploadThing at {Url}",
                file.FileName,
                file.Length,
                url);

            return Ok(ApiResponse<object>.Success(new { url }, "File uploaded successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "UploadThing rejected the upload of {FileName}", file.FileName);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail(ex.Message));
        }
    }
}
