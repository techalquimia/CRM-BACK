namespace WorkflowApi.Application.Interfaces;

/// <summary>
/// Uploads evidence images to cloud storage (e.g. GCP) and returns the public URL.
/// </summary>
public interface IEvidenceStorageService
{
    /// <summary>
    /// Uploads an image and returns its public URL.
    /// </summary>
    /// <param name="content">Image stream.</param>
    /// <param name="fileName">Original file name (used for extension).</param>
    /// <param name="contentType">MIME type (e.g. image/jpeg).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Public URL of the uploaded object.</returns>
    Task<string> UploadImageAsync(Stream content, string fileName, string folderName, string contentType, CancellationToken cancellationToken = default);
}
