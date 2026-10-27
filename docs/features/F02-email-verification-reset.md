# F02: Email delivery, verification and password reset

## 1. Summary

New accounts now get a "verify your email" link. Following it marks the account verified, and an unverified user sees a dashboard banner with a resend button. Users who forget their password can ask for a reset link and choose a new one, optionally signing out of every device. All email goes through a transactional outbox: the API writes an `email_outbox` row in the same transaction as the change that causes it, and `Nclex.Worker` sends it over SMTP (Mailpit locally) every 2 seconds, retrying with backoff. A `VerifiedEmail` authorization policy is ready for F08 to protect non-sample attempts.

## 2. How to try it

```bash
docker compose up -d                 # Postgres, Redis, MinIO, Mailpit
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F02_EmailAndTokens
dotnet run --project src/Nclex.Api                                        # http://localhost:5080

# second terminal: sends the emails
dotnet run --project src/Nclex.Worker

# third terminal
cd nclex-fe
npm run dev                          # http://localhost:3000
```

Mailpit's inbox is at http://localhost:8025.

> **Port clash on this machine.** Another project's `acgsf-mailhog` container already uses 1025 and 8025, so Mailpit can't bind them. I didn't stop it. To use Mailpit alongside it, use other ports and point the worker at them:
> ```bash
> MAILPIT_SMTP_PORT=1026 MAILPIT_UI_PORT=8026 docker compose up -d mailpit
> Email__Smtp__Port=1026 dotnet run --project src/Nclex.Worker     # inbox at http://localhost:8026
> ```
> Or, if `acgsf-mailhog` is running anyway, leave the defaults: the worker then delivers to MailHog's inbox on 8025, which works the same for this feature.

In the browser:
1. Sign up at `/signup`. The dashboard shows **Verify your email address** with a resend button.
2. Within about 2 seconds, the email appears in Mailpit. Click **Verify my email**: `/verify-email?token=…` says "Your email is verified". Back on the dashboard, the banner has gone; focusing the tab refetches the profile.
3. Open the same link again: "This link is invalid or has expired".
4. Log out, open `/login`, and choose **Forgot password?**. Enter your email (or any email): you get the same "Check your email" message either way.
5. Open the reset email and choose a new password. Leave "Sign out on every device" ticked. Log in with the new password; the old one no longer works.
6. With an unverified account, press **Resend verification email** twice more. The third press says "Try again in 60 minutes" (the sign-up email counts towards the limit of 3 per hour).

Test accounts: none are seeded. Sign up with any address; Mailpit catches everything.

Sample requests:

```bash
# Verify (token from the email link)
curl -i -X POST http://localhost:5080/api/v1/auth/verify-email -H 'content-type: application/json' -d '{"token":"<token>"}'
# 204, or 400 {"code":"INVALID_TOKEN","title":"This link is invalid or has expired.",...}

# Resend (needs the access token from signup or login)
curl -i -X POST http://localhost:5080/api/v1/auth/verify-email/resend -H "Authorization: Bearer $ACCESS"
# 204, or 429 with "Retry-After: 3412" and {"code":"RATE_LIMITED","retryAfterSec":3412,...}

# Forgot password: always 202, for real and unknown emails alike
curl -i -X POST http://localhost:5080/api/v1/auth/password/forgot -H 'content-type: application/json' -d '{"email":"ada@example.com"}'

# Reset (signOutEverywhere defaults to true)
curl -i -X POST http://localhost:5080/api/v1/auth/password/reset -H 'content-type: application/json' \
  -d '{"token":"<token>","newPassword":"a-brand-new-password","signOutEverywhere":true}'
# 204

# Watch the outbox
docker exec nclex-postgres-1 psql -U nclex -d nclex -c \
  "select template, status, attempts, send_after, payload from email_outbox order by created_at desc limit 5;"
```

## 3. Reading order

