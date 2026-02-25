using Microsoft.EntityFrameworkCore;
using RouteEvidence.Application.Interfaces;
using RouteEvidence.Domain.Entities;
using RouteEvidence.Infrastructure.Persistence;

namespace RouteEvidence.Infrastructure.Repositories;

public class RouteEvidenceRepository : IRouteEvidenceRepository
{
    private readonly RouteEvidenceDbContext _context;

    public RouteEvidenceRepository(RouteEvidenceDbContext context)
    {
        _context = context;
    }

    public async Task<RouteEvidenceEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.RouteEvidences
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RouteEvidenceEntity>> GetByRouteIdAsync(Guid routeId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.RouteEvidences.AsNoTracking().Where(e => e.RouteId == routeId);
        if (activeOnly) query = query.Where(e => e.IsActive);
        return await query
            .OrderBy(e => e.RecordedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RouteEvidenceEntity>> GetAllAsync(int page, int pageSize, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.RouteEvidences.AsNoTracking();
        if (activeOnly) query = query.Where(e => e.IsActive);
        return await query
            .OrderByDescending(e => e.RecordedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.RouteEvidences.AsNoTracking();
        if (activeOnly) query = query.Where(e => e.IsActive);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<RouteEvidenceEntity> AddAsync(RouteEvidenceEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.RouteEvidences.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(RouteEvidenceEntity entity, CancellationToken cancellationToken = default)
    {
        _context.RouteEvidences.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RouteEvidences.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _context.RouteEvidences.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
