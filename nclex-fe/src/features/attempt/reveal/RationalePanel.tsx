import { toNumber } from "@/lib/format/toNumber";
import type { ItemReveal } from "../api/attemptApi";
import { choiceLabels } from "@/features/player/renderers/choiceLabels";
import { choiceMarks, optionMarkLabels } from "@/features/player/renderers/revealMarks";
import type { ItemType } from "@/features/player/renderers/types";
import styles from "./reveal.module.css";

interface RationalePanelProps {
  reveal: ItemReveal;
  itemType: ItemType;
  /** The item's content, which gives each "why" the text of its option, cell, blank choice or segment. */
  content: unknown;
}

/**
 * The rationale shown after the reveal (interfaces.md 5.3, RationalePanel): how the answer scored,
 * why the right answer is right, a "why" for every option (PRD 5.1) and the educational objective.
 */
export function RationalePanel({ reveal, itemType, content }: RationalePanelProps) {
  const marks = choiceMarks(itemType, reveal);
  const optionText = choiceLabels(itemType, content);
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
