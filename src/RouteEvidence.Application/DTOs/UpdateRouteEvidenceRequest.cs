namespace RouteEvidence.Application.DTOs;

public record UpdateRouteEvidenceRequest(
    string? Description,
    string? Metadata,
    string? AttachmentUrl);
