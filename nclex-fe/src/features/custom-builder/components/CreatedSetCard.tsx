"use client";

import Link from "next/link";
import { useEffect, useId, useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import { modeLabel, questionCountLabel, revealModeLabel, timingLabel } from "@/features/practice/format";
import type { CreateCustomSetResponse } from "../api/builderApi";
import styles from "./builder.module.css";

interface CreatedSetCardProps {
  created: CreateCustomSetResponse;
  onBuildAnother: () => void;
}

/** Confirms the new set. Focus moves to its heading so keyboard and screen-reader users hear the result. */
export function CreatedSetCard({ created, onBuildAnother }: CreatedSetCardProps) {
  const set = created.testSet;
  const headingRef = useRef<HTMLHeadingElement>(null);
  const hintId = useId();

  useEffect(() => {
    headingRef.current?.focus();
  }, []);

  return (
    <section aria-labelledby="created-set-heading" className={`${ui.card} ${styles.created}`}>
      <h2 id="created-set-heading" ref={headingRef} tabIndex={-1}>
        Your test is ready
      </h2>
      <p>
        <strong>{set.title}</strong>
      </p>
      <ul className={styles.facts}>
        <li>{questionCountLabel(set.questionCount)}</li>
        <li>{modeLabel(set.mode)}</li>
        <li>{revealModeLabel(set.revealMode)}</li>
        <li>{timingLabel(set)}</li>
      </ul>
      <div className={styles.actions}>
        <button type="button" className={`${ui.button} ${styles.startButton}`} disabled aria-describedby={hintId}>
          Start
        </button>
        <button type="button" className={ui.buttonSecondary} onClick={onBuildAnother}>
          Build another
        </button>
        <Link href="/practice" className={ui.buttonGhost}>
          Back to practice
        </Link>
      </div>
      <p id={hintId} className={styles.hint}>
        Starting a test is coming in the next update.
      </p>
    </section>
  );
}
