namespace RouteEvidence.DTOs;

public record UpdateEvidenceCatalogRequest(
    string Type,
    string? Description = null,
    bool Active = true,
    bool Ocr = false
);
