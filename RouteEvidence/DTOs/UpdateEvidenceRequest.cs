namespace RouteEvidence.DTOs;

public record UpdateEvidenceRequest(
    string GcsBucket,
    string GcsObjectKey,
    DateTime DateTime,
    double Latitude,
    double Longitude,
    string EvidenceType,
    bool IsSynced,
    Guid UnitId,
    double? TotalWeight = null,
    double? Tara = null,
    double? NetWeight = null
);
