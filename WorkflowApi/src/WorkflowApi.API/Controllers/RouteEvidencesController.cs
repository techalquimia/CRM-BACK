using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowApi.API.Models;
using WorkflowApi.Application.DTOs;
using WorkflowApi.Application.Exceptions;
using WorkflowApi.Application.Interfaces;
using WorkflowApi.Domain.Entities;

namespace WorkflowApi.API.Controllers;

/// <summary>
/// CRUD for route evidences (unit, type, GPS location, date/time, image URL).
/// POST accepts multipart/form-data; image is uploaded to GCP Storage first, then the evidence is created.
/// </summary>
[ApiController]
[Route("api/route-evidences")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RouteEvidencesController : ControllerBase
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IEvidenceStorageService _storageService;
    private readonly ILogger<RouteEvidencesController> _logger;

    public RouteEvidencesController(
        IEvidenceRepository evidenceRepository,
        IUnitRepository unitRepository,
        IEvidenceStorageService storageService,
        ILogger<RouteEvidencesController> logger)
    {
        _evidenceRepository = evidenceRepository;
        _unitRepository = unitRepository;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>Gets all route evidences, optionally filtered by unit.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EvidenceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? unitId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Evidence> evidences = unitId.HasValue
            ? await _evidenceRepository.GetByUnitIdAsync(unitId.Value, cancellationToken)
            : await _evidenceRepository.GetAllAsync(cancellationToken);
        return Ok(evidences.Select(MapToResponse));
    }

    /// <summary>Gets a route evidence by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EvidenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var evidence = await _evidenceRepository.GetByIdAsync(id, cancellationToken);
        if (evidence == null)
            return NotFound(new { message = "Evidence not found." });
        return Ok(MapToResponse(evidence));
    }

    /// <summary>
    /// Creates a new route evidence. Send as multipart/form-data with UnitId, TypeEvidence, optional Latitude, Longitude, RecordedAtUtc, and optional ImageFile.
    /// If ImageFile is present, it is uploaded to GCP Storage first and the returned URL is saved as ImageUrl.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EvidenceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create([FromForm] CreateEvidenceFormRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });
        if (string.IsNullOrWhiteSpace(request.UnitId) || !Guid.TryParse(request.UnitId, out var unitId))
            return BadRequest(new { message = "Valid UnitId is required." });
        if (string.IsNullOrWhiteSpace(request.TypeEvidence))
            return BadRequest(new { message = "TypeEvidence is required." });

        var unitExists = await _unitRepository.GetByIdAsync(unitId, cancellationToken);
        if (unitExists == null)
            return NotFound(new { message = "Unit not found." });

        string? imageUrl = null;
        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            try
            {
                var contentType = request.ImageFile.ContentType ?? "application/octet-stream";
                if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/jpeg";
                await using var stream = request.ImageFile.OpenReadStream();
                imageUrl = await _storageService.UploadImageAsync(stream, request.ImageFile.FileName, unitExists.NumberUnit, contentType, cancellationToken);
            }
            catch (EvidenceStorageException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
            }
        }

        decimal? latitude = ParseLatitude(request.Latitude);
        decimal? longitude = ParseLongitude(request.Longitude);

        DateTime recordedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.RecordedAtUtc) && DateTime.TryParse(request.RecordedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            recordedAtUtc = parsed.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc) : parsed.ToUniversalTime();

        var evidence = new Evidence
        {
            Id = Guid.NewGuid(),
            UnitId = unitId,
            TypeEvidence = request.TypeEvidence.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            RecordedAtUtc = recordedAtUtc,
            ImageUrl = imageUrl
        };

        var created = await _evidenceRepository.AddAsync(evidence, cancellationToken);
        _logger.LogInformation("Route evidence created: {Id}", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    /// <summary>Updates a route evidence.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EvidenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEvidenceRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is invalid." });

        var existing = await _evidenceRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
            return NotFound(new { message = "Evidence not found." });

        var evidence = new Evidence
        {
            Id = id,
            UnitId = existing.UnitId,
            TypeEvidence = request.TypeEvidence ?? existing.TypeEvidence,
            Latitude = ClampLatitude(request.Latitude ?? existing.Latitude),
            Longitude = ClampLongitude(request.Longitude ?? existing.Longitude),
            RecordedAtUtc = request.RecordedAtUtc ?? existing.RecordedAtUtc,
            ImageUrl = request.ImageUrl ?? existing.ImageUrl
        };

        var updated = await _evidenceRepository.UpdateAsync(evidence, cancellationToken);
        if (updated == null)
            return NotFound(new { message = "Evidence not found." });

        _logger.LogInformation("Route evidence updated: {Id}", id);
        return Ok(MapToResponse(updated));
    }

    /// <summary>Deletes a route evidence.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _evidenceRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { message = "Evidence not found." });
        _logger.LogInformation("Route evidence deleted: {Id}", id);
        return NoContent();
    }

    private static EvidenceResponse MapToResponse(Evidence evidence)
    {
        return new EvidenceResponse(
            evidence.Id,
            evidence.UnitId,
            evidence.TypeEvidence,
            evidence.Latitude,
            evidence.Longitude,
            evidence.RecordedAtUtc,
            evidence.ImageUrl,
            evidence.CreatedAtUtc
        );
    }

    /// <summary>Parsea y limita la latitud al rango [-90, 90] y 7 decimales (decimal(10,7)).</summary>
    private static decimal? ParseLatitude(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d))
            return null;
        d = Math.Clamp(d, -90m, 90m);
        return Math.Round(d, 7);
    }

    /// <summary>Parsea y limita la longitud al rango [-180, 180] y 7 decimales (decimal(10,7)).</summary>
    private static decimal? ParseLongitude(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d))
            return null;
        d = Math.Clamp(d, -180m, 180m);
        return Math.Round(d, 7);
    }

    private static decimal? ClampLatitude(decimal? value)
    {
        if (value == null) return null;
        return Math.Round(Math.Clamp(value.Value, -90m, 90m), 7);
    }

    private static decimal? ClampLongitude(decimal? value)
    {
        if (value == null) return null;
        return Math.Round(Math.Clamp(value.Value, -180m, 180m), 7);
    }
}
