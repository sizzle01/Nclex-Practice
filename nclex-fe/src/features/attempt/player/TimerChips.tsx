"use client";

import { useEffect, useRef, useState } from "react";
import { crossedAnnouncement, formatCountdown, spokenCountdown } from "./attemptClock";
import styles from "./player.module.css";

interface TimerChipsProps {
  /** Exam time left in ms; null for an untimed test. */
  examMs: number | null;
  /** Time left on this question in ms; null when questions are not timed. */
  itemMs: number | null;
  /** While paused the chips show the frozen time. */
  paused: boolean;
}

/**
 * The exam timer and, when the set has one, the question timer in the top bar (interfaces.md 5.3).
 * Each chip is a `timer`, which screen readers do not read out as it changes; the separate live
 * region speaks only at 5 and 1 minutes left (interfaces.md section 4).
 */
export function TimerChips({ examMs, itemMs, paused }: TimerChipsProps) {
  if (examMs === null && itemMs === null) return null;
  return (
    <div className={styles.timers}>
      {examMs !== null && (
        <TimerChip label="Test time remaining" ms={examMs} paused={paused} icon="⏱" />
      )}
      {itemMs !== null && (
        <TimerChip label="Time remaining for this question" ms={itemMs} paused={paused} icon="⌛" />
      )}
      <TimeAnnouncer examMs={examMs} itemMs={itemMs} />
    </div>
  );
}

function TimerChip({ label, ms, paused, icon }: { label: string; ms: number; paused: boolean; icon: string }) {
  // Low time is shown by the words "time low" as well as colour (interfaces.md section 4).
  const low = ms > 0 && ms <= 60_000;
  return (
    <span
      role="timer"
      aria-label={`${label}: ${spokenCountdown(ms)}${paused ? ", paused" : ""}`}
      className={`${styles.timerChip} ${low ? styles.timerLow : ""}`}
    >
      <span aria-hidden="true">{icon}</span> {formatCountdown(ms)}
      {paused && <span className={styles.timerNote}> paused</span>}
      {low && !paused && <span className={styles.timerNote}> time low</span>}
    </span>
  );
}

/** A polite live region that speaks when a timer crosses 5 or 1 minutes left. */
function TimeAnnouncer({ examMs, itemMs }: { examMs: number | null; itemMs: number | null }) {
  const [message, setMessage] = useState("");
  const previous = useRef({ examMs, itemMs });

  useEffect(() => {
    const before = previous.current;
    previous.current = { examMs, itemMs };
    const exam = before.examMs !== null && examMs !== null ? crossedAnnouncement(before.examMs, examMs) : null;
    const item = before.itemMs !== null && itemMs !== null ? crossedAnnouncement(before.itemMs, itemMs) : null;
    const next = exam ? `${exam} in the test` : item ? `${item} for this question` : null;
    // Only when a threshold is crossed: the region never counts down by itself.
    if (next) setMessage(next);
  }, [examMs, itemMs]);

  return (
    <span aria-live="polite" aria-atomic="true" className={styles.visuallyHidden}>
      {message}
    </span>
  );
}