Backend (`nclex-be/src`):
1. `Nclex.Domain/Entities/EmailToken.cs` and `EmailTokenPurpose.cs`: a single-use link token; lifetimes, `Consume`, `Supersede`.
2. `Nclex.Domain/Email/EmailOutboxMessage.cs`, `OutboxStatus.cs` and `OutboxRetryPolicy.cs`: an outbox row's life (`Queue`, `StartAttempt`, `MarkSent`, `MarkAttemptFailed`, `ReleaseExpiredClaim`) and the backoff.
3. `Nclex.Domain/Entities/User.cs`: the new `MarkEmailVerified` and `ChangePassword`.
4. `Nclex.Infrastructure/Persistence/Configurations/EmailTokenConfiguration.cs`, `EmailOutboxMessageConfiguration.cs` and `Migrations/*_F02_EmailAndTokens.cs`: the mapping to the DDL.
5. `Nclex.Application/Abstractions/IEmailOutbox.cs` and `IEmailSender.cs`: queuing versus sending.
6. `Nclex.Application/Email/EmailTemplateNames.cs`: template names and payload records.
7. `Nclex.Application/Auth/EmailTokens/EmailTokenService.cs` and `AccountEmails.cs`: issue and find tokens; queue the two account emails.
8. `Nclex.Application/Auth/Signup/SignupHandler.cs`: one added line queues the verification email in the sign-up transaction.
9. `Nclex.Application/Auth/VerifyEmail/`, `ResendVerification/`, `ForgotPassword/`, `ResetPassword/`: the four use cases.
10. `Nclex.Infrastructure/Email/EfEmailOutbox.cs`, `EmailPayloadJson.cs`, `EmailLinks.cs`, `EmailOptions.cs`.
11. `Nclex.Infrastructure/Email/Templates/`: `LinkEmailLayout` (shared HTML and text) plus one class per template.
12. `Nclex.Infrastructure/Email/OutboxDispatcher.cs`: claim with `FOR UPDATE SKIP LOCKED`, send, record, release stuck claims.
13. `Nclex.Infrastructure/Email/SmtpEmailSender.cs` and `EmailServiceCollectionExtensions.cs`.
14. `Nclex.Worker/EmailOutboxDispatcherService.cs` and `Program.cs`: the 2-second loop.
15. `Nclex.Api/Auth/VerifiedEmailRequirement.cs`, `ApiAuthorizationResultHandler.cs`, and `AuthSetup.cs` (policy registration).
16. `Nclex.Api/Errors/AppExceptionHandler.cs`: 429 with `Retry-After` and `retryAfterSec`.
17. `Nclex.Api/Controllers/AuthController.cs`: the four new actions.

Frontend (`nclex-fe/src`):

18. `lib/api/errors.ts`: `ApiError.retryAfterSec`.
19. `features/auth/api/authApi.ts`: `verifyEmail`, `resendVerification`, `forgotPassword`, `resetPassword`.
20. `features/auth/schemas.ts`: `forgotPasswordSchema`, `resetPasswordSchema` (with the confirm-password check).
21. `features/auth/components/ResendVerificationButton.tsx`: shared resend button with a live region.
22. `features/auth/components/VerifyEmailResult.tsx`, `ForgotPasswordForm.tsx`, `ResetPasswordForm.tsx`, and `components/forms/FocusedHeading.tsx`.
23. `features/dashboard/components/VerifyEmailBanner.tsx`, and where `DashboardPlaceholder.tsx` shows it.
24. `app/verify-email`, `app/forgot-password`, `app/reset-password`: pages (note the Suspense boundaries).

## 4. Request flow: signing up, then following the link

**Sign-up queues the email**
1. **API.** `POST /auth/signup` goes to `SignupHandler` as in F01. After `sessions.Start`, it calls `AccountEmails.QueueVerificationAsync(user)`.
2. **Application.** `EmailTokenService.IssueAsync`:
   - marks any older unused verification tokens as consumed, so only the newest link works
   - generates a 256-bit token with `ISecureTokenGenerator`
   - adds an `EmailToken` with only the SHA-256 hash and `expires_at = now + 24h`

   Then `IEmailOutbox.Enqueue` adds an `email_outbox` row: template `verify_email`, payload `{"displayName":"Ada","token":"<raw token>"}`.
