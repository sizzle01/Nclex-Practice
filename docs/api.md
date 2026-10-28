# API Specification (v1)

REST over HTTPS with JSON bodies. Base path: `/api/v1`. The ASP.NET Core backend generates an OpenAPI document at `/api/v1/openapi.json` with `Microsoft.AspNetCore.OpenApi`, and the Next.js frontend's typed client is generated from it (ADR-8). JSON uses `System.Text.Json` web defaults (camelCase), and enums are serialized as `snake_case` strings with `JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)`.

## 1. Conventions

| Topic | Convention |
|---|---|
| Auth | `Authorization: Bearer <accessToken>` (JWT, 15 min). Refresh token is in an `HttpOnly` cookie scoped to `/api/v1/auth`. |
| Casing | JSON fields use `camelCase`. Enum values use `snake_case`, matching the database. |
| IDs | UUID strings. |
| Time | Timestamps are ISO-8601 UTC strings. Durations are integer milliseconds (`…Ms`) or seconds (`…Sec`). |
| Pagination | Cursor-based: `?limit=20&cursor=<opaque>` → `{ items: [...], nextCursor: string \| null }`. |
| Idempotency | `POST` endpoints that create things accept an `Idempotency-Key` header, cached for 24 hours. Submit and autosave are naturally idempotent. |
| Attempt lease | Every attempt mutation must send an `X-Attempt-Client: <clientId>` header. |
| Rate limits | `429` responses include `Retry-After`. |
| Shared schemas | Shown below in TypeScript, the shape the frontend sees. The backend defines matching C# records. |
| Versioning | Breaking changes go in `/api/v2`. Additive changes don't need a new version. |

### Error format (RFC 7807-style)

```json
{
  "type": "https://errors.nclex-practice.app/attempt-time-expired",
  "status": 409,
  "code": "ATTEMPT_TIME_EXPIRED",
  "title": "The time limit for this attempt has passed.",
  "detail": "The attempt was auto-submitted at 2026-10-13T14:02:11Z.",
  "requestId": "req_01J…",
  "errors": [ { "field": "password", "code": "TOO_SHORT" } ]
}
```

| Code | HTTP | Meaning |
|---|---|---|
| `VALIDATION_FAILED` | 400 | Body or query failed validation (see `errors[]`) |
| `UNAUTHENTICATED` | 401 | Missing or expired access token |
| `INVALID_CREDENTIALS` | 401 | Login failed. The message is generic. |
| `ACCOUNT_LOCKED` | 423 | Too many failed logins. `retryAfterSec` is included. |
| `EMAIL_NOT_VERIFIED` | 403 | A non-sample attempt requires a verified email |
| `FORBIDDEN` | 403 | Wrong role, or the resource isn't yours |
| `PLAN_UPGRADE_REQUIRED` | 403 | Premium set or bank |
| `NOT_FOUND` | 404 | |
| `EMAIL_TAKEN` | 409 | |
| `ATTEMPT_OPENED_ELSEWHERE` | 409 | Another client holds the lease. Call `/open` to take over. |
| `ATTEMPT_NOT_ACTIVE` | 409 | The attempt is submitted, abandoned or paused |
| `ATTEMPT_PAUSED` | 423 | Item content is hidden while paused |
| `ATTEMPT_TIME_EXPIRED` | 409 | The exam timer ran out. The attempt has been finalized. |
| `ITEM_LOCKED` | 409 | Item already submitted (tutor mode) or its item timer expired |
| `BACK_NAVIGATION_DISABLED` | 409 | The set doesn't allow going back |
| `PAUSE_LIMIT_REACHED` | 409 | |
| `INSUFFICIENT_QUESTIONS` | 422 | The builder found fewer items than requested. `available` is included. |
| `RATE_LIMITED` | 429 | |

### Shared schemas

```ts
type Track = 'RN' | 'PN';
type ItemType = /* see interfaces.md §2 */ string;

interface TimerState {
  serverNow: string;                 // for client clock-offset
  exam: null | {                     // null = untimed exam
    running: boolean;
    deadlineAt: string | null;       // set while running
    remainingMs: number;             // authoritative when paused; informational when running
  };
  item: null | { running: boolean; deadlineAt: string | null; remainingMs: number };
  pause: { used: number; max: number | null; maxPauseSec: number | null; startedAt: string | null };
}
```

