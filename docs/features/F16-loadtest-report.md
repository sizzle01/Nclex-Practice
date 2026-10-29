# F16 load test report

**Result: the target is met on a laptop once the connection pool is capped.** With 500 concurrent
candidates, API p95 is 88 ms for autosave and 61 ms for item fetches, against the 150 ms target
(architecture.md section 7). No requests failed. The first run, with default settings, missed the
target and failed 1% of requests. The cause, and the fix now in the compose file, are below.

## What was tested

- **Script:** [loadtest/autosave-and-items.js](../../loadtest/autosave-and-items.js) (k6 1.3.0).
- **Load:** 500 virtual users. Each signs up during setup, which is not measured, starts the RN
  Sample Set and claims the device lease. Then, once a second, each user fetches an item and
  autosaves an answer to it.
- **Ramp:** up to 500 users over 1 minute, held for 3 minutes, then down over 30 seconds.
- **Thresholds:** p95 under 150 ms for `autosave` and `item`, and checks passing at over 99%.
- **Target:** the API directly (`http://host.docker.internal:8090`), not through Next.js, because
  the 150 ms target is for the API.
- **Machine:** one MacBook (8 CPUs, 8 GB for Docker). k6, the API, Postgres, Redis, the frontend, the
  worker and the OpenTelemetry dashboard all ran on it from `docker compose --profile app`, with one
  API container. Other projects' idle containers were also running.

## Results

| Run | Requests/s | Autosave p95 | Item p95 | Failed requests |
|---|---|---|---|---|
| 1. Default Npgsql pool (100 per process) | 497 | 832 ms ✗ | 787 ms ✗ | 1.07% (732 × 500, plus 404s that followed) |
| 2. `Maximum Pool Size=40` | **685** | **88 ms ✓** | **61 ms ✓** | **0%** |

In run 2 the median was 18 ms for autosave and 9 ms for items, and the slowest request took 428 ms.
CPU sampled mid-run: API 185%, Postgres 117%, k6 46%, OpenTelemetry dashboard 36%. Nothing was
saturated.

## Finding: Postgres ran out of connections

In run 1, every 500 was `53300: sorry, too many clients already`:

- Postgres allows 100 connections by default.
- Npgsql's pool allows up to 100 per process.
- Under load, the API alone tried to open more than Postgres accepts, and the worker and Hangfire
  need connections too.

The failed requests timed out and retried, which raised everyone's latency. The 404s came from
virtual users whose attempt start had failed.

**Fix:**

- The compose file now sets `Maximum Pool Size=40` in the shared connection string.
- [operations.md](../operations.md) section 2 says to always set it, and gives the sizing rule:
  (API tasks × pool) + (worker tasks × pool) + headroom must stay below `max_connections`. Or use
  RDS Proxy.

A smaller pool was also faster here: 40 connections queue requests briefly instead of contending
for row locks and CPU.

## What this does not prove

- **Not staging hardware.** On a laptop, everything shares 8 CPUs. Staging, with separate RDS and
  two API tasks, should do better, but re-run this against staging before calling the target met
  there. The command is in the script header; set `API_URL`.
- **The next item is fetched straight after each answer**, so this is heavier than a real candidate,
  who reads for 30 to 90 seconds per item.
- **Only two calls are covered.** Submit and grading, start, the SSE stream and the Next.js layer
  were not loaded. Grading is per-attempt CPU work (ADR-5), and a spike of submits at the end of a
  scheduled exam is the next scenario worth adding.
- **The sample set's first two items are multiple choice**, so payloads are small. Large drag-and-drop
  or bow-tie answers weren't tested.

## How to run it

```bash
docker compose --profile app up -d --build --wait
docker run --rm -i --add-host=host.docker.internal:host-gateway \
  -e API_URL=http://host.docker.internal:8080 -v "$PWD/loadtest:/scripts" \
  grafana/k6:1.3.0 run --summary-export /scripts/results/summary.json /scripts/autosave-and-items.js
```

Set `CANDIDATES=50` for a quick check. Each run creates `CANDIDATES` new accounts, so run it against
throwaway data only.
