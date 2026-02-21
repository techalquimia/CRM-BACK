# Route Evidence API

REST API for route evidence management. Captures, stores, and syncs geolocated evidence associated with vehicle units.

## Features

- **JWT authentication** – Bearer token security
- **MariaDB** – Persistence with Entity Framework Core
- **Swagger/OpenAPI** – Interactive documentation
- **Geolocated evidence** – Coordinates, images, types, and sync status

## Requirements

- .NET 8.0 SDK
- MariaDB 10.5+
- dotnet-ef (for migrations)

## Configuration

### 1. Connection string

In `RouteEvidence/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RouteEvidence;User=root;Password=your_password"
  },
  "Jwt": {
    "Key": "YourSecretKeyMin32CharactersForHS256!",
    "Issuer": "RouteEvidenceAPI",
    "Audience": "RouteEvidenceApp",
    "ExpirationMinutes": 60
  }
}
```

> In production, use environment variables or a vault for the JWT key.

**GCP Storage** (for image uploads):

```json
"Gcp": {
  "StorageBucket": "route-evidence-images"
}
```

Set `GOOGLE_APPLICATION_CREDENTIALS` to the path of your GCP service account JSON key file.

**Google Cloud Vision** (for ticket OCR): The service account must have the `roles/vision.user` role for Document Text Detection. When creating evidence with `EvidenceType=ticket` or a catalog type with `Ocr=true`, the image is sent to Vision API to extract weight values (peso total, tara, peso neto).

### 2. Database

Install EF Core tools (if not installed):

```bash
dotnet tool install --global dotnet-ef
```

Create and apply migrations:

```bash
cd RouteEvidence
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run

```bash
cd RouteEvidence
dotnet run
```

API available at `https://localhost:5001` (or configured port). Swagger: `https://localhost:5001/swagger`.

### 4. Docker

Build and run with Docker Compose (API + MariaDB):

```bash
docker compose up -d --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- MariaDB: `localhost:3306` (user: root, password: routeevidence2025)

**Apply migrations** after first run (inside the API container or locally with DB host `localhost`):

```bash
cd RouteEvidence
dotnet ef database update
```

**Create tables manually** (alternative to EF migrations):

```bash
./scripts/run-create-tables.sh
# Or with local MySQL: ./scripts/run-create-tables.sh local
```

**Seed data** (EvidenceCatalog types and sample Units):

```bash
./scripts/run-seed.sh
# Or with local MySQL: ./scripts/run-seed.sh local
```

## API Structure

### Authentication

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/login` | Login (demo: `admin` / `admin`) |
| GET | `/api/auth/validate` | Validate token (requires JWT) |

### Units

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/unit` | List units (`?active=true`) |
| GET | `/api/unit/{id}` | Get unit by ID |
| POST | `/api/unit` | Create unit |
| PUT | `/api/unit/{id}` | Update unit |
| DELETE | `/api/unit/{id}` | Delete unit |

### Evidence Catalog

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/EvidenceCatalog` | List types (`?active=true`) |
| GET | `/api/EvidenceCatalog/{id}` | Get by ID |
| POST | `/api/EvidenceCatalog` | Create evidence type |
| PUT | `/api/EvidenceCatalog/{id}` | Update evidence type |
| DELETE | `/api/EvidenceCatalog/{id}` | Delete evidence type |

### Evidence

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/evidence` | List evidence (`?unitId=`, `?isSynced=`) |
| GET | `/api/evidence/{id}` | Get evidence by ID |
| POST | `/api/evidence` | Create evidence (multipart/form-data with Image file) |
| POST | `/api/evidence/with-reference` | Create evidence with existing GCS reference |
| PUT | `/api/evidence/{id}` | Update evidence |
| PATCH | `/api/evidence/{id}/sync` | Mark as synced |
| DELETE | `/api/evidence/{id}` | Delete evidence |

## Models

### Evidence

- `Id` (UUID)
- `GcsBucket` – GCP Storage bucket name
- `GcsObjectKey` – Object path within bucket (full ref: `gs://{bucket}/{key}`)
- `DateTime` (capture timestamp)
- `Latitude`, `Longitude` (coordinates)
- `EvidenceType`
- `IsSynced` (boolean)
- `CreatedAt`
- `UnitId` (FK to Units)
- **Ticket fields** (optional): `TotalWeight`, `Tara`, `NetWeight` (TotalWeight = Tara + NetWeight)

### Unit

- `Id`, `Plate`, `EconomicNumber`, `Brand`, `Model`, `Year`, `Active`

### Evidence Catalog

- `Id`, `Type`, `Description`, `Active`, `Ocr`

## Project Structure

```
RouteEvidence/
├── Controllers/     # AuthController, EvidenceController, UnitController, EvidenceCatalogController
├── Data/            # RouteEvidenceDbContext, Migrations
├── DTOs/            # Request/response DTOs
├── Models/          # Evidence, Unit, EvidenceCatalog
└── Program.cs
```

## Code Quality

- **CodeQL** – GitHub Actions workflow for security analysis
- **EditorConfig** – Consistent code style

## Deploy to GCP Cloud Run

Push to `main`/`master` triggers deployment via GitHub Actions. See [docs/GCP-DEPLOY-SETUP.md](docs/GCP-DEPLOY-SETUP.md) for setup.

## License

MIT
