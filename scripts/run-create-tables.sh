#!/bin/bash
# Create RouteEvidence database and tables.
# Usage: ./scripts/run-create-tables.sh [local]
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

if [ "$1" = "local" ]; then
  echo "Creating tables in local MySQL..."
  mysql -u root -p < "$SCRIPT_DIR/create-tables.sql"
else
  echo "Creating tables in Docker MariaDB..."
  CONTAINER=$(cd "$PROJECT_ROOT" && docker compose ps -q mariadb 2>/dev/null || true)
  if [ -z "$CONTAINER" ]; then
    echo "MariaDB container not running. Start with: docker compose up -d mariadb"
    exit 1
  fi
  docker exec -i "$CONTAINER" mysql -uroot -proouteevidence2025 < "$SCRIPT_DIR/create-tables.sql"
fi
echo "Tables created."
