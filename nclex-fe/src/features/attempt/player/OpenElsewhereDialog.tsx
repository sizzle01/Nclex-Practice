"use client";

import { useEffect, useId, useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import styles from "./player.module.css";

interface OpenElsewhereDialogProps {
  busy: boolean;
  onContinueHere: () => void;
}

/**
 * Blocks the player when another device has taken the attempt (interfaces.md 5.4). It is part of
 * the page rather than a browser confirm(), and its button gets focus when it appears.
 */
export function OpenElsewhereDialog({ busy, onContinueHere }: OpenElsewhereDialogProps) {
  const titleId = useId();
  const textId = useId();
  const buttonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    buttonRef.current?.focus();
  }, []);

  return (
    <div className={styles.backdrop}>
      <div role="alertdialog" aria-modal="true" aria-labelledby={titleId} aria-describedby={textId} className={`${ui.card} ${styles.dialog}`}>
        <h2 id={titleId}>This attempt is open on another device</h2>
        <p id={textId} className={ui.muted}>
          Answers are only saved from one device at a time. Continue here to keep working on this device; the other
          one will stop saving.
        </p>
        <button ref={buttonRef} type="button" className={ui.button} disabled={busy} onClick={onContinueHere}>
          {busy ? "Taking over…" : "Continue here"}
        </button>
      </div>
    </div>
  );
}
