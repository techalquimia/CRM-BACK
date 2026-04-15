using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowApi.Application.DTOs;
using WorkflowApi.Application.Interfaces;

namespace WorkflowApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login by unit number. Validates against the Units table; the field evaluated is NumberUnit.
    /// </summary>
    /// <param name="request">Transport unit number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token and basic data if the unit exists and is active.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.NumberUnit))
        {
            _logger.LogWarning("Login attempted without NumberUnit");
            return BadRequest(new { message = "NumberUnit is required." });
        }

        var response = await _authService.LoginAsync(request, cancellationToken);
        if (response == null)
        {
            _logger.LogWarning("Login failed for unit: {NumberUnit}", request.NumberUnit);
            return Unauthorized(new { message = "Invalid unit number or inactive unit." });
        }

        _logger.LogInformation("Login successful for unit: {NumberUnit}", request.NumberUnit);
        return Ok(response);
    }
}
