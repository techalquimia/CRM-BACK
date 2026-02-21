# GCP Cloud Run - Setup from GitHub

## 1. GCP Project Setup

1. Create or select a project in [Google Cloud Console](https://console.cloud.google.com/).
2. Enable APIs:
   - **Cloud Run API**
   - **Artifact Registry API**
   - **Cloud Build API**
   - **Secret Manager API** (optional, for secrets)
   - **Cloud Storage API**
   - **Cloud Vision API**

3. Create a **Service Account** for GitHub Actions:
   - IAM → Service Accounts → Create
   - Name: `github-actions-deploy`
   - Grant roles:
     - **Cloud Run Admin**
     - **Service Account User**
     - **Storage Admin** (or Object Creator for your bucket)
     - **Cloud Vision API User**
   - Create key (JSON) → Download

4. Create **Artifact Registry** repository (if not auto-created):
   - Artifact Registry → Create repository
   - Name: `cloud-run-source-deploy`
   - Format: Docker
   - Region: same as Cloud Run (e.g. `us-central1`)

## 2. Database (Cloud SQL)

Create a Cloud SQL instance (MySQL/MariaDB compatible) and get the connection string:
```
Server=IP_OR_SOCKET;Database=RouteEvidence;User=USER;Password=PASSWORD;
```

Or use the [Cloud SQL Auth Proxy](https://cloud.google.com/sql/docs/mysql/connect-auth-proxy) connection format.

## 3. GitHub Secrets

In your repo: **Settings → Secrets and variables → Actions** → New repository secret.

| Secret | Description |
|--------|-------------|
| `GCP_SA_KEY` | Full JSON content of the service account key file |
| `GCP_PROJECT_ID` | Your GCP project ID |
| `GCP_REGION` | (Optional) Region, default `us-central1` |
| `GCP_STORAGE_BUCKET` | GCS bucket name for evidence images |
| `DB_CONNECTION` | MariaDB/MySQL connection string |
| `JWT_KEY` | JWT signing key (min 32 characters) |

## 4. Deploy

- **Automatic**: Push to `main` or `master`.
- **Manual**: Actions → Deploy to GCP Cloud Run → Run workflow.

## 5. GCP Credentials in Cloud Run

For GCP Storage and Vision, Cloud Run uses the **default service account** of the project. Ensure it has:
- `roles/storage.objectAdmin` (or appropriate Storage role)
- `roles/vision.user`

Or configure Workload Identity and attach a custom service account to the Cloud Run service.

## 6. Post-Deploy

- Run migrations against your Cloud SQL database.
- Run the seed script: `scripts/seed-data.sql`.
