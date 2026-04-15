using WorkflowApi.Domain.Entities;

namespace WorkflowApi.Application.Interfaces;

public interface IEvidenceRepository
{
    Task<Evidence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Evidence>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Evidence>> GetByUnitIdAsync(Guid unitId, CancellationToken cancellationToken = default);
    Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default);
    Task<Evidence?> UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
