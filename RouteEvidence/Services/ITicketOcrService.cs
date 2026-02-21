namespace RouteEvidence.Services;

/// <summary>
/// Result of ticket OCR extraction from Google Cloud Vision.
/// </summary>
public record TicketOcrResult(
    string FullText,
    double? TotalWeight,
    double? Tara,
    double? NetWeight);

public interface ITicketOcrService
{
    /// <summary>
    /// Extracts text from image in GCP Storage and parses ticket weight data.
    /// </summary>
    /// <param name="gcsBucket">GCS bucket name</param>
    /// <param name="gcsObjectKey">GCS object key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Extracted full text and parsed weight values if found</returns>
    Task<TicketOcrResult> ExtractTicketDataAsync(
        string gcsBucket,
        string gcsObjectKey,
        CancellationToken ct = default);
}
