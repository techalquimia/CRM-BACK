using RouteEvidence.Domain.Common;

namespace RouteEvidence.Domain.Entities;

public class UnitEntity : Entity
{
    public string NumberUnit { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private UnitEntity() { }

    private UnitEntity(string numberUnit, bool activo)
    {
        NumberUnit = numberUnit;
        Activo = activo;
    }

    public static UnitEntity Create(string numberUnit, bool activo = true)
    {
        if (string.IsNullOrWhiteSpace(numberUnit))
            throw new ArgumentException("NumberUnit is required.", nameof(numberUnit));
        return new UnitEntity(numberUnit.Trim(), activo);
    }

    public void SetActivo(bool activo)
    {
        Activo = activo;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
