namespace RouteEvidence.DTOs;

public record CreateEvidenceRequest(
    string GcsBucket,
    string GcsObjectKey,
    DateTime DateTime,
    double Latitude,
    double Longitude,
    string EvidenceType,
    Guid UnitId,
    double? TotalWeight = null,
    double? Tara = null,
    double? NetWeight = null
);
