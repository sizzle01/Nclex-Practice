import { percent } from "@/features/results/format";
import { bandLabel, performanceBand } from "../format";
import styles from "./ScorePercent.module.css";

/**
 * A score as a whole percentage, coloured red, amber or green by its band. A hidden label names the
 * band as well, because colour alone must never carry meaning (interfaces.md 4).
 */
export function ScorePercent({ value }: { value: number | string }) {
  const band = performanceBand(value);
  return (
    <span className={styles[band]} data-band={band}>
      {percent(value)}
      <span className={styles.visuallyHidden}> ({bandLabel[band]})</span>
    </span>
  );
}
