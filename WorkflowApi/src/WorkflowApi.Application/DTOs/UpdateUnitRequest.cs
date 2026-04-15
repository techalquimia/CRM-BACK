namespace WorkflowApi.Application.DTOs;

public record UpdateUnitRequest(
    string? NumberUnit = null,
    string? Description = null,
    bool? IsActive = null
);
