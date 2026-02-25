using RouteEvidence.Domain.Common;
using RouteEvidence.Domain.ValueObjects;

namespace RouteEvidence.Domain.Entities;

public class RouteEvidenceEntity : Entity
{
    public Guid RouteId { get; private set; }
    public string EvidenceType { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? Metadata { get; private set; }
    public string? AttachmentUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    private RouteEvidenceEntity() { }

    private RouteEvidenceEntity(
        Guid routeId,
        string evidenceType,
        string? description,
        DateTime recordedAtUtc,
        double? latitude,
        double? longitude,
        string? metadata,
        string? attachmentUrl)
    {
        RouteId = routeId;
        EvidenceType = evidenceType;
        Description = description;
        RecordedAtUtc = recordedAtUtc;
        Latitude = latitude;
        Longitude = longitude;
        Metadata = metadata;
        AttachmentUrl = attachmentUrl;
    }

    public static RouteEvidenceEntity Create(
        Guid routeId,
        string evidenceType,
        string? description,
        DateTime recordedAtUtc,
        double? latitude = null,
        double? longitude = null,
        string? metadata = null,
        string? attachmentUrl = null)
    {
        if (string.IsNullOrWhiteSpace(evidenceType))
            throw new ArgumentException("Evidence type is required.", nameof(evidenceType));

        if (latitude.HasValue != longitude.HasValue)
            throw new ArgumentException("Both latitude and longitude must be provided or both null.");

        if (latitude.HasValue && Location.Create(latitude.Value, longitude!.Value) is { IsSuccess: false } locResult)
            throw new ArgumentException(locResult.Error);

        return new RouteEvidenceEntity(
            routeId,
            evidenceType.Trim(),
            description?.Trim(),
            recordedAtUtc.Kind == DateTimeKind.Utc ? recordedAtUtc : DateTime.SpecifyKind(recordedAtUtc, DateTimeKind.Utc),
            latitude,
            longitude,
            metadata?.Trim(),
            attachmentUrl?.Trim());
    }

    public void Update(string? description, string? metadata, string? attachmentUrl)
    {
        Description = description?.Trim();
        Metadata = metadata?.Trim();
        AttachmentUrl = attachmentUrl?.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
