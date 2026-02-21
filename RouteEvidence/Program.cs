using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RouteEvidence.Data;
using RouteEvidence.Services;

var builder = WebApplication.CreateBuilder(args);

// GCP credentials - config or env GOOGLE_APPLICATION_CREDENTIALS
var credsPath = builder.Configuration["Gcp:CredentialsPath"];
if (!string.IsNullOrWhiteSpace(credsPath) && File.Exists(credsPath))
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credsPath);

builder.Services.AddControllers();
builder.Services.AddScoped<IGcpStorageService, GcpStorageService>();
builder.Services.AddScoped<ITicketOcrService, TicketOcrService>();

// MariaDB with Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=RouteEvidence;User=root;Password=;";
builder.Services.AddDbContext<RouteEvidenceDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RouteEvidenceAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RouteEvidenceApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Route Evidence API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Route Evidence API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
