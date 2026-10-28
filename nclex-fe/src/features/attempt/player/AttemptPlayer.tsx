"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { ItemAnswer } from "../renderers/ItemAnswer";
import { NavigatorDrawer } from "./NavigatorDrawer";
import { OpenElsewhereDialog } from "./OpenElsewhereDialog";
import styles from "./player.module.css";
import { SaveStatusText } from "./SaveStatusText";
import { useAttemptSession } from "./useAttemptSession";

/**
 * The exam player at /attempts/[id] (interfaces.md 5.3), untimed for now: top bar, the item,
 * bottom bar with mark-for-review, the save indicator and Prev/Next, and a navigator drawer.
 * Timers and pause arrive in F09, submitting in F10.
 */
export function AttemptPlayer({ attemptId }: { attemptId: string }) {
  const session = useAttemptSession(attemptId);
  const { attempt, item, position, total } = session;
  const [navigatorOpen, setNavigatorOpen] = useState(false);
  const navigatorId = useId();
  const stemId = useId();
  const stemRef = useRef<HTMLHeadingElement>(null);
  const navigatorButtonRef = useRef<HTMLButtonElement>(null);

  // A new question: move focus to its text, so screen-reader users hear it first.
  useEffect(() => {
    if (item) stemRef.current?.focus();
  }, [item?.questionVersionId]); // eslint-disable-line react-hooks/exhaustive-deps

  if (session.openState === "failed") return <OpenFailed error={session.openError} />;
  if (session.openState === "opening" || !attempt) return <p className={ui.muted}>Opening your test…</p>;

  const locked = item?.state === "locked";
  const allowBack = attempt.settings.allowBackNav;

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

      {navigatorOpen && <NavigatorDrawer id={navigatorId} attempt={attempt} onGoTo={goTo} onClose={closeNavigator} />}

      <article className={`${ui.card} ${styles.item}`} aria-busy={!item}>
        {session.itemError ? (
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
              disabled={locked || session.leaseLost}
              struckOptions={item.struckOptions}
              labelledBy={stemId}
            />
            {locked && <p className={styles.notice}>This answer is locked and can no longer be changed.</p>}
          </>
        )}
      </article>

      <footer className={`${ui.card} ${styles.bottomBar}`}>
        <div className={styles.actions}>
          <button
            type="button"
            className={ui.buttonGhost}
            aria-pressed={item?.flagged ?? false}
            disabled={!item || locked}
            onClick={() => void session.toggleFlag()}
          >
            <span aria-hidden="true">⚑</span> Mark for review
          </button>
          <SaveStatusText status={session.saveStatus} />
        </div>
        <div className={styles.actions}>
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
        </div>
        {session.navError && (
          <p role="alert" className={styles.error}>
            {session.navError}
          </p>
        )}
        {position === total && (
          <p className={styles.notice}>
            This is the last question. Your answers are saved; submitting a test arrives in a later update.
          </p>
        )}
      </footer>

      {session.leaseLost && <OpenElsewhereDialog busy={session.busy} onContinueHere={() => void session.continueHere()} />}
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