3. **Database.** A single `SaveChangesAsync`, so a single transaction, inserts `users`, `refresh_sessions`, `email_tokens` and `email_outbox`. If anything fails, nothing is saved, so there's never an account without its email, or an email for an account that doesn't exist (ADR-7).

**The worker sends it**

4. **Worker.** `EmailOutboxDispatcherService` wakes every 2 seconds and calls `OutboxDispatcher.DispatchDueAsync` in a fresh DI scope.
5. **Claim.** A short transaction runs `SELECT … WHERE status='pending' AND send_after <= now ORDER BY send_after LIMIT 20 FOR UPDATE SKIP LOCKED`.
   - Each row gets `StartAttempt`: status `sending`, `attempts + 1`, and `send_after = now + 10 min` as a claim lease.
   - The transaction commits before any email goes out, so slow SMTP never holds row locks.
   - A second worker skips the locked rows instead of waiting for them or sending them too.
6. **Send.** `EmailTemplates.Get("verify_email")` renders `LinkEmailLayout`, with every value HTML-encoded. `EmailLinks` builds `http://localhost:3000/verify-email?token=…`. `SmtpEmailSender` hands the email to Mailpit and returns our own `Message-ID`.
7. **Record.** `MarkSent` sets `sent`, `sent_at` and `provider_id`, and replaces the payload with `{}` so the raw token doesn't stay in the database. On failure, `MarkAttemptFailed` stores `last_error` and sets the next `send_after` from `OutboxRetryPolicy` (30 s, 2 min, 10 min, 1 h), or marks the row `failed` after 5 attempts. Each row is saved on its own straight after its send.

**The user follows the link**

8. **UI.** `/verify-email?token=…` renders `VerifyEmailResult`. It reads the token with `useSearchParams`, which is why the page has a Suspense boundary, and calls `verifyEmail(token)` once from an effect.
9. **API.** `POST /auth/verify-email` goes to `VerifyEmailHandler`. `EmailTokenService.FindUsableAsync` hashes the token and loads it with its user. It returns null if the token is unknown, for another purpose, consumed, expired, or belongs to a deleted account; each of those becomes `400 INVALID_TOKEN`.
10. **Domain.** `EmailToken.Consume(now)`, then `User.MarkEmailVerified(now)`.
11. **Database.** One UPDATE for each table. `consumed_at` is an EF concurrency token, so the token UPDATE has `AND consumed_at IS NULL`. If two clicks race, one wins and the other gets `INVALID_TOKEN` (the integration test fires 5 at once).
12. **Back out.** 204. The page shows "Your email is verified" and invalidates the cached `me` query, so the dashboard banner disappears.

## 5. Database changes

Migration `F02_EmailAndTokens`:

| Change | Why |
|---|---|
| Enum `email_token_purpose` (`verify_email`, `reset_password`, `change_email`) in DDL order | database.md 2.1. The migration is hand-edited because EF sorts labels alphabetically, as in F01. |
| Table `email_tokens`: every column of database.md 2.2. FK `user_id → users ON DELETE CASCADE`. | Single-use link tokens, stored hashed |
| Unique index on `email_tokens.token_hash` | Look up a presented token by its hash |
| Partial index `(user_id, purpose) WHERE consumed_at IS NULL` | Superseding older links when a new one is sent |
| Table `email_outbox`: every column of database.md 2.8, including `status` text with CHECK `email_outbox_status_check` and defaults. FK `user_id → users ON DELETE CASCADE`. | Transactional outbox |
| Partial index `outbox_pending_idx (send_after) WHERE status = 'pending'`, named as in the DDL | The worker's 2-second poll |
| Index `ix_email_outbox_user_id` (not in the DDL) | Added automatically by EF for the FK, like F01's `ix_refresh_sessions_replaced_by`. It keeps the cascade delete of a user from scanning the whole outbox. |