---

## 2. Auth and account

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/auth/signup` | – | Create account, send verification email, sign in |
| POST | `/auth/login` | – | Email + password login |
| POST | `/auth/refresh` | cookie | Rotate the refresh token and issue a new access token |
| POST | `/auth/logout` | cookie | Revoke the current session |
| POST | `/auth/logout-all` | bearer | Revoke all sessions |
| POST | `/auth/verify-email` | – | Consume a verification token |
| POST | `/auth/verify-email/resend` | bearer | Resend verification email (rate-limited to 3 per hour) |
| POST | `/auth/password/forgot` | – | Always returns `202` |
| POST | `/auth/password/reset` | – | Consume a reset token |
| POST | `/auth/email/confirm-change` | – | Consume a change-email token |
| GET | `/me` | bearer | Current profile |
| PATCH | `/me` | bearer | Update displayName, track, timezone, targetExamDate |
| POST | `/me/password` | bearer | Change password (re-authenticate) |
| POST | `/me/email` | bearer | Request email change (re-authenticate) |
| DELETE | `/me` | bearer | Delete account (re-authenticate) |

**`POST /auth/signup`**
```json
// request
{ "email": "ada@example.com", "password": "••••••••••", "track": "RN", "displayName": "Ada" }
// 201
{
  "user": { "id": "…", "email": "ada@example.com", "emailVerified": false, "role": "candidate",
            "track": "RN", "planTier": "free", "displayName": "Ada" },
  "accessToken": "eyJ…", "accessTokenExpiresAt": "2026-10-13T14:15:00Z"
}
// + Set-Cookie: rt=…; HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth; Max-Age=2592000
```
Validation: email in RFC 5322 format, at most 254 characters. Password must be 10–128 characters and not on the breached-password list.

**`POST /auth/login`** takes `{ email, password }` and returns the same `200` body as signup. Failures return `401 INVALID_CREDENTIALS` or `423 ACCOUNT_LOCKED`. A login from a new device creates a `security` notification.

**`POST /auth/refresh`** needs the `X-Requested-With: fetch` header. It returns `{ accessToken, accessTokenExpiresAt }` and sets a rotated cookie. If a revoked refresh token is reused, the whole session family is revoked and the call returns `401`.

**`POST /auth/password/reset`** takes `{ token, newPassword, signOutEverywhere: true }` and returns `204`. It also sends a `security` notification and email.

**`POST /me/password`** takes `{ currentPassword, newPassword }` and returns `204`.
**`POST /me/email`** takes `{ currentPassword, newEmail }` and returns `202`. A confirmation link goes to the new address and a notice goes to the old one.

---

## 3. Catalog (taxonomy, banks, test sets)

| Method | Path | Description |
|---|---|---|
| GET | `/taxonomy?track=RN` | Client-needs tree, body systems, item types (with MVP availability), CJMM skills |
| GET | `/question-banks?track=RN` | Banks visible to the user, with item counts and lock state |
| GET | `/test-sets?track=RN&kind=prebuilt` | Curated sets (Practice, Challenge, Sample) |
| GET | `/test-sets/{id}` | Set details and settings |
| GET | `/test-sets/custom/available-count?…filters` | Live pool size for the builder |
| POST | `/test-sets/custom` | Generate a custom set |

**`GET /taxonomy?track=RN` → 200**
```json
{
  "clientNeeds": [
    { "id": 1, "code": "SECE", "name": "Safe & Effective Care Environment", "children": [
      { "id": 2, "code": "SECE.MOC", "name": "Management of Care", "weightMin": 15, "weightMax": 21 },
      { "id": 3, "code": "SECE.SIC", "name": "Safety and Infection Control", "weightMin": 10, "weightMax": 16 } ] }
  ],
  "bodySystems": [ { "id": 1, "slug": "cardiovascular", "name": "Cardiovascular" } ],
  "itemTypes": [ { "type": "sata", "label": "Select All That Apply", "available": true } ],
  "cjmmSkills": [ { "key": "recognize_cues", "label": "Recognize cues", "prompt": "What matters most?" } ]
}
```

**`GET /test-sets?kind=prebuilt` → 200**
```json
{ "items": [ {
  "id": "…", "title": "Comprehensive Challenge Exam 1", "mode": "challenge", "revealMode": "exam",
  "questionCount": 75, "timed": true, "timeBudgetSec": 9000, "perQuestionSec": null,
  "passThreshold": 0.65, "isSample": false, "accessTier": "free", "locked": false,
  "myLastAttempt": { "id": "…", "scorePct": 0.71, "submittedAt": "…" }
} ], "nextCursor": null }
```

**`POST /test-sets/custom`**
```json
// request
{
  "track": "RN",
  "clientNeedsIds": [2, 7],          // leaf or parent IDs; parent expands to leaves
  "bodySystemIds": [],               // empty = any
  "itemTypes": ["sata", "matrix_mc"],
  "difficulties": ["moderate", "difficult"],
  "pool": "unused",                  // all | unused | incorrect | flagged
  "questionCount": 30,               // 5–150
  "mode": "practice",                // practice | challenge
  "revealMode": "tutor",             // tutor | exam
  "timing": { "timed": true, "perQuestionSec": null, "timeBudgetSec": 3600 },
  "startNow": true
}
// 201
{ "testSet": { "id": "…", "questionCount": 30, "...": "..." },
  "attempt": { /* AttemptState, only when startNow=true */ } }
