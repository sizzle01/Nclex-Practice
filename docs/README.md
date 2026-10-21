# Design Documentation

Technical design for the NCLEX Practice & Mock Examination Platform, derived from the PRD (v1.0, August 2026).

| Doc | What it covers |
|---|---|
| [architecture.md](architecture.md) | System context, containers, backend modules, timer/autosave/grading/notification designs, security, NFR mapping, deployment, decisions |
| [database.md](database.md) | ERD, PostgreSQL DDL, indexes, invariants, phase 2/3 tables |
| [api.md](api.md) | REST API contract (`/api/v1`), auth model, error format, real-time stream |
| [interfaces.md](interfaces.md) | Item-type payload contracts, grader interface, frontend routes, screens and component interfaces |

## Assumptions made while designing (confirm or change)

1. **Stack (confirmed):** **Next.js** (React, TypeScript) in `nclex-fe`, **.NET 10 / ASP.NET Core** with EF Core in `nclex-be`, and **PostgreSQL 16**. Supporting pieces: **Redis 7** (distributed rate limits, real-time fan-out), **Hangfire** on Postgres (background jobs), and S3-compatible object storage with a CDN.
2. **Launch scope:** RN and PN are both modelled from day one. Content can ship RN-first without any schema change (PRD open question 30).
3. **Monetization:** a `plan_tier` / `access_tier` gate is built in, but no payment integration ships in the MVP (PRD open question 27).
4. **Readiness estimate:** it's computed from a configurable per-set `pass_threshold` and always labelled as an estimate (PRD open question 29).
5. **Email verification:** unverified users can only start sets marked `is_sample`. Every other set requires a verified email (PRD §5.7, flow 7.1).
6. **Select N scoring:** points are awarded per correct option (NCSBN style), where the PRD's §2.6 table says "0/1". A per-item override can score it all-or-nothing instead ([interfaces.md §2.3](interfaces.md#23-select_n)).
7. **Trend, chart, audio, video and graphic-option items** are modelled as a base item type plus exhibits or media, not as separate types.
