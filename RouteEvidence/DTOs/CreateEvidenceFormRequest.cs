namespace RouteEvidence.DTOs;

/// <summary>
/// Request for creating evidence with image file upload.
/// Use multipart/form-data with form fields and Image file.
/// </summary>
public class CreateEvidenceFormRequest
{
    public IFormFile Image { get; set; } = null!;
    public DateTime DateTime { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public double? TotalWeight { get; set; }
    public double? Tara { get; set; }
    public double? NetWeight { get; set; }
}