`notifications` and `notification_preferences` (database.md 2.8) are **not** created: `notifications` has an FK to `attempts`, which doesn't exist yet, and the feature doesn't list them.

## 6. Key decisions and deviations

**Your plan answers.** You said "go", so I used the defaults I proposed:
- **Branch base.** This branch is based on `feature/ui-teal-blue-theme`, not `main`. `main` still has none of F00, F01 or the UI work.
- **`INVALID_TOKEN` (400)** is a new code, not in api.md. It covers unknown, expired, used, superseded and wrong-purpose tokens alike, so a response never reveals a token's state.
- **SMTP** uses the built-in `System.Net.Mail.SmtpClient`, so there's no new dependency. Microsoft recommends MailKit for new code; production will likely use a provider's API behind `IEmailSender` anyway (see Follow-ups).
- **Payload redaction.** Once an email is sent, the dispatcher replaces its payload with `{}` when the template says the payload holds a secret (`IEmailTemplate.PayloadHoldsSecret`). Until it's sent, the raw token has to sit in `email_outbox.payload`, so the worker can build the link. That's the one place a usable token exists at rest, for at most a few seconds normally.
- **Resend limit.** It counts verification tokens created in the last rolling hour, including the sign-up one, so a new user gets 2 resends in the first hour. `Retry-After` is when the oldest of those turns an hour old.
- **A reset also verifies the email**, because following a link sent to the address proves ownership.

**Contract details api.md doesn't spell out.** Please confirm them so I can add them to api.md:
- `POST /auth/verify-email` returns **204**. Resend returns **204**, including the no-op when the email is already verified. Forgot returns **202** with an empty body.
- `POST /auth/verify-email/resend` **429** has `Retry-After` (seconds) and also `retryAfterSec` in the body. api.md only mentions `retryAfterSec` for `ACCOUNT_LOCKED`.
- `POST /auth/password/forgot` with a **malformed** email returns `400 VALIDATION_FAILED`. This reveals nothing about accounts; "always 202" applies to well-formed emails.
- `signOutEverywhere` defaults to `true` when omitted. The OpenAPI document says so (`@default true`).
- Verifying a token doesn't need sign-in, so links work on any device.

**Backend:**
- **The VerifiedEmail policy checks the database, not a JWT claim.** Otherwise verifying on a phone would only take effect on the laptop after its access token refreshed (up to 15 minutes). It costs one indexed primary-key read per protected request. `ApiAuthorizationResultHandler` turns that policy's failure into `403 EMAIL_NOT_VERIFIED`; every other authorization result keeps the default handling. Nothing uses the policy yet except a test-only controller; F08 adds it to attempt routes.
- **Two-phase claiming.** Rows are marked `sending` in their own short transaction, then sent, rather than sent while locked. If a worker dies mid-send, the row stays `sending` until its 10-minute lease (`send_after`) runs out. `ReleaseExpiredClaimsAsync` then returns it to `pending`, and that attempt still counts. The release runs once a minute, not every 2 seconds, because it can't use the pending-only index. The trade-off is at-least-once delivery: a crash after SMTP accepted the email but before the row was saved sends it again.
- **`AppException` gained an optional `RetryAfter`, and `AppErrorKind` gained `TooManyRequests`.** F03's lockout (`ACCOUNT_LOCKED`, `retryAfterSec`) can reuse both.
- **Password reset revokes sessions *before* saving the new password.** If the save then fails, the worst case is being signed out with the old password still working, never a new password with an attacker's session still alive.
- **The worker gets email delivery; the API only queues.** `AddInfrastructure` registers `IEmailOutbox` for both hosts; `AddEmailDelivery` (templates, SMTP, dispatcher, validated `Email` options) is called only by the worker and by the integration tests.
- **Email settings** live in `Nclex.Worker/appsettings*.json` (`Email:From`, `Email:FrontendBaseUrl`, `Email:Smtp:Host/Port/EnableSsl`).
- **Logging.** The dispatcher logs outbox ids, template names and attempt numbers only, never payloads (tokens) or addresses (personal data). `last_error` stores the exception type and message.

