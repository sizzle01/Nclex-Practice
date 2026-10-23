"use client";

import { type HealthState, useReadiness } from "../api/readiness";
import styles from "./HealthStatus.module.css";

// Words and symbols, not colour alone, carry the state (interfaces.md section 4).
const stateLabels: Record<HealthState, string> = {
  ok: "OK",
  degraded: "Degraded",
  unhealthy: "Unavailable",
};

const stateSymbols: Record<HealthState, string> = {
  ok: "✓",
  degraded: "!",
  unhealthy: "✗",
};

function DependencyRow({ name, state }: { name: string; state: HealthState }) {
  return (
    <li className={styles[state]}>
      <span aria-hidden="true">{stateSymbols[state]}</span> {name}: {stateLabels[state]}
    </li>
  );
}

/** Shows whether the API and its dependencies are up, using GET /readyz. */
export function HealthStatus() {
  const { data, isPending, isError, refetch, isFetching } = useReadiness();

  return (
    <section aria-labelledby="health-heading">
      <h1 id="health-heading">System status</h1>

      <div role="status" aria-live="polite">
        {isPending && <p>Checking…</p>}
        {isError && <p className={styles.unhealthy}>✗ Cannot reach the API.</p>}
        {data && (
          <ul className={styles.list}>
            <DependencyRow name="Postgres" state={data.checks.postgres} />
            <DependencyRow name="Redis" state={data.checks.redis} />
          </ul>
        )}
      </div>

      <button type="button" onClick={() => refetch()} disabled={isFetching}>
        {isFetching ? "Checking…" : "Check again"}
      </button>
    </section>
  );
}
