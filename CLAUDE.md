# NCLEX Practice Platform: project rules

## What we are building
An NCLEX-RN/PN practice and mock-exam web platform. The approved design is the source of truth:
- docs/architecture.md   system architecture and key designs (timers, autosave, grading, notifications)
- docs/database.md       PostgreSQL schema (DDL is authoritative for table/column names)
- docs/api.md            REST API contract (/api/v1)
- docs/interfaces.md     item-type payloads, grader interface, frontend routes/components, accessibility
Read the sections a task references before writing code. If the design is ambiguous or seems wrong,
stop and ask me. Never silently diverge from it.

## Stack
- nclex-fe: Next.js (App Router) + TypeScript (strict), TanStack Query, Zustand, react-hook-form + Zod,
  dnd-kit, Recharts. API types/client generated from the backend OpenAPI document (never hand-written).
- nclex-be: .NET 10, ASP.NET Core controllers, EF Core 10 + Npgsql, FluentValidation, Hangfire
  (PostgreSQL storage), StackExchange.Redis, Serilog + OpenTelemetry. Solution layout exactly as in
  docs/architecture.md section 4 (Nclex.Domain, Nclex.Application, Nclex.Infrastructure, Nclex.Api,
  Nclex.Worker, tests/).
- Database: PostgreSQL 16. Local services via docker compose (Postgres, Redis, MinIO, Mailpit).
- Tests: xUnit + Shouldly + Testcontainers (backend); Vitest + React Testing Library (frontend);
  Playwright (end-to-end, from F16).

## How we work: one feature at a time
1. I give you one feature (Fxx). Before writing code, reply with a short PLAN:
   files you will create/change, DB migration, endpoints, screens, tests, and any questions.
   Then STOP and wait for me to reply "go".
2. Work on a branch named feature/fxx-short-name created from main.
3. Build only what the feature lists. Anything listed under "Out of scope" waits for its own feature.
   If you notice something needed later, write it under "Follow-ups" in the walkthrough instead of
   building it.
4. Commit in small, logical steps (for example: migration, domain, application, API, frontend, tests).
   Conventional commit messages: feat(fxx): ..., test(fxx): ..., docs(fxx): ...
5. Before saying you are done: dotnet build, dotnet test, and in nclex-fe npm run lint, npm run typecheck,
   npm test, all passing. Report the actual results. Never claim something passes without running it.
6. Write the walkthrough docs/features/Fxx-short-name.md (template below), commit it, then STOP.
   Never start the next feature, and never merge to main, until I say so.

## Code style
- Optimise for a reader who is learning the codebase: clear names, small classes and functions,
  one responsibility each. Prefer straightforward code over clever code.
- Comments explain WHY (business rule, design-doc reference such as "see architecture.md 5.1"),
  not WHAT. Public domain types get XML doc comments.
- Backend: vertical slices in Nclex.Application/<Feature>/; controllers stay thin; graders and timer
  maths are pure functions in Nclex.Domain; inject TimeProvider for anything time-based; async all the way;
  errors returned as ProblemDetails with the codes in docs/api.md section 1.
- Database: snake_case naming convention; one EF Core migration per feature, named Fxx_<Name>;
  migrations must match docs/database.md DDL (types, constraints, indexes).
- Frontend: feature folders under src/features/<feature>/; server state in TanStack Query; no
  access tokens in localStorage; every interactive element keyboard-accessible (docs/interfaces.md 4).
- Security: never send answer_key or rationale to a candidate before the reveal rules allow it;
  never log passwords, tokens or answer keys.
- Do not add dependencies beyond the stack above without asking me first.

## Walkthrough template (docs/features/Fxx-short-name.md)
1. Summary: what the feature does for the user, in 3 to 5 sentences.
2. How to try it: exact commands, URLs, sample requests, and test accounts.
3. Reading order: a numbered list of files to read, in order, each with one line on its role.
4. Request flow: trace one main request end to end (UI, then API, Application, Domain, database, and
   back).
5. Database changes: tables/columns/indexes added and why.
6. Key decisions and deviations: anything that differs from the design docs and why.
7. Tests: what is covered and how to run them.
8. Follow-ups and known limitations.
9. Try it yourself: 2 or 3 small exercises to modify this feature (for learning).
