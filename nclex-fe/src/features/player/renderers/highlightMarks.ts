import type { OptionMark } from "./revealMarks";

/** Reveal wording for highlight items, where "selected" means "highlighted". */
export const highlightMarkLabels: Record<OptionMark, string> = {
  correct: "✓ Correct highlight",
  incorrect: "✗ Should not be highlighted",
  missed: "Should be highlighted",
};
