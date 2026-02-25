using RouteEvidence.Domain.Entities;

namespace RouteEvidence.Application.Interfaces;

public interface IRouteEvidenceRepository
{
    Task<RouteEvidenceEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RouteEvidenceEntity>> GetByRouteIdAsync(Guid routeId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RouteEvidenceEntity>> GetAllAsync(int page, int pageSize, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<int> CountAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<RouteEvidenceEntity> AddAsync(RouteEvidenceEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(RouteEvidenceEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
