using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RouteEvidence.Models;

namespace RouteEvidence.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates and returns a JWT token.
    /// Demo credentials: admin / admin
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Demo validation - in production validate against database
        if (request.Username != "admin" || request.Password != "admin")
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        var token = GenerateJwtToken(request.Username);
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        _logger.LogInformation("Successful login for user: {Username}", request.Username);

        return Ok(new LoginResponse(
            token,
            DateTime.UtcNow.AddMinutes(expirationMinutes)
        ));
    }

    /// <summary>
    /// Validates the current token (protected endpoint).
    /// </summary>
    [HttpGet("validate")]
    [Authorize]
    public IActionResult Validate()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { Message = "Token is valid", Username = username });
    }

    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
