#!/usr/bin/env bash
# Nightly dump of the test VM's database (cron line added by setup.sh). Keeps the last 7 days in
# ~/nclex/backups. Restore steps: docs/operations.md section 9.
# The dumps sit on the same disk as the database, which is enough for a test environment, not
# for real candidates' data.
set -euo pipefail

APP_DIR="$HOME/nclex"
BACKUP_DIR="$APP_DIR/backups"
mkdir -p "$BACKUP_DIR"

file="$BACKUP_DIR/nclex-$(date -u +%Y%m%d-%H%M%S).dump"
# Custom format (-Fc): compressed, and pg_restore can restore single tables from it.
docker compose -f "$APP_DIR/compose.vm.yml" --env-file "$APP_DIR/.env" \
  exec -T postgres pg_dump -U nclex -d nclex -Fc > "$file"
echo "$(date -u +%FT%TZ) wrote $file ($(du -h "$file" | cut -f1))"

find "$BACKUP_DIR" -name 'nclex-*.dump' -mtime +7 -delete
