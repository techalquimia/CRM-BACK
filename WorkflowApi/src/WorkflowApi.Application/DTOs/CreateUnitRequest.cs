namespace WorkflowApi.Application.DTOs;

public record CreateUnitRequest(
    string NumberUnit,
    string? Description = null,
    bool IsActive = true
);
