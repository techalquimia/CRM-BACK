using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
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
            ?? "Server=localhost;Database=WorkflowDb;User=root;Password=;";

        var serverVersion = ServerVersion.Parse("8.0.0");

        services.AddDbContext<WorkflowDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IEvidenceRepository, EvidenceRepository>();

        // Storage: Azure Blob Storage (implementación por defecto)
        services.AddScoped<IEvidenceStorageService, AzureBlobEvidenceStorageService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
