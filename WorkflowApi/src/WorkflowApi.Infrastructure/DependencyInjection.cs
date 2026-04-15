using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkflowApi.Application.Interfaces;
using WorkflowApi.Infrastructure.Persistence;
using WorkflowApi.Infrastructure.Repositories;
using WorkflowApi.Infrastructure.Services;

namespace WorkflowApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=WorkflowDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";

        services.AddDbContext<WorkflowDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IEvidenceRepository, EvidenceRepository>();

        // Storage: Azure Blob Storage (implementación por defecto)
        services.AddScoped<IEvidenceStorageService, AzureBlobEvidenceStorageService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
