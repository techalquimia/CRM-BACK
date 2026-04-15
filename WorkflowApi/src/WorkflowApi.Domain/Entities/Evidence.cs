namespace WorkflowApi.Domain.Entities;

/// <summary>
/// Route evidence: id, unit, type, GPS location, date/time, image URL.
/// </summary>
public class Evidence
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string TypeEvidence { get; set; } = string.Empty;
    /// <summary>GPS latitude.</summary>
    public decimal? Latitude { get; set; }
    /// <summary>GPS longitude.</summary>
    public decimal? Longitude { get; set; }
    /// <summary>Date and time when the evidence was recorded.</summary>
    public DateTime RecordedAtUtc { get; set; }
    /// <summary>URL of the evidence image.</summary>
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Unit? Unit { get; set; }
}
