namespace RouteEvidence.Services;

public interface IGcpStorageService
{
    /// <summary>
    /// Uploads a file stream to GCP Storage and returns the object key.
    /// </summary>
    /// <param name="stream">File content stream</param>
    /// <param name="objectKey">Destination object key (path) in the bucket</param>
    /// <param name="contentType">MIME type (e.g., image/jpeg)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The bucket name and object key where the file was stored</returns>
    Task<(string Bucket, string ObjectKey)> UploadAsync(
        Stream stream,
        string objectKey,
        string contentType,
        CancellationToken ct = default);
}
