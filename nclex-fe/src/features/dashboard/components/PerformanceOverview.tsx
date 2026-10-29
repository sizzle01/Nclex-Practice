"use client";

import ui from "@/components/ui/ui.module.css";
import { usePerformanceSummary } from "@/features/performance/api/performanceApi";
import { percent } from "@/features/results/format";
import { toNumber } from "@/lib/format/toNumber";
import styles from "./Dashboard.module.css";

/** Overall score and the weakest Client Needs categories across all tests (interfaces.md 5.2, dashboard). */
export function PerformanceOverview() {
  const { data, error, isPending } = usePerformanceSummary();

  if (isPending) return <p className={ui.muted}>Loading your performance…</p>;
  if (error)
    return (
      <p role="alert" className={styles.error}>
        Your performance could not be loaded.
      </p>
    );
  if (toNumber(data.attemptsCount) === 0)
    return <p className={ui.muted}>Once you finish a test, your overall score and weakest categories show up here.</p>;

  return (
    <>
      <p className={styles.headline}>
        <strong>{percent(data.accuracy)}</strong> overall on {toNumber(data.itemsAnswered)} questions from{" "}
        {toNumber(data.attemptsCount)} {toNumber(data.attemptsCount) === 1 ? "test" : "tests"}
      </p>
      {data.weakest.length > 0 ? (
        <>
          <h3 className={styles.subheading}>Weakest categories</h3>
          <ol className={styles.list}>
            {data.weakest.map((row) => (
              <li key={row.key} className={styles.listRow}>
                <span>{row.name}</span>
                <span className={styles.rowMeta}>
                  <strong>{percent(row.accuracy)}</strong>
                  <span className={ui.muted}>{toNumber(row.itemsAnswered)} questions</span>
                </span>
              </li>
            ))}
          </ol>
        </>
      ) : (
        <p className={ui.muted}>Answer a few more questions per category to see where to focus.</p>
      )}
    </>
  );
}
