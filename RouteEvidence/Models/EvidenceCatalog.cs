using System.ComponentModel.DataAnnotations;

namespace RouteEvidence.Models;

/// <summary>
/// Catalog of route evidence types.
/// </summary>
public class EvidenceCatalog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool Active { get; set; } = true;

    public bool Ocr { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
