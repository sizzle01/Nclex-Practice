import type { GradeDetail, ItemReveal } from "@/features/attempt/api/attemptApi";
import { toNumber } from "@/lib/format/toNumber";
import type { ItemType } from "./types";

/** How one choice fared once the answer is revealed. */
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
    else if (delta < 0 || detailValue(detail.selected) !== null) marks[detail.target] = "incorrect";
  }
  return marks;
}

/**
 * The marks keyed like the item's choices (and its rationale.perOption keys). Most graders target
 * choices already, but some grade a slot that holds one choice: matrix multiple choice grades each
 * row, drop-downs grade each blank and ordered response grades each position. For those, the
 * chosen and the expected choice inside the slot are marked.
 */
export function choiceMarks(itemType: ItemType, reveal: ItemReveal | null | undefined): Record<string, OptionMark> {
  switch (itemType) {
    case "matrix_mc":
    case "dropdown_cloze":
    case "dropdown_table":
      return slotChoiceMarks(reveal);
    case "ordered_response":
      return positionMarks(reveal);
    default:
      return optionMarks(reveal);
  }
}

/** The grade detail for one target (a row, blank, position or the calc answer), once revealed. */
export function detailFor(reveal: ItemReveal | null | undefined, target: string): GradeDetail | undefined {
  return reveal?.grade.detail.find((detail) => detail.target === target);
}

/** A detail's selected or expected value as text; the API types them loosely (string or number). */
export function detailValue(value: unknown): string | null {
  if (value === null || value === undefined) return null;
  return String(value);
}

// Matrix rows and drop-down blanks: choice ids are "slot:choice" (interfaces.md 2.5 and 2.6).
function slotChoiceMarks(reveal: ItemReveal | null | undefined): Record<string, OptionMark> {
  const marks: Record<string, OptionMark> = {};
  for (const detail of reveal?.grade.detail ?? []) {
    const selected = detailValue(detail.selected);
    const expected = detailValue(detail.expected);
    if (expected !== null) marks[`${detail.target}:${expected}`] = selected === expected ? "correct" : "missed";
    if (selected !== null && selected !== expected) marks[`${detail.target}:${selected}`] = "incorrect";
  }
  return marks;
}

// Ordered response: each detail targets the item that belongs at that position, with delta 1 when it is there.
function positionMarks(reveal: ItemReveal | null | undefined): Record<string, OptionMark> {
  const marks: Record<string, OptionMark> = {};
  for (const detail of reveal?.grade.detail ?? []) {
    marks[detail.target] = toNumber(detail.delta) > 0 ? "correct" : "incorrect";
  }
  return marks;
}
