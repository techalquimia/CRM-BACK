using RouteEvidence.Domain.Entities;

namespace RouteEvidence.Application.Interfaces;

public interface IUnitRepository
{
    Task<UnitEntity?> GetByNumberUnitAndActivoAsync(string numberUnit, bool activo = true, CancellationToken cancellationToken = default);
}
