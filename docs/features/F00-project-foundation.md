# F00: Project foundation

## 1. Summary

F00 sets up the skeleton every later feature builds on. There are no user-facing features yet.

- **Backend:** a .NET 10 solution with the project layout from architecture.md section 4, and an API host that:
  - gives every request an id and logs it with Serilog
  - returns errors as ProblemDetails in the api.md section 1 shape
  - publishes its OpenAPI document
  - exposes `/healthz` (liveness) and `/readyz` (readiness)
- **Database:** an initial EF Core migration that installs the Postgres extensions the schema needs.
- **Worker:** an empty Generic Host, ready for Hangfire later.
- **Frontend:** a Next.js app with an app shell and a `/health` page that calls `/readyz` through a client generated from the API's OpenAPI document.
- **Local and CI:** Docker Compose runs Postgres, Redis, MinIO and Mailpit locally, and GitHub Actions runs the same checks as below.

## 2. How to try it

```bash
docker compose up -d --wait

cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --project src/Nclex.Api          # http://localhost:5080

# in a second terminal
cd nclex-fe
npm install
npm run dev                                 # http://localhost:3000/health
```

Sample requests:

```bash
curl -i http://localhost:5080/healthz
# 200 {"status":"ok"}  plus an X-Request-Id: req_... header

curl http://localhost:5080/readyz
# 200 {"status":"ok","checks":{"postgres":"ok","redis":"ok"}}

docker compose stop redis && curl -i http://localhost:5080/readyz
# 200 {"status":"degraded","checks":{"postgres":"ok","redis":"degraded"}}

docker compose stop postgres && curl -i http://localhost:5080/readyz
# 503 {"status":"unhealthy","checks":{"postgres":"unhealthy","redis":"degraded"}}

docker compose start postgres redis

curl -i http://localhost:5080/api/v1/does-not-exist
# 404 application/problem+json
# {"type":"https://errors.nclex-practice.app/not-found","title":"Not Found","status":404,
#  "code":"NOT_FOUND","requestId":"req_..."}

curl http://localhost:5080/api/v1/openapi.json
cd nclex-fe && npm run gen:api              # regenerates src/lib/api/schema.d.ts
```

Test accounts: none yet (authentication is a later feature).

Port clashes: every published port in `docker-compose.yml` can be overridden. For example, put
`MINIO_API_PORT=9010` in an untracked `.env` at the repo root (see the README). If port 3000 is
taken, `next dev` picks the next free port and prints it.

## 3. Reading order

1. `docker-compose.yml`: local Postgres 16, Redis 7, MinIO and Mailpit, with healthchecks and named volumes.
2. `nclex-be/Directory.Build.props` and `nclex-be/Directory.Packages.props`: shared compiler settings (nullable, warnings as errors, analyzers) and the single place where package versions live.
3. `nclex-be/NclexPractice.sln`: the five `src/` projects and three `tests/` projects. Each `.csproj` shows the dependency rule Domain ← Application ← Infrastructure / Api / Worker.
4. `nclex-be/src/Nclex.Application/Abstractions/IAppDbContext.cs`: how use cases will see the database without depending on EF Core.
5. `nclex-be/src/Nclex.Infrastructure/Persistence/AppDbContext.cs`: the EF Core context. Declares the `pgcrypto` and `citext` extensions.
6. `nclex-be/src/Nclex.Infrastructure/DependencyInjection.cs`: registers Npgsql with snake_case naming and the Redis connection (allowed to be down).
7. `nclex-be/src/Nclex.Infrastructure/Persistence/Migrations/*_F00_Initial.cs`: the first migration (extensions only).
8. `nclex-be/src/Nclex.Api/Program.cs`: the API host. Covers service registration and the middleware order.
9. `nclex-be/src/Nclex.Api/Middleware/RequestIdMiddleware.cs` and `Logging/RequestIdEnricher.cs`: how one `req_...` id reaches the response header, every log line and error bodies.
10. `nclex-be/src/Nclex.Api/Errors/` (`ErrorCodes`, `ProblemDetailsSetup`, `ProblemDetailsExceptionHandler`): the error format from api.md section 1.
11. `nclex-be/src/Nclex.Api/Health/` and `Controllers/HealthController.cs`: the readiness checks and the `/healthz` and `/readyz` endpoints.
12. `nclex-be/src/Nclex.Worker/Program.cs`: the worker host.
13. `nclex-be/tests/Nclex.Api.IntegrationTests/Infrastructure/ApiFactory.cs`: how tests host the real API against a Testcontainers Postgres.
14. `nclex-fe/next.config.ts`: the proxy from the Next.js server to the API.
15. `nclex-fe/src/app/layout.tsx`, `providers.tsx`, `components/app-shell/TopBar.tsx`: the app shell and the TanStack Query provider.
16. `nclex-fe/src/lib/api/client.ts` and `schema.d.ts`: the typed client and the generated types.
17. `nclex-fe/src/features/health/`: the first feature folder (query, component, test).
18. `.github/workflows/ci.yml`: the CI pipeline.