// 422 INSUFFICIENT_QUESTIONS { "available": 18 }
```

---

## 4. Attempts (session engine)

| Method | Path | Description |
|---|---|---|
| POST | `/attempts` | Start an attempt for a test set |
| GET | `/attempts?status=in_progress,paused` | Drafts list (dashboard) |
| GET | `/attempts?status=submitted` | History (paginated) |
| GET | `/attempts/{id}` | Attempt state (no item content) |
| POST | `/attempts/{id}/open` | Claim the device lease and resume the item timer (call when the player mounts) |
| GET | `/attempts/{id}/items/{position}` | Item for display, plus the rationale if revealed |
| PUT | `/attempts/{id}/items/{position}/response` | Autosave the response |
| PATCH | `/attempts/{id}/items/{position}` | Toggle mark-for-review, strike-through |
| POST | `/attempts/{id}/items/{position}/submit` | Tutor mode: lock, grade, reveal |
| POST | `/attempts/{id}/navigate` | Move to a position (starts and stops item timers) |
| POST | `/attempts/{id}/pause` | Pause or Save & Exit |
| POST | `/attempts/{id}/resume` | Resume from pause |
| POST | `/attempts/{id}/submit` | Finish and grade (idempotent) |
| DELETE | `/attempts/{id}` | Discard a draft (marks it `abandoned`) |
| GET | `/attempts/{id}/result` | Score summary and category breakdown |
| GET | `/attempts/{id}/review` | Item-by-item review with keys and rationales |

### AttemptState

```ts
interface AttemptState {
  id: string;
  testSet: { id: string; title: string };
  status: 'in_progress' | 'paused' | 'submitted' | 'abandoned';
  settings: {
    mode: 'practice' | 'challenge' | 'cat';
    revealMode: 'tutor' | 'exam';
    allowBackNav: boolean;
    perQuestionSec: number | null;
    timeBudgetSec: number | null;
    passThreshold: number | null;
  };
  totalItems: number;
  currentPosition: number;
  progress: { answered: number; locked: number; flagged: number };
  navigator: Array<{ position: number; state: 'unseen'|'seen'|'answered'|'locked'; flagged: boolean }>;
  timer: TimerState;
  startedAt: string;
  lastActivityAt: string;
  staleDraft: boolean;              // lastActivityAt > 30 days → UI prompts resume/discard
  resultAvailable: boolean;
}
```

**`POST /attempts`**
```json
// request
{ "testSetId": "…" }
// 201 AttemptState. Exam timer is NOT running yet; it starts on /open.
```
Errors: `EMAIL_NOT_VERIFIED`, `PLAN_UPGRADE_REQUIRED`. If the user already has an active draft of the same prebuilt set, the call returns `200` with that draft instead of creating a duplicate.

**`POST /attempts/{id}/open`**
Headers: `X-Attempt-Client: c-7f3a…`, a fresh ID per page load. It takes over the lease from any other device. If the attempt is `paused` it stays paused (the UI shows the overlay), but if it is `in_progress` the timers resume. Returns `200 AttemptState`.

**`GET /attempts/{id}/items/{position}` → 200**
```json
{
  "position": 12,
  "questionVersionId": "…",
  "itemType": "sata",
  "stem": "The nurse is caring for a client with … Select all that apply.",
  "content": { "options": [ { "id": "a", "text": "…" }, { "id": "b", "text": "…" } ] },
  "exhibits": [],
  "caseStudy": null,
  "state": "answered",
  "response": { "selected": ["a", "d"] },
  "clientSeq": 41,
  "flagged": false,
  "struckOptions": ["c"],
  "timeAllotmentSec": 120,
  "reveal": null
}
```
`reveal` is non-null only when the item is locked in tutor mode, or when the attempt is submitted:
```json
"reveal": {
  "answerKey": { "correct": ["a", "b", "e"] },
  "grade": { "points": 1, "maxPoints": 3, "isCorrect": false,
             "detail": [ { "target": "a", "delta": 1 }, { "target": "d", "delta": -1 }, { "target": "b", "delta": 0, "missed": true } ] },
  "rationale": {
    "correct": "…why A, B, E are correct…",
    "perOption": { "a": "…", "b": "…", "c": "…", "d": "…", "e": "…" }
  },
  "educationalObjective": "…",
  "tags": { "clientNeeds": "SECE.SIC", "bodySystem": "respiratory", "cjmmSkill": "take_action", "difficulty": "moderate" }
}
```
Returns `423 ATTEMPT_PAUSED` while paused, because content is hidden during a pause (PRD §5.4).

**`PUT /attempts/{id}/items/{position}/response`** (autosave)
```json
// request
{ "response": { "selected": ["a", "d"] }, "clientSeq": 42 }
// 200
{ "savedAt": "…", "clientSeq": 42, "state": "answered", "timer": { /* TimerState */ } }
```
- A stale `clientSeq` (≤ the stored value) returns `200` with the stored `clientSeq` and changes nothing.
- The response is validated against the item type's response schema, which returns `400` if it's malformed.
- `response: null` clears the answer.
- Errors: `ITEM_LOCKED`, `ATTEMPT_TIME_EXPIRED`, `ATTEMPT_OPENED_ELSEWHERE`, `ATTEMPT_PAUSED`.

**`PATCH /attempts/{id}/items/{position}`** takes `{ "flagged": true, "struckOptions": ["c"] }` and returns `200`.

**`POST /attempts/{id}/items/{position}/submit`** (tutor mode only; returns `409` in exam mode)
Takes `{ "response": {…}, "clientSeq": 43 }`, which is optional and saves a final response before locking. Returns `200 { "reveal": { … } }`.

**`POST /attempts/{id}/navigate`**
```json
{ "toPosition": 13 }
// 200 { "currentPosition": 13, "timer": { … } }
```
When `allowBackNav=false` and `toPosition < currentPosition`, it returns `409 BACK_NAVIGATION_DISABLED`. In that mode, moving forward also locks the item being left.

**`POST /attempts/{id}/pause`**
```json
{ "reason": "break" }          // break | save_exit
// 200 { "status": "paused", "timer": { … } }
```
Returns `409 PAUSE_LIMIT_REACHED` when the pause limit is used up.

**`POST /attempts/{id}/resume`** returns `200 AttemptState`. If the pause ran past `maxPauseSec` in challenge mode, the extra time has already been deducted (architecture §5.1).

**`POST /attempts/{id}/submit`**
```json
// request (optional final flush)
{ "finalResponses": [ { "position": 75, "response": { … }, "clientSeq": 12 } ] }
// 200
{ "attemptId": "…", "status": "submitted", "submitReason": "user", "result": { /* AttemptResultSummary */ } }
```
Repeated calls return the same result.

**`GET /attempts/{id}/result` → 200**
```json
{
  "attemptId": "…", "title": "Comprehensive Challenge Exam 1", "submittedAt": "…", "submitReason": "user",
  "score": { "pointsEarned": 68.0, "maxPoints": 92.0, "pct": 0.739,
             "itemsCorrect": 52, "itemsAnswered": 74, "itemsTotal": 75 },
  "time": { "totalMs": 6120000, "avgPerItemMs": 81600 },
  "readiness": { "passThreshold": 0.65, "passed": true, "label": "likely_ready",
                 "disclaimer": "This is an estimate based on your practice score, not a prediction of your NCLEX result." },
  "breakdown": {
    "client_needs": [ { "key": "SECE.MOC", "name": "Management of Care", "items": 12, "correct": 8,
                        "pct": 0.70, "avgTimeMs": 90000 } ],
    "client_needs_parent": [ … ], "body_system": [ … ], "item_type": [ … ], "cjmm_skill": [ … ]
  },
  "weakest": [ { "dimension": "client_needs", "key": "PI.PPT", "name": "Pharmacological & Parenteral Therapies", "pct": 0.48 } ]
}
```

**`GET /attempts/{id}/review?position=&filter=incorrect|flagged|all`** returns paginated items in the same shape as the item endpoint, with `reveal` always present. Only available for submitted attempts, or for locked items in tutor mode.

---

## 5. Performance and analytics

| Method | Path | Description |
|---|---|---|
| GET | `/me/performance/summary?track=RN` | Totals, overall accuracy, avg time, streak, items answered |
| GET | `/me/performance/breakdown?dimension=client_needs` | Snapshot per category (accuracy, avg time, count) |
| GET | `/me/performance/trend?dimension=client_needs&key=PI.PPT&bucket=week&from=&to=` | Time series for charts |

```json
// GET /me/performance/trend?dimension=overall&bucket=week
{ "series": [ { "bucketStart": "2026-09-07", "items": 120, "pct": 0.62, "avgTimeMs": 84000 },
              { "bucketStart": "2026-09-14", "items": 210, "pct": 0.68, "avgTimeMs": 79000 } ] }
