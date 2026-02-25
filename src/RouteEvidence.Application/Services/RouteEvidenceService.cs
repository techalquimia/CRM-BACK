using RouteEvidence.Application.DTOs;
using RouteEvidence.Application.Interfaces;
using RouteEvidence.Domain.Entities;

namespace RouteEvidence.Application.Services;

public class RouteEvidenceService : IRouteEvidenceService
{
    private readonly IRouteEvidenceRepository _repository;

    public RouteEvidenceService(IRouteEvidenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<RouteEvidenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<RouteEvidenceDto>> GetByRouteIdAsync(Guid routeId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByRouteIdAsync(routeId, activeOnly, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<(IReadOnlyList<RouteEvidenceDto> Items, int TotalCount)> GetAllAsync(int page, int pageSize, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(page, pageSize, activeOnly, cancellationToken);
        var total = await _repository.CountAsync(activeOnly, cancellationToken);
        return (entities.Select(MapToDto).ToList(), total);
    }

    public async Task<RouteEvidenceDto> CreateAsync(CreateRouteEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        var entity = RouteEvidenceEntity.Create(
            request.RouteId,
            request.EvidenceType,
            request.Description,
            request.RecordedAtUtc,
            request.Latitude,
            request.Longitude,
            request.Metadata,
            request.AttachmentUrl);
        var added = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(added);
    }

    public async Task<RouteEvidenceDto?> UpdateAsync(Guid id, UpdateRouteEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return null;
        entity.Update(request.Description, request.Metadata, request.AttachmentUrl);
        await _repository.UpdateAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return false;
        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static RouteEvidenceDto MapToDto(RouteEvidenceEntity e) =>
        new(
            e.Id,
            e.RouteId,
            e.EvidenceType,
            e.Description,
            e.RecordedAtUtc,
            e.Latitude,
            e.Longitude,
            e.Metadata,
            e.AttachmentUrl,
            e.IsActive,
            e.CreatedAtUtc,
            e.UpdatedAtUtc);
}
