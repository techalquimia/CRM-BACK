using Microsoft.EntityFrameworkCore;
using WorkflowApi.Application.Interfaces;
using WorkflowApi.Domain.Entities;
using WorkflowApi.Infrastructure.Persistence;

namespace WorkflowApi.Infrastructure.Repositories;

public class EvidenceRepository : IEvidenceRepository
{
    private readonly WorkflowDbContext _context;

    public EvidenceRepository(WorkflowDbContext context)
    {
        _context = context;
    }

    public async Task<Evidence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Evidences
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Evidence>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Evidences
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Evidence>> GetByUnitIdAsync(Guid unitId, CancellationToken cancellationToken = default)
    {
        return await _context.Evidences
            .AsNoTracking()
            .Where(e => e.UnitId == unitId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default)
    {
        evidence.CreatedAtUtc = DateTime.UtcNow;
        _context.Evidences.Add(evidence);
        await _context.SaveChangesAsync(cancellationToken);
        return evidence;
    }

    public async Task<Evidence?> UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Evidences.FindAsync(evidence.Id, cancellationToken);
        if (existing == null)
            return null;

        existing.TypeEvidence = evidence.TypeEvidence;
        existing.Latitude = evidence.Latitude;
        existing.Longitude = evidence.Longitude;
        existing.RecordedAtUtc = evidence.RecordedAtUtc;
        existing.ImageUrl = evidence.ImageUrl;
        existing.UnitId = evidence.UnitId;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evidence = await _context.Evidences.FindAsync(id, cancellationToken);
        if (evidence == null)
            return false;

        _context.Evidences.Remove(evidence);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
