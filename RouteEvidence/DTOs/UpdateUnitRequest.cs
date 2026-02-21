namespace RouteEvidence.DTOs;

public record UpdateUnitRequest(
    string Plate,
    string? EconomicNumber = null,
    string? Brand = null,
    string? Model = null,
    string? Year = null,
    bool Active = true
);
