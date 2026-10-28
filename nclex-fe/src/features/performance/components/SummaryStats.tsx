import { toNumber } from "@/lib/format/toNumber";
import { elapsed, percent } from "@/features/results/format";
import type { PerformanceSummary } from "../api/performanceApi";
import styles from "./performance.module.css";

/** The headline numbers across every finished test, as a description list of stat tiles. */
export function SummaryStats({ summary }: { summary: PerformanceSummary }) {
  return (
    <dl className={styles.stats}>
      <div className={styles.stat}>
        <dt>Overall score</dt>
        <dd>{percent(summary.accuracy)}</dd>
      </div>
      <div className={styles.stat}>
        <dt>Questions answered</dt>
        <dd>{toNumber(summary.itemsAnswered)}</dd>
      </div>
      <div className={styles.stat}>
        <dt>Tests finished</dt>
        <dd>{toNumber(summary.attemptsCount)}</dd>
      </div>
      <div className={styles.stat}>
        <dt>Avg time per question</dt>
        <dd>{elapsed(summary.avgTimeMs)}</dd>
      </div>
    </dl>
  );
}
