# Route Evidence API

API .NET para gestión de evidencias de ruta (Route Evidence), implementada con **DDD**, **Clean Code**, **Swagger**, **Repository** y **SQL Server**.

## Estructura de la solución (DDD)

```
src/
├── RouteEvidence.Domain        # Entidades, value objects, lógica de dominio
├── RouteEvidence.Application   # Casos de uso, DTOs, interfaces (repositorios)
├── RouteEvidence.Infrastructure# EF Core, SQL Server, implementación del repositorio
└── RouteEvidence.API           # Controllers, Swagger, entrada HTTP
```

## Requisitos

- .NET 10 SDK
- SQL Server (local, LocalDB o Azure SQL)

## Configuración

1. **Cadena de conexión**: edita `src/RouteEvidence.API/appsettings.json` o `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=RouteEvidenceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Para LocalDB en desarrollo:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RouteEvidenceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

2. **Ejecutar la API** (aplica migraciones automáticamente en el arranque):

```bash
cd src/RouteEvidence.API
dotnet run
```

3. **Swagger**: en desarrollo abre `https://localhost:<puerto>/swagger`.

### JWT (autenticación)

La API usa **JWT Bearer**. Los endpoints de Route Evidence requieren el header `Authorization: Bearer <token>`.

- **Obtener token**: `POST /api/Auth/login` con body `{ "userName": "tu_usuario", "password": "tu_password" }`.  
  (Demo: se acepta cualquier usuario/contraseña no vacíos.)
- Configuración en `appsettings.json`:
  - `Jwt:Key`: clave secreta (mín. 32 caracteres en producción).
  - `Jwt:Issuer`, `Jwt:Audience`: emisor y audiencia.
  - `Jwt:ExpiryMinutes`: validez del token (por defecto 1440 = 24 h).

En Swagger o Postman, tras hacer login copia el `accessToken` y en las peticiones a `/api/RouteEvidence/*` añade el header:  
`Authorization: Bearer <token>`.

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Auth/login` | Login (obtener JWT) — **público** |
| GET | `/api/RouteEvidence/{id}` | Obtener evidencia por ID |
| GET | `/api/RouteEvidence/route/{routeId}` | Listar evidencias por ruta |
| GET | `/api/RouteEvidence?page=1&pageSize=20` | Listar con paginación |
| POST | `/api/RouteEvidence` | Crear evidencia |
| PUT | `/api/RouteEvidence/{id}` | Actualizar evidencia |
| DELETE | `/api/RouteEvidence/{id}` | Eliminar evidencia |

## Migraciones EF Core (opcional)

Si instalas la herramienta global `dotnet-ef`:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/RouteEvidence.Infrastructure --startup-project src/RouteEvidence.API
```

Para crear una nueva migración:

```bash
dotnet ef migrations add NombreMigracion --project src/RouteEvidence.Infrastructure --startup-project src/RouteEvidence.API
```

## Compilar

```bash
dotnet build
```

## Tests

```bash
dotnet test
```
