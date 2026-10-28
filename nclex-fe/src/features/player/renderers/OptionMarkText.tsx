import styles from "./renderers.module.css";
import { type OptionMark, optionMarkLabels } from "./revealMarks";

interface OptionMarkTextProps {
  mark: OptionMark | undefined;
  /** Replaces the standard wording, e.g. "✗ Wrong position" for ordered response. */
  label?: string;
}

/** The ✓/✗ note after a choice once the answer is revealed; nothing before that. */
export function OptionMarkText({ mark, label }: OptionMarkTextProps) {
  if (!mark) return null;
  // The space keeps the choice text and the mark apart in the label a screen reader announces.
  return (
    <>
      {" "}
      <span className={`${styles.mark} ${styles[mark]}`}>{label ?? optionMarkLabels[mark]}</span>
    </>
  );
}
