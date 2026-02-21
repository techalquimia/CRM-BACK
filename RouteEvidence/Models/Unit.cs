using System.ComponentModel.DataAnnotations;

namespace RouteEvidence.Models;

/// <summary>
/// Basic information of a vehicle unit.
/// </summary>
public class Unit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(20)]
    public string Plate { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EconomicNumber { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(50)]
    public string? Year { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Evidence> EvidenceItems { get; set; } = [];
}
