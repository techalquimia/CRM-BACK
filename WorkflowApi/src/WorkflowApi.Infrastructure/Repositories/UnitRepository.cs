using Microsoft.EntityFrameworkCore;
using WorkflowApi.Application.Interfaces;
using WorkflowApi.Domain.Entities;
using WorkflowApi.Infrastructure.Persistence;

namespace WorkflowApi.Infrastructure.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly WorkflowDbContext _context;

    public UnitRepository(WorkflowDbContext context)
    {
        _context = context;
    }

    public async Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Unit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Units
            .AsNoTracking()
            .OrderBy(u => u.NumberUnit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Unit?> GetByNumberUnitAsync(string numberUnit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numberUnit))
            return null;

        return await _context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NumberUnit == numberUnit.Trim(), cancellationToken);
    }

    public async Task<Unit> AddAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        unit.CreatedAtUtc = DateTime.UtcNow;
        _context.Units.Add(unit);
        await _context.SaveChangesAsync(cancellationToken);
        return unit;
    }

    public async Task<Unit?> UpdateAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Units.FindAsync(unit.Id, cancellationToken);
        if (existing == null)
            return null;

        existing.NumberUnit = unit.NumberUnit;
        existing.Description = unit.Description;
        existing.IsActive = unit.IsActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unit = await _context.Units.FindAsync(id, cancellationToken);
        if (unit == null)
            return false;

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
