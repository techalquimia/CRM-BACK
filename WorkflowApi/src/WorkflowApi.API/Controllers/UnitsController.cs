using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowApi.Application.DTOs;
using WorkflowApi.Application.Interfaces;
using WorkflowApi.Domain.Entities;

namespace WorkflowApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UnitsController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitRepository unitRepository, ILogger<UnitsController> logger)
    {
        _unitRepository = unitRepository;
        _logger = logger;
    }

    /// <summary>Gets all units.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UnitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var units = await _unitRepository.GetAllAsync(cancellationToken);
        var response = units.Select(MapToResponse);
        return Ok(response);
    }

    /// <summary>Gets a unit by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(id, cancellationToken);
        if (unit == null)
            return NotFound(new { message = "Unit not found." });
        return Ok(MapToResponse(unit));
    }

    /// <summary>Creates a new unit.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.NumberUnit))
            return BadRequest(new { message = "NumberUnit is required." });

        var existing = await _unitRepository.GetByNumberUnitAsync(request.NumberUnit.Trim(), cancellationToken);
        if (existing != null)
            return Conflict(new { message = $"A unit with NumberUnit '{request.NumberUnit}' already exists." });

        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            NumberUnit = request.NumberUnit.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        var created = await _unitRepository.AddAsync(unit, cancellationToken);
        _logger.LogInformation("Unit created: {NumberUnit}", created.NumberUnit);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    /// <summary>Updates an existing unit.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is invalid." });

        var existing = await _unitRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            return NotFound(new { message = "Unit not found." });

        var unit = new Unit
        {
            Id = id,
            NumberUnit = request.NumberUnit ?? existing.NumberUnit,
            Description = request.Description ?? existing.Description,
            IsActive = request.IsActive ?? existing.IsActive
        };

        if (unit.NumberUnit != existing.NumberUnit)
        {
            var duplicate = await _unitRepository.GetByNumberUnitAsync(unit.NumberUnit, cancellationToken);
            if (duplicate != null)
                return Conflict(new { message = $"A unit with NumberUnit '{unit.NumberUnit}' already exists." });
        }

        var updated = await _unitRepository.UpdateAsync(unit, cancellationToken);
        if (updated == null)
            return NotFound(new { message = "Unit not found." });

        _logger.LogInformation("Unit updated: {Id}", id);
        return Ok(MapToResponse(updated));
    }

    /// <summary>Deletes a unit.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _unitRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { message = "Unit not found." });
        _logger.LogInformation("Unit deleted: {Id}", id);
        return NoContent();
    }

    private static UnitResponse MapToResponse(Unit unit)
    {
        return new UnitResponse(
            unit.Id,
            unit.NumberUnit,
            unit.Description,
            unit.IsActive,
            unit.CreatedAtUtc,
            unit.UpdatedAtUtc
        );
    }
}