## 4. Request flow: opening /health

1. **UI.** The browser loads `http://localhost:3000/health`.
   - `src/app/health/page.tsx` renders `HealthStatus`, a client component.
   - `HealthStatus` calls `useReadiness()`, a TanStack Query hook keyed `["health", "readiness"]`.
2. **Client.** The hook's `fetchReadiness()` calls `apiClient.GET("/readyz")`. `apiClient` is an `openapi-fetch` client typed by the generated `paths`, so the path and response shape are checked at compile time.
3. **Proxy.** The request goes to the Next.js server at `/readyz` (same origin). The rewrite in `next.config.ts` forwards it to `http://localhost:5080/readyz`.
4. **API pipeline** (in `Program.cs` order):
   1. `RequestIdMiddleware` sets `HttpContext.TraceIdentifier = req_<uuidv7>` and registers the `X-Request-Id` header.
   2. `UseSerilogRequestLogging` times the request.
   3. `UseExceptionHandler` and `UseStatusCodePages` stand ready to turn failures into ProblemDetails.
   4. Routing reaches `HealthController.Readiness`.
5. **Checks.** The controller asks ASP.NET Core's `HealthCheckService` to run the registered checks (`Health/HealthSetup.cs`):
   - `PostgresHealthCheck` calls `AppDbContext.Database.CanConnectAsync()`. This opens a connection and runs `SELECT 1` against Postgres.
   - `RedisHealthCheck` sends `PING` over the shared `IConnectionMultiplexer`. It is registered with failure status *Degraded*.
   - No Domain or Application code is involved: health is a host concern, and F00 has no business logic yet.
6. **Response.** The controller maps the report to `ReadinessResponse`. Enums are serialized as snake_case strings ("ok", "degraded", "unhealthy"). The status is 503 only if Postgres failed. `RequestIdEnricher` stamps every log line written during the request with the same `req_...` id.
7. **Back in the browser.**
   - `fetchReadiness` returns the body. A 503 body is returned as data too, because it says which dependency is down.
   - The component renders "✓ Postgres: OK" and "✓ Redis: OK".
   - If the API can't be reached, the query fails and the page says "Cannot reach the API."

## 5. Database changes

Migration `F00_Initial` (in `Nclex.Infrastructure/Persistence/Migrations/`):

| Change | Why |
|---|---|
| `CREATE EXTENSION IF NOT EXISTS pgcrypto` | `gen_random_uuid()` for UUID primary keys (database.md section 2.1) |
| `CREATE EXTENSION IF NOT EXISTS citext` | Case-insensitive email columns (database.md section 2.1) |
| EF history table named `__ef_migrations_history` | Keeps EF's bookkeeping table snake_case like everything else |

No business tables yet. The enums from database.md section 2.1 are created by the features that first use them.

