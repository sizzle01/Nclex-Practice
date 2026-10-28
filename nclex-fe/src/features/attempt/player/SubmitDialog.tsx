"use client";

import { useEffect, useId, useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import styles from "./player.module.css";

interface SubmitDialogProps {
  unanswered: number;
  flagged: number;
  busy: boolean;
  error: string | null;
  onSubmit: () => void;
  onCancel: () => void;
}

/**
 * Confirms the final Submit with the unanswered and flagged counts (interfaces.md 5.4). It is part
 * of the page rather than a browser confirm(). "Keep working" gets focus, so pressing Enter by
 * accident does not end the test, and Escape closes it.
 */
export function SubmitDialog({ unanswered, flagged, busy, error, onSubmit, onCancel }: SubmitDialogProps) {
  const titleId = useId();
  const textId = useId();
  const cancelRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    cancelRef.current?.focus();
  }, []);

  return (
    <div className={styles.backdrop}>
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={textId}
        className={`${ui.card} ${styles.dialog}`}
        onKeyDown={(event) => {
          if (event.key === "Escape" && !busy) onCancel();
        }}
      >
        <h2 id={titleId}>Submit this test?</h2>
        <div id={textId}>
          <p>
            {countText(unanswered, "unanswered question")} and {countText(flagged, "question")} marked for review.
          </p>
          <p className={ui.muted}>Unanswered questions score zero. You can&apos;t change your answers after submitting.</p>
        </div>
        {error && (
          <p role="alert" className={styles.error}>
            {error}
          </p>
        )}
        <div className={styles.actions}>
          <button ref={cancelRef} type="button" className={ui.buttonSecondary} disabled={busy} onClick={onCancel}>
            Keep working
          </button>
          <button type="button" className={ui.button} disabled={busy} onClick={onSubmit}>
            {busy ? "Submitting…" : "Submit test"}
          </button>
        </div>
      </div>
    </div>
  );
}

function countText(count: number, noun: string): string {
  return `${count === 0 ? "No" : count} ${noun}${count === 1 ? "" : "s"}`;
}
