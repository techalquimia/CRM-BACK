namespace WorkflowApi.API.Models;

/// <summary>
/// Form model for creating route evidence with optional image file (multipart/form-data).
/// When ImageFile is present, it is uploaded to GCP Storage first and the returned URL is saved as ImageUrl.
/// </summary>
public class CreateEvidenceFormRequest
{
    public string? UnitId { get; set; }
    public string? TypeEvidence { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? RecordedAtUtc { get; set; }
    public IFormFile? ImageFile { get; set; }
}
