# Operations

How the NCLEX Practice platform is configured, deployed, rolled back, backed up and watched. The
design behind it is in [architecture.md](architecture.md) sections 6 to 9. This page covers how to
run it day to day.

## 1. Environments

| Environment | Where | How it gets code | Data |
|---|---|---|---|
| `local` | Your machine: `docker compose up -d` for Postgres, Redis, MinIO and Mailpit, then `dotnet run` and `npm run dev` (see the [README](../README.md)) | Your working tree | Throwaway |
| `local` (full stack) | `docker compose --profile app up -d --build --wait`: the same images staging runs | Your working tree, built into images | Throwaway; sample content imported on start |
| `staging` | AWS (section 4), its own account or VPC | Every push to `main`, automatically | Test accounts only, never real candidates |
| `production` | AWS, Multi-AZ | The images staging ran, once someone approves the `production` environment in GitHub | Real candidates |

The full local stack listens on:

| Service | Address |
|---|---|
| Frontend | http://localhost:3000 (`APP_WEB_PORT`) |
| API | http://localhost:8080 (`APP_API_PORT`) |
| Mailpit inbox | http://localhost:8025 |
| Traces, metrics and logs (Aspire dashboard) | http://localhost:18888 (`OTEL_DASHBOARD_PORT`) |

