import { toNumber } from "@/lib/format/toNumber";
import type { ItemReveal } from "../api/attemptApi";
import { optionMarkLabels, optionMarks } from "@/features/player/renderers/revealMarks";
import type { Option } from "@/features/player/renderers/types";
import styles from "./reveal.module.css";

interface RationalePanelProps {
  reveal: ItemReveal;
  /** The item's content; its options, when it has them, give each "why" its option text. */
  content: unknown;
}

/**
 * The rationale shown after the reveal (interfaces.md 5.3, RationalePanel): how the answer scored,
 * why the right answer is right, a "why" for every option (PRD 5.1) and the educational objective.
 */
export function RationalePanel({ reveal, content }: RationalePanelProps) {
  const marks = optionMarks(reveal);
  const optionText = new Map(optionsOf(content).map((o) => [o.id, o.text]));
  const perOption = Object.entries(reveal.rationale.perOption);

  return (
    <section className={styles.panel} aria-label="Rationale">
      <h3 className={styles.outcome}>{outcomeText(reveal)}</h3>
      <p>{reveal.rationale.correct}</p>
      {perOption.length > 0 && (
        <>
          <h3 className={styles.subheading}>Why each option is right or wrong</h3>
          <dl className={styles.options}>
            {perOption.map(([id, why]) => (
              <div key={id} className={styles.option}>
                <dt>
                  {optionText.get(id) ?? id}
                  {marks[id] && <span className={styles.mark}> ({optionMarkLabels[marks[id]]})</span>}
                </dt>
                <dd>{why}</dd>
              </div>
            ))}
          </dl>
        </>
      )}
      <p className={styles.objective}>
        <strong>Educational objective:</strong> {reveal.educationalObjective}
      </p>
    </section>
  );
}

/** "Correct", "Partially correct" or "Incorrect", with the points, so partial credit is visible (PRD 5.6). */
export function outcomeText(reveal: ItemReveal): string {
  const points = toNumber(reveal.grade.points);
  const max = toNumber(reveal.grade.maxPoints);
  const outcome = reveal.grade.isCorrect ? "Correct" : points > 0 ? "Partially correct" : "Incorrect";
  return `${outcome}: ${formatPoints(points)} of ${formatPoints(max)} ${max === 1 ? "point" : "points"}`;
}

function formatPoints(value: number): string {
  return Number.isInteger(value) ? String(value) : value.toFixed(2);
}

function optionsOf(content: unknown): Option[] {
  const options = (content as { options?: unknown } | null)?.options;
  return Array.isArray(options) ? (options as Option[]) : [];
}
