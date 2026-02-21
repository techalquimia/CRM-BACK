using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteEvidence.Data;
using RouteEvidence.DTOs;
using RouteEvidence.Models;

namespace RouteEvidence.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitController : ControllerBase
{
    private readonly RouteEvidenceDbContext _db;
    private readonly ILogger<UnitController> _logger;

    public UnitController(RouteEvidenceDbContext db, ILogger<UnitController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? active, CancellationToken ct)
    {
        var query = _db.Units.AsNoTracking();
        if (active.HasValue)
            query = query.Where(u => u.Active == active.Value);

        var units = await query.OrderBy(u => u.Plate).ToListAsync(ct);
        return Ok(units);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var unit = await _db.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (unit is null)
            return NotFound();

        return Ok(unit);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUnitRequest request, CancellationToken ct)
    {
        var exists = await _db.Units.AnyAsync(u => u.Plate == request.Plate, ct);
        if (exists)
            return BadRequest(new { Message = "A unit with this plate already exists" });

        var unit = new Unit
        {
            Plate = request.Plate,
            EconomicNumber = request.EconomicNumber,
            Brand = request.Brand,
            Model = request.Model,
            Year = request.Year
        };

        _db.Units.Add(unit);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = unit.Id }, unit);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateUnitRequest request, CancellationToken ct)
    {
        var unit = await _db.Units.FindAsync([id], ct);
        if (unit is null)
            return NotFound();

        var plateExists = await _db.Units.AnyAsync(u => u.Plate == request.Plate && u.Id != id, ct);
        if (plateExists)
            return BadRequest(new { Message = "A unit with this plate already exists" });

        unit.Plate = request.Plate;
        unit.EconomicNumber = request.EconomicNumber;
        unit.Brand = request.Brand;
        unit.Model = request.Model;
        unit.Year = request.Year;
        unit.Active = request.Active;
        unit.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(unit);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var unit = await _db.Units.FindAsync([id], ct);
        if (unit is null)
            return NotFound();

        var hasEvidence = await _db.Evidence.AnyAsync(e => e.UnitId == id, ct);
        if (hasEvidence)
            return Conflict(new { Message = "Cannot delete unit with associated evidence. Remove evidence first." });

        _db.Units.Remove(unit);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
