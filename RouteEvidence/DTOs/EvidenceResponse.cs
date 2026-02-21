namespace RouteEvidence.DTOs;

public record EvidenceResponse(
    Guid Id,
    string GcsBucket,
    string GcsObjectKey,
    DateTime DateTime,
    double Latitude,
    double Longitude,
    string EvidenceType,
    bool IsSynced,
    DateTime CreatedAt,
    Guid UnitId,
    double? TotalWeight,
    double? Tara,
    double? NetWeight
);
