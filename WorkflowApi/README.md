# WorkflowApi

API .NET 8 con autenticación JWT y login por **número de unidad**. Arquitectura DDD y Clean Code.

## Estructura (DDD)

- **WorkflowApi.Domain**: Entidad `Unit` (unidad de transporte).
- **WorkflowApi.Application**: DTOs (`LoginRequest`, `LoginResponse`), interfaces `IAuthService`, `IUnitRepository`.
- **WorkflowApi.Infrastructure**: EF Core, repositorios, generación JWT, implementación de `IAuthService`.
- **WorkflowApi.API**: Controlador `AuthController` con método `POST /api/auth/login`.

## Login

El login se valida contra la tabla **Units**. El campo evaluado es **NumberUnit**:

- Si existe una unidad con ese número y está activa (`IsActive = true`), se devuelve un JWT.
- Si no existe o está inactiva, se responde `401 Unauthorized`.

### Ejemplo de petición

```http
POST /api/auth/login
Content-Type: application/json

{ "numberUnit": "UNIT-001" }
```

### Ejemplo de respuesta (200)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "numberUnit": "UNIT-001",
  "expiresAtUtc": "2025-02-27T12:00:00Z"
}
```

## Configuración

- **ConnectionStrings:DefaultConnection**: cadena de conexión a MySQL (base `WorkflowDb`).
- **Jwt:Key**, **Jwt:Issuer**, **Jwt:Audience**, **Jwt:ExpiryMinutes**: configuración del JWT.
- **Azure:Storage:ConnectionString** (opcional): cadena de conexión de la cuenta de Azure Storage. Si está configurada, se usa **Azure Blob Storage** (opción nativa de Microsoft) para las imágenes.
- **Azure:Storage:ContainerName** (opcional): nombre del contenedor (por defecto `evidences`).
- **Gcp:Storage:BucketName**: nombre del bucket de Google Cloud Storage (se usa solo si no hay Azure).
- **Gcp:Storage:HmacAccessKeyId** y **Gcp:Storage:HmacSecretKey** (opcional): claves HMAC para la API XML (compatible S3). Si están configuradas, se usan en lugar de la cuenta de servicio.
- **Gcp:Storage:CredentialsPath** (opcional): ruta al archivo JSON de la cuenta de servicio. Se usa solo si no hay claves HMAC.

La API elige el almacenamiento así: si existe **Azure:Storage:ConnectionString**, se usa Azure Blob Storage; en caso contrario, se usa GCP (HMAC o cuenta de servicio).

### Subida de imágenes: Azure Blob Storage (Microsoft)

1. Crea una cuenta de almacenamiento en [Azure Portal](https://portal.azure.com) (Storage account).
2. Obtén la **cadena de conexión** en Claves de acceso (o usa variables de entorno en producción).
3. Configura `Azure:Storage:ConnectionString` y opcionalmente `Azure:Storage:ContainerName` (por defecto `evidences`). El contenedor se crea automáticamente con acceso de lectura pública para los blobs.

### Subida de imágenes a GCP Storage

Puedes usar **HMAC** (recomendado para evitar dependencia de facturación/cuenta de servicio) o **cuenta de servicio**:

- **Con HMAC**: en la consola de GCP, crea una clave HMAC para una cuenta de servicio con acceso al bucket (Cloud Storage → Configuración de interoperabilidad). Configura `Gcp:Storage:HmacAccessKeyId` y `Gcp:Storage:HmacSecretKey` en appsettings. La API usará la API XML (S3-compatible) contra `https://storage.googleapis.com`.
- **Con cuenta de servicio**: crear un bucket y una cuenta de servicio en GCP, descargar el JSON y:
   - Definir `GOOGLE_APPLICATION_CREDENTIALS` apuntando al archivo, o
   - Configurar `Gcp:Storage:CredentialsPath` con la ruta al JSON.
3. **Permisos**: la cuenta de servicio debe tener rol **Storage Object Creator** (o **Storage Admin**) sobre el bucket.
4. **URLs públicas**: la API devuelve URLs del tipo `https://storage.googleapis.com/{bucket}/evidences/{id}.jpg`. Para que los clientes puedan ver la imagen, el bucket (o la carpeta `evidences`) debe tener permisos de lectura pública, o deberás servir las imágenes con URLs firmadas (no implementado en esta versión).

## Docker (API + MySQL)

Despliegue en un solo comando con Docker Compose (contenedor de la API + contenedor MySQL):

```bash
cd WorkflowApi
docker compose up -d --build
```

- **API**: http://localhost:8080 (Swagger: http://localhost:8080/swagger)
- **MySQL**: puerto 3306 (usuario `root`, base `WorkflowDb`)

Variables opcionales (archivo `.env` o entorno):

| Variable | Default | Descripción |
|----------|---------|-------------|
| `MYSQL_ROOT_PASSWORD` | WorkflowSecret123 | Contraseña root de MySQL |
| `MYSQL_DATABASE` | WorkflowDb | Nombre de la base |
| `MYSQL_PORT` | 3306 | Puerto expuesto de MySQL |
| `API_PORT` | 8080 | Puerto expuesto de la API |

La API aplica las migraciones al arrancar y espera a que MySQL esté listo (healthcheck).

## Ejecutar (local sin Docker)

```bash
cd WorkflowApi
dotnet run --project src/WorkflowApi.API
```

Swagger: https://localhost:5001/swagger (o http://localhost:5000/swagger).

## Migraciones

La migración inicial crea la tabla `Units` con: `Id`, `NumberUnit`, `Description`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc`. Se aplica al arrancar la API.

Para crear datos de prueba, inserta en `Units` una fila con `NumberUnit` (por ejemplo `UNIT-001`) y `IsActive = 1` y usa ese valor en el login.
