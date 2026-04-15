namespace WorkflowApi.Application.DTOs;

public record UnitResponse(
    Guid Id,
    string NumberUnit,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
