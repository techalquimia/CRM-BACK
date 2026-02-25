namespace RouteEvidence.Application.DTOs;

public record RouteEvidenceDto(
    Guid Id,
    Guid RouteId,
    string EvidenceType,
    string? Description,
    DateTime RecordedAtUtc,
    double? Latitude,
    double? Longitude,
    string? Metadata,
    string? AttachmentUrl,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
