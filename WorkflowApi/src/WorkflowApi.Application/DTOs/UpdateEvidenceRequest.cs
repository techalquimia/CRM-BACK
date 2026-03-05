namespace WorkflowApi.Application.DTOs;

public record UpdateEvidenceRequest(
    string? TypeEvidence = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    DateTime? RecordedAtUtc = null,
    string? ImageUrl = null
);