**Frontend:**
- **Reset with "sign out everywhere" also clears this browser's session** (`clearSession()`), because its refresh token was just revoked.
- **The banner's dismissal** is kept in `sessionStorage` under the user's id. It returns in a new browser session, and another account in the same tab still sees it. Storage failures are caught, so the banner then just shows. After dismissing, focus moves to the dashboard heading.
- **Focus after a form is replaced by its result** goes to the result's heading (`FocusedHeading`), so keyboard and screen-reader users aren't dropped at the top of the page.
- **The verify page sends the token once.** It does so from an effect, behind a `useRef` guard. While testing I found that TanStack Query only calls the function once even without the guard under StrictMode, so the test can't show what the guard prevents. I kept the guard as cheap protection for a single-use token and named the test for what it actually checks.
- **Pages that read `?token=` sit inside `<Suspense>`.** That's required with `cacheComponents: true`, and `npm run build` confirms it.

## 7. Tests

| Project | What's covered | Run |
|---|---|---|
| `Nclex.Domain.Tests` (34, 25 new) | Token lifetimes (24 h and 30 min, boundary ticks), single use, can't consume when expired, supersede keeps the original time. Outbox: queue, claim lease, release only after the lease, sent with and without redaction, retry schedule, failed after 5 attempts, error truncation, invalid transitions. Retry policy delays. `User.MarkEmailVerified` (first time wins) and `ChangePassword`. | `cd nclex-be && dotnet test tests/Nclex.Domain.Tests` |
| `Nclex.Application.Tests` (33, 11 new) | Verify, forgot and reset validators (token required, email format, password 10–128 with boundaries); `SignOutEverywhere` defaults to true. | same pattern |
| `Nclex.Api.IntegrationTests` (102, 45 new) | **Signup:** one pending outbox row in the same transaction; only the token hash is stored. **Verify:** success; **single use**; **expired**; unknown; missing; **superseded after resend**; reset token rejected; **5 concurrent uses → exactly 1 success**. **Resend:** queues another; **4th in an hour → 429 + Retry-After + retryAfterSec**; old tokens don't count; no-op when verified; 401 without a token. **Forgot:** queues for an existing account (any case); **unknown email → same 202, empty body, nothing queued**; malformed → 400. **Reset:** new password works and the old fails; **revokes every session by default**; can keep sessions; verifies the email; single use; **expires at 30 min**; superseded; verification token rejected; validation. **VerifiedEmail policy:** 403 `EMAIL_NOT_VERIFIED`; the same access token passes right after verifying; 401 without a token. **Dispatcher** (fake sender, runs on its own): **pending → sent with provider id**; payload token cleared; not sent twice; display name HTML-encoded; reset link; **failure → retry later**; **last failure → failed**; unknown template fails only that email; **two dispatchers at once → each of 30 emails sent exactly once**; stuck claim released and resent; claim within its lease left alone. **Migration:** enum label order, both partial indexes, status CHECK. | `cd nclex-be && dotnet test` (Docker must be running) |
| Frontend (49, 17 new) | **`VerifyEmailResult`:** one call with the link's token, success; dashboard link for signed-in users and login for others; invalid link → resend (signed in) or log in (signed out); no token → no API call. **`ForgotPasswordForm`:** validates first; same confirmation, focused. **`ResetPasswordForm`:** password rules and mismatch; sends the token and `signOutEverywhere: true` by default and clears the session; can keep sessions; expired link → alert with "Get a new link"; no token. **`VerifyEmailBanner`:** shows the address; resend announced; 429 → "Try again in 25 minutes"; dismissal remembered; still shown for another user. | `cd nclex-fe && npm test` |