```

---

## 6. Notifications

| Method | Path | Description |
|---|---|---|
| GET | `/notifications?cursor=&unreadOnly=false` | Feed, newest first |
| GET | `/notifications/unread-count` | `{ "count": 3 }` |
| POST | `/notifications/{id}/read` | Mark one read |
| POST | `/notifications/read-all` | Mark all read |
| POST | `/notifications/stream-ticket` | `{ "ticket": "…", "expiresInSec": 30 }` (EventSource can't send headers) |
| GET | `/notifications/stream?ticket=…` | SSE stream |
| GET | `/me/notification-preferences` | Preferences per category |
| PUT | `/me/notification-preferences` | Replace preferences |

**Notification object**
```json
{
  "id": "…", "type": "result_fail", "category": "results",
  "title": "You didn't reach the passing threshold this time",
  "body": "You scored 58%. Focus next on Pharmacological & Parenteral Therapies and Reduction of Risk Potential.",
  "data": { "scorePct": 0.58, "weakest": ["PI.PPT", "PI.RRP"], "deepLink": "/attempts/…/results#breakdown" },
  "relatedAttemptId": "…", "readAt": null, "createdAt": "…"
}
```

**SSE events**
```
event: notification
data: { …Notification… }

event: unread_count
data: { "count": 4 }

