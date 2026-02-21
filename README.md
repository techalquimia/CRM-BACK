# Logistics API - Evidencias de Viaje

API/Microservicio en Java + Spring Boot para registro de usuarios y envío de evidencias de viaje. Las evidencias pueden ser **tickets** (con extracción OCR de peso total, tara y peso neto) o **otros tipos** tipificados.

## Tecnologías

- **Java 17** + **Spring Boot 3.2**
- **MariaDB** - Persistencia de datos
- **GCP Cloud Storage** - Almacenamiento de imágenes (URL referenciada)
- **Google Cloud Vision** - OCR para extracción de datos de tickets

## Estructura del Proyecto

- **Identificador:** `com.techalquimia.logistics`
- **IDE sugerido:** IntelliJ IDEA

## Modelo de datos

### Evidencias

| Campo | Tipo | Descripción |
|-------|------|-------------|
| id | UUID | Identificador único |
| imageUrl | String | URL de la imagen en GCP (gs://bucket/path) |
| dateTime | Instant | Fecha y hora del registro |
| latitude / longitude | Double | Coordenadas |
| evidenceType | String | TIPO: `TICKET`, `OTHER`, etc. |
| isSynced | Boolean | Indicador de sincronización |
| createdAt | Instant | Fecha de creación |
| ticketWeight | TicketWeight | Relación 1:1 para tickets |

### Ticket weights (tabla relacionada)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| id | UUID | Identificador |
| evidence_id | UUID | FK a evidences |
| peso_total | Double | Peso total |
| tara | Double | Tara |
| peso_neto | Double | Peso neto (peso_total = tara + peso_neto) |

## Documentación Swagger

- **Swagger UI:** http://localhost:8080/swagger-ui.html
- **OpenAPI JSON:** http://localhost:8080/v3/api-docs

Para probar endpoints protegidos en Swagger UI: 1) Llamar a `POST /api/v1/auth/login` con email y contraseña, 2) Copiar el token de la respuesta, 3) Usar **Authorize** → Bearer → pegar el token.

## Endpoints

### Autenticación

- `POST /api/v1/auth/login` - Login con email y contraseña, devuelve token JWT (público)

### Usuario

- `POST /api/v1/users/register` - Registro (público)

### Catálogo de tipos

- `GET /api/v1/evidence-types` - Listar tipos de evidencia (público)

### Evidencias (requieren token JWT en header: `Authorization: Bearer <token>`)

- `POST /api/v1/evidences` - Crear evidencia (multipart/form-data)
- `GET /api/v1/evidences` - Listar evidencias (opcional: ?evidenceType=TICKET)
- `GET /api/v1/evidences/{id}` - Obtener por ID
- `PATCH /api/v1/evidences/{id}/sync` - Marcar como sincronizada

### Crear evidencia (form-data)

- `image` - Archivo de imagen
- `dateTime` - ISO 8601 (ej: 2025-02-18T14:30:00Z)
- `latitude`, `longitude` - Opcionales
- `evidenceType` - Código del catálogo: `WEIGHT_TICKET`, `TRUCK_PHOTO`, `SEAL_PHOTO`, `DELIVERY_PROOF`, `INCIDENT_REPORT`

### Tipos de evidencia (catálogo)

| Código | Descripción | OCR |
|--------|-------------|-----|
| WEIGHT_TICKET | Ticket de báscula | Sí (peso total, tara, peso neto) |
| TRUCK_PHOTO | Foto del camión | No |
| SEAL_PHOTO | Foto del sello/precinto | No |
| DELIVERY_PROOF | Prueba de entrega firmada | No |
| INCIDENT_REPORT | Reporte de incidente | No |

## Configuración

### Variables de entorno

| Variable | Descripción |
|----------|-------------|
| DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD | Conexión MariaDB |
| GCP_BUCKET_NAME | Nombre del bucket en GCP |
| GCP_PROJECT_ID | ID del proyecto GCP |
| GOOGLE_APPLICATION_CREDENTIALS | Ruta al JSON de credenciales de servicio |
| JWT_SECRET | Clave secreta para firmar JWT (mín. 32 caracteres) |
| JWT_EXPIRATION_MS | Expiración del token en ms (default: 24h) |

### Ejecución local

1. **IntelliJ IDEA**: Abrir el proyecto como proyecto Gradle; IDEA descargará el wrapper automáticamente.
2. MariaDB en ejecución (o `docker-compose up -d mariadb`)
3. Credenciales GCP configuradas:
   - Opción A: `./run-with-gcp.sh` (usa por defecto `~/Downloads/t-bounty-482716-d4-5f95f2d3e6ad.json`)
   - Opción B: `export GOOGLE_APPLICATION_CREDENTIALS="/ruta/a/tu-credenciales.json"` y luego `./gradlew bootRun`
4. `./gradlew bootRun` (o ejecutar `LogisticsApiApplication` desde IDEA)

### Docker

```bash
docker-compose up -d
```

La API queda en `http://localhost:8080`.
