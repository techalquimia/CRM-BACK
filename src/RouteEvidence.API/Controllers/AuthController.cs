using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RouteEvidence.Application.Interfaces;

namespace RouteEvidence.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUnitRepository _unitRepository;

    public AuthController(IConfiguration configuration, IUnitRepository unitRepository)
    {
        _configuration = configuration;
        _unitRepository = unitRepository;
    }

    /// <summary>Login to obtain a JWT Bearer token. Validates against Units table (NumberUnit and Activo).</summary>
    /// <param name="request">NumberUnit: unit number; must exist in Units with Activo = true.</param>
    /// <response code="200">Returns the access token.</response>
    /// <response code="401">Unit not found or inactive.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var numberUnit = request?.NumberUnit?.Trim();
        if (string.IsNullOrEmpty(numberUnit))
            return Unauthorized(new { error = "NumberUnit is required." });

        var unit = await _unitRepository.GetByNumberUnitAndActivoAsync(numberUnit, activo: true, cancellationToken);
        if (unit is null)
            return Unauthorized(new { error = "Unit not found or inactive." });

        var token = GenerateJwt(unit.NumberUnit);
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : 1440;
        return Ok(new LoginResponse(token, DateTime.UtcNow.AddMinutes(expiryMinutes)));
    }

    private string GenerateJwt(string numberUnit)
    {
        var key = _configuration["Jwt:Key"] ?? "RouteEvidenceSecretKeyMin32Chars!!";
        var issuer = _configuration["Jwt:Issuer"] ?? "RouteEvidenceApi";
        var audience = _configuration["Jwt:Audience"] ?? "RouteEvidenceApi";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : 1440; // 24h

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, numberUnit),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, numberUnit)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string NumberUnit);
public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