To run a second copy next to the default one (for example while another feature's stack is up), give
it its own project name and ports: `docker compose -p nclex-f16 --env-file my-ports.env --profile app up -d`.

## 2. Configuration

Settings use .NET configuration: `appsettings.json` for safe defaults, then environment variables,
where a double underscore separates the levels (`Jwt__SigningKey` sets `Jwt:SigningKey`).
`appsettings.Development.json` only loads when `ASPNETCORE_ENVIRONMENT=Development`, so deployed
environments never see the local keys in it.

### API (`Nclex.Api`)

| Setting | Secret | Example / default | Notes |
|---|---|---|---|
| `ConnectionStrings__Postgres` | yes | `Host=…;Database=nclex;Username=nclex_app;Password=…;Maximum Pool Size=40` | Required. Always set `Maximum Pool Size`: Npgsql's default of 100 per process can exceed Postgres's `max_connections` and fail requests with "too many clients" (seen in the F16 load test). Keep (API tasks × pool) + (worker tasks × pool) + 10 for admin below `max_connections`, or put RDS Proxy in front. |
| `ConnectionStrings__Redis` | yes | `primary.xxx.cache.amazonaws.com:6379,ssl=true,password=…` | Required. If Redis is down the API keeps serving, with `/readyz` reporting `degraded`. |
| `Jwt__SigningKey` | yes | 32 or more random bytes | The API refuses to start without it. Changing it signs everyone out within 15 minutes. |
| `Jwt__Issuer`, `Jwt__Audience` | no | `nclex-api`, `nclex-web` | From `appsettings.json`. |
| `MediaStorage__SigningKey` | yes | 32 or more random bytes | Signs media links (api.md section 10). The API refuses to start without it. |
| `MediaStorage__LocalRoot` | no | `/data/media` | Where media files live. See the media limitation in section 8. |
| `ForwardedHeaders__KnownNetworks__0`, `__1`, … | no | `10.0.0.0/16` | CIDR ranges of the load balancer and the frontend tasks. Only these may set `X-Forwarded-For`. Without them every request appears to come from the proxy, and the per-IP login limit is shared by everyone. |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | no | `http://localhost:4317` | The OpenTelemetry collector. When unset, nothing is exported. `OTEL_EXPORTER_OTLP_HEADERS` and `OTEL_EXPORTER_OTLP_PROTOCOL` work as usual. |
| `ASPNETCORE_ENVIRONMENT` | no | `Production` | `Production` in staging too: HSTS is on and logs are JSON. |

### Worker (`Nclex.Worker`)

| Setting | Secret | Example / default | Notes |
|---|---|---|---|
| `ConnectionStrings__Postgres`, `ConnectionStrings__Redis` | yes | as for the API | |
| `Email__From` | no | `NCLEX Practice <no-reply@example.com>` | Required. |
| `Email__FrontendBaseUrl` | no | `https://app.example.com` | Required. Used to build the links in emails. |
| `Email__Smtp__Host`, `__Port`, `__EnableSsl` | no | `email-smtp.us-east-1.amazonaws.com`, `587`, `true` | See the SMTP credentials limitation in section 8. |
| `Notifications__DraftReminderAfterDays`, `__DraftReminderCron` | no | `3`, `0 * * * *` | |
| `MediaStorage__LocalRoot` | no | `/data/media` | The content importer writes media here. |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | no | as for the API | |

### Frontend (`nclex-fe`)

| Setting | When | Default | Notes |
|---|---|---|---|
| `API_URL` | **Build time** (`docker build --build-arg`) | `http://api:8080` in the image | Next.js compiles the `/api` rewrites into the server, so this can't change at run time. Every environment reaches the API by the same internal name, `api:8080` (ECS Service Connect, or the compose service name), so one image serves all of them. |
| `PORT` | run time | `3000` | |

### Secrets

Secrets live only in AWS Secrets Manager and reach the containers as environment variables through
the ECS task definition's `secrets` block. They are never stored in the repository, in GitHub, or in
image layers. The compose file's keys only work for the local stack.

To rotate a secret, put the new value in Secrets Manager, then redeploy the service (re-run the
workflow, or run `aws ecs update-service --force-new-deployment`). Rotating `Jwt__SigningKey` signs
every user out at their next refresh.

## 3. Images and CI

| Image | Built from | Runs |
|---|---|---|
| `nclex-api` | `nclex-be/Dockerfile --target api` | `Nclex.Api` on `aspnet:10.0`, as a non-root user, on port 8080 |
| `nclex-worker` | `nclex-be/Dockerfile --target worker` | `Nclex.Worker`. The same image runs `import-questions <file>` to load content. |
| `nclex-migrations` | `nclex-be/Dockerfile --target migrations` | The EF Core migration bundle (`efbundle`). It reads `ConnectionStrings__Postgres` or takes `--connection "…"`. |
| `nclex-web` | `nclex-fe/Dockerfile` | The standalone Next.js server on port 3000 |

CI (`.github/workflows/ci.yml`) runs on every pull request and every push to `main`:

1. **Backend:** `dotnet format` check, Release build, all tests (Testcontainers), and the vulnerable-package list.
2. **Frontend:** `npm audit --audit-level=high`, lint, typecheck, unit tests and build.
3. **Dependency review** (pull requests only): fails if a new dependency has a known high-severity vulnerability.
4. **End-to-end:** builds and starts the full stack with `docker compose --profile app`, then runs the Playwright suite against it. On failure it uploads the Playwright report and the container logs.

To run the end-to-end suite locally:

```bash
docker compose --profile app up -d --build --wait
cd nclex-fe && npx playwright install chromium && npm run e2e
```

## 4. Deploying

Reference target: AWS (architecture.md section 8). The workflow is `.github/workflows/deploy.yml`,
and it calls `deploy/aws/deploy.sh`.

```
push to main ──► build and push 4 images to ECR, tagged with the commit (first 12 characters)
             ──► staging:    run the migration bundle as a one-off ECS task ─► roll nclex-api,
                             nclex-worker and nclex-web ─► wait until stable ─► GET /readyz
             ──► production: waits for approval, then the same steps with the same images
```

**What has to exist in AWS first.** This feature provisions none of it. The deploy only updates
what already exists:

- ECR repositories `nclex-api`, `nclex-worker`, `nclex-migrations` and `nclex-web`.
- An ECS cluster with Fargate services `nclex-api` (at least 2 tasks across AZs), `nclex-worker` and
  `nclex-web`, plus a task definition family `nclex-migrations`. Each task definition takes its
  secrets from Secrets Manager. Turn on the deployment circuit breaker with rollback.
- ECS Service Connect, so `nclex-web` reaches the API as `api:8080`.
- An ALB with HTTPS in front of `nclex-web`. Its health check is `/healthz`, and the API target's is `/readyz`.
- RDS PostgreSQL 16 (Multi-AZ, PITR 14 days), ElastiCache Redis, and an OpenTelemetry collector
  (AWS Distro for OpenTelemetry sidecar or service) for `OTEL_EXPORTER_OTLP_ENDPOINT`.
- A deploy IAM role that GitHub can assume through OIDC. It needs ECR push, `ecs:RegisterTaskDefinition`,
  `ecs:UpdateService`, `ecs:RunTask`, `ecs:Describe*` and `iam:PassRole` for the task roles.

**GitHub settings.** Create the environments `staging` and `production`, and add required reviewers
on `production`. Give each environment these variables: `AWS_REGION`, `AWS_DEPLOY_ROLE_ARN`,
`ECR_REGISTRY`, `ECS_CLUSTER`, `ECS_SUBNETS` (comma-separated private subnets), `ECS_SECURITY_GROUP`
and `APP_URL`. Images are pushed once, to the staging registry. If production is in another account,
let it pull from that registry, or copy the images over in the production job.

**Migrations** run before any new code starts, so every migration must work with the code that is
still running (expand → migrate → contract, architecture.md section 7). In practice:

- Add columns as nullable or with a default. Backfill in a later deploy if you need to.
- Never rename or drop a column the running version reads. Stop using it in one release, and drop
  it in the next.
- If the migration task fails, the script stops and nothing is rolled out. The old version keeps
  serving. Read the `nclex-migrations` logs, fix the migration, and push again.

**Content** is loaded with the worker image:
`aws ecs run-task … --task-definition nclex-worker --overrides '{"containerOverrides":[{"name":"nclex-worker","command":["import-questions","/content/file.json"]}]}'`
(the content file must be in the image or on a mounted volume).

## 5. Rolling back

| What went wrong | What to do |
|---|---|
| New tasks fail their health checks | Nothing. The circuit breaker keeps the old tasks and rolls the service back. The workflow fails at "Waiting for the services to become stable". |
| New version is healthy but broken | Re-run the deploy workflow for the last good commit (Actions → Deploy → that run → "Re-run all jobs"). It redeploys the old images. Its migrations are already applied, so the bundle does nothing. |
| Faster, from a terminal | `aws ecs update-service --cluster <cluster> --service nclex-api --task-definition nclex-api:<previous revision>` (and the same for `nclex-worker` and `nclex-web`). |
| A migration did damage | Do not run `efbundle` backwards. Write a new forward migration that repairs it. If data was lost, restore it (section 6). |

The expand/contract rule is what makes image rollbacks safe: the previous version still works
against the newer schema.

## 6. Backups and restore

**What is backed up:**

- **Postgres:** RDS automated backups with point-in-time recovery for 14 days, encrypted with KMS
  (architecture.md section 8). Take a manual snapshot before any risky migration:
  `aws rds create-db-snapshot --db-instance-identifier nclex-prod --db-snapshot-identifier pre-<change>-<date>`.
- **Redis:** not backed up. It only holds data that can be rebuilt (rate-limit windows, stream
  tickets, pub/sub).
- **Media:** today, on the API tasks' storage (see section 8). Once media moves to S3, use bucket
  versioning.
- **Email outbox, jobs:** in Postgres, so covered above.

**Restore runbook (Postgres):**

1. **Decide the point in time.** Choose just before the incident, in UTC. Check the API logs and traces.
2. **Restore to a new instance.** A restore never overwrites the live database:
   ```bash
   aws rds restore-db-instance-to-point-in-time \
     --source-db-instance-identifier nclex-prod \
     --target-db-instance-identifier nclex-prod-restore-<date> \
     --restore-time 2026-10-29T08:00:00Z \
     --db-subnet-group-name <subnet group> --vpc-security-group-ids <db security group> --multi-az
   aws rds wait db-instance-available --db-instance-identifier nclex-prod-restore-<date>
   ```
3. **Check the restored data.** Connect with `psql` and look at the rows that matter: the attempts,
   results or questions the incident touched.
4. **Choose one of two ways forward:**
   - **Copy rows back** (most incidents, such as one user's attempts deleted): export them from the
     restored instance with `\copy` and insert them into the live database. No downtime.
   - **Switch over** (the whole database is bad): scale `nclex-api` and `nclex-worker` to 0. Point
     the `ConnectionStrings__Postgres` secret at the restored instance, then scale back up. Candidates
     lose any writes made after the restore time. Their in-browser save queues resend recent answers
     on reconnect, as long as those attempts are still open.
5. **Keep it, then delete it.** Keep the restored instance until the incident review is done, then
   delete it.

**Restore drill:** once a quarter, do steps 1 to 3 into staging's VPC from a production snapshot.
Then run the E2E suite against a stack pointed at it. A backup nobody has restored isn't proven.

## 7. On-call checks

Signals come from OpenTelemetry (architecture.md section 9):

- **Traces:** each request from the ALB through the API to Postgres and Redis.
- **Metrics:** the meter `Nclex`, plus ASP.NET Core's `http.server.request.duration`.
- **Logs:** JSON on stdout. Every line has `RequestId` and, when known, `UserId` and `AttemptId`,
  plus `@tr`, the trace id, which joins log lines to their trace. A candidate's error message shows
  the requestId; search the logs for it.

Alert rules are in [deploy/alerts/nclex-alerts.yml](../deploy/alerts/nclex-alerts.yml) (Prometheus
format).

| Alert | Fires when | First checks |
|---|---|---|
| `OutboxLagHigh` | The oldest due email has waited more than 60 s | Is `nclex-worker` running? Look at its logs for "Email outbox round failed" (SMTP refused, or the database unreachable). Check the email provider's status page. |
| `OutboxLagMissing` | No worker has reported for 5 minutes | `nclex-worker` has no running task. Check the ECS service events. Expiry jobs and reminders are also stopped. |
| `AutosaveErrorRateHigh` | More than 1% of autosaves fail (refusals such as time up or paused don't count) | Filter `nclex.autosave.duration` by `outcome`. `failed` means 500s: look for the exception in the logs. Check the Postgres connection count and locks (autosave takes a row lock per attempt). |
| `ApiLatencyHigh` | API p95 over 500 ms for 10 minutes (target 150 ms) | Find the slow routes in the traces. Check RDS CPU and IOPS, and the ECS task CPU (autoscaling should add tasks). |
| `GradingFailed` | An attempt failed to grade | The submit returned 500 and the attempt rolled back, so it is still in progress. Find the trace and fix the cause. The candidate's next submit, the expiry job or the sweeper finishes it. |
| `SweeperFinalizing` | The sweeper finalized more than 5 attempts in 30 minutes | Delayed expiry jobs are being lost. Check that Hangfire is processing in the worker logs, and the `hangfire` schema in Postgres. |

**Daily glance** (2 minutes):

- `/readyz` shows `ok`.
- Outbox lag is near 0.
- Sweeper finalizations are near 0.
- There are no `failed` autosaves.
- p95 is under 150 ms.
- Last night's backup exists (RDS → Snapshots).

## 8. Known limitations

- **Media is on local disk.** `LocalDiskMediaStorage` writes under `MediaStorage__LocalRoot`. On
  Fargate that storage belongs to each task and is lost when the task is replaced. Until S3 storage
  exists, mount a shared EFS volume at `/data/media` on the API and worker tasks.
- **SMTP has no username or password setting.** SES, Postmark and Resend all require one, so email
  can't be sent from staging or production until `Email:Smtp` gains credentials (read from the
  secret store).
- **The email webhook is missing.** `POST /webhooks/email` (api.md section 9), for bounces and
  complaints, isn't built yet.
- **The deploy has not been run against AWS.** The infrastructure in section 4 is not provisioned by
  this repository yet.
