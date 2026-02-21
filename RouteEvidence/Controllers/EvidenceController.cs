using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteEvidence.Data;
using RouteEvidence.DTOs;
using RouteEvidence.Models;
using RouteEvidence.Services;

namespace RouteEvidence.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EvidenceController : ControllerBase
{
    private readonly RouteEvidenceDbContext _db;
    private readonly IGcpStorageService _storageService;
    private readonly ITicketOcrService _ticketOcrService;
    private readonly ILogger<EvidenceController> _logger;

    public EvidenceController(
        RouteEvidenceDbContext db,
        IGcpStorageService storageService,
        ITicketOcrService ticketOcrService,
        ILogger<EvidenceController> logger)
    {
        _db = db;
        _storageService = storageService;
        _ticketOcrService = ticketOcrService;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var evidence = await _db.Evidence
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (evidence is null)
            return NotFound();

        return Ok(MapToResponse(evidence));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? unitId,
        [FromQuery] bool? isSynced,
        CancellationToken ct)
    {
        var query = _db.Evidence.AsNoTracking();

        if (unitId.HasValue)
            query = query.Where(e => e.UnitId == unitId.Value);
        if (isSynced.HasValue)
            query = query.Where(e => e.IsSynced == isSynced.Value);

        var evidenceList = await query
            .OrderByDescending(e => e.DateTime)
            .ToListAsync(ct);

        return Ok(evidenceList.Select(MapToResponse));
    }

