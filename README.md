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

## Endpoints

### Usuario

- `POST /api/v1/users/register` - Registro (público)

### Evidencias (requieren autenticación Basic)

- `POST /api/v1/evidences` - Crear evidencia (multipart/form-data)
- `GET /api/v1/evidences` - Listar evidencias (opcional: ?evidenceType=TICKET)
- `GET /api/v1/evidences/{id}` - Obtener por ID
- `PATCH /api/v1/evidences/{id}/sync` - Marcar como sincronizada

### Crear evidencia (form-data)

- `image` - Archivo de imagen
- `dateTime` - ISO 8601 (ej: 2025-02-18T14:30:00Z)
- `latitude`, `longitude` - Opcionales
- `evidenceType` - `TICKET` u otros (ej: `OTHER`, `RECEIPT`)

## Configuración

### Variables de entorno

| Variable | Descripción |
|----------|-------------|
| DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD | Conexión MariaDB |
| GCP_BUCKET_NAME | Nombre del bucket en GCP |
| GCP_PROJECT_ID | ID del proyecto GCP |
| GOOGLE_APPLICATION_CREDENTIALS | Ruta al JSON de credenciales de servicio |

### Ejecución local

1. **IntelliJ IDEA**: Abrir el proyecto como proyecto Gradle; IDEA descargará el wrapper automáticamente.
2. MariaDB en ejecución (o `docker-compose up -d mariadb`)
3. Credenciales GCP configuradas (`GOOGLE_APPLICATION_CREDENTIALS` apuntando al JSON de cuenta de servicio)
4. `./gradlew bootRun` (o ejecutar `LogisticsApiApplication` desde IDEA)

### Docker

```bash
docker-compose up -d
```

La API queda en `http://localhost:8080`.
