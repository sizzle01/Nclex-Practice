"use client";

import { useEffect, useId, useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import { toNumber } from "@/lib/format/toNumber";
import type { TimerState } from "../api/attemptApi";
import styles from "./player.module.css";

interface PauseOverlayProps {
  pause: TimerState["pause"];
  busy: boolean;
  error: string | null;
  onResume: () => void;
}

/**
 * Covers the player while the test is paused (interfaces.md 5.3). The question is not even loaded
 * behind it: the server refuses item content during a pause (PRD 5.4).
 */
export function PauseOverlay({ pause, busy, error, onResume }: PauseOverlayProps) {
  const titleId = useId();
  const textId = useId();
  const buttonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    buttonRef.current?.focus();
  }, []);

  return (
    <div className={styles.backdrop}>
      <div role="dialog" aria-modal="true" aria-labelledby={titleId} aria-describedby={textId} className={`${ui.card} ${styles.dialog}`}>
        <h2 id={titleId}>Test paused</h2>
        <div id={textId}>
          <p className={ui.muted}>Your time is stopped and the questions are hidden until you resume.</p>
          <p className={ui.muted}>{pausesLeftText(pause)}</p>
          {pause.maxPauseSec !== null && pause.maxPauseSec !== undefined && (
            <p className={ui.muted}>
              A pause longer than {breakLength(toNumber(pause.maxPauseSec))} uses up test time, as at a testing centre.
            </p>
          )}
        </div>
        {error && (
          <p role="alert" className={styles.error}>
            {error}
          </p>
        )}
        <button ref={buttonRef} type="button" className={ui.button} disabled={busy} onClick={onResume}>
          {busy ? "Resuming…" : "Resume"}
        </button>
      </div>
    </div>
  );
}

/** "2 of 3 pauses left", or that pauses are unlimited. */
export function pausesLeftText(pause: TimerState["pause"]) {
  if (pause.max === null || pause.max === undefined) return "You can pause as often as you need.";
  const max = toNumber(pause.max);
  const left = Math.max(0, max - toNumber(pause.used));
  return `${left} of ${max} ${max === 1 ? "pause" : "pauses"} left.`;
}

function breakLength(seconds: number) {
  if (seconds % 60 !== 0) return `${seconds} seconds`;
  const minutes = seconds / 60;
  return `${minutes} ${minutes === 1 ? "minute" : "minutes"}`;
}
