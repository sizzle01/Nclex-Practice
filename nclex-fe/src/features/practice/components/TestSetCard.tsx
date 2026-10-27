import ui from "@/components/ui/ui.module.css";
import type { TestSetSummary } from "../api/practiceApi";
import { modeLabel, questionCountLabel, timingLabel } from "../format";
import styles from "./practice.module.css";

interface TestSetCardProps {
  set: TestSetSummary;
  /** True while this set's details are open. */
  selected: boolean;
  /** Id of the detail panel the "View details" button opens. */
  panelId: string;
  onOpen: (id: string) => void;
}

/** The id of a card's "View details" button, so focus can return to it when the panel closes. */
export function detailsButtonId(setId: string): string {
  return `test-set-${setId}-details`;
}

/** One curated set in the catalog: title, mode, size, timing and its Sample / Locked badges. */
export function TestSetCard({ set, selected, panelId, onOpen }: TestSetCardProps) {
  const headingId = `test-set-${set.id}-title`;
  return (
    <article aria-labelledby={headingId} className={`${ui.card} ${styles.card} ${selected ? styles.cardSelected : ""}`}>
      <div className={styles.cardHeader}>
        <h2 id={headingId} className={ui.cardTitle}>
          {set.title}
        </h2>
        <div className={styles.badges}>
          {set.isSample && <span className={ui.badge}>Sample</span>}
          {set.locked && (
            <span className={`${ui.badge} ${styles.lockedBadge}`}>
              <span aria-hidden="true">🔒</span> Locked: premium
            </span>
          )}
        </div>
      </div>
      <ul className={styles.facts}>
        <li>{modeLabel(set.mode)}</li>
        <li>{questionCountLabel(set.questionCount)}</li>
        <li>{timingLabel(set)}</li>
      </ul>
      <div className={styles.cardAction}>
        <button
          id={detailsButtonId(set.id)}
          type="button"
          className={ui.buttonSecondary}
          aria-expanded={selected}
          aria-controls={selected ? panelId : undefined}
          aria-describedby={headingId}
          onClick={() => onOpen(set.id)}
        >
          View details
        </button>
      </div>
    </article>
  );
}
