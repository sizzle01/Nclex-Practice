# F16: Production readiness

## 1. Summary

F16 adds no new screens. It makes the app safe to run, observable and deployable:

- **Security:** every endpoint is checked against architecture.md section 6 by tests that fail when a
  new endpoint skips the rules. The API and the frontend send security headers and a CSP. Client IP
  addresses behind the load balancer are now read correctly, so the per-IP login limit isn't shared
  by everyone. CI fails on vulnerable dependencies.
- **Observability:** the API and worker emit OpenTelemetry traces and the metrics in section 9.
  Every log line carries the requestId, userId and attemptId.
- **Packaging and deploy:** the API, worker, migration bundle and frontend each have a Docker image.
  `docker compose --profile app` runs the whole app from those images, and the Playwright suite runs
  against that stack in CI. A GitHub Actions workflow deploys to AWS staging on every push to `main`,
  and to production after approval.
- **Verified under load:** a k6 test held 500 concurrent candidates under the 150 ms p95 target.
  [docs/operations.md](../operations.md) is the runbook.

Two real bugs turned up along the way:

- **Live result notifications never reached the page in a production build.** The production
  Next.js server gzipped the event stream and held the events back.
- **Postgres ran out of connections** under load.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f16`, and was branched from
`main` at F15 (`8a30d4e`). **F13 and F14 are not merged yet**, so the endpoint tests here cover F00 to
F12 and F15. When F13 and F14 land, rebase and re-run `EndpointAuthorizationTests`: the anonymous
allowlist and the admin-role rule check their endpoints automatically.

**The whole app in Docker** (no SDKs needed):

```bash
docker compose --profile app up -d --build --wait
```

| What | Where |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:8080/readyz |
| Emails (Mailpit) | http://localhost:8025 |
| Traces, metrics and structured logs (Aspire dashboard) | http://localhost:18888 |

If those ports are taken (other sessions' stacks), run a separate copy:
`docker compose -p nclex-f16 --env-file ports.env --profile app up -d --build --wait`, with
`ports.env` setting `APP_WEB_PORT`, `APP_API_PORT`, `POSTGRES_PORT` and so on (README step 1).

**Test accounts:** none are seeded. The E2E suite signs up a fresh account per test, and you can do
the same at `/signup`; the verification email arrives in Mailpit.

**End-to-end tests:**

```bash
cd nclex-fe
npx playwright install chromium
npm run e2e                                  # E2E_BASE_URL / MAILPIT_URL if you changed ports
```

**Load test:** see [F16-loadtest-report.md](F16-loadtest-report.md) for the command.

**Things to look at:**

```bash
curl -sI http://localhost:3000/login | grep -iE "content-security|x-frame|strict"   # page headers
curl -sI http://localhost:8080/healthz | grep -iE "content-security|nosniff"        # API headers
docker compose logs api | tail -3      # JSON lines with RequestId, UserId, AttemptId and @tr (trace id)
```

Take an exam in the browser, then open the dashboard: **Traces** shows the request through Npgsql to
Postgres, and **Metrics → Nclex** shows `nclex.autosave.duration` and `nclex.grading.duration`.

## 3. Reading order

**Security:**

1. `nclex-be/tests/Nclex.Api.IntegrationTests/Security/EndpointAuthorizationTests.cs`: the endpoint
   review as code. It holds the anonymous allowlist (with reasons), the staff-role rule for
   `/admin`, and the answer-key contract test over the OpenAPI document.
2. `nclex-be/src/Nclex.Api/Middleware/ForwardedHeadersSetup.cs`: which proxies may set `X-Forwarded-For`, and why.
3. `nclex-be/src/Nclex.Api/Middleware/SecurityHeadersMiddleware.cs`: the API's headers.
4. `nclex-fe/src/lib/security/securityHeaders.ts`: the page CSP, and why it isn't nonce-based.

**Observability:**

5. `nclex-be/src/Nclex.Application/Common/NclexMetrics.cs`: every application metric in one place.
6. `nclex-be/src/Nclex.Infrastructure/Telemetry/TelemetryServiceCollectionExtensions.cs`: the
   OpenTelemetry setup both hosts share. Then see `Program.cs` in `Nclex.Api` for the ASP.NET Core
   instrumentation, and how probes are kept out of the logs.
7. `nclex-be/src/Nclex.Api/Logging/RequestContextEnricher.cs`: requestId, userId and attemptId on each log line.

**Packaging and tests:**

8. `nclex-be/Dockerfile` and `nclex-fe/Dockerfile`: one backend file with three targets, plus the frontend.
9. `nclex-be/src/Nclex.Infrastructure/Persistence/AppDbContextFactory.cs`: why the migration bundle
   needs its own way to build the DbContext.
10. `docker-compose.yml`: the `app` profile, from migrate to seed to api, worker and web.
11. `nclex-fe/e2e/` (start with `support/`, then the `*.e2e.ts` files) and `nclex-fe/playwright.config.ts`.
12. `loadtest/autosave-and-items.js`, then `docs/features/F16-loadtest-report.md`.

**Deploy and operations:**

13. `.github/workflows/ci.yml` (the new scan, dependency-review and e2e jobs),
    `.github/workflows/deploy.yml`, and `deploy/aws/deploy.sh`.
14. `deploy/alerts/nclex-alerts.yml`, then `docs/operations.md`.

## 4. Request flow: one autosave, traced

1. **UI:** the candidate picks an option. The player's save queue sends
   `PUT /api/v1/attempts/{id}/items/3/response` to the Next.js server, which forwards it to `api:8080`
   and adds the browser's address to `X-Forwarded-For`.
2. **Forwarded headers:** `UseForwardedHeaders` sees that the request came from a trusted network
   (`ForwardedHeaders:KnownNetworks`) and sets `RemoteIpAddress` to the browser's address.
   `SecurityHeadersMiddleware` registers the response headers, and `RequestIdMiddleware` assigns
   `req_…`.
3. **Tracing:** the ASP.NET Core instrumentation starts a span. Serilog stamps the span's trace id
   (`@tr`) on every log line, and `RequestContextEnricher` adds `RequestId`, `UserId` (from the
   token's `sub`) and `AttemptId` (from the route, on AttemptsController only).
4. **Application:** `SaveResponseHandler.HandleAsync` starts a timer and calls `SaveAsync`, which is
   unchanged from F08 and F09. It locks the attempt `FOR UPDATE`, validates and stores the answer,
   and commits. Npgsql's instrumentation adds a child span for each SQL command.
5. **Metric:** the handler records `nclex.autosave.duration` with `outcome=saved` (or `stale`, an
   api.md code such as `ATTEMPT_PAUSED`, or `failed` for an unexpected error). An `AppException` also
   passes through `AppExceptionHandler`, which counts `nclex.api.error_responses{code}`.
   `ATTEMPT_TIME_EXPIRED` rejections are counted there.
6. **Export:** every few seconds the OTLP exporter sends spans and metrics to
   `OTEL_EXPORTER_OTLP_ENDPOINT`: the Aspire dashboard locally, an ADOT collector in AWS. The alert
   rules (`AutosaveErrorRateHigh`, `ApiLatencyHigh`) are evaluated on those metrics.

## 5. Database changes

None, so there is no `F16_` migration. What changed is how migrations are run:

- **The bundle.** `AppDbContextFactory` lets the bundle (`dotnet ef migrations bundle`, the
  `migrations` image) create the context without starting the API host. The compose `migrate`
  service and the deploy script run it before any new code starts.
- **Shared settings.** The app and the factory both call `UsePostgres(...)` in
  `Infrastructure/DependencyInjection.cs`: snake_case naming, mapped enums and the
  `__ef_migrations_history` table. That way the tools and the app see the same model.
- **Connection pool.** The compose connection string sets `Maximum Pool Size=40` (load test finding).

## 6. Key decisions and deviations

- **The deploy target is AWS (ECS Fargate).** That's the reference in architecture.md section 8.
  You didn't pick one, so I used the plan's recommendation. The workflow updates infrastructure that
  already exists; it doesn't create any. There's no Terraform, as suggested in the plan, and
  operations.md section 4 lists what has to exist.
- **The CSP is static, not nonce-based.** This changes my plan. The Next.js docs bundled with this
  version say nonce-based CSP "is incompatible" with partial prerendering, and
  `next.config.ts` enables `cacheComponents`. Nonces would mean turning that off and rendering every
  page dynamically. So scripts allow `'unsafe-inline'`, and everything else is locked to the app's
  own origin: no framing, no plugins, no other origins for fetch, media or forms. If you want the
  stricter policy, the cost is turning off `cacheComponents`; ask and I'll switch.
- **No `upgrade-insecure-requests` in the CSP.** HSTS already upgrades production, and the directive
  breaks the plain-http compose stack.
- **The frontend's `API_URL` is fixed at build time.** Next.js compiles rewrites into the standalone
  server. All environments use the internal name `api:8080` (Service Connect, or the compose service
  name), so one image works everywhere.
- **The migration bundle is framework-dependent and runs on `aspnet:10.0`.** A self-contained bundle
  needs a restore for that runtime, which `dotnet ef migrations bundle` skips. This is simpler and
  just as reproducible.
- **`dotnet ef` now uses `AppDbContextFactory`.** It reads `ConnectionStrings__Postgres`, falling
  back to the compose database. That's the same value `appsettings.Development.json` has, so the
  README commands and the per-feature `nclex_fXX` databases work as before.
- **There is no dedicated time-expired counter.** `ATTEMPT_TIME_EXPIRED` rejections are
  `nclex.api.error_responses{code="ATTEMPT_TIME_EXPIRED"}`, one counter for every expected error.
- **Alerts are Prometheus rules,** which work with Amazon Managed Prometheus, Grafana or self-hosted
  Prometheus. They are not CloudWatch alarms.
- **Fixes to existing features:**
  - The SSE stream now sends `Cache-Control: no-transform` (F12's bug: compression held events back).
  - `dotnet format` problems already on `main` are fixed in their own `style(f16)` commits (CI's
    format step would have failed).
  - The autosave metric counts unexpected 500s as `failed`.
- **New dependencies,** approved in the plan:
  - OpenTelemetry: `Extensions.Hosting`, `Instrumentation.AspNetCore`, `Instrumentation.Http`,
    `Exporter.OpenTelemetryProtocol`, and `Instrumentation.StackExchangeRedis` (only published as a
    beta, `1.20.1-beta.1`).
  - `Npgsql.OpenTelemetry`.
  - `@playwright/test`.
  - k6 (a Docker image only) and the Aspire dashboard (a compose image only).

**Endpoint review: what was checked and found.** Each row is a rule from section 6:

| Rule | Result |
|---|---|
| Every endpoint needs a bearer token unless it is allowlisted | ✓ 15 anonymous endpoints, each with a reason: signup and login, email-token links, refresh and logout (cookie), signed media links, the SSE stream (ticket), probes, and OpenAPI. Now enforced by a test. |
| Admin routes are role-gated | ✓ `/admin/flags` needs reviewer or admin. Now enforced by a test for every `/admin` route F14 adds. |
| No candidate DTO has the answer key before reveal | ✓ Only `ItemReveal` has `answerKey` or `rationale` in schemas candidates can reach. Now enforced by a contract test over the OpenAPI document. |
| Refresh CSRF guard, verified-email guard, tier guard, ownership (404) | ✓ Already in place, each with tests from the feature that added it (F01 refresh, F06 tier, F08 verified email and ownership). |
| Rate limiting per IP | ✗ → fixed. Behind a proxy every client had the proxy's IP. Forwarded headers are now trusted from configured networks only. |
| Security headers / CSP | ✗ → fixed, on both the API and the pages. |
| Secrets only from the environment or secret store | ✓ Keys exist only in `appsettings.Development.json`, and the API refuses to start without them (validated options). Compose-only keys are labelled as such. |
| `POST /webhooks/email` (api.md section 9) | ✗ Not built. Follow-up. |

## 7. Tests

| Test | What it covers |
|---|---|
| `Security/EndpointAuthorizationTests` | The anonymous allowlist, auth on everything else, staff roles on `/admin`, and the answer-key contract over the OpenAPI document |
| `Security/SecurityHeadersTests` | Headers on 200, 401 and 500 responses; `X-Forwarded-For` believed from loopback, ignored from an unknown client |
| `Observability/MetricsTests` | The autosave outcomes `saved`, `stale` and an error code; the error-response counter; grading duration; the outbox lag gauge |
| `Observability/RequestContextEnricherTests` | requestId, userId and attemptId on log events; ids from other controllers not taken for an attempt id |
| `Notifications/NotificationStreamTests` (extended) | The stream sends `no-transform` |
| `nclex-fe/src/lib/security/securityHeaders.test.ts` | Production versus development CSP, HSTS only in production |
| `nclex-fe/e2e/*.e2e.ts` (Playwright, 4 journeys) | Sign up and verify by the emailed link; a tutor set with rationale; a timed exam with pause, resume and submit, then the result notification arriving live in the bell; a draft resumed with its answer in a second browser, which takes the lease |
| `loadtest/autosave-and-items.js` (k6) | 500 concurrent candidates; p95 thresholds |

Run them:

```bash
cd nclex-be && dotnet format --verify-no-changes && dotnet build && dotnet test
cd nclex-fe && npm run lint && npm run typecheck && npm test
docker compose --profile app up -d --build --wait && (cd nclex-fe && npm run e2e)
```

## 8. Follow-ups and known limitations

**Before staging can serve real users:**

- **Media storage on S3.** `LocalDiskMediaStorage` isn't shared between Fargate tasks. Until S3
  exists, mount EFS at `/data/media`.
- **SMTP credentials.** `Email:Smtp` has no username or password, and SES, Postmark and Resend need
  them, so no email can be sent from AWS until it does.
- **Provision the AWS infrastructure** in operations.md section 4, possibly as infrastructure-as-code
  in its own feature.
- **Deploy has not been verified.** The workflow has never run against AWS. "Staging deploys from
  main" stays open until it does.
- **Load test against staging.** The report's numbers are from a laptop.

**Other follow-ups:**

- **`POST /webhooks/email`** for bounces and complaints (api.md section 9).
- **ASP.NET Core Data Protection keys** are stored inside the container, which is warned about at
  start-up. Nothing uses Data Protection today; persist the keys if something starts to.
- **Load-test scenarios:** a burst of submits at the end of a scheduled exam, the SSE stream with 500
  connections, and the Next.js layer.
- **Rebase after F13 and F14 merge.** Re-run the endpoint tests, and add any new anonymous endpoint
  to the allowlist deliberately.
- **If the stricter CSP is wanted:** drop `cacheComponents` and move to nonces (section 6).

## 9. Try it yourself

1. **Add an endpoint and watch the guard catch it.** Put `[AllowAnonymous]` on
   `NotificationsController.UnreadCount` and run `dotnet test --project tests/Nclex.Api.IntegrationTests --filter-class
   Nclex.Api.IntegrationTests.Security.EndpointAuthorizationTests`. The allowlist test fails and names the endpoint. Revert it.
2. **Add a metric.** Count how often candidates pause, as `nclex.attempts.pauses` tagged with the
   reason (`break` or `save_exit`), in `NclexMetrics` and `PauseAttemptHandler`. Take an exam in the
   compose stack, pause it, and find the metric in the dashboard at http://localhost:18888.
3. **Break the stream again.** Remove `no-transform` in `NotificationStreamEndpoint`, rebuild the API
   (`docker compose --profile app up -d --build api`), and run `npm run e2e -- timed-exam`. The
   notification test times out, the way the bug showed in production. Then put it back.
