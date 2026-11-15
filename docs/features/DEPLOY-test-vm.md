# Deploy: informal test environment on one VM

Branch: `feature/deploy-test-vm`, based on `main` (efb65cb, F00–F17).

## 1. Summary

Testers can use the whole app at **https://129-146-180-15.sslip.io**: a free Oracle Cloud VM
running the same Docker images as the full local stack. Caddy gives it a real HTTPS certificate
with no domain to buy (sslip.io turns the IP into a host name), and verification, reset and
result emails go out for real through Brevo. A script on your Mac builds the images, copies them
to the VM, migrates the database and restarts the app; another backs the database up every night.
To make Brevo work, the worker's SMTP settings gained an optional username and password, the only
application code change.

## 2. How to try it

Prerequisites: Docker Desktop on an Apple Silicon Mac, and the SSH key `~/.ssh/oracle_nclex`.

```bash
cd <repo root>
deploy/vm/deploy.sh bootstrap        # done once already: Docker, firewall, secrets, backups
deploy/vm/deploy.sh deploy --seed    # first deploy (imports the sample questions)
deploy/vm/deploy.sh deploy           # every later deploy
deploy/vm/deploy.sh make-admin you@example.com
deploy/vm/deploy.sh logs worker
```

Then open https://129-146-180-15.sslip.io, sign up, and find the verification email (check spam:
see section 8). After `make-admin`, sign out and in again to get the admin screens.

In the Oracle console the subnet's security list must allow TCP 80 and 443 from `0.0.0.0/0`.

## 3. Reading order

1. `docs/operations.md` section 9: what the test VM is and how it is run.
2. `deploy/vm/compose.vm.yml`: the stack; compare it with the `app` profile in `docker-compose.yml`.
3. `deploy/vm/Caddyfile`: HTTPS and the single proxy rule.
4. `deploy/vm/.env.example`: every setting; the real `.env` lives only on the VM.
5. `deploy/vm/setup.sh`: one-time VM preparation, written to be safe to re-run.
6. `deploy/vm/deploy.sh`: build, ship, migrate, restart.
7. `deploy/vm/backup.sh`: nightly `pg_dump`.
8. `nclex-be/src/Nclex.Infrastructure/Email/EmailOptions.cs` and `SmtpEmailSender.cs`: the SMTP login.
9. `nclex-be/tests/Nclex.Api.IntegrationTests/Email/SmtpSettingsTests.cs`: its tests.
10. `nclex-fe/src/features/admin/components/AdminGate.tsx` and `src/app/admin/layout.tsx`: the
    build fix (section 6).

## 4. Request flow

A tester signs up:

1. The browser posts to `https://…sslip.io/api/v1/auth/signup`. **Caddy** ends TLS and forwards to
   `web:3000`, adding `X-Forwarded-For`.
2. The **Next.js server** matches its `/api` rewrite (compiled in with `API_URL=http://api:8080`)
   and forwards to the **API**, which trusts forwarded headers from Docker's private ranges, so rate
   limits see the tester's real IP.
3. The API creates the user and a `verify_email` row in `email_outbox` in one transaction
   (architecture.md ADR-7), and returns 201.
4. The **worker**'s outbox dispatcher picks the row up, renders the template, and `SmtpEmailSender`
   connects to `smtp-relay.brevo.com:587`, starts TLS and logs in with `Email:Smtp:Username` and
   `Password`. Brevo delivers it; the row is marked `sent`.
5. The link in the email points at `Email:FrontendBaseUrl` (`https://…sslip.io/verify-email?token=…`).

## 5. Database changes

None. No migration; the VM's database is created by the existing migration bundle.

## 6. Key decisions and deviations

- **An extra, informal `test` environment.** architecture.md section 8 plans AWS for staging and
  production; that design is unchanged. This VM is documented separately (operations.md section 9).
- **Images are built on the Mac, not on the VM, and copied over SSH.** The VM has one core; both
  machines are arm64. No registry and no GitHub push are needed (origin still stops at F16).
- **Brevo instead of Mailpit**, using a sender address on gmail.com (see section 8).
- **SMTP login is optional.** Without a username `SmtpClient` stays anonymous, so Mailpit and the
  local stack behave as before. A username without a password stops the worker at start-up rather
  than failing every send. This removes the "SMTP has no username or password" limitation.
- **Left out of the VM:** MinIO (unused), Mailpit (real email instead) and the Aspire dashboard
  (memory, and it would need its own login). Logs are read with `deploy.sh logs`.
- **The frontend production build was broken on `main`, so this branch fixes it.** `next build`
  stopped at the admin pages: they passed role-check functions from server components into the
  client `AdminGate`. Pages now pass a role-group name (`allow="admin"`, `"author"`, `"reviewer"`),
  and `AdminGate` looks up the check. The admin layout also gained a `<Suspense>` boundary, because
  with `cacheComponents` on, dynamic routes like `/admin/banks/[id]` may only read the URL inside
  one. `npm run dev` and the unit tests never prerender pages, which is why nothing caught it.
- **Seeding is opt-in** (`--seed`), because the importer overwrites admin edits to sample questions.

## 7. Tests

- `SmtpSettingsTests` (3 tests): no username means anonymous; a username and key become the
  credentials; a username without a key is rejected at start-up.
  `cd nclex-be && dotnet test --filter "FullyQualifiedName~SmtpSettingsTests"`
- Full suites: `dotnet test` in `nclex-be`; `npm run lint`, `npm run typecheck`, `npm test` in `nclex-fe`.
- `npm run build` in `nclex-fe` now passes (it failed on `main`).
- The deploy files were checked with `docker compose config` and `caddy validate`, and by the
  real deployment to the VM.

## 8. Follow-ups and known limitations

- **Email from `@gmail.com` through Brevo fails Gmail's sender checks (DMARC)**, so messages may
  land in spam. Verifying a domain you own in Brevo and sending from it fixes this.
- **The Brevo SMTP key was pasted into a chat transcript.** Generate a new key in Brevo, then
  update `SMTP_PASSWORD` in `~/nclex/.env` on the VM and run `deploy.sh deploy`.
- Backups sit on the same disk as the database. Copy them off the VM (for example to Oracle Object
  Storage) if the data starts to matter.
- **CI should run `npm run build`**, so a broken production build is caught before a deploy.
- **Flaky integration tests:** two full runs of `dotnet test` each had failures (2 in the first,
  1 in the second), in different tests each time, all in code this branch doesn't touch. The second
  run's failure was `SnapshotJobTests` with Serilog's "The logger is already frozen", which happens
  when several test hosts start in one process. Worth fixing on its own branch.
- No rollback by image tag: redeploy an older commit instead. No uptime alerts.
- One core: a deploy restarts the app (about a minute of downtime), and heavy use will be slow.
- `deploy.sh bootstrap` with `NCLEX_SMTP_KEY` finished its work on the VM, but the SSH session did
  not close by itself the first time; it had to be stopped. Harmless, not yet explained.

## 9. Try it yourself

1. Add `Notifications__DraftReminderAfterDays: "1"` to the worker in `compose.vm.yml`, deploy, and
   leave a draft attempt for a day to receive the reminder.
2. Add a `mail.{$SITE_HOST}` site to the `Caddyfile` that serves the Mailpit UI behind `basic_auth`, and
   add Mailpit to `compose.vm.yml`, so testers can use made-up email addresses.
3. Change `backup.sh` to keep 14 days, and run it by hand on the VM (`~/nclex/backup.sh`) to check
   the new file appears.
