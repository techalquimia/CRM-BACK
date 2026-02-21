#!/bin/bash
# Ejecuta la aplicación con credenciales de GCP (Vision + Storage)
# Edita la ruta si tu archivo de credenciales está en otra ubicación

export GOOGLE_APPLICATION_CREDENTIALS="${GCP_CREDENTIALS_PATH:-/Users/rasecmc/Downloads/t-bounty-482716-d4-5f95f2d3e6ad.json}"
export GCP_ENABLED=true
export GCP_PROJECT_ID="${GCP_PROJECT_ID:-t-bounty-482716}"

./gradlew bootRun
