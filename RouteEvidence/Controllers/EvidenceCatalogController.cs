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
public class EvidenceCatalogController : ControllerBase
{
    private readonly RouteEvidenceDbContext _db;

    public EvidenceCatalogController(RouteEvidenceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? active, CancellationToken ct)
    {
        var query = _db.EvidenceCatalog.AsNoTracking();
        if (active.HasValue)
            query = query.Where(c => c.Active == active.Value);

        var catalogItems = await query.OrderBy(c => c.Type).ToListAsync(ct);
        return Ok(catalogItems);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var item = await _db.EvidenceCatalog.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateEvidenceCatalogRequest request, CancellationToken ct)
    {
        var exists = await _db.EvidenceCatalog.AnyAsync(c => c.Type == request.Type, ct);
        if (exists)
            return BadRequest(new { Message = "An evidence type with this name already exists" });

        var catalogItem = new EvidenceCatalog
        {
            Type = request.Type,
            Description = request.Description,
            Ocr = request.Ocr
        };

        _db.EvidenceCatalog.Add(catalogItem);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = catalogItem.Id }, catalogItem);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateEvidenceCatalogRequest request, CancellationToken ct)
    {
        var item = await _db.EvidenceCatalog.FindAsync([id], ct);
        if (item is null)
            return NotFound();

        var typeExists = await _db.EvidenceCatalog.AnyAsync(c => c.Type == request.Type && c.Id != id, ct);
        if (typeExists)
            return BadRequest(new { Message = "An evidence type with this name already exists" });

        item.Type = request.Type;
        item.Description = request.Description;
        item.Active = request.Active;
        item.Ocr = request.Ocr;

        await _db.SaveChangesAsync(ct);
        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.EvidenceCatalog.FindAsync([id], ct);
        if (item is null)
            return NotFound();

        _db.EvidenceCatalog.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
