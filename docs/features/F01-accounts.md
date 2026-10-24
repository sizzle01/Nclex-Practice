# F01: Accounts (sign up, log in, sessions)

## 1. Summary

Candidates can create an account (email, password, RN/PN track, optional display name), log in and out, and stay signed in across page reloads and browser restarts for up to 30 days.

How sessions work:
- **Access token:** a 15-minute JWT, held only in browser memory.
- **Refresh token:** an opaque token in an `HttpOnly` cookie. It's rotated on every refresh. If a token that was already used shows up again, the whole session is revoked, because that pattern indicates theft.
- **Logging out:** users can log out on this device or on every device.

The API also exposes the signed-in user's profile at `GET /me` and lets them edit it with `PATCH /me` (display name, track, time zone, target exam date). The frontend has `/signup`, `/login`, a protected placeholder `/dashboard` and a user menu in the top bar.

## 2. How to try it

```bash
docker compose up -d --wait

cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F01_Accounts
dotnet run --project src/Nclex.Api                                        # http://localhost:5080

# second terminal
cd nclex-fe
npm install
npm run dev            # http://localhost:3000 (or the next free port it prints)
```

In the browser:
1. Open `/dashboard`. You are sent to `/login?next=%2Fdashboard`.
2. Follow **Create an account** to `/signup`. Submit the empty form to see validation, then fill it in. You land on `/dashboard` with "Welcome, <name>".
3. Reload the page. You stay signed in, because the refresh cookie restores the in-memory token.
4. Open the user menu (top right) and choose **Log out**. Opening `/dashboard` now redirects to `/login`.
5. Log in with a wrong password (generic error), then the right one.

Test accounts: none are seeded. Create one at `/signup`; any email works, because verification arrives in F02.

Sample requests (curl stores the cookie in `jar.txt`):

```bash
curl -i -c jar.txt -X POST http://localhost:5080/api/v1/auth/signup \
  -H 'content-type: application/json' \
  -d '{"email":"ada@example.com","password":"correct-horse-battery","track":"RN","displayName":"Ada"}'
# 201 {"user":{...,"role":"candidate","track":"RN","planTier":"free"},"accessToken":"eyJ...","accessTokenExpiresAt":"..."}
# Set-Cookie: rt=...; max-age=2592000; path=/api/v1/auth; secure; samesite=strict; httponly

TOKEN=eyJ...   # accessToken from the response
curl http://localhost:5080/api/v1/me -H "authorization: Bearer $TOKEN"
curl -X PATCH http://localhost:5080/api/v1/me -H "authorization: Bearer $TOKEN" \
  -H 'content-type: application/json' -d '{"timezone":"Europe/London","targetExamDate":"2027-03-01"}'

# curl sends Secure cookies over http://localhost, so the jar works here
curl -i -b jar.txt -c jar.txt -X POST http://localhost:5080/api/v1/auth/refresh -H 'X-Requested-With: fetch'
curl -i -b jar.txt -X POST http://localhost:5080/api/v1/auth/logout

# Errors
curl -X POST http://localhost:5080/api/v1/auth/signup -H 'content-type: application/json' -d '{"email":"nope","password":"short"}'
# 400 {"code":"VALIDATION_FAILED","errors":[{"field":"email","code":"INVALID_FORMAT"},
#      {"field":"password","code":"TOO_SHORT"},{"field":"track","code":"REQUIRED"}],...}
```

## 3. Reading order

Backend (`nclex-be/src`):
1. `Nclex.Domain/Entities/User.cs`: the account; `Register` and `UpdateProfile`.
2. `Nclex.Domain/Entities/RefreshSession.cs`: one refresh token; `Start`, `StateAt` and `Rotate` hold the rotation rules.
3. `Nclex.Domain/Entities/ExamTrack.cs`, `UserRole.cs`, `PlanTier.cs`: the three enums.
4. `Nclex.Infrastructure/Persistence/Configurations/*.cs` and `Migrations/*_F01_Accounts.cs`: how the entities map to the DDL.
5. `Nclex.Infrastructure/DependencyInjection.cs`: Postgres enum mapping (note the shared name translator).
6. `Nclex.Application/Abstractions/`: `IAppDbContext`, `IPasswordHasher`, `ISecureTokenGenerator`, `IAccessTokenIssuer`, `ICurrentUser`.
7. `Nclex.Application/Common/AppError.cs`, `AppException.cs`, `Validation/`: how use cases report failures and field rules.
8. `Nclex.Application/Auth/Sessions/SessionService.cs`: start, rotate and revoke sessions.
9. `Nclex.Application/Auth/Signup/`, `Login/`, `Refresh/`, `Logout/`, `LogoutAll/`: one folder per use case (request, validator, handler).
10. `Nclex.Application/Users/`: `UserProfile`, `GetMe`, `UpdateMe`.
11. `Nclex.Infrastructure/Security/`: Argon2id hasher, token generator, JWT issuer and options.
12. `Nclex.Api/Auth/`: JWT bearer and role policies (`AuthSetup`), the refresh cookie, `HttpCurrentUser`.
13. `Nclex.Api/Errors/AppExceptionHandler.cs`, `FluentValidationFilter.cs`, `ValidationProblems.cs`: errors to ProblemDetails.
14. `Nclex.Api/Controllers/AuthController.cs`, `MeController.cs`: thin controllers.
15. `Nclex.Api/Program.cs`: what F01 added (filters, auth middleware, `RequireAuthorization`).

