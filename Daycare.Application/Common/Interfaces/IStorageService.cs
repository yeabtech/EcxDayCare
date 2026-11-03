namespace Daycare.Application.Common.Interfaces;

/// <summary>
/// Abstraction over remote file storage (currently UploadThing).
/// Implemented in the Infrastructure layer.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file to remote storage and returns its publicly accessible URL.
    /// </summary>
    /// <param name="content">The file content stream.</param>
    /// <param name="fileName">The original file name (used to infer the content type).</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The public URL of the uploaded file.</returns>
    Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