## 6. Key decisions and deviations

Open questions from the plan, answered with the defaults I proposed (you said "go" without choosing):

- **Health endpoints live at the root** (`/healthz`, `/readyz`), not under `/api/v1`. api.md section 9 lists them without the base path, and probes conventionally use fixed root paths.
- **`/readyz` body** is `{ status, checks: { postgres, redis } }`. Every value is the single enum `HealthState` (`ok` | `degraded` | `unhealthy`). This is simpler than the "failed" value I floated in the plan: Postgres down is reported as `unhealthy`, Redis down as `degraded`. Postgres down returns 503; Redis down still returns 200 ("degraded-OK").
- **`/healthz` body** is `{ "status": "ok" }`. It never touches dependencies, so a database outage does not make the orchestrator restart the API.
- **Request id format** is `req_` followed by a UUIDv7 in hex (time-ordered, no extra dependency) instead of the ULID shown in api.md's example. It is always generated server-side; an incoming `X-Request-Id` is ignored so clients can't forge log correlation.
- **Codegen tool:** `openapi-typescript` (types) plus `openapi-fetch` (a tiny typed fetch wrapper), one of the two options in interfaces.md 5.1.
- **Extra packages beyond the stack list:**
  - backend: `EFCore.NamingConventions`, `Serilog.AspNetCore`, `Serilog.Formatting.Compact`, `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`, `xunit.v3`
  - frontend: `openapi-typescript`, `openapi-fetch`, `jsdom`, `@vitejs/plugin-react`, `@testing-library/jest-dom`
- **OpenTelemetry** is left out of F00 (not in its build list). See follow-ups.

Other deviations and choices:

- **`INTERNAL_ERROR` code.** api.md section 1 has no code for unexpected 500s, so I added `INTERNAL_ERROR` (type `https://errors.nclex-practice.app/internal-error`). Please confirm, or name the code you want, and I'll add it to api.md.
- **Framework errors are mapped too.** Errors such as an unknown route (404) also get a `code` and `requestId`, so every error body looks the same.
- **The exception message is never sent to the client.** It may contain internals; it is logged with the requestId instead. The test checks this.
- **Logging format.** Readable text locally, JSON (`RenderedCompactJsonFormatter`) in every other environment. This is chosen in code because .NET configuration merges arrays index by index, which would mix the two sinks' settings.
- **Request ids in framework logs.** ASP.NET Core's own hosting scope also adds a `RequestId` (connection-based). `RequestIdEnricher` overwrites it so each request has one id across all lines.
- **Test runner.** The .NET 10 SDK only supports xUnit v3 through Microsoft.Testing.Platform, so `global.json` opts into it and the VSTest packages are not used. `Nclex.Domain.Tests` and `Nclex.Application.Tests` are empty and pass `--ignore-exit-code 8` ("zero tests"). Remove that line when their first test lands.
- **EF Core is pinned to 10.0.13 transitively.** `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3` pulls 10.0.4, which conflicted with `Microsoft.EntityFrameworkCore.Design`.
- **No migrations on startup.** Run `dotnet ef database update` locally. Deployments use the migration bundle (architecture.md section 8). Integration tests apply migrations to their container.
- **Frontend data path.** The browser calls the API through Next.js rewrites (same origin), so there's no CORS configuration.
- **Scaffold defaults.** `create-next-app` 16.4 enabled `cacheComponents` and `partialPrefetching` by default; I left them on. I removed the template's Google fonts so builds need no network access.
- **`docker-compose.yml` ports are overridable** through environment variables. On this machine another project already holds 9000/9001/1025/8025.
- **Housekeeping.** I stopped tracking `.DS_Store` files and a Word lock file (`docs/word/~$-Design-Package-Overview.docx`), and added them to `.gitignore`. The untracked `~$LEX-Build-Prompt-Playbook.docx` lock file is ignored too.

## 7. Tests

