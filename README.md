# Nclex-Practice
NCLEX Practice &amp; Preparation Platform is an interactive web application designed to help nursing students and NCLEX candidates prepare effectively for the NCLEX-RN examination through realistic, exam-style practice questions.

## Design docs

See [docs/](docs/README.md) for the system architecture, database design, API specification and interfaces.
Each built feature has a walkthrough in [docs/features/](docs/features/).

## Run locally

### Prerequisites

- Docker Desktop (Docker Compose v2)
- .NET SDK 10.0.402 or a later 10.0 feature band (pinned in `nclex-be/global.json`)
- Node.js 24 and npm

### 1. Start the backing services

```bash
docker compose up -d --wait
```

| Service | Address | Credentials |
|---|---|---|
| PostgreSQL 16 | `localhost:5432`, database `nclex` | `nclex` / `nclex` |
| Redis 7 | `localhost:6379` | none |
| MinIO (S3) | API http://localhost:9000, console http://localhost:9001 | `nclex` / `nclex-dev-secret` |
| Mailpit | SMTP `localhost:1025`, inbox http://localhost:8025 | none |

If another project already uses one of these ports, override it in an untracked `.env` file at the
repo root, for example `MINIO_API_PORT=9010`. The variables are `POSTGRES_PORT`, `REDIS_PORT`,
`MINIO_API_PORT`, `MINIO_CONSOLE_PORT`, `MAILPIT_SMTP_PORT` and `MAILPIT_UI_PORT`. If you move
Postgres or Redis, update the connection strings too (see step 2).

### 2. Run the API

```bash
cd nclex-be
dotnet tool restore                                   # installs dotnet-ef (local tool)
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --project src/Nclex.Api                    # http://localhost:5080
```

- Liveness: http://localhost:5080/healthz
- Readiness (Postgres and Redis): http://localhost:5080/readyz
- OpenAPI document: http://localhost:5080/api/v1/openapi.json

Local connection strings live in `src/Nclex.Api/appsettings.Development.json`. Override them with
environment variables `ConnectionStrings__Postgres` and `ConnectionStrings__Redis`.

The background worker is a separate host: `dotnet run --project src/Nclex.Worker`.

### 3. Run the frontend

```bash
cd nclex-fe
npm install
npm run dev                                           # http://localhost:3000
```

Open http://localhost:3000/health. It should show "Postgres: OK". The dev server forwards `/api/*`,
`/healthz` and `/readyz` to the API (set `API_URL` if the API is not on http://localhost:5080).

### Or run everything in Docker

```bash
docker compose --profile app up -d --build --wait     # migrations, sample content, API, worker, frontend
cd nclex-fe && npx playwright install chromium && npm run e2e   # end-to-end tests against it
```

The frontend is on http://localhost:3000, the API on http://localhost:8080, and traces, metrics
and logs on http://localhost:18888. Configuration, deploys, rollback and on-call checks are in
[docs/operations.md](docs/operations.md).

### Regenerate the typed API client

With the API running:

```bash
cd nclex-fe
npm run gen:api                                       # writes src/lib/api/schema.d.ts
```

Commit the regenerated file together with the backend change that caused it.

### Checks (same as CI)

```bash
cd nclex-be && dotnet format --verify-no-changes && dotnet build && dotnet test   # Docker must be running
cd nclex-fe && npm run lint && npm run typecheck && npm test && npm run build
```
