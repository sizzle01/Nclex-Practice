import ui from "@/components/ui/ui.module.css";
import { ScorePercent } from "@/features/performance/components/ScorePercent";
import { type PerformanceBand, performanceBand } from "@/features/performance/format";
import { toNumber } from "@/lib/format/toNumber";
import type { AttemptResultView } from "../attempt/api/attemptApi";
import { elapsed, percent } from "./format";
import styles from "./results.module.css";

type ClinicalJudgment = NonNullable<AttemptResultView["clinicalJudgment"]>;
type StepResult = ClinicalJudgment["steps"][number];

/**
 * The words of the candidate's NCLEX performance report, so practice results read the same way. The bands are
 * F23's pass marks: below 70%, 70% to 85%, 86% and above.
 */
export const standardText: Record<PerformanceBand, string> = {
  red: "Below passing standard",
  amber: "Near passing standard",
  green: "Above passing standard",
};

/**
 * Clinical judgment results (F25): each step of the NCSBN model in order, and the first half of the test against
 * the second, with time per question. The candidate's NCLEX reports showed judgment holding early and falling away
 * late, so the halves make that pattern visible after every practice test.
 */
export function ClinicalJudgmentPanel({ judgment }: { judgment: ClinicalJudgment }) {
  const { firstHalf, secondHalf } = judgment;
  return (
    <section aria-labelledby="judgment-heading" className={ui.card}>
      <h2 id="judgment-heading" className={ui.cardTitle}>
        Clinical judgment
      </h2>
      <p className={ui.muted}>
        Each step of the clinical judgment model, and whether your reasoning held up in the second half of the test.
      </p>

      {judgment.secondHalfDrop && (
        <p role="note" className={styles.judgmentAlert}>
          Your score fell from {percent(firstHalf.pct)} in the first half to {percent(secondHalf.pct)} in the second
          half. Practice longer timed blocks to build stamina, and notice whether you rush or second-guess late in a
          test.
        </p>
      )}

      <ul className={styles.facts}>
        <li>
          First half: {toNumber(firstHalf.items)} questions, <ScorePercent value={firstHalf.pct} />,{" "}
          {elapsed(firstHalf.avgTimeMs)} a question
        </li>
        <li>
          Second half: {toNumber(secondHalf.items)} questions, <ScorePercent value={secondHalf.pct} />,{" "}
          {elapsed(secondHalf.avgTimeMs)} a question
        </li>
      </ul>

      {judgment.steps.length > 0 && (
        <div className={styles.tableScroll}>
          <table className={styles.table}>
            <caption>By clinical judgment step, in model order</caption>
            <thead>
              <tr>
                <th scope="col">Step</th>
                <th scope="col">Questions</th>
                <th scope="col">Score</th>
                <th scope="col">Standard</th>
                <th scope="col">First half</th>
                <th scope="col">Second half</th>
                <th scope="col">Avg time</th>
              </tr>
            </thead>
            <tbody>
              {judgment.steps.map((step) => (
                <tr key={step.key}>
                  <th scope="row">{step.name}</th>
                  <td>{toNumber(step.items)}</td>
                  <td>
                    <ScorePercent value={step.pct} />
                  </td>
                  <td>{standardText[performanceBand(step.pct)]}</td>
                  <td>
                    <HalfScore step={find(firstHalf.steps, step.key)} />
                  </td>
                  <td>
                    <HalfScore step={find(secondHalf.steps, step.key)} />
                  </td>
                  <td>{elapsed(step.avgTimeMs)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

function find(steps: StepResult[], key: string) {
  return steps.find((s) => s.key === key);
}

/** A step's score in one half, or a dash with a spoken "none" when that half had no question on the step. */
function HalfScore({ step }: { step: StepResult | undefined }) {
  if (!step) {
    return (
      <>
        <span aria-hidden="true">–</span>
        <span className={styles.visuallyHidden}>No questions</span>
      </>
    );
  }
  return <ScorePercent value={step.pct} />;
}