| Suite | What it covers | Run |
|---|---|---|
| `Nclex.Api.IntegrationTests` (6 tests) | `/healthz` returns 200; `/readyz` returns 200 with Postgres `ok` and Redis `degraded` (Redis is deliberately unreachable in tests); an unhandled exception returns a 500 ProblemDetails with `code`, `type`, `requestId` matching `X-Request-Id`, and without the exception message; an unknown route returns a 404 ProblemDetails; the F00 migration installs `pgcrypto` and `citext` | `cd nclex-be && dotnet test` (Docker must be running) |
| `Nclex.Domain.Tests`, `Nclex.Application.Tests` | Empty for now | same |
| `HealthStatus.test.tsx` (3 tests) | Renders dependency states for a 200; still renders states for a 503 body; shows "Cannot reach the API" on a network error. Only `apiClient.GET` is mocked. | `cd nclex-fe && npm test` |

The integration tests share one API host and one Postgres container per run (`[assembly: AssemblyFixture]`).

Results on this branch:

- `dotnet build`: 0 warnings, 0 errors (Debug and Release)
- `dotnet format --verify-no-changes`: clean
- `dotnet test`: 6 passed
- `npm run lint`: clean
- `npm run typecheck`: clean
- `npm test`: 3 passed
- `npm run build`: succeeds

Manual checks:
- `/readyz` returns ok, degraded (Redis stopped) and 503 (Postgres stopped).
- `/health` rendered "Postgres: OK" in headless Chrome.
- The worker host starts and logs.

## 8. Follow-ups and known limitations

- **OpenTelemetry** traces and metrics (architecture.md section 9): not wired yet.
- **CI drift check for the generated client** (ADR-8: "CI fails if the generated client is out of date"). This needs the OpenAPI document produced at build time, for example with `Microsoft.Extensions.ApiDescription.Server`, an extra package that needs your approval. It could then run `npm run gen:api` against the file and run `git diff --exit-code`.
- **`VALIDATION_FAILED` with `errors[]`** (`{ field, code }`) arrives with FluentValidation in the first feature that accepts a request body. Today a framework 400 only gets the `code`.
- **Domain exception to ProblemDetails mapping.** Later features need a way for use cases to raise api.md codes such as `EMAIL_TAKEN` (e.g. an error/result type plus a mapping in the exception handler). Not built until a feature needs it.
- **Hangfire** in the worker, and Redis usage beyond the readiness check, are out of scope (their own features).
- **`npm audit`** reports 5 "high" findings. They are all one advisory in `braces`, pulled in by `eslint-config-next`, which is dev-only. `npm audit --omit=dev` is clean. Wait for an upstream fix rather than `npm audit fix --force`, which downgrades the lint config.
- **`dotnet` is not on your shell PATH.** The SDK is at `/usr/local/share/dotnet`; add it to PATH to run the commands above as written.
- **Local port clashes on this machine:** ports 9000/9001/1025/8025 (another project's MinIO and MailHog) and 3000 (another dev server). The ports can be overridden; see section 2.

## 9. Try it yourself

1. **Add a third dependency to `/readyz`.** Write a `MinioHealthCheck` (an HTTP GET to MinIO's `/minio/health/live`), register it as degraded-OK in `HealthSetup`, and add `minio` to `ReadinessChecks`. Then run `npm run gen:api` and watch `npm run typecheck` fail until `HealthStatus` renders the new row.
2. **Change the error contract.** Make the exception handler include `detail` (a generic sentence, not the exception message) and extend `Unhandled_exception_returns_500_problem_details_with_request_id` to assert it. Run the test first and watch it fail.
3. **See request ids in action.** Run the API, call `/readyz` with Redis stopped, and find the `Health check redis ... Degraded` log line. Confirm it shares its `RequestId` with the `HTTP GET /readyz responded 200` line and the response's `X-Request-Id`. Then comment out `RequestIdEnricher`'s registration in `Program.cs` and see the ids diverge.
