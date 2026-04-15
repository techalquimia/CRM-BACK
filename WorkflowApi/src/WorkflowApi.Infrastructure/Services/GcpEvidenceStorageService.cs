using Amazon.S3;
using Amazon.S3.Model;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkflowApi.Application.Exceptions;
using WorkflowApi.Application.Interfaces;

namespace WorkflowApi.Infrastructure.Services;

/// <summary>
/// Sube imágenes a Google Cloud Storage usando credenciales HMAC (API XML, compatible S3)
/// o cuenta de servicio (API JSON). Si están configurados HmacAccessKeyId y HmacSecretKey se usa HMAC.
/// </summary>
public class GcpEvidenceStorageService : IEvidenceStorageService
{
    private const string Folder = "evidences";
    private const string GcsEndpoint = "https://storage.googleapis.com";

    private readonly string _bucketName;
    private readonly ILogger<GcpEvidenceStorageService> _logger;
    private readonly IAmazonS3? _s3Client;
    private readonly StorageClient? _storageClient;

    public GcpEvidenceStorageService(IConfiguration configuration, ILogger<GcpEvidenceStorageService> logger)
    {
        _bucketName = configuration["Gcp:Storage:BucketName"]
            ?? throw new InvalidOperationException("Gcp:Storage:BucketName is required. Configure in appsettings (Gcp:Storage:BucketName).");

        var hmacAccessKey = configuration["Gcp:Storage:HmacAccessKeyId"];
        var hmacSecretKey = configuration["Gcp:Storage:HmacSecretKey"];

        if (!string.IsNullOrWhiteSpace(hmacAccessKey) && !string.IsNullOrWhiteSpace(hmacSecretKey))
        {
            // HMAC: API XML (S3-compatible) contra GCS
            var config = new AmazonS3Config
            {
                ServiceURL = GcsEndpoint,
                ForcePathStyle = true,
                AuthenticationRegion = "auto"
            };
            _s3Client = new AmazonS3Client(hmacAccessKey, hmacSecretKey, config);
            _storageClient = null;
            _logger = logger;
            _logger.LogInformation("GCP Storage: using HMAC (XML API) for bucket {BucketName}", _bucketName);
        }
        else
        {
            // Cuenta de servicio (API JSON)
            _s3Client = null;
            var credentialsPath = configuration["Gcp:Storage:CredentialsPath"] ?? configuration["Gcp:CredentialsPath"];
            if (!string.IsNullOrWhiteSpace(credentialsPath))
            {
                if (!Path.IsPathRooted(credentialsPath))
                    credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, credentialsPath);
                if (File.Exists(credentialsPath))
                {
                    var credential = GoogleCredential.FromFile(credentialsPath);
                    _storageClient = StorageClient.Create(credential);
                }
                else
                {
                    _storageClient = StorageClient.Create();
                }
            }
            else
            {
                _storageClient = StorageClient.Create();
            }
            _logger = logger;
            _logger.LogInformation("GCP Storage: using service account (JSON API) for bucket {BucketName}", _bucketName);
        }
    }

    public async Task<string> UploadImageAsync(Stream content, string fileName, string folderName, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
            extension = contentType switch
            {
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".bin"
            };
        var objectName = $"{folderName}/{Guid.NewGuid():N}{extension}";

        if (_s3Client != null)
            await UploadViaHmacAsync(content, objectName, contentType, cancellationToken);
        else
            await UploadViaServiceAccountAsync(content, objectName, contentType, cancellationToken);

        var url = $"{GcsEndpoint}/{_bucketName}/{objectName}";
        _logger.LogInformation("Uploaded evidence image to {ObjectName}", objectName);
        return url;
    }

    private async Task UploadViaHmacAsync(Stream content, string objectName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectName,
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false
            };
            await _s3Client!.PutObjectAsync(request, cancellationToken);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogWarning(ex, "GCP Storage (HMAC) upload failed: {Message}", ex.Message);
            var message = ex.StatusCode == System.Net.HttpStatusCode.Forbidden && (ex.Message?.Contains("billing", StringComparison.OrdinalIgnoreCase) == true)
                ? "El proyecto de Google Cloud no tiene facturación activa. Enlaza una cuenta de facturación en la consola de GCP."
                : $"Error al subir la imagen a Storage: {ex.Message}";
            throw new EvidenceStorageException(message, ex);
        }
    }

    private async Task UploadViaServiceAccountAsync(Stream content, string objectName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            await _storageClient!.UploadObjectAsync(
                _bucketName,
                objectName,
                contentType,
                content,
                null,
                cancellationToken);
        }
        catch (GoogleApiException ex)
        {
            _logger.LogWarning(ex, "GCP Storage upload failed: {Message}", ex.Message);
            var message = ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden && ex.Message.Contains("billing", StringComparison.OrdinalIgnoreCase)
                ? "El proyecto de Google Cloud no tiene facturación activa. Enlaza una cuenta de facturación en la consola de GCP."
                : $"Error al subir la imagen a Storage: {ex.Message}";
            throw new EvidenceStorageException(message, ex);
        }
    }
}