Frontend (`nclex-fe/src`):

16. `lib/api/errors.ts`: `ApiError` parsed from ProblemDetails.
17. `features/auth/session/sessionStore.ts`: in-memory token and session status.
18. `features/auth/session/refreshSession.ts`: single-flight refresh behind a cross-tab Web Lock.
19. `features/auth/session/authMiddleware.ts`: bearer header plus retry-once-after-401.
20. `features/auth/api/authApi.ts`: typed calls to the auth endpoints.
21. `features/auth/AuthProvider.tsx`: bootstrap on load, scheduled refresh, `useAuth`, `useMe`.
22. `features/auth/schemas.ts`, `components/forms/TextField.tsx`, `features/auth/components/SignupForm.tsx`, `LoginForm.tsx`, `serverErrors.ts`, `nextPath.ts`.
23. `features/auth/components/RequireAuth.tsx`, `RedirectIfSignedIn.tsx`, `UserMenu.tsx`, and `app/signup`, `app/login`, `app/dashboard`.

## 4. Request flow: signing up

1. **UI.** `SignupForm` validates with Zod (`signupSchema`) through react-hook-form.
   - Errors are shown next to each field, linked with `aria-describedby`, and the first invalid field gets focus.
   - On success, a TanStack Query mutation calls `signup()` in `authApi.ts`.
2. **Client.** `apiClient.POST("/api/v1/auth/signup", { body })`. The path and body type come from the generated `schema.d.ts`. `authMiddleware` skips auth endpoints, so no bearer header is added.
3. **Proxy.** The Next.js rewrite forwards `/api/*` to `http://localhost:5080` on the same origin, so the cookie set by the response belongs to the app's own origin.
4. **API pipeline.**
   1. Request id, Serilog and the exception handler run, then authentication finds no token, which is fine here.
   2. `AuthController.Signup` is `[AllowAnonymous]`.
   3. MVC binds `SignupRequest`. Malformed JSON or an unknown track becomes `VALIDATION_FAILED` / `INVALID_VALUE` via `ValidationProblems.FromModelState`.
   4. `FluentValidationFilter` runs `SignupRequestValidator`.
5. **Application.** `SignupHandler`:
   1. Checks `Users.AnyAsync(email == …)`. `email` is `citext`, so the check ignores case.
   2. Hashes the password with Argon2id.
   3. Calls `User.Register`, then `SessionService.Start`, which generates a 256-bit token, stores its SHA-256 and starts a new family.
   4. Saves both rows in one `SaveChangesAsync`.
   5. A unique-index race becomes `EMAIL_TAKEN`.
6. **Domain.** `User.Register` fixes the defaults: candidate, free, UTC, unverified. `RefreshSession.Start` sets a 30-day expiry.
7. **Database.** `INSERT INTO users …; INSERT INTO refresh_sessions …` in one transaction.
8. **Back out.**
   1. The handler returns `SignInResult`.
   2. The controller writes the `rt` cookie (`RefreshTokenCookie.Write`) and returns `201 AuthResponse`. The refresh token never appears in the body.
9. **UI.**
   1. `signIn()` puts the user into the `["me"]` query cache and the access token into the Zustand store, in memory only.
   2. `AuthProvider` schedules a refresh 60 seconds before expiry.
   3. `router.replace("/dashboard")`.
   4. `RequireAuth` sees `authenticated` and renders the dashboard.

**On reload**, memory is empty:
1. `AuthProvider` calls `refreshSession()`, which takes the Web Lock and calls `POST /auth/refresh` with `X-Requested-With: fetch`.
2. `RefreshHandler` finds the session by token hash and checks `StateAt`:
   - **Active:** `SessionService.Rotate` revokes the old row and inserts its successor, and the response sets the new cookie.
   - **Revoked (reuse):** every active row in the family is revoked and the response is 401.
