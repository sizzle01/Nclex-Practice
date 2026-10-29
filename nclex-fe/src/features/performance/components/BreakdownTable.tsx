import { toNumber } from "@/lib/format/toNumber";
import { elapsed, percent } from "@/features/results/format";
import type { BreakdownRow } from "../api/performanceApi";
import styles from "./performance.module.css";

/** Every category of one dimension across all finished tests. */
export function BreakdownTable({ title, column, rows }: { title: string; column: string; rows: BreakdownRow[] }) {
  return (
    <div className={styles.tableScroll}>
      <table className={styles.table}>
        <caption>{title}</caption>
        <thead>
          <tr>
            <th scope="col">{column}</th>
            <th scope="col">Questions</th>
            <th scope="col">Fully correct</th>
            <th scope="col">Score</th>
            <th scope="col">Avg time</th>
            <th scope="col">Tests</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.key}>
              <th scope="row">{row.name}</th>
              <td>{toNumber(row.itemsAnswered)}</td>
              <td>{toNumber(row.itemsCorrect)}</td>
              <td>{percent(row.accuracy)}</td>
              <td>{elapsed(row.avgTimeMs)}</td>
              <td>{toNumber(row.attemptsCount)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
