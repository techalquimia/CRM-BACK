namespace WorkflowApi.Application.DTOs;

public record LoginResponse(
    string Token,
    Guid UnitId,
    string NumberUnit,
    DateTime ExpiresAtUtc
);
