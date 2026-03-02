#!/usr/bin/env sh
set -eu

if [ -z "${POSTGRES_USER:-}" ] || [ -z "${POSTGRES_DB:-}" ]; then
  echo "POSTGRES_USER and POSTGRES_DB must be set (typically via .env)."
  exit 1
fi

BACKUP_DIR="${BACKUP_DIR:-./backups}"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
FILE="$BACKUP_DIR/gezlnomics-$TIMESTAMP.sql.gz"

mkdir -p "$BACKUP_DIR"

echo "Creating backup: $FILE"
docker compose exec -T postgres \
  pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --no-owner --no-privileges \
  | gzip > "$FILE"

echo "Backup complete: $FILE"