3. `RevokedAt` is an EF concurrency token, so two requests racing with the same token can't both rotate it. The loser is treated as reuse.

## 5. Database changes

Migration `F01_Accounts`:

| Change | Why |
|---|---|
| Enums `exam_track ('RN','PN')`, `user_role (candidate, educator, author, reviewer, admin)`, `plan_tier (free, premium)` | database.md 2.1. Labels are in DDL order; Postgres sorts enums by that order. |
| Table `users`: every column of database.md 2.2, including `locked_until` and `password_changed_at` (unused until F02/F03) | Accounts |
| Unique index `ix_users_email` on `email` (`citext`) | One account per email, ignoring case |
| Table `refresh_sessions`: every column of database.md 2.2, FK `user_id → users ON DELETE CASCADE`, FK `replaced_by → refresh_sessions` | Refresh-token rotation chains |
| Unique index on `token_hash` | Lookup by the hash of the presented token |
| Partial index `(user_id) WHERE revoked_at IS NULL` | "Log out everywhere" touches only active rows |
| Index on `family_id` | Reuse detection revokes a whole family |
| Index on `replaced_by` | Not in the DDL. EF Core always indexes foreign keys. It's harmless, and it speeds up the FK check on delete. |

## 6. Key decisions and deviations

**My open questions, answered with the defaults I proposed** (you said "go" without choosing):
- **Scaffolding and branch base.** F00 had already scaffolded the project, so I built on it instead of scaffolding again. The .NET 10 SDK was missing from this machine, so I installed it with Homebrew.
- **This branch is based on `feature/f00-project-foundation`, not `main`.** F00 isn't merged yet, and the checkout was on the F00 branch when I branched. F01 needs F00's code, so this is the only workable base. Once F00 is merged, F01's commits sit directly on top of it, and `git rebase main` is a no-op apart from moving the base.
- **`GET /me` / `PATCH /me` response.** It's the signup `user` object plus `timezone` and `targetExamDate`. Signup and login return the same `UserProfile`. This is additive to api.md.
- **API client.** F00 had already chosen `openapi-typescript` + `openapi-fetch`, so I used those rather than the orval option in my plan.
- **Small spec gaps:**
  - `displayName` is optional, trimmed, and at most 80 characters.
  - A missing `X-Requested-With` returns `401 UNAUTHENTICATED`.
  - `logout` revokes the device's whole token family.
  - The "new device" security notification and lockout (`ACCOUNT_LOCKED`) are not built; see Follow-ups.

**Contract details api.md doesn't spell out.** Please confirm them so I can add them to api.md:
- **Field error codes:** `REQUIRED`, `INVALID_FORMAT`, `INVALID_VALUE`, `TOO_SHORT`, `TOO_LONG`. api.md only shows `TOO_SHORT`.
- **Unreadable bodies:** malformed JSON or an unknown enum value return `VALIDATION_FAILED` with code `INVALID_VALUE`. The field is the JSON property, or `"body"` if the whole body can't be read.
- **PATCH /me semantics:** an omitted or `null` field means unchanged, and `displayName: ""` removes the display name. `targetExamDate` can't be cleared yet; see Follow-ups.
- **Time zone values:** `timezone` must be an IANA id. Windows ids such as "GMT Standard Time" are rejected.

**Backend:**
- **Enum label order.** EF writes Postgres enum labels alphabetically, so I hand-edited the `Up` method of `F01_Accounts` to use DDL order. There's a comment in the file, and a migration test checks the order. The model snapshot is unchanged, so later migrations won't see a difference.
- **Indexes instead of constraints.** The DDL's `UNIQUE` constraints became unique indexes (EF's convention). Enforcement is the same. An alternate key would have made `email` read-only in EF, which would block the email change in F02.
- **`ExamTrack` casing.** `ExamTrack` carries `[JsonStringEnumMemberName("RN")]`, a BCL attribute, so the API sends "RN"/"PN" as in api.md while the other enums stay snake_case. Npgsql maps it with a pass-through name translator. That translator **must be a single shared instance**: a new one per DbContext made EF build a new internal service provider for every request until it threw. The integration tests caught this.
- **EF Core in Application.** `Nclex.Application` now references `Microsoft.EntityFrameworkCore`, without the Postgres provider, because `IAppDbContext` exposes `DbSet<T>`. The F00 comment anticipated entity sets on this interface.
- **How errors are raised.** Use cases throw `AppException(AppError)`. `AppExceptionHandler` turns it into ProblemDetails with the code and doesn't log it as an error. Validation runs in a global MVC filter, so handlers can assume valid input. Validation responses are written through `IProblemDetailsService`; otherwise the controllers' `[Produces("application/json")]` would replace the `application/problem+json` content type.
- **Secure by default.** `MapControllers().RequireAuthorization()` means every controller action needs a token unless it's marked `[AllowAnonymous]`. That includes `/healthz`, `/readyz`, the auth endpoints and the test-only throwing controller. I tried a fallback policy first, but it also turned 404s for unknown routes into 401s, which broke F00's contract.
- **Role policies.** They're registered as `role:candidate`, `role:admin` and so on (`Policies.Admin`). Nothing uses them yet; `/me` only needs a signed-in user.
- **JWT details:**
  - HS256 with claims `sub`, `role` and `jti`.
  - 30 seconds of clock skew instead of the default 5 minutes, which would stretch a 15-minute token by a third.
  - The dev signing key is in `appsettings.Development.json`. Deployed environments must set `Jwt__SigningKey` (at least 32 bytes); startup fails without it.
