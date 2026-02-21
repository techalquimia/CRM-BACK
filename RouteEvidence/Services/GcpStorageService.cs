using Google.Cloud.Storage.V1;

namespace RouteEvidence.Services;

public class GcpStorageService : IGcpStorageService
{
    private readonly string _bucketName;
    private readonly StorageClient _storageClient;
    private readonly ILogger<GcpStorageService> _logger;

    public GcpStorageService(IConfiguration configuration, ILogger<GcpStorageService> logger)
    {
        _bucketName = configuration["Gcp:StorageBucket"] ?? throw new InvalidOperationException("Gcp:StorageBucket is not configured");
        _storageClient = StorageClient.Create();
        _logger = logger;
    }

    public async Task<(string Bucket, string ObjectKey)> UploadAsync(
        Stream stream,
        string objectKey,
        string contentType,
        CancellationToken ct = default)
    {
        await _storageClient.UploadObjectAsync(
            _bucketName,
            objectKey,
            contentType,
            stream,
            cancellationToken: ct);

        _logger.LogInformation("Uploaded object to gs://{Bucket}/{Key}", _bucketName, objectKey);

        return (_bucketName, objectKey);
    }
}
