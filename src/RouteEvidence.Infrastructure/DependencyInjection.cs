using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RouteEvidence.Application.Interfaces;
using RouteEvidence.Application.Services;
using RouteEvidence.Infrastructure.Persistence;
using RouteEvidence.Infrastructure.Repositories;

namespace RouteEvidence.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=.;Database=RouteEvidenceDb;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<RouteEvidenceDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IRouteEvidenceRepository, RouteEvidenceRepository>();
        services.AddScoped<IRouteEvidenceService, RouteEvidenceService>();
        services.AddScoped<IUnitRepository, UnitRepository>();

        return services;
    }
}
