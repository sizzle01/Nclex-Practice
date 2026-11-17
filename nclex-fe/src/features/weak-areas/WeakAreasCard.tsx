"use client";

import { useRouter } from "next/navigation";
import forms from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import { useStartWeakAreaPractice, useWeakAreas, type WeakAreasView } from "./api/weakAreasApi";
import styles from "./WeakAreas.module.css";

/** The test size the card aims for; a smaller number means the weak areas ran short (architecture.md 5.7.1). */
const targetQuestions = 25;

/**
 * "Practice your weak areas" (docs/features/F18): the Client Needs areas under 65% in the latest
 * 10 tests, and one button that starts a tutor-mode practice test from them. Shown on the
 * dashboard and on the results page. It loads on its own, so a failure here leaves the page intact.
 */
export function WeakAreasCard() {
  const { data, isPending, isError } = useWeakAreas();
  const start = useStartWeakAreaPractice();
  const router = useRouter();

  function startPractice() {
    start.mutate(undefined, {
      onSuccess: (started) => {
        if (started.attempt) router.push(`/attempts/${started.attempt.id}`);
      },
    });
  }

  return (
    <section aria-labelledby="weak-areas-heading" className={ui.card}>
      <h2 id="weak-areas-heading" className={ui.cardTitle}>
        Practice your weak areas
      </h2>
      {isPending ? (
        <p className={ui.muted}>Looking at your recent tests…</p>
      ) : isError ? (
        <p className={ui.muted}>Your weak areas could not be loaded. Please try again later.</p>
      ) : (
        <WeakAreasBody view={data} />
      )}

      {data && toNumber(data.questionCount) > 0 && (
        <>
          <p>
            <button type="button" className={ui.button} onClick={startPractice} disabled={start.isPending}>
              {start.isPending ? "Starting…" : `Practice ${toNumber(data.questionCount)} questions`}
            </button>
          </p>
          {start.error && (
            <p role="alert" className={forms.formError}>
              {startErrorMessage(start.error)}
            </p>
          )}
        </>
      )}
    </section>
  );
}

function WeakAreasBody({ view }: { view: WeakAreasView }) {
  const attempts = toNumber(view.attemptsConsidered);
  if (attempts === 0) {
    return <p>Finish a test and we&apos;ll point out the areas to practise.</p>;
  }

  // Say so plainly rather than inventing a weakness.
  if (view.areas.length === 0) {
    return (
      <p>
        No weak areas in your last {attempts === 1 ? "test" : `${attempts} tests`}: every area with 5 or more questions
        is at 65% or above.
      </p>
    );
  }

  const questionCount = toNumber(view.questionCount);
  return (
    <>
      <ul className={styles.areas}>
        {view.areas.map((area) => (
          <li key={area.code}>
            <span>{area.name}</span>
            <strong>{Math.round(toNumber(area.pct) * 100)}%</strong>
            <span className={ui.muted}>{toNumber(area.items)} questions</span>
          </li>
        ))}
      </ul>
      <p className={ui.muted}>
        Under 65% in your last {attempts === 1 ? "test" : `${attempts} tests`}. The practice test shows each rationale as
        you go, and starts with questions you missed.
      </p>
      {questionCount === 0 ? (
        <p>There aren&apos;t enough questions in these areas for a practice test yet.</p>
      ) : (
        questionCount < targetQuestions && (
          <p>
            Only {questionCount} questions are available in these areas, so this test is shorter than the usual{" "}
            {targetQuestions}.
          </p>
        )
      )}
    </>
  );
}

function startErrorMessage(error: Error): string {
  if (isApiError(error, "EMAIL_NOT_VERIFIED")) return "Verify your email address to start a test.";
  if (isApiError(error, "NO_WEAK_AREAS") || isApiError(error, "INSUFFICIENT_QUESTIONS")) {
    return "Your weak areas have changed. Reload the page to see the latest.";
  }
  return "The practice test could not be started. Please try again.";
}