    /// <summary>
    /// Create evidence with image file upload. Image is uploaded to GCP Storage.
    /// Use multipart/form-data with Image file and form fields.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Post([FromForm] CreateEvidenceFormRequest request, CancellationToken ct)
    {
        var unitExists = await _db.Units.AnyAsync(u => u.Id == request.UnitId, ct);
        if (!unitExists)
            return BadRequest(new { Message = "The specified unit does not exist" });

        if (request.Image.Length == 0)
            return BadRequest(new { Message = "Image file is required" });

        if (request.TotalWeight.HasValue && request.Tara.HasValue && request.NetWeight.HasValue)
        {
            var expectedTotal = request.Tara.Value + request.NetWeight.Value;
            if (Math.Abs(request.TotalWeight.Value - expectedTotal) > 0.001)
                return BadRequest(new { Message = "TotalWeight must equal Tara + NetWeight" });
        }

        var extension = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension)) extension = ".jpg";
        var objectKey = $"evidence/{request.UnitId:N}/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{extension}";
        var contentType = request.Image.ContentType ?? "image/jpeg";

        await using var stream = request.Image.OpenReadStream();
        var (bucket, key) = await _storageService.UploadAsync(stream, objectKey, contentType, ct);

        double? totalWeight = request.TotalWeight;
        double? tara = request.Tara;
        double? netWeight = request.NetWeight;
        string? ocrText = null;

        var catalogItem = await _db.EvidenceCatalog
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Type == request.EvidenceType, ct);
        var useOcr = catalogItem?.Ocr == true || string.Equals(request.EvidenceType, "ticket", StringComparison.OrdinalIgnoreCase);

        if (useOcr)
        {
            try
            {
                var ocrResult = await _ticketOcrService.ExtractTicketDataAsync(bucket, key, ct);
                ocrText = ocrResult.FullText;

                if (!totalWeight.HasValue && ocrResult.TotalWeight.HasValue)
                    totalWeight = ocrResult.TotalWeight;
                if (!tara.HasValue && ocrResult.Tara.HasValue)
                    tara = ocrResult.Tara;
                if (!netWeight.HasValue && ocrResult.NetWeight.HasValue)
                    netWeight = ocrResult.NetWeight;

                if (totalWeight.HasValue || tara.HasValue || netWeight.HasValue)
                    _logger.LogInformation("Extracted ticket data: Total={Total}, Tara={Tara}, Net={Net}", totalWeight, tara, netWeight);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract ticket data from image");
            }
        }

        if (totalWeight.HasValue && tara.HasValue && netWeight.HasValue)
        {
            var expectedTotal = tara.Value + netWeight.Value;
            if (Math.Abs(totalWeight.Value - expectedTotal) > 0.001)
                return BadRequest(new { Message = "TotalWeight must equal Tara + NetWeight" });
        }

        var evidence = new Evidence
        {
            GcsBucket = bucket,
            GcsObjectKey = key,
            DateTime = request.DateTime,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            EvidenceType = request.EvidenceType,
            UnitId = request.UnitId,
            IsSynced = false,
            TotalWeight = totalWeight,
            Tara = tara,
            NetWeight = netWeight,
            OcrText = ocrText
        };

        _db.Evidence.Add(evidence);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Evidence created: {Id}", evidence.Id);

        return CreatedAtAction(nameof(Get), new { id = evidence.Id }, MapToResponse(evidence));
    }

    /// <summary>
    /// Create evidence with pre-uploaded GCS reference (bucket + object key).
    /// Use when image is already in GCP Storage.
    /// </summary>
    [HttpPost("with-reference")]
    public async Task<IActionResult> PostWithReference([FromBody] CreateEvidenceRequest request, CancellationToken ct)
    {
        var unitExists = await _db.Units.AnyAsync(u => u.Id == request.UnitId, ct);
        if (!unitExists)
            return BadRequest(new { Message = "The specified unit does not exist" });

        if (request.TotalWeight.HasValue && request.Tara.HasValue && request.NetWeight.HasValue)
        {
            var expectedTotal = request.Tara.Value + request.NetWeight.Value;
            if (Math.Abs(request.TotalWeight.Value - expectedTotal) > 0.001)
                return BadRequest(new { Message = "TotalWeight must equal Tara + NetWeight" });
        }

        var evidence = new Evidence
        {
            GcsBucket = request.GcsBucket,
            GcsObjectKey = request.GcsObjectKey,
            DateTime = request.DateTime,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            EvidenceType = request.EvidenceType,
            UnitId = request.UnitId,
            IsSynced = false,
            TotalWeight = request.TotalWeight,
            Tara = request.Tara,
            NetWeight = request.NetWeight
        };

        _db.Evidence.Add(evidence);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Evidence created with reference: {Id}", evidence.Id);

        return CreatedAtAction(nameof(Get), new { id = evidence.Id }, MapToResponse(evidence));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateEvidenceRequest request, CancellationToken ct)
    {
        var evidence = await _db.Evidence.FindAsync([id], ct);
        if (evidence is null)
            return NotFound();

        var unitExists = await _db.Units.AnyAsync(u => u.Id == request.UnitId, ct);
        if (!unitExists)
            return BadRequest(new { Message = "The specified unit does not exist" });

        if (request.TotalWeight.HasValue && request.Tara.HasValue && request.NetWeight.HasValue)
        {
            var expectedTotal = request.Tara.Value + request.NetWeight.Value;
            if (Math.Abs(request.TotalWeight.Value - expectedTotal) > 0.001)
                return BadRequest(new { Message = "TotalWeight must equal Tara + NetWeight" });
        }

        evidence.GcsBucket = request.GcsBucket;
        evidence.GcsObjectKey = request.GcsObjectKey;
        evidence.DateTime = request.DateTime;
        evidence.Latitude = request.Latitude;
        evidence.Longitude = request.Longitude;
        evidence.EvidenceType = request.EvidenceType;
        evidence.IsSynced = request.IsSynced;
        evidence.UnitId = request.UnitId;
        evidence.TotalWeight = request.TotalWeight;
        evidence.Tara = request.Tara;
        evidence.NetWeight = request.NetWeight;

        await _db.SaveChangesAsync(ct);
        return Ok(MapToResponse(evidence));
    }

    [HttpPatch("{id:guid}/sync")]
    public async Task<IActionResult> MarkAsSynced(Guid id, CancellationToken ct)
    {
        var evidence = await _db.Evidence.FindAsync([id], ct);
        if (evidence is null)
            return NotFound();

        evidence.IsSynced = true;
        await _db.SaveChangesAsync(ct);

        return Ok(MapToResponse(evidence));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var evidence = await _db.Evidence.FindAsync([id], ct);
        if (evidence is null)
            return NotFound();

        _db.Evidence.Remove(evidence);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static EvidenceResponse MapToResponse(Evidence e) =>
        new(e.Id, e.GcsBucket, e.GcsObjectKey, e.DateTime, e.Latitude, e.Longitude, e.EvidenceType, e.IsSynced, e.CreatedAt, e.UnitId, e.TotalWeight, e.Tara, e.NetWeight, e.OcrText);
}
