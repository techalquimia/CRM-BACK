using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkflowApi.Application.Exceptions;
using WorkflowApi.Application.Interfaces;

namespace WorkflowApi.Infrastructure.Services;

/// <summary>
/// Sube imágenes a Azure Blob Storage (opción nativa de Microsoft).
/// Configuración: Azure:Storage:ConnectionString y Azure:Storage:ContainerName.
/// </summary>
public class AzureBlobEvidenceStorageService : IEvidenceStorageService
{
    private const string Folder = "evidences";

    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobEvidenceStorageService> _logger;

    public AzureBlobEvidenceStorageService(IConfiguration configuration, ILogger<AzureBlobEvidenceStorageService> logger)
    {
        _logger = logger;

        var connectionString = configuration["Azure:Storage:ConnectionString"]
            ?? throw new InvalidOperationException("Azure:Storage:ConnectionString is required when using Azure Blob Storage.");

        var containerName = configuration["Azure:Storage:ContainerName"] ?? "evidences";

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _logger.LogInformation("Azure Blob Storage: container {ContainerName} for evidence images", containerName);
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

        var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
        var blobName = $"{folderName}/{datePrefix}/{Guid.NewGuid():N}{extension}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        try
        {
            // Crear contenedor solo si no existe (sin especificar acceso público; evita PublicAccessNotPermitted en cuentas con acceso público deshabilitado)
            await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };
            await blobClient.UploadAsync(content, options, cancellationToken);
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Azure Blob Storage upload failed: {Message}", ex.Message);
            throw new EvidenceStorageException($"Error al subir la imagen a Azure Blob Storage: {ex.Message}", ex);
        }

        string url = blobClient.Uri.ToString();
        _logger.LogInformation("Uploaded evidence image to {BlobName}", blobName);
        return url;
    }
}
