using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteEvidence.Application.DTOs;
using RouteEvidence.Application.Services;

namespace RouteEvidence.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class RouteEvidenceController : ControllerBase
{
    private readonly IRouteEvidenceService _service;

    public RouteEvidenceController(IRouteEvidenceService service)
    {
        _service = service;
    }

    /// <summary>Get route evidence by ID.</summary>
    /// <param name="id">Route evidence ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the route evidence.</response>
    /// <response code="404">Evidence not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RouteEvidenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteEvidenceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Get all evidence for a route.</summary>
    /// <param name="routeId">Route ID.</param>
    /// <param name="activeOnly">If true, returns only active evidence (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("route/{routeId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<RouteEvidenceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteEvidenceDto>>> GetByRouteId(
        Guid routeId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetByRouteIdAsync(routeId, activeOnly, cancellationToken);
        return Ok(items);
    }

    /// <summary>Get all route evidence with pagination.</summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="activeOnly">If true, returns only active evidence (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RouteEvidenceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RouteEvidenceDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;
        var (items, totalCount) = await _service.GetAllAsync(page, pageSize, activeOnly, cancellationToken);
        return Ok(new PagedResult<RouteEvidenceDto>(items, totalCount, page, pageSize));
    }

    /// <summary>Create a new route evidence.</summary>
    /// <param name="request">Create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Evidence created.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RouteEvidenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RouteEvidenceDto>> Create(
        [FromBody] CreateRouteEvidenceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an existing route evidence.</summary>
    /// <param name="id">Route evidence ID.</param>
    /// <param name="request">Update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Evidence updated.</response>
    /// <response code="404">Evidence not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RouteEvidenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteEvidenceDto>> Update(
        Guid id,
        [FromBody] UpdateRouteEvidenceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete route evidence.</summary>
    /// <param name="id">Route evidence ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Evidence deleted.</response>
    /// <response code="404">Evidence not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
