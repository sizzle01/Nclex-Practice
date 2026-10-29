# Interfaces

This document covers four things:
1. **Domain contracts:** the JSON shapes exchanged between the .NET backend and the Next.js frontend, written as TypeScript for readability. The backend mirrors them as C# records, and the frontend gets them from the generated OpenAPI client.
2. **Item-type contracts:** the content, answer-key, response and rationale payloads for every NGN item type, plus its scoring rule.
3. **Grader interface:** the plug-in boundary for auto-grading.
4. **Frontend interfaces:** routes, screens, the player and component props, and the accessibility contract.

---

## 1. Common item types

```ts
type OptionId = string;            // short, stable within a version: 'a','b',... or 'r1c2'
type Markdown = string;            // sanitized subset: bold, italic, lists, tables, sub/sup

interface Option {
  id: OptionId;
  text: Markdown;
  mediaId?: string;                // graphic-option items
}

interface Exhibit {                // makes "Trend", "Chart/Exhibit", "Audio", "Video" items
  id: string;
  kind: 'table' | 'flowsheet' | 'image' | 'audio' | 'video' | 'text';
  title: string;
  // flowsheet = trend data: columns are timestamps, rows are parameters (vitals/labs)
  table?: { columns: string[]; rows: Array<{ label: string; cells: string[] }> };
  mediaId?: string;
  body?: Markdown;
}

interface MediaView {               // GET /media/{id} (api.md section 10): how a mediaId is shown
  id: string;
  kind: 'image' | 'audio' | 'video';
  url: string;                     // signed, relative, valid for 10 minutes
  captionsUrl: string | null;      // WebVTT; required for audio and video
  transcriptUrl: string | null;    // plain text; required for audio and video (section 4, Media)
  altText: string | null;          // required for images
  width: number | null; height: number | null; durationMs: number | null;
  expiresAt: string;
}

interface CaseStudyScenario {      // case_studies.scenario (shape settled in F11)
  // one tab per record: nurses' notes, vitals, labs, orders, history
  // at least one tab; each tab has a unique id, a title and at least one exhibit.
  // The player lists these tabs before the item's own exhibits, so their ids must differ.
  tabs: Array<{ id: string; title: string; exhibits: Exhibit[] }>;
}
// Import file (F11): top-level caseStudies: [{ id, title, scenario: CaseStudyScenario }], and each
// question may carry caseStudyId + casePosition (1..6, both or neither). A case's positions run
// 1..n with no gap. A curated set that uses any item of a case has all of them, back to back, in order.

interface Rationale {
  correct: Markdown;                           // why the correct answer(s) are correct
  perOption: Record<string, Markdown>;         // REQUIRED for every option / cell / blank choice
  references?: string[];
  mediaIds?: string[];                         // diagrams in the explanation
}

interface GradeDetail {
  target: string;                  // optionId, rowId, blankId, groupId, segmentId...
  delta: 1 | 0 | -1;               // contribution to score
  selected?: string | string[];
  expected?: string | string[];
  missed?: boolean;                // correct choice not selected (delta 0)
}

interface GradeResult {
  points: number;
  maxPoints: number;
  isCorrect: boolean;              // points === maxPoints
  detail: GradeDetail[];           // drives "partial credit visibility" (PRD §5.6)
}
```

---

## 2. Item-type contracts

