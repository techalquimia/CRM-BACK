using WorkflowApi.Application.DTOs;

namespace WorkflowApi.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
