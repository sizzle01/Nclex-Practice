"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import { useStartAttempt } from "@/features/attempt/api/attemptApi";
import { isApiError } from "@/lib/api/errors";
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
  const router = useRouter();
  const start = useStartAttempt();

  function startSet() {
    start.mutate(set.id, { onSuccess: (attempt) => router.push(`/attempts/${attempt.id}`) });
  }

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
        <button type="button" className={`${ui.button} ${styles.startButton}`} disabled={start.isPending} onClick={startSet}>
          {start.isPending ? "Starting…" : "Start"}
        </button>
        <button type="button" className={ui.buttonSecondary} onClick={onBuildAnother}>
          Build another
        </button>
        <Link href="/practice" className={ui.buttonGhost}>
          Back to practice
        </Link>
      </div>
      {start.error && (
        <p role="alert" className={styles.hint}>
          {isApiError(start.error, "EMAIL_NOT_VERIFIED")
            ? "Verify your email address to start this test."
            : "The test could not be started. Please try again."}
        </p>
      )}
    </section>
  );
}
