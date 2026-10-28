"use client";

import { useEffect, useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import { toNumber } from "@/lib/format/toNumber";
import type { AttemptState, ItemState } from "../api/attemptApi";
import styles from "./player.module.css";

interface NavigatorDrawerProps {
  id: string;
  attempt: AttemptState;
  onGoTo: (position: number) => void;
  onClose: () => void;
}

const stateLabels: Record<ItemState, string> = {
  unseen: "not seen",
  seen: "not answered",
  answered: "answered",
  locked: "locked",
};

/**
 * A grid of every position, showing which are answered and which are marked for review
 * (interfaces.md 5.3). Escape closes it. Without back-navigation, earlier items are disabled.
 */
export function NavigatorDrawer({ id, attempt, onGoTo, onClose }: NavigatorDrawerProps) {
  const currentRef = useRef<HTMLButtonElement>(null);
  const current = toNumber(attempt.currentPosition);

  useEffect(() => {
    currentRef.current?.focus();
  }, []);

  return (
    <section
      id={id}
      aria-label="Question navigator"
      className={ui.card}
      onKeyDown={(event) => {
        if (event.key === "Escape") onClose();
      }}
    >
      <ul className={styles.navigator}>
        {attempt.navigator.map((entry) => {
          const position = toNumber(entry.position);
          const isCurrent = position === current;
          const label = `Question ${position}, ${stateLabels[entry.state]}${entry.flagged ? ", marked for review" : ""}`;
          return (
            <li key={position}>
              <button
                ref={isCurrent ? currentRef : undefined}
                type="button"
                aria-label={label}
                aria-current={isCurrent ? "step" : undefined}
                disabled={!attempt.settings.allowBackNav && position < current}
                className={`${styles.navButton} ${entry.state === "answered" || entry.state === "locked" ? styles.navAnswered : ""}`}
                onClick={() => onGoTo(position)}
              >
                {position}
                {entry.flagged && (
                  <span aria-hidden="true" className={styles.navFlag}>
                    ⚑
                  </span>
                )}
              </button>
            </li>
          );
        })}
      </ul>
      <p className={styles.legend}>Shaded: answered. ⚑ Marked for review.</p>
    </section>
  );
}
