using System.ComponentModel.DataAnnotations;

namespace RouteEvidence.Models;

/// <summary>
/// Route evidence captured by a unit.
/// Can consist of tickets (e.g., weighing tickets) with extracted weight data.
/// Images are stored in GCP Storage and referenced by bucket + object key.
/// </summary>
public class Evidence
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// GCP Storage bucket name where the image is stored.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string GcsBucket { get; set; } = string.Empty;

    /// <summary>
    /// GCP Storage object key (path) within the bucket.
    /// Full reference: gs://{GcsBucket}/{GcsObjectKey}
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string GcsObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the evidence was captured.
    /// </summary>
    public DateTime DateTime { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    [Required]
    [MaxLength(100)]
    public string EvidenceType { get; set; } = string.Empty;

    public bool IsSynced { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    /// <summary>
    /// Ticket-specific: Total weight (peso total). TotalWeight = Tara + NetWeight.
    /// </summary>
    public double? TotalWeight { get; set; }

    /// <summary>
    /// Ticket-specific: Tare weight.
    /// </summary>
    public double? Tara { get; set; }

    /// <summary>
    /// Ticket-specific: Net weight (peso). TotalWeight = Tara + NetWeight.
    /// </summary>
    public double? NetWeight { get; set; }

    /// <summary>
    /// Raw text extracted from ticket image via Google Cloud Vision OCR.
    /// </summary>
    [MaxLength(8000)]
    public string? OcrText { get; set; }
}