Each item type defines four payloads (C# records plus a validator on the backend): **Content** (sent to the client), **AnswerKey** (server only until reveal), **Response** (the candidate's answer) and a **scoring rule**. `rationale.perOption` keys must cover every selectable ID in the content.

| Type | Phase | Scoring | Max points |
|---|---|---|---|
| `multiple_choice` | MVP | 0/1 | 1 |
| `sata` | MVP | +/− (floor 0) | number of correct options |
| `select_n` | MVP | 0/1 per correct selection | N |
| `matrix_mc` | MVP | 0/1 per row | rows |
| `matrix_mr` | MVP | +/− per group (floor 0 per group) | total correct cells |
| `multiple_response_grouping` | MVP | +/− per group (floor 0 per group) | total correct options |
| `dropdown_cloze` | MVP | 0/1 per blank | blanks |
| `dropdown_table` | MVP | 0/1 per blank | blanks |
| `highlight_text` | MVP | +/− (floor 0) | correct segments |
| `highlight_table` | MVP | +/− (floor 0) | correct rows |
| `fill_blank_calc` | MVP | 0/1 (tolerance or rounding) | 1 |
| `ordered_response` | MVP | 0/1 full sequence (default) or per position | 1 or items |
| `dropdown_rationale` | P2 | Rationale (all-or-nothing per group) | groups |
| `drag_drop_cloze` | P2 | 0/1 per target | targets |
| `drag_drop_rationale` | P2 | Rationale (all-or-nothing per pair or triad) | groups |
| `bowtie` | P2 | 0/1 per well slot | 5 |
| `hotspot` | P2 | 0/1 | 1 |

### 2.1 `multiple_choice`
```ts
interface MCContent   { options: Option[] }                 // typically 4
interface MCKey       { correct: OptionId }
interface MCResponse  { selected: OptionId | null }
// points = selected === correct ? 1 : 0
```

### 2.2 `sata`
```ts
interface SATAContent  { options: Option[] }                // 5–10
interface SATAKey      { correct: OptionId[] }              // ≥ 1
interface SATAResponse { selected: OptionId[] }
// points = max(0, |selected ∩ correct| − |selected \ correct|); maxPoints = |correct|
```

### 2.3 `select_n`
```ts
interface SelectNContent  { options: Option[]; selectCount: number }
interface SelectNKey      { correct: OptionId[] }           // length === selectCount
interface SelectNResponse { selected: OptionId[] }          // validator: length ≤ selectCount
// points = |selected ∩ correct|; maxPoints = selectCount
```
> The PRD lists Select N as "0/1". This design applies 0/1 **per option**, as the NCSBN does. Setting `scoringOverride: 'all_or_nothing'` on the key scores it as a single point instead.

### 2.4 `matrix_mc` (one choice per row)
```ts
interface MatrixContent {
  rows: Array<{ id: string; text: Markdown }>;
  columns: Array<{ id: string; text: Markdown }>;
}
interface MatrixMCKey      { byRow: Record<string, string> }            // rowId → columnId
interface MatrixMCResponse { byRow: Record<string, string | null> }
// per row: 1 if equal; maxPoints = rows.length
```

### 2.5 `matrix_mr` and `multiple_response_grouping` (share one grader)
```ts
// matrix_mr: MatrixContent; groups are rows or columns (groupAxis)
interface MatrixMRContent extends MatrixContent { groupAxis: 'row' | 'column' }
// grouping: explicit groups of options
interface GroupingContent { groups: Array<{ id: string; label: Markdown; options: Option[] }> }

interface GroupedKey      { byGroup: Record<string, string[]> }        // groupId → correct cell/option ids
interface GroupedResponse { byGroup: Record<string, string[]> }
// per group: max(0, hits − falsePicks); maxPoints = Σ |byGroup[g]|
// matrix cell ids: `${rowId}:${columnId}`
```

### 2.6 `dropdown_cloze` and `dropdown_table`
```ts
interface Blank { id: string; options: Option[] }
interface DropdownClozeContent {
  template: Markdown;          // "The nurse should first {{b1}} because the client is at risk for {{b2}}."
  blanks: Blank[];
}
interface DropdownTableContent {
  columns: string[];
  rows: Array<{ id: string; cells: Array<{ text: Markdown } | { blankId: string }> }>;
  blanks: Blank[];
}
interface DropdownKey      { byBlank: Record<string, OptionId> }
interface DropdownResponse { byBlank: Record<string, OptionId | null> }
// 0/1 per blank. rationale.perOption keys: `${blankId}:${optionId}`
```
`dropdown_rationale` (phase 2) adds `rationaleGroups: string[][]` to the key, for example `[['b1','b2']]`. Each group scores 1 only if every blank in it is correct.

### 2.7 `highlight_text` and `highlight_table`
```ts
interface HighlightTextContent {
  // passage pre-tokenized by author tool; only selectable segments are clickable
  segments: Array<{ id: string; text: string; selectable: boolean }>;
}
interface HighlightTableContent {
  columns: string[];
  rows: Array<{ id: string; cells: string[]; selectable: boolean }>;
}
interface HighlightKey      { correct: string[] }          // segment or row ids
interface HighlightResponse { selected: string[] }
// +/− with floor 0; maxPoints = |correct|
```

### 2.8 `fill_blank_calc`
```ts
interface CalcContent  { unit: string; decimals: number; inputLabel?: string }   // e.g. 'mL/hr', 1
interface CalcKey      { value: number; tolerance?: number; rounding: 'exact' | 'round' }
interface CalcResponse { value: string }                    // raw input, parsed server-side
// parse → number (reject non-numeric); 'round': round(input, decimals) === round(value, decimals)
// tolerance (if set): |input − value| ≤ tolerance. 0/1.
```

### 2.9 `ordered_response`
```ts
interface OrderedContent  { items: Option[] }              // shuffled per attempt (seeded by attempt id)
interface OrderedKey      { order: OptionId[]; scoring: 'all_or_nothing' | 'per_position' }
interface OrderedResponse { order: OptionId[] }            // validator: permutation of items
```

### 2.10 Phase 2 types
```ts
// drag_drop_cloze
interface DDClozeContent  { template: Markdown; targets: Array<{ id: string }>; tokens: Option[]; reusable: boolean }
interface DDClozeKey      { byTarget: Record<string, OptionId> }
interface DDClozeResponse { byTarget: Record<string, OptionId | null> }

// drag_drop_rationale: template has 2 or 3 targets per sentence ("The client is at risk for {{t1}} as evidenced by {{t2}}")
interface DDRationaleKey  { byTarget: Record<string, OptionId>; rationaleGroups: string[][] }

// bowtie
interface BowtieContent {
  condition: Option[];        // pick 1 (center)
  actions: Option[];          // pick 2 (left)
  parameters: Option[];       // pick 2 (right)
}
interface BowtieKey      { condition: OptionId; actions: [OptionId, OptionId]; parameters: [OptionId, OptionId] }
interface BowtieResponse { condition: OptionId | null; actions: OptionId[]; parameters: OptionId[] }
// 0/1 per well slot, max 5

// hotspot
interface HotspotContent  { mediaId: string; width: number; height: number;
                            regions: Array<{ id: string; polygon: Array<[number, number]>; label: string }> } // labels for a11y
interface HotspotKey      { correctRegionId: string }
interface HotspotResponse { regionId: string | null }     // client resolves click→region; server re-validates id
```

---

## 3. Grader plug-in interface (backend, C#)

The TypeScript shapes in §1–2 are the **JSON wire contract**: they're what the frontend sees through the generated client. On the backend, each one is mirrored as an immutable C# `record` in `Nclex.Domain/ItemTypes/<Type>/`, serialised with `System.Text.Json` (camelCase). Example for SATA:

```csharp
namespace Nclex.Domain.ItemTypes.Sata;

public sealed record SataContent(IReadOnlyList<Option> Options);
public sealed record SataKey(IReadOnlyList<string> Correct);
public sealed record SataResponse(IReadOnlyList<string> Selected);

public sealed class SataModule : ItemTypeModule<SataContent, SataKey, SataResponse>
{
    public override ItemType Type => ItemType.Sata;
    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(SataContent c, SataKey k) => k.Correct.Count;

    public override GradeResult Grade(SataContent c, SataKey k, SataResponse? r)
    {
        var selected = r?.Selected.ToHashSet() ?? [];
        var correct  = k.Correct.ToHashSet();
        var detail = c.Options.Select(o => (sel: selected.Contains(o.Id), ok: correct.Contains(o.Id), o.Id) switch
        {
            (true,  true,  var id) => new GradeDetail(id, +1),
            (true,  false, var id) => new GradeDetail(id, -1),
            (false, true,  var id) => new GradeDetail(id, 0, Missed: true),
            (_, _, var id)         => new GradeDetail(id, 0),
        }).ToList();
        var points = Math.Max(0, detail.Sum(d => d.Delta));          // +/- scoring, floor 0
        return GradeResult.From(points, MaxPoints(c, k), detail);
    }
    // ValidateAuthoring / ValidateResponse omitted
}
```

Core abstractions:

```csharp
public enum Phase { Mvp, P2 }

public sealed record ValidationIssue(string Path, string Code, string Message);
public sealed record GradeDetail(string Target, int Delta, object? Selected = null,
                                 object? Expected = null, bool Missed = false);
public sealed record GradeResult(decimal Points, decimal MaxPoints, bool IsCorrect,
                                 IReadOnlyList<GradeDetail> Detail)
{
    public static GradeResult From(decimal points, decimal max, IReadOnlyList<GradeDetail> d)
        => new(points, max, points == max, d);
}

/// Non-generic face used by the registry and application services (works on raw JSON).
public interface IItemTypeModule
{
    ItemType Type { get; }
    Phase Phase { get; }
    /// Cross-field checks at authoring time: key refers to real ids, rationale.perOption covers every option.
    IReadOnlyList<ValidationIssue> ValidateAuthoring(JsonElement content, JsonElement key, Rationale rationale);
    /// Response checks on autosave: ids exist, select_n count, permutation for ordered, numeric for calc.
    IReadOnlyList<ValidationIssue> ValidateResponse(JsonElement content, JsonElement response);
    decimal MaxPoints(JsonElement content, JsonElement key);
    /// Pure and deterministic. A null response scores 0 (blank or timed out).
    GradeResult Grade(JsonElement content, JsonElement key, JsonElement? response);
    /// Per-attempt presentation, e.g. seeded option shuffle. Ids are preserved, so grading is unaffected.
    JsonElement Present(JsonElement content, string seed);
}

/// Typed base class: deserialises once, then calls the strongly-typed overrides.
public abstract class ItemTypeModule<TContent, TKey, TResponse> : IItemTypeModule
    where TResponse : class
{
    public abstract ItemType Type { get; }
    public abstract Phase Phase { get; }
    public abstract decimal MaxPoints(TContent content, TKey key);
    public abstract GradeResult Grade(TContent content, TKey key, TResponse? response);
    public virtual IReadOnlyList<ValidationIssue> ValidateAuthoring(TContent c, TKey k, Rationale r) => [];
    public virtual IReadOnlyList<ValidationIssue> ValidateResponse(TContent c, TResponse r) => [];
    public virtual TContent Present(TContent content, string seed) => content;
    // explicit IItemTypeModule implementations deserialise with ItemJson.Options and delegate
}

public sealed class ItemTypeRegistry(IEnumerable<IItemTypeModule> modules)
{
    private readonly Dictionary<ItemType, IItemTypeModule> _byType = modules.ToDictionary(m => m.Type);
    public IItemTypeModule Get(ItemType type) => _byType[type];
}

// Application layer
public interface IGradingService
{
    Task<GradeResult> GradeItemAsync(Guid attemptId, int position, CancellationToken ct);
    /// Runs inside the caller's DB transaction (attempt row already locked FOR UPDATE).
    Task<AttemptResultSummary> FinalizeAttemptAsync(Guid attemptId, SubmitReason reason, CancellationToken ct);
}

public interface IReadinessPolicy
{
    (bool? Passed, ReadinessLabel? Label) Evaluate(decimal scorePct, decimal? threshold);
}

public interface INotificationPublisher
{
    /// Adds the notification and, per the user's preferences, an email_outbox row to the current
    /// DbContext (same transaction). Publishes to Redis for SSE after SaveChanges commits.
    Task PublishAsync(NewNotification notification, CancellationToken ct);
}

public interface IJobScheduler      // wraps Hangfire so Application doesn't reference it
{
    void ScheduleAttemptExpiry(Guid attemptId, int timerEpoch, DateTimeOffset at);
    void EnqueueSnapshotRefresh(Guid userId, Guid attemptId);
}
```

Every module is registered in DI (`services.AddSingleton<IItemTypeModule, SataModule>()`). At startup, a check confirms there's a module for every `ItemType` whose phase is enabled.

**Testing contract:** each module has a `fixtures/*.json` file of `{content, key, response, expectedPoints}` cases in `Nclex.Domain.Tests`, including the PRD §2.7 examples. One xUnit `[Theory]` runs them all.

---

## 4. Accessibility contract per item type

These requirements cover WCAG 2.1 AA (PRD §6.4, §8). Every renderer must satisfy them before it ships.

| Type | Keyboard model | Screen-reader semantics | Touch |
|---|---|---|---|
| MC | Arrow keys within the radio group | `radiogroup` / `radio` | Large hit targets (44 px or more) |
| SATA, Select N | Tab between checkboxes, Space toggles | `group` / `checkbox`, plus "select N" in the description | " |
| Matrix | Arrow keys move across cells, Space selects | `table` with a `radio` or `checkbox` in each cell, row and column headers | Stacks into one card per row on mobile |
| Dropdown | Native `<select>`, or a combobox | Blank labelled with its sentence context | Native picker |
| Highlight | Tab to each selectable segment, Space toggles | `button` with `aria-pressed` | Tap to toggle |
| Calc | Numeric input, unit shown as a suffix | `aria-describedby` gives the unit and decimals | `inputmode="decimal"` |
| Ordered | Select an item, then move it with arrow keys, or use "Move up / Move down" buttons | Live region announces the new position | Tap-to-select, tap-to-place |
| Drag-drop (P2) | Same as Ordered: pick a token, then pick a target | Live region announcements | Tap-to-place (PRD §8) |
| Hotspot (P2) | Tab cycles through regions | Each region has a `label` | Tap |
| Media | Captions (WebVTT) required, transcript link | — | — |

Global requirements: visible focus rings; text scale from 100% to 200% without horizontal scroll; colour contrast of 4.5:1 or better; feedback never conveyed by colour alone (✓/✗ icons plus text); the timer is announced at 5 and 1 minutes remaining, without a live countdown that floods the screen reader.

---

## 5. Frontend (`nclex-fe`)

### 5.1 Stack

- **Next.js** (App Router) with TypeScript.
- **TanStack Query** for server state. **Zustand** for player-local state (save queue, clock offset).
- **openapi-typescript / orval** to generate the typed client from `/api/v1/openapi.json`.
- **dnd-kit** for drag-and-drop and ordered items, because it has keyboard sensors built in.
- **Recharts** for performance charts. **react-hook-form + Zod** for forms.
- **Auth:** the access token is held in memory only. A silent `/auth/refresh` runs on load and before expiry. Nothing goes in localStorage.

### 5.2 Routes and screens

| Route | Screen | Key data |
|---|---|---|
| `/` | Landing / marketing | — |
| `/signup`, `/login` | Auth forms (track selection on signup) | `/auth/*` |
| `/verify-email?token=` | Verification result | `/auth/verify-email` |
| `/forgot-password`, `/reset-password?token=` | Password reset | `/auth/password/*` |
| `/dashboard` | Drafts to resume (with % complete and time left), last results, weakest categories, verify-email banner | `/attempts?status=…`, `/me/performance/summary` |
| `/practice` | Prebuilt sets (Practice, Challenge, Sample), locked badges | `/test-sets?kind=prebuilt` |
| `/practice/custom` | Custom test builder with live available count | `/taxonomy`, `/test-sets/custom/*` |
| `/attempts/[id]` | **Exam player** | `/attempts/{id}/*` |
| `/attempts/[id]/results` | Score, readiness estimate, category breakdown, time per item | `/attempts/{id}/result` |
| `/attempts/[id]/review` | Item-by-item rationale review, filter incorrect / flagged | `/attempts/{id}/review` |
| `/performance` | Breakdown by dimension, trend charts | `/me/performance/*` |
| `/history` | Past attempts table | `/attempts?status=submitted` |
| `/notifications` | Full notification history | `/notifications` |
| `/settings/account` | Profile, track, change email or password, delete account | `/me*` |
| `/settings/notifications` | Per-category in-app / email toggles | `/me/notification-preferences` |
| `/settings/reports` | "My reports": questions I reported, their status and the resolution note (added in F15) | `/me/flags` |
| `/admin/questions` | Question search and list | `/admin/questions` |
| `/admin/questions/[id]` | Version history plus editor (item-type form + live preview + preview-grade) | `/admin/question-*` |
| `/admin/review` | Reviewer queue (`in_review`) | `/admin/questions?status=in_review` |
| `/admin/flags` | Moderation queue | `/admin/flags` |
| `/admin/test-sets`, `/admin/banks` | Curated sets and banks | `/admin/test-sets`, `/admin/question-banks` |

App shell (all authenticated pages): top bar with the track badge, the **notification bell** (unread badge fed by SSE) and the user menu.

### 5.3 Exam player layout

```
┌───────────────────────────────────────────────────────────────────────┐
│ Comprehensive Challenge 1      Q 12 / 75     ⏱ 01:43:20   ⏸ Pause  ⋯ │  TopBar (exam timer; item timer chip when enabled)
├───────────────────────────────────────────────────────────────────────┤
│ [Case study tabs: Nurses' Notes | Vitals | Labs]   (only for cases)   │  ExhibitPanel (split view ≥1024px, drawer on mobile)
│                                                                       │
│ Stem text …                                                           │
│ ☐ A …                                                                 │  ItemRenderer (by itemType)
│ ☑ B …                                                                 │
│ ☐ C …                                                                 │
│                                                                       │
│ ── Rationale (tutor mode, after Submit) ───────────────────────────── │  RationalePanel
├───────────────────────────────────────────────────────────────────────┤
│ ⚑ Mark for review   ⚐ Report issue   ✓ Saved     ◀ Prev   Submit / Next ▶ │  BottomBar
└───────────────────────────────────────────────────────────────────────┘
  Navigator drawer: grid of positions coloured by state, with flag markers
  Pause overlay: hides all content; "Resume" + pauses remaining
  Save & Exit in ⋯ menu
```

### 5.4 Player component interfaces

```ts
/** Every item renderer implements this; registry keyed by itemType. */
interface ItemRendererProps<C, R> {
  content: C;
  response: R | null;
  onChange(next: R | null): void;        // feeds the save queue; never calls the API directly
  disabled: boolean;                     // locked / paused / expired
  struckOptions: string[];
  onToggleStrike(optionId: string): void;
  reveal: Reveal | null;                 // when present, renderer shows ✓/✗ marks per target
  labelledBy: string;                    // id of the stem element
}
const renderers: Record<ItemType, React.ComponentType<ItemRendererProps<any, any>>>;

/** Client-side save queue (Zustand store). */
interface SaveQueue {
  clientId: string;                                   // generated on player mount
  enqueue(position: number, response: unknown): void; // debounced 750ms, coalesced per position
  flush(): Promise<void>;                             // on navigate, pause, submit, pagehide
  status: 'idle' | 'saving' | 'saved' | 'offline' | 'error';
  pending: Record<number, { response: unknown; clientSeq: number }>;  // mirrored to localStorage
}

/** Server-synced clock. */
interface AttemptClock {
  offsetMs: number;                                   // serverNow − Date.now() at last response
  remaining(deadlineAt: string | null, fallbackMs: number): number;
}

interface PlayerController {
  open(): Promise<AttemptState>;                      // POST /open, claims lease
  goTo(position: number): Promise<void>;              // flush → POST /navigate → prefetch next item
  submitItem(): Promise<Reveal>;                      // tutor mode
  pause(reason: 'break' | 'save_exit'): Promise<void>;
  resume(): Promise<void>;
  submitAttempt(): Promise<void>;                     // flush → POST /submit → route to results
}
```

**Player behaviour rules:**
- When the clock reaches zero, the UI disables input and calls `submitAttempt()`. If the server already finalized the attempt (`ATTEMPT_TIME_EXPIRED`), the player routes to results.
- On `ATTEMPT_OPENED_ELSEWHERE`, a blocking dialog says "This attempt is open on another device" and offers a **Continue here** button.
- On going offline, the banner reads "Offline – answers are saved on this device and will sync". The timer keeps counting down from the last server deadline.
- The next item is prefetched with `GET /items/{n+1}` once the current one renders, to stay inside the 1-second render target.
- Before submitting, the player shows a confirmation dialog listing unanswered and flagged counts. It's built into the page; there's no browser `confirm()`.

### 5.5 Notification bell

```ts
interface NotificationCenterState {
  unreadCount: number;
  items: Notification[];          // first page loaded when the bell opens
  connect(): void;                // POST /stream-ticket → EventSource; backoff reconnect; 60s polling fallback
  markRead(id: string): Promise<void>;
  markAllRead(): Promise<void>;
}
```
Clicking a `result_pass` or `result_fail` notification marks it read and opens `data.deepLink`, which is the results page anchored at `#breakdown` (PRD flow 7.5).

### 5.6 Notification copy (templates)

| Type | Title | Body |
|---|---|---|
| `result_pass` | "You passed your mock exam — great work!" | "You scored {pct}% on {set}. Review your rationales to lock it in." |
| `result_fail` | "You didn't reach the passing threshold this time" | "You scored {pct}%. Here's what to focus on: {weakest1}, {weakest2}." |
| `result_ready` | "Your results are ready" | "You scored {pct}% on {set}." (sets with no threshold) |
| `draft_reminder` | "Pick up where you left off" | "{set} is {pctComplete}% done with {timeLeft} left." |
| `flag_resolved` | "Thanks — we reviewed your report" | "{resolutionNote}" |
| `security` | "Your password was changed" / "New sign-in" | Device, approximate location, time, and a "Not you?" link |
