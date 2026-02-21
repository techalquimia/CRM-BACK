namespace RouteEvidence.DTOs;

public record CreateEvidenceCatalogRequest(string Type, string? Description = null, bool Ocr = false);
