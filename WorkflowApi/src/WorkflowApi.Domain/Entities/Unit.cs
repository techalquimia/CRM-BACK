namespace WorkflowApi.Domain.Entities;

/// <summary>
/// Transport unit. Login is validated against the unit number (NumberUnit).
/// </summary>
public class Unit
{
    public Guid Id { get; set; }
    public string NumberUnit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
