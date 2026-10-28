import type { ItemReveal } from "../api/attemptApi";
import { toNumber } from "@/lib/format/toNumber";

/** How one option fared once the answer is revealed. */
export type OptionMark = "correct" | "incorrect" | "missed";

/** Visible text for each mark, so the result never depends on colour alone (interfaces.md section 4). */
export const optionMarkLabels: Record<OptionMark, string> = {
  correct: "✓ Correct",
  incorrect: "✗ Incorrect",
  missed: "Correct answer, not selected",
};

/**
 * The mark of each option, from the grade's per-target detail (+1 right choice, -1 wrong choice,
 * missed for a right choice left out). Multiple choice scores a wrong pick 0 rather than -1, so a
 * selected target that earned nothing is a wrong pick too.
 */
export function optionMarks(reveal: ItemReveal | null | undefined): Record<string, OptionMark> {
  const marks: Record<string, OptionMark> = {};
  for (const detail of reveal?.grade.detail ?? []) {
    const delta = toNumber(detail.delta);
    if (detail.missed) marks[detail.target] = "missed";
    else if (delta > 0) marks[detail.target] = "correct";
    else if (delta < 0 || (detail.selected !== null && detail.selected !== undefined)) marks[detail.target] = "incorrect";
  }
  return marks;
}