- **Dead cookies are cleared.** A failed refresh tells the browser to delete the cookie. Because the exception handler clears headers, this is registered with `Response.OnStarting`.
- **Ids.** User and session ids are UUIDv7, generated in the app so they're time-ordered. The columns keep their `gen_random_uuid()` default from the DDL.
- **Login timing.** Login always runs an Argon2 verification, even for unknown emails, so response times don't reveal which emails have accounts.
- **Where the security unit tests live.** The hasher and token generator tests are in `Nclex.Api.IntegrationTests/Security/`. architecture.md section 4 has no Infrastructure test project, and that's the project that references Infrastructure.

**Frontend:**
- **Session store.** Session state is a Zustand store, because the API middleware (not React) must read the token. The profile itself is server state in TanStack Query (`["me"]`).
- **Cross-tab refresh.** A Web Lock (`navigator.locks`) serializes refreshes across tabs. Without it, two tabs loading at once would both present the same single-use token, and the second would trigger reuse revocation. I checked this in headless Chrome: two tabs reloaded together five times and both stayed signed in, with no reuse warnings.
- **`?next=` handling.** It's read from `window.location` on submit rather than with `useSearchParams`, which under `cacheComponents` needs a Suspense boundary. `nextPathFrom` only accepts same-site paths, so the login page can't be used as an open redirect.
- **Route protection is client-side** (`RequireAuth`), because the session lives in the browser. The API enforces auth regardless.
- **Dependency versions.** `@hookform/resolvers` is pinned to 5.9.2, because 6.0.0 has an optional-peer conflict through `effect`.

**Extra packages beyond the stack list:**
- **Backend:**
  - `Konscious.Security.Cryptography.Argon2` (named in architecture.md).
  - `FluentValidation.DependencyInjectionExtensions` (registers the validators).
  - `Microsoft.IdentityModel.JsonWebTokens` (already a dependency of JwtBearer; referenced directly by the issuer).
- **Frontend:**
  - `@hookform/resolvers` (connects Zod to react-hook-form).
  - `@testing-library/user-event`.
- `zustand`, `react-hook-form` and `zod` are in the stack list.

## 7. Tests

| Suite | What it covers | Run |
|---|---|---|
| `Nclex.Domain.Tests` (9) | `RefreshSession`: start, expiry boundary, rotation (revokes, links successor, same family, new expiry), a rotated token reports `Revoked` and can't rotate again, an expired token can't rotate. `User`: registration defaults, partial profile updates, clearing the display name. | `cd nclex-be && dotnet test` |
| `Nclex.Application.Tests` (22) | Signup validator (email format, multiple and "Name <addr>" forms, 254 limit, password 10–128 with boundaries, track required, display name ≤ 80). UpdateMe validator (IANA time zones only, display name). | same |
| `Nclex.Api.IntegrationTests` (57, 51 new) | **Signup:** 201 body, exact cookie attributes, no refresh token in the body, Argon2id hash stored, case-insensitive duplicate gives 409, field errors, unknown track. **Login:** success, case-insensitive email, wrong password, unknown email gives the same answer, missing fields, malformed JSON. **Refresh:** rotation, chained rotation, missing header, missing or unknown cookie (and the cookie is cleared), **reuse revokes the family**, reuse doesn't affect other devices, expired token. **Logout:** revokes and clears, after rotation, unknown token. **Logout-all:** every device, not other users, needs a token. **`/me`:** profile, no token, **expired access token** (with a fresh-token control), token signed with another key, PATCH partial update, clearing the display name, invalid values, no token. **Security:** hasher format, salt, verify, malformed hashes; token generator. **Migration:** enum labels in DDL order, partial index. | same (Docker must be running) |
| Frontend (22, 19 new) | **`SignupForm`:** field errors without calling the API, malformed email and short password, submits trimmed values then signs in and redirects, `EMAIL_TAKEN` on the email field, API field errors placed on fields. **`LoginForm`:** generic credentials error, `?next=` redirect. **`RequireAuth`:** redirects to `/login?next=%2Fdashboard`, waits while unknown, renders for signed-in users. **`nextPathFrom`:** open-redirect cases. **`refreshSession`:** stores the token and sends the CSRF header, single request for concurrent calls, signed out on 401, nothing in localStorage. | `cd nclex-fe && npm test` |

