"use client";

import Link from "next/link";
import ui from "@/components/ui/ui.module.css";
import { useRecentResults } from "@/features/history/api/historyApi";
import { percent } from "@/features/results/format";
import { ResultOutcome } from "@/features/results/ResultOutcome";
import styles from "./Dashboard.module.css";

/** The latest finished tests with their scores and outcomes (interfaces.md 5.2, dashboard "last results"; F23). */
export function RecentResults() {
  const { data, error, isPending } = useRecentResults();

  if (isPending) return <p className={ui.muted}>Loading your results…</p>;
  if (error)
    return (
      <p role="alert" className={styles.error}>
        Your recent results could not be loaded.
      </p>
    );
  if (data.items.length === 0) return <p className={ui.muted}>Your scores show up here once you finish a test.</p>;

  return (
    <ul className={styles.list} aria-label="Recent results">
      {data.items.map((attempt) => (
        <li key={attempt.id} className={styles.listRow}>
          <Link href={`/attempts/${attempt.id}/results`}>{attempt.testSet.title}</Link>
          <span className={styles.rowMeta}>
            <strong>{attempt.scorePct === null ? "—" : percent(attempt.scorePct)}</strong>
            {attempt.readinessLabel && <ResultOutcome label={attempt.readinessLabel} />}
            {attempt.submittedAt && <span className={ui.muted}>{new Date(attempt.submittedAt).toLocaleDateString()}</span>}
          </span>
        </li>
      ))}
    </ul>
  );
}
