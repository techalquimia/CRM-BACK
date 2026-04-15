namespace WorkflowApi.Application.DTOs;

public record EvidenceResponse(
    Guid Id,
    Guid UnitId,
    string TypeEvidence,
    decimal? Latitude,
    decimal? Longitude,
    DateTime RecordedAtUtc,
    string? ImageUrl,
    DateTime CreatedAtUtc
);
