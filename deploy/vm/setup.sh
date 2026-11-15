#!/usr/bin/env bash
# One-time preparation of a fresh Oracle Linux 9 VM for the test environment
# (docs/operations.md section 9). deploy.sh copies this file to ~/nclex and runs it with
# `deploy.sh bootstrap`; running it again is safe: every step checks before it changes anything.
#
# It installs Docker, opens ports 80 and 443, adds swap, writes ~/nclex/.env with fresh secrets
# (asking for the Brevo SMTP key), and schedules the nightly database backup.
set -euo pipefail

APP_DIR="$HOME/nclex"
cd "$APP_DIR"

echo "==> Docker"
if ! command -v docker >/dev/null; then
  sudo dnf -y install dnf-plugins-core
  # Docker publishes no Oracle Linux repo; the CentOS/RHEL 9 one works on it.
  sudo dnf config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
  # --allowerasing replaces the podman/runc packages Oracle Linux ships with.
  sudo dnf -y install --allowerasing docker-ce docker-ce-cli containerd.io docker-compose-plugin
fi
sudo systemctl enable --now docker
# Lets this user run docker without sudo (takes effect on the next SSH login).
sudo usermod -aG docker "$USER"

echo "==> Firewall (ports 80 and 443)"
# Oracle Linux runs firewalld. The VCN security list in the Oracle console must allow them too.
sudo firewall-cmd --permanent --add-service=http --add-service=https >/dev/null
sudo firewall-cmd --reload >/dev/null

echo "==> Swap"
# 2 GB of headroom for the 1-core, 5 GB VM; the stack itself fits in memory (compose.vm.yml limits).
if ! sudo swapon --show | grep -q /swapfile; then
  sudo fallocate -l 2G /swapfile
  sudo chmod 600 /swapfile
  sudo mkswap /swapfile >/dev/null
  sudo swapon /swapfile
  grep -q '^/swapfile' /etc/fstab || echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab >/dev/null
fi

echo "==> Secrets (.env)"
if [ ! -f .env ]; then
  cp .env.example .env
fi
chmod 600 .env
# Fills an empty KEY= line with a value; never replaces one that is already set, so re-running
# setup.sh does not sign everyone out or lock the app out of its own database.
fill_if_empty() {
  local key="$1" value="$2"
  if grep -q "^${key}=$" .env; then
    sed -i "s|^${key}=$|${key}=${value}|" .env
  fi
}
fill_if_empty POSTGRES_PASSWORD "$(openssl rand -hex 24)"
fill_if_empty JWT_SIGNING_KEY "$(openssl rand -hex 32)"
fill_if_empty MEDIA_SIGNING_KEY "$(openssl rand -hex 32)"
if grep -q '^SMTP_PASSWORD=$' .env; then
  # NCLEX_SMTP_KEY lets a non-interactive run pass the key in; otherwise ask without echoing it.
  smtp_key="${NCLEX_SMTP_KEY:-}"
  if [ -z "$smtp_key" ]; then
    read -rsp "Brevo SMTP key (input hidden): " smtp_key
    echo
  fi
  fill_if_empty SMTP_PASSWORD "$smtp_key"
fi

echo "==> Nightly backup (03:00, keeps 7 days)"
sudo dnf -y install cronie >/dev/null
sudo systemctl enable --now crond
chmod +x "$APP_DIR/backup.sh"
backup_line="0 3 * * * $APP_DIR/backup.sh >> $APP_DIR/backups/backup.log 2>&1"
mkdir -p "$APP_DIR/backups"
( crontab -l 2>/dev/null | grep -v 'nclex/backup.sh' || true; echo "$backup_line" ) | crontab -

echo "Setup finished. Next, on your Mac: deploy/vm/deploy.sh deploy --seed"
