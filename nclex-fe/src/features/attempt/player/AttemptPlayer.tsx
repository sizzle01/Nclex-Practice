"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import { ItemAnswer } from "@/features/player/renderers/ItemAnswer";
import { RationalePanel } from "../reveal/RationalePanel";
import { AttemptMenu } from "./AttemptMenu";
import { NavigatorDrawer } from "./NavigatorDrawer";
import { OpenElsewhereDialog } from "./OpenElsewhereDialog";
import { PauseOverlay } from "./PauseOverlay";
import styles from "./player.module.css";
import { SaveStatusText } from "./SaveStatusText";
import { SubmitDialog } from "./SubmitDialog";
import { TimerChips } from "./TimerChips";
import { useAttemptSession } from "./useAttemptSession";

/**
 * The exam player at /attempts/[id] (interfaces.md 5.3): top bar with the timers, Pause and the ⋯
 * menu (Save &amp; Exit), the item with its rationale once revealed, bottom bar with
 * mark-for-review, the save indicator, Prev/Next and Submit, and a navigator drawer.
 */
export function AttemptPlayer({ attemptId }: { attemptId: string }) {
  const session = useAttemptSession(attemptId);
  const { attempt, item, position, total } = session;
  const [navigatorOpen, setNavigatorOpen] = useState(false);
  const [confirmingSubmit, setConfirmingSubmit] = useState(false);
  const navigatorId = useId();
  const stemId = useId();
  const stemRef = useRef<HTMLHeadingElement>(null);
  const navigatorButtonRef = useRef<HTMLButtonElement>(null);

  // A new question: move focus to its text, so screen-reader users hear it first.
  useEffect(() => {
    if (item) stemRef.current?.focus();
  }, [item?.questionVersionId]); // eslint-disable-line react-hooks/exhaustive-deps

  if (session.openState === "ended") return <Ended byTime={session.endedByTime} resultsPath={session.resultsPath} />;
  if (session.openState === "failed") return <OpenFailed error={session.openError} />;
  if (session.openState === "opening" || !attempt) return <p className={ui.muted}>Opening your test…</p>;

  const locked = item?.state === "locked";
  const allowBack = attempt.settings.allowBackNav;
  const pauseState = attempt.timer.pause;
  const noPausesLeft = pauseState.max !== null && toNumber(pauseState.used) >= toNumber(pauseState.max);
  const inputDisabled = locked || session.leaseLost || session.examTimeUp || session.itemTimeUp || session.submitting;
  const tutor = attempt.settings.revealMode === "tutor";
  const unanswered = attempt.navigator.filter((entry) => entry.state === "unseen" || entry.state === "seen").length;
  const flagged = attempt.navigator.filter((entry) => entry.flagged).length;

  function goTo(target: number) {
    setNavigatorOpen(false);
    void session.goTo(target);
  }

  function closeNavigator() {
    setNavigatorOpen(false);
    navigatorButtonRef.current?.focus();
  }

  return (
    <section aria-labelledby={`${stemId}-title`} className={styles.player}>
      <header className={`${ui.card} ${styles.topBar}`}>
        <h1 id={`${stemId}-title`} className={styles.title}>
          {attempt.testSet.title}
        </h1>
        <span className={styles.position}>
          Question {position} of {total}
        </span>
        <TimerChips examMs={session.examMs} itemMs={session.itemMs} paused={session.paused} />
        <div className={styles.actions}>
          <button
            type="button"
            className={ui.buttonSecondary}
            disabled={session.busy || session.paused || noPausesLeft || session.examTimeUp}
            onClick={() => void session.pause("break")}
          >
            <span aria-hidden="true">⏸</span> Pause
          </button>
          {noPausesLeft && <span className={ui.muted}>No pauses left</span>}
          <AttemptMenu
            disabled={session.busy || session.paused || noPausesLeft || session.examTimeUp}
            onSaveAndExit={() => void session.pause("save_exit")}
          />
        </div>
        <button
          ref={navigatorButtonRef}
          type="button"
          className={ui.buttonSecondary}
          aria-expanded={navigatorOpen}
          aria-controls={navigatorId}
          onClick={() => (navigatorOpen ? closeNavigator() : setNavigatorOpen(true))}
        >
          All questions
        </button>
      </header>

      {session.pauseError && !session.paused && (
        <p role="alert" className={styles.error}>
          {session.pauseError}
        </p>
      )}

      {navigatorOpen && <NavigatorDrawer id={navigatorId} attempt={attempt} onGoTo={goTo} onClose={closeNavigator} />}

      <article className={`${ui.card} ${styles.item}`} aria-busy={!item}>
        {session.paused ? (
          <p className={ui.muted}>The test is paused.</p>
        ) : session.itemError ? (
          <p role="alert" className={styles.error}>
            This question could not be loaded. Please try again.
          </p>
        ) : !item ? (
          <p className={ui.muted}>Loading question…</p>
        ) : (
          <>
            <h2 id={stemId} ref={stemRef} tabIndex={-1} className={styles.stem}>
              {item.stem}
            </h2>
            <ItemAnswer
              itemType={item.itemType}
              content={item.content}
              response={session.response}
              onChange={session.answer}
              disabled={inputDisabled}
              struckOptions={item.struckOptions}
              labelledBy={stemId}
              reveal={item.reveal}
            />
            {locked && <p className={styles.notice}>This answer is locked and can no longer be changed.</p>}
            {item.reveal && <RationalePanel reveal={item.reveal} content={item.content} />}
            {session.itemTimeUp && !locked && (
              <p role="alert" className={styles.notice}>
                Time is up for this question. Your answer is kept; move on to the next one.
              </p>
            )}
            {session.examTimeUp && (
              <p role="alert" className={styles.notice}>
                Time is up. Your answers are being submitted…
              </p>
            )}
          </>
        )}
      </article>

      <footer className={`${ui.card} ${styles.bottomBar}`}>
        <div className={styles.actions}>
          <button
            type="button"
            className={ui.buttonGhost}
            aria-pressed={item?.flagged ?? false}
            disabled={!item || locked || session.paused}
            onClick={() => void session.toggleFlag()}
          >
            <span aria-hidden="true">⚑</span> Mark for review
          </button>
          <SaveStatusText status={session.saveStatus} />
        </div>
        <div className={styles.actions}>
          {tutor && !locked && (
            <button
              type="button"
              className={ui.buttonSecondary}
              disabled={!item || session.busy || inputDisabled || session.paused}
              onClick={() => void session.submitItem()}
            >
              Submit answer
            </button>
          )}
          <button
            type="button"
            className={ui.buttonSecondary}
            disabled={session.busy || position === 1 || !allowBack}
            onClick={() => goTo(position! - 1)}
          >
            ◀ Previous
          </button>
          <button
            type="button"
            className={ui.button}
            disabled={session.busy || position === total}
            onClick={() => goTo(position! + 1)}
          >
            Next ▶
          </button>
          <button
            type="button"
            className={ui.button}
            disabled={session.busy || session.paused || session.leaseLost || session.examTimeUp}
            onClick={() => setConfirmingSubmit(true)}
          >
            Submit test
          </button>
        </div>
        {session.navError && (
          <p role="alert" className={styles.error}>
            {session.navError}
          </p>
        )}
        {session.submitError && !confirmingSubmit && (
          <p role="alert" className={styles.error}>
            {session.submitError}
          </p>
        )}
      </footer>

      {session.paused && !session.leaseLost && (
        <PauseOverlay
          pause={pauseState}
          busy={session.busy}
          error={session.pauseError}
          onResume={() => void session.resume()}
        />
      )}

      {confirmingSubmit && !session.leaseLost && (
        <SubmitDialog
          unanswered={unanswered}
          flagged={flagged}
          busy={session.submitting}
          error={session.submitError}
          onSubmit={() => void session.submitAttempt()}
          onCancel={() => setConfirmingSubmit(false)}
        />
      )}

      {session.leaseLost && <OpenElsewhereDialog busy={session.busy} onContinueHere={() => void session.continueHere()} />}
    </section>
  );
}

/**
 * The attempt is submitted, by the candidate or because its time ran out (ATTEMPT_TIME_EXPIRED).
 * The player is already moving to the results page; the link covers a slow or failed redirect.
 */
function Ended({ byTime, resultsPath }: { byTime: boolean; resultsPath: string }) {
  return (
    <section className={ui.card}>
      <h1>{byTime ? "Time is up" : "Test submitted"}</h1>
      <p role="alert">
        {byTime ? "Your test has been submitted with the answers you saved." : "Your answers have been graded."}
      </p>
      <Link href={resultsPath} className={ui.button}>
        See your results
      </Link>
    </section>
  );
}

function OpenFailed({ error }: { error: unknown }) {
  const message = isApiError(error, "NOT_FOUND")
    ? "This test was not found."
    : isApiError(error, "ATTEMPT_NOT_ACTIVE")
      ? "This test has already been submitted or discarded."
      : "This test could not be opened. Please try again.";
  return (
    <section className={ui.card}>
      <p role="alert">{message}</p>
      <Link href="/dashboard" className={ui.buttonSecondary}>
        Back to the dashboard
      </Link>
    </section>
  );
}
