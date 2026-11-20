"use client";

import { useRouter } from "next/navigation";
import { useEffect, useId, useRef, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { useStartAttempt } from "@/features/attempt/api/attemptApi";
import { CLOSE_TO_PASS_MARK, PASS_MARK } from "@/features/results/ResultOutcome";
import { isApiError } from "@/lib/api/errors";
import { useTestSet } from "../api/practiceApi";
import {
  formatDuration,
  modeLabel,
  pausePolicyLabel,
  percentLabel,
  revealModeLabel,
  setLengthLabel,
  timingLabel,
} from "../format";
import styles from "./practice.module.css";

interface TestSetDetailPanelProps {
  id: string;
  setId: string;
  onClose: () => void;
}

/**
 * The open set's settings and a Start button, which starts the set (or resumes its draft) and
 * opens the player. Focus moves to the panel's heading when it opens, so keyboard and
 * screen-reader users land on what they asked for. An adaptive exam (F19) first asks whether to
 * take it timed or untimed; a draft already started keeps the timing it started with.
 */
export function TestSetDetailPanel({ id, setId, onClose }: TestSetDetailPanelProps) {
  const { data: set, error, isPending } = useTestSet(setId);
  const headingRef = useRef<HTMLHeadingElement>(null);
  const hintId = useId();
  const headingId = `${id}-heading`;
  const router = useRouter();
  const start = useStartAttempt();
  const [timed, setTimed] = useState(true);
  const timingName = useId();

  function startSet() {
    const input = set?.adaptive ? { testSetId: setId, timed } : { testSetId: setId };
    start.mutate(input, { onSuccess: (attempt) => router.push(`/attempts/${attempt.id}`) });
  }

  useEffect(() => {
    if (set) headingRef.current?.focus();
  }, [set]);

  return (
    <section id={id} aria-labelledby={headingId} className={`${ui.card} ${styles.panel}`}>
      <div className={styles.panelHeader}>
        <h2 id={headingId} ref={headingRef} tabIndex={-1}>
          {set?.title ?? "Test set"}
        </h2>
        <button type="button" className={ui.buttonGhost} aria-label="Close details" onClick={onClose}>
          Close
        </button>
      </div>

      {isPending && <p className={ui.muted}>Loading details…</p>}
      {error && (
        <p role="alert" className={styles.error}>
          {isApiError(error, "NOT_FOUND") ? "This set is no longer available." : "The details could not be loaded. Please try again."}
        </p>
      )}

      {set && (
        <>
          {set.description && <p className={ui.muted}>{set.description}</p>}
          <div className={styles.badges}>
            {set.isSample && <span className={ui.badge}>Sample</span>}
            {set.locked && (
              <span className={`${ui.badge} ${styles.lockedBadge}`}>
                <span aria-hidden="true">🔒</span> Locked: premium
              </span>
            )}
          </div>
          <dl className={styles.settings}>
            <dt>Mode</dt>
            <dd>{modeLabel(set.mode)}</dd>
            <dt>Questions</dt>
            <dd>
              {setLengthLabel(set)}
              {set.adaptive && ", stopping once your result is clear"}
            </dd>
            <dt>Timing</dt>
            <dd>{timingLabel(set)}</dd>
            <dt>Answers</dt>
            <dd>{revealModeLabel(set.revealMode)}</dd>
            {/* Every fixed-length test is graded against the same pass mark (F23); an adaptive exam passes on its
                ability estimate, and a timed case-study block gives no outcome at all (F24). */}
            {!set.adaptive && !set.outcomeHidden && (
              <>
                <dt>Pass mark</dt>
                <dd>
                  {percentLabel(PASS_MARK)} of points ({percentLabel(CLOSE_TO_PASS_MARK)} or more is close to pass)
                </dd>
              </>
            )}
            <dt>Going back</dt>
            <dd>{set.allowBackNav ? "Allowed" : "Not allowed"}</dd>
            <dt>Pauses</dt>
            <dd>{pausePolicyLabel(set.pausePolicy)}</dd>
          </dl>
          {set.adaptive && (
            <fieldset className={styles.timingChoice}>
              <legend>How do you want to take it?</legend>
              <label>
                <input type="radio" name={timingName} checked={timed} onChange={() => setTimed(true)} />
                Timed{set.timeBudgetSec != null && ` (${formatDuration(set.timeBudgetSec)})`}: submits by itself when
                time runs out
              </label>
              <label>
                <input type="radio" name={timingName} checked={!timed} onChange={() => setTimed(false)} />
                Untimed: no clock; the exam still stops by itself
              </label>
              <p className={ui.muted}>If you already started this exam, Start resumes it with the timing you chose then.</p>
            </fieldset>
          )}
          <div>
            <button
              type="button"
              className={`${ui.button} ${styles.startButton}`}
              disabled={set.locked || start.isPending}
              aria-describedby={set.locked ? hintId : undefined}
              onClick={startSet}
            >
              {start.isPending ? "Starting…" : "Start"}
            </button>
            {set.locked && (
              <p id={hintId} className={styles.startHint}>
                This set needs a premium plan.
              </p>
            )}
            {start.error && (
              <p role="alert" className={styles.error}>
                {startErrorMessage(start.error)}
              </p>
            )}
          </div>
        </>
      )}
    </section>
  );
}

function startErrorMessage(error: unknown) {
  if (isApiError(error, "EMAIL_NOT_VERIFIED")) return "Verify your email address to start this set. Sample sets can be started now.";
  if (isApiError(error, "PLAN_UPGRADE_REQUIRED")) return "This set needs a premium plan.";
  if (isApiError(error, "NOT_FOUND")) return "This set is no longer available.";
  return "The set could not be started. Please try again.";
}