event: ping            (every 25s keep-alive)
data: {}
```

**Preferences**
```json
{ "preferences": [
  { "category": "results",    "inApp": true, "email": true },
  { "category": "reminders",  "inApp": true, "email": false },
  { "category": "engagement", "inApp": true, "email": false },
  { "category": "content",    "inApp": true, "email": false },
  { "category": "security",   "inApp": true, "email": true, "locked": true }
] }
```

---

## 7. Content feedback (flags)

| Method | Path | Description |
|---|---|---|
| POST | `/question-versions/{id}/flags` | Report an issue, `{ reason, comment?, attemptId? }` |
| GET | `/me/flags` | My reports and their status |

Returns `409` for a duplicate (same user, version and reason).

---

## 8. Admin and content (role-gated)

Roles: **A** = author, **R** = reviewer, **Ad** = admin.

| Method | Path | Roles | Description |
|---|---|---|---|
| GET | `/admin/questions?track=&status=&itemType=&clientNeedsId=&q=` | A R Ad | Search questions |
| POST | `/admin/questions` | A Ad | Create a question with version 1 as a draft |
| GET | `/admin/questions/{id}` | A R Ad | Question with all versions (full payloads, including the key) |
| POST | `/admin/questions/{id}/versions` | A Ad | New draft version, cloned from the current one |
| PUT | `/admin/question-versions/{id}` | A Ad | Edit a draft (validated against the item-type schema) |
| POST | `/admin/question-versions/{id}/submit-for-review` | A Ad | draft → in_review |
| POST | `/admin/question-versions/{id}/approve` | R Ad | in_review → approved (reviewer ≠ author) |
| POST | `/admin/question-versions/{id}/reject` | R Ad | in_review → rejected, `{ notes }` |
| POST | `/admin/question-versions/{id}/publish` | R Ad | approved → published (supersedes the previous version) |
| POST | `/admin/questions/{id}/retire` | R Ad | Remove from future sets |
| POST | `/admin/question-versions/{id}/preview-grade` | A R Ad | `{ response }` → GradeResult (test the key) |
| POST | `/admin/media/upload-url` | A Ad | Presigned S3 PUT `{ mimeType, bytes }` → `{ mediaId, uploadUrl }` |
| PATCH | `/admin/media/{id}` | A Ad | Alt text, captions |
| GET / POST / PATCH | `/admin/test-sets[/{id}]` | Ad | Manage prebuilt sets |
| GET / POST / PATCH | `/admin/question-banks[/{id}]` | Ad | Manage banks and membership |
| GET | `/admin/flags?status=open` | R Ad | Moderation queue |
| PATCH | `/admin/flags/{id}` | R Ad | `{ status, resolutionNote, assigneeId }`. Resolving notifies the reporter. |
| GET | `/admin/users?q=` / PATCH `/admin/users/{id}` | Ad | Role, plan tier, unlock |

---

## 9. Operations

| Method | Path | Description |
|---|---|---|
| GET | `/healthz` | Liveness |
| GET | `/readyz` | Readiness (Postgres, and Redis as degraded-OK) |
| POST | `/webhooks/email` | Provider bounce and complaint events (signed) |

---

## 10. Media (added in F11)

Question media (images, audio, video) are referenced by `mediaId` in exhibits and graphic options (interfaces.md section 1). The browser loads them with `<img>`, `<audio>` and `<video>`, which can't send the in-memory access token, so the API hands out **signed, short-lived links** instead. A link works for 10 minutes, and the signature covers the asset id, the part (file or captions) and the expiry, like a presigned S3 URL.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/media/{id}` | bearer | Kind, alt text, size, and signed links to the file and its captions |
| GET | `/media/{id}/file?exp=&sig=` | signed link | The image, audio or video file. Range requests are supported, so clips can seek. |
| GET | `/media/{id}/captions?exp=&sig=` | signed link | The WebVTT captions (`text/vtt`) of an audio or video clip |

**`GET /media/{id}` → 200**
```json
{
  "id": "f11a0000-0000-4000-8000-000000000002",
  "kind": "audio",
  "url": "/api/v1/media/f11a…0002/file?exp=1793180000&sig=Xq2…",
  "captionsUrl": "/api/v1/media/f11a…0002/captions?exp=1793180000&sig=9fA…",
  "altText": null,
  "width": null, "height": null, "durationMs": 4000,
  "expiresAt": "2026-10-28T13:43:20Z"
}
```
- The links are relative, so they resolve against the app's own origin, where the Next.js server forwards `/api` to the API.
- File responses carry `Cache-Control: private, max-age=600`.
- Errors:
  - `404 NOT_FOUND`: unknown asset, or no captions.
  - `403 FORBIDDEN`: a link that is changed, expired or unsigned. The player then fetches fresh links once.
- Not yet: premium gating of media. Any signed-in user can get links for any asset id.
