namespace RouteEvidence.Application.DTOs;

public record CreateRouteEvidenceRequest(
    Guid RouteId,
    string EvidenceType,
    string? Description,
    DateTime RecordedAtUtc,
    double? Latitude,
    double? Longitude,
    string? Metadata,
    string? AttachmentUrl);
