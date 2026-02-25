using RouteEvidence.Application.DTOs;

namespace RouteEvidence.Application.Services;

public interface IRouteEvidenceService
{
    Task<RouteEvidenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RouteEvidenceDto>> GetByRouteIdAsync(Guid routeId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<RouteEvidenceDto> Items, int TotalCount)> GetAllAsync(int page, int pageSize, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<RouteEvidenceDto> CreateAsync(CreateRouteEvidenceRequest request, CancellationToken cancellationToken = default);
    Task<RouteEvidenceDto?> UpdateAsync(Guid id, UpdateRouteEvidenceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