Results on this branch:
- `dotnet build`: 0 warnings, 0 errors.
- `dotnet format --verify-no-changes`: clean.
- `dotnet test`: 88 passed (9 + 22 + 57).
- `npm run lint`: clean.
- `npm run typecheck`: clean.
- `npm test`: 22 passed.
- `npm run build`: succeeds.
- `npm audit` and `dotnet list package --vulnerable`: none.

Manual checks:
- I ran the "Done when" flow against the production build in headless Chrome 155:
  - signed-out redirect
  - signup lands on `/dashboard`
  - no token in web storage
  - the cookie is `HttpOnly`/`Strict`/scoped and invisible to `document.cookie`
  - a reload stays signed in
  - log out and redirect
  - wrong password, then log back in
  - `/login` redirects a signed-in user away
- The two-tab concurrent refresh check described in section 6.
- curl checks of each endpoint. The API log has no error-level lines, the reuse warning carries only ids, and no password, token or hash appears in the logs.

## 8. Follow-ups and known limitations

- **Email verification and the verify-email banner** (F02); password change and reset (F02).
- **Rate limiting and lockout:** `ACCOUNT_LOCKED`, `locked_until`, and the "new device" `security` notification on login (F03, plus notifications).
- **Breached-password check:** there's a `TODO` in `AccountRules.ValidNewPassword` (architecture.md section 6).
- **Access tokens stay valid after logout** for up to 15 minutes. This is inherent to stateless JWTs; a deny-list in Redis would close the gap if needed.
- **Expired and revoked `refresh_sessions` rows are never purged.** database.md asks for a purge 30 days after expiry; it needs the Hangfire worker.
- **Client IP behind a proxy.** `ip` is the direct peer address. Behind a load balancer or the Next.js proxy it's the proxy's address until forwarded headers are configured.
- **`targetExamDate` can't be cleared** through PATCH (null means unchanged). A clear-able field needs either JSON Merge Patch semantics or an explicit flag.
- **api.md updates** to make: the `UserProfile` shape for `/me`, the field error codes, the PATCH semantics and IANA time zones (see section 6).
- **OpenAPI security scheme:** the document doesn't declare bearer auth yet, which would let tools show the lock icon and send the header.
- **Safari and `Secure` cookies on `http://localhost`:** verified in Chrome only. If Safari drops the cookie in local dev, serve the app over HTTPS locally.
- **Session list UI** ("signed in on these devices"): the data is already in `refresh_sessions`.
- **Client clock skew:** refresh scheduling uses the local clock. A badly wrong clock only means the first request after expiry gets a 401, which the middleware recovers from with one refresh and retry.

## 9. Try it yourself

1. **Watch rotation happen.** In `appsettings.Development.json` add `"AccessTokenLifetime": "00:02:00"` under `Jwt`, restart the API, sign in, and keep the browser's Network tab open. About a minute later a `refresh` call appears with a new `Set-Cookie`. Then run `select id, family_id, revoked_at, replaced_by from refresh_sessions order by created_at;` and follow the chain through `replaced_by`.
2. **Play the attacker.** Sign up with curl (section 2) and save the first `rt` value. Refresh once, then send the **old** value again. You get a 401, a `Refresh token reuse detected` warning in the API log, and every row in that family revoked. Then extend `Reusing_a_rotated_token_revokes_the_whole_family` to also assert, through `AppDbContext`, that no row in the family has `revoked_at IS NULL`.
3. **Add a field rule end to end.** Reject display names that contain `<` or `>`:
   1. Write a failing test in `SignupRequestValidatorTests` first.
   2. Add the rule to `AccountRules.ValidDisplayName` with code `INVALID_FORMAT`.
   3. Add the matching `.refine` to `signupSchema` in `schemas.ts` and a test in `SignupForm.test.tsx`.

   Check that the API rejects the name even when you bypass the form with curl.
