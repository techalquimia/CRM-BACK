using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace WorkflowApi.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateToken(string numberUnit, DateTime expiresAtUtc);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string numberUnit, DateTime expiresAtUtc)
    {
        var key = _configuration["Jwt:Key"] ?? "WorkflowApiSecretKeyMin32Characters!!";
        var issuer = _configuration["Jwt:Issuer"] ?? "WorkflowApi";
        var audience = _configuration["Jwt:Audience"] ?? "WorkflowApi";
        var keyBytes = Encoding.UTF8.GetBytes(key);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, numberUnit),
            new Claim("number_unit", numberUnit)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
