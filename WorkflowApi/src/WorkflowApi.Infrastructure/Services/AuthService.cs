using Microsoft.Extensions.Configuration;
using WorkflowApi.Application.DTOs;
using WorkflowApi.Application.Interfaces;
using WorkflowApi.Infrastructure.Services;

namespace WorkflowApi.Infrastructure;

public class AuthService : IAuthService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUnitRepository unitRepository,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _unitRepository = unitRepository;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var unit = await _unitRepository.GetByNumberUnitAsync(request.NumberUnit, cancellationToken);
        if (unit == null || !unit.IsActive)
            return null;

        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : 60;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var token = _jwtTokenService.GenerateToken(unit.NumberUnit, expiresAtUtc);

        return new LoginResponse(token, unit.Id, unit.NumberUnit, expiresAtUtc);
    }
}
