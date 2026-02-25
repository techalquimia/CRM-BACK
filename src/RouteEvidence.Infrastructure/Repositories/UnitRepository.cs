using Microsoft.EntityFrameworkCore;
using RouteEvidence.Application.Interfaces;
using RouteEvidence.Domain.Entities;
using RouteEvidence.Infrastructure.Persistence;

namespace RouteEvidence.Infrastructure.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly RouteEvidenceDbContext _context;

    public UnitRepository(RouteEvidenceDbContext context)
    {
        _context = context;
    }

    public async Task<UnitEntity?> GetByNumberUnitAndActivoAsync(string numberUnit, bool activo = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numberUnit)) return null;
        return await _context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NumberUnit == numberUnit.Trim() && u.Activo == activo, cancellationToken);
    }
}
