import type { ApiSchema } from "@/lib/api/types";
import styles from "./ResultOutcome.module.css";

/** The share that passes: 86% and above (F23; ReadinessPolicy on the server). */
export const PASS_MARK = 0.86;

/** The share from which a score is close to pass; anything below fails. */
export const CLOSE_TO_PASS_MARK = 0.7;

export type ReadinessLabel = NonNullable<ApiSchema<"ReadinessLabel">>;

/**
 * The words for each readiness label (F23): 86% and above passes, 70% to 85% is close to pass,
 * below 70% fails. An adaptive exam only ever passes or fails.
 */
export const outcomeText: Record<ReadinessLabel, string> = {
  likely_ready: "Pass",
  close_to_pass: "Close to pass",
  needs_more_practice: "Fail",
};

/** Pass, Close to pass or Fail in green, amber or red. The word carries the meaning; the colour only supports it. */
export function ResultOutcome({ label, className }: { label: ReadinessLabel; className?: string }) {
  return (
    <span className={`${styles[label]} ${className ?? ""}`.trim()} data-outcome={label}>
      {outcomeText[label]}
    </span>
  );
}
