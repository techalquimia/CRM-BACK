namespace WorkflowApi.Application.DTOs;

public record CreateEvidenceRequest(
    Guid UnitId,
    string TypeEvidence,
    decimal? Latitude = null,
    decimal? Longitude = null,
    DateTime? RecordedAtUtc = null,
    string? ImageUrl = null
);
