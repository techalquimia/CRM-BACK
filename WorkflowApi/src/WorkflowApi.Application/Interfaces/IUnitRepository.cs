using WorkflowApi.Domain.Entities;

namespace WorkflowApi.Application.Interfaces;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unit>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Unit?> GetByNumberUnitAsync(string numberUnit, CancellationToken cancellationToken = default);
    Task<Unit> AddAsync(Unit unit, CancellationToken cancellationToken = default);
    Task<Unit?> UpdateAsync(Unit unit, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
