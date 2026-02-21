namespace RouteEvidence.DTOs;

public record CreateUnitRequest(
    string Plate,
    string? EconomicNumber = null,
    string? Brand = null,
    string? Model = null,
    string? Year = null
);
