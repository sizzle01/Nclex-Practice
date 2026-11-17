#!/usr/bin/env bash
# Deploys the informal test environment to one VM over SSH (docs/operations.md section 9).
# Run from your Mac, in the repository root:
#
#   deploy/vm/deploy.sh bootstrap            first time only: copy files, run setup.sh on the VM
#                                            (asks for the Brevo SMTP key, or reads NCLEX_SMTP_KEY)
#   deploy/vm/deploy.sh deploy [--seed]      build images here, copy them over, migrate, restart
#                                            (--seed also imports the sample and the RN question bank)
#   deploy/vm/deploy.sh restart              apply changed settings (.env, Caddyfile) without new images
#   deploy/vm/deploy.sh make-admin <email>   give an existing account the admin role
#   deploy/vm/deploy.sh logs [service]       follow the logs (api, worker, web, caddy, ...)
#
# Images are built on the Mac because the free VM has one CPU core; both are ARM (arm64), so the
# images run there as built. They travel over SSH (docker save | docker load), so no registry or
# GitHub push is needed.
#
# Override the target with VM=user@host and SSH_KEY=path.
set -euo pipefail

VM="${VM:-opc@129.146.180.15}"
SSH_KEY="${SSH_KEY:-$HOME/.ssh/oracle_nclex}"
APP_DIR="nclex" # ~/nclex on the VM
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

ssh_vm() { ssh -i "$SSH_KEY" -o ServerAliveInterval=30 "$VM" "$@"; }
# Every remote compose command runs against the VM's compose file and .env.
compose() { ssh_vm "cd ~/$APP_DIR && docker compose -f compose.vm.yml --env-file .env $*"; }

# Copies the compose file, scripts and sample content. Never touches .env or backups on the VM.
sync_files() {
  echo "==> Copying deploy files and content to $VM:~/$APP_DIR"
  ssh_vm "mkdir -p ~/$APP_DIR"
  # review/ holds the generated review packet; it isn't content and isn't needed on the VM.
  COPYFILE_DISABLE=1 tar -C "$REPO_ROOT" --no-mac-metadata --exclude 'nclex-be/content/rn-bank/review' -czf - \
      -s ",^deploy/vm/,," -s ",^nclex-be/content,content," \
      deploy/vm/compose.vm.yml deploy/vm/Caddyfile deploy/vm/.env.example \
      deploy/vm/setup.sh deploy/vm/backup.sh nclex-be/content \
    | ssh_vm "tar -C ~/$APP_DIR -xzf -"
}

build_and_ship_images() {
  echo "==> Building arm64 images"
  docker build --platform linux/arm64 --target api -t nclex-api:vm "$REPO_ROOT/nclex-be"
  docker build --platform linux/arm64 --target worker -t nclex-worker:vm "$REPO_ROOT/nclex-be"
  docker build --platform linux/arm64 --target migrations -t nclex-migrations:vm "$REPO_ROOT/nclex-be"
  docker build --platform linux/arm64 -t nclex-web:vm "$REPO_ROOT/nclex-fe"

  echo "==> Copying images to the VM (one archive, so shared layers travel once)"
  docker save nclex-api:vm nclex-worker:vm nclex-migrations:vm nclex-web:vm \
    | gzip -1 | ssh_vm "gunzip | docker load"
}

deploy() {
  local seed=false
  [ "${1:-}" = "--seed" ] && seed=true

  sync_files
  build_and_ship_images

  echo "==> Starting Postgres and Redis"
  compose up -d --wait postgres redis

  # Migrations run before new code starts, and a failure stops the deploy with the old version
  # still running (operations.md section 4: expand, migrate, contract).
  echo "==> Running migrations"
  compose run --rm migrate

  if [ "$seed" = true ]; then
    # Only on request: the importer updates questions it already stored, which would undo
    # edits made in the admin screens.
    import_content
  fi

  echo "==> Starting the app"
  # The images keep the same :vm tags on every deploy, and on 2026-11-17 compose left the old
  # containers running after new images were loaded. Every deploy ships new images, so always
  # recreate the app containers; caddy keeps its own image and only needs to be up.
  compose up -d --wait --remove-orphans --force-recreate api worker web
  compose up -d --wait caddy
  ssh_vm "docker image prune -f >/dev/null"

  echo "==> Deployed. Open https://$(site_host)"
}

# Imports the sample file first (it creates the original subtopics), then every RN bank file in
# name order, as docs/features/RN-question-bank.md describes. One SSH session runs the whole loop,
# and the first failing file stops it; each file is its own transaction, so earlier ones stay.
import_content() {
  echo "==> Importing the sample questions and the RN question bank"
  ssh_vm "bash -s $APP_DIR" <<'REMOTE'
set -euo pipefail
cd ~/"$1"
files=(/content/sample-questions.rn.json)
for f in content/rn-bank/rn-bank-*.rn.json; do files+=("/$f"); done
for f in "${files[@]}"; do
  out=$(docker compose -f compose.vm.yml --env-file .env run --rm -T jobs import-questions "$f" 2>&1) \
    || { echo "$out"; echo "Import failed: $f" >&2; exit 1; }
  echo "   ${f##*/}: $(echo "$out" | grep -m1 'question(s)')"
done
REMOTE
}

site_host() { ssh_vm "grep '^SITE_HOST=' ~/$APP_DIR/.env | cut -d= -f2"; }

case "${1:-}" in
  bootstrap)
    sync_files
    if [ -n "${NCLEX_SMTP_KEY:-}" ]; then
      # Non-interactive: send the key on stdin so it never appears in the VM's process list.
      printf %s "$NCLEX_SMTP_KEY" | ssh_vm "NCLEX_SMTP_KEY=\$(cat) bash ~/$APP_DIR/setup.sh"
    else
      # -t: setup.sh asks for the SMTP key, which needs a terminal.
      ssh -t -i "$SSH_KEY" "$VM" "bash ~/$APP_DIR/setup.sh"
    fi
    ;;
  deploy)
    deploy "${2:-}"
    ;;
  restart)
    # Recreates only the containers whose settings changed; images stay as they are.
    sync_files
    compose up -d --wait --remove-orphans api worker web caddy
    # A changed Caddyfile is a bind mount, so compose sees no change: reload it explicitly.
    compose exec -T caddy caddy reload --config /etc/caddy/Caddyfile
    ;;
  make-admin)
    [ -n "${2:-}" ] || { echo "Usage: $0 make-admin <email>" >&2; exit 2; }
    compose run --rm jobs set-role "$2" admin
    ;;
  logs)
    compose logs -f --tail 200 "${2:-}"
    ;;
  *)
    sed -n '2,17p' "$0"
    exit 2
    ;;
esac
