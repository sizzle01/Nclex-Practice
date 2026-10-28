import styles from "./renderers.module.css";
import { type OptionMark, optionMarkLabels } from "./revealMarks";

/** The ✓/✗ note at the end of an option once the answer is revealed; nothing before that. */
export function OptionMarkText({ mark }: { mark: OptionMark | undefined }) {
  if (!mark) return null;
  return <span className={`${styles.mark} ${styles[mark]}`}>{optionMarkLabels[mark]}</span>;
}
