#!/bin/bash
# Run seed data script. Usage:
#   ./scripts/run-seed.sh              # with Docker Compose
#   ./scripts/run-seed.sh local        # local MySQL

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

if [ "$1" = "local" ]; then
  echo "Running seed against local MySQL..."
  mysql -u root -p RouteEvidence < "$SCRIPT_DIR/seed-data.sql"
else
  echo "Running seed against Docker MariaDB..."
  CONTAINER=$(cd "$PROJECT_ROOT" && docker compose ps -q mariadb 2>/dev/null || true)
  if [ -z "$CONTAINER" ]; then
    echo "MariaDB container not running. Start with: docker compose up -d"
    exit 1
  fi
  docker exec -i "$CONTAINER" mysql -uroot -proouteevidence2025 RouteEvidence < "$SCRIPT_DIR/seed-data.sql"
fi
echo "Seed completed."