Results on this branch:
- `dotnet build`: 0 warnings, 0 errors.
- `dotnet format --verify-no-changes`: clean.
- `dotnet test`: 169 passed (34 + 33 + 102). I ran the integration tests 3 times in total, to check the concurrency tests for flakiness; all passed each time.
- `npm run lint`: clean.
- `npm run typecheck`: clean.
- `npm test`: 49 passed.
- `npm run build`: succeeds.

Manual end-to-end check, with the real API, the real worker and Mailpit on 1026/8026:
- Signup → the email reached Mailpit about 3 seconds later, with our Message-ID.
- The link's token verified the account (`/me` says `emailVerified: true`), and a second use gave `INVALID_TOKEN`.
- Forgot returned 202 for a known and an unknown address.
- The reset link changed the password: the old one gives 401 and the new one 200. The old refresh cookie gives 401.
- Both outbox rows are `sent` with payload `{}`.
- Limit: I did not click through the new pages in a browser. They're covered by the component tests and the production build.

## 8. Follow-ups and known limitations

- **F03:**
  - change email (`change_email` tokens: `EmailToken.Issue` throws for them until then)
  - the `security` notification and email after a password reset (api.md lists them under reset)
  - Redis-backed rate limiting. The resend limit is a per-user database count, so two requests at the same instant could both get under it. Forgot-password has no limit at all yet.
- **Timing side channel on forgot-password.** An unknown email returns slightly faster, because nothing is written. One fix is to queue the work for every request and decide in the worker; another is a fixed minimum response time.
- **At-least-once delivery.** A worker crash between SMTP accepting an email and the row being saved sends that email again after the lease expires.
- **Production email provider.** `SmtpEmailSender` has no authentication or STARTTLS settings beyond `EnableSsl`. A provider's HTTP API behind `IEmailSender` would also give a real `provider_id` and bounce webhooks.
- **Retention.** Sent and failed outbox rows, and consumed tokens, are never purged. database.md section 4 retention needs a Hangfire job.
- **`notifications` / `notification_preferences`** (database.md 2.8) arrive with the notifications feature. Result emails will check preferences before queuing.
- **No worker heartbeat or health check.** If the worker isn't running, emails just stay `pending`. A readiness check or a metric on the oldest pending `send_after` would catch that.
- **api.md updates** to make: `INVALID_TOKEN`, the status codes and defaults listed in section 6, and `Retry-After` / `retryAfterSec` on 429.
- **Port clash with `acgsf-mailhog`** on this machine (section 2).
- **Breached-password check** still a `TODO` in `AccountRules.ValidNewPassword`; it now applies to reset too.

## 9. Try it yourself

1. **Watch a retry happen.** Start the API and worker, stop Mailpit (`docker compose stop mailpit`), then sign up. Run the `email_outbox` query from section 2 every few seconds: `attempts` goes to 1, `last_error` says why, and `send_after` jumps 30 seconds ahead. Start Mailpit again; after the next `send_after` the row becomes `sent` and the email arrives.
2. **Add a "password changed" email.**
   1. Add `EmailTemplateNames.PasswordChanged` and a `PasswordChangedPayload(DisplayName)`.
   2. Add a `PasswordChangedTemplate` (no link, so `PayloadHoldsSecret => false`) and register it in `AddEmailDelivery`.
   3. Queue it from `ResetPasswordHandler` with `IEmailOutbox.Enqueue` before the save, so it's in the same transaction.
   4. Extend `A_pending_email_is_sent_and_marked_sent`-style tests to check that its payload is *kept* after sending.
3. **Make the resend limit configurable.** Move `ResendVerificationHandler.MaxPerHour` into an options class bound from configuration. Write a failing integration test that sets it to 1 through `builder.UseSetting` in a custom factory, then make it pass. Check that `Retry-After` still points at the oldest email in the window.
