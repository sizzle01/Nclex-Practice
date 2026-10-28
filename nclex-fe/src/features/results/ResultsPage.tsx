"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useEffect } from "react";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import { type AttemptResultView, type CategoryResult, fetchResult, resultQueryKey } from "../attempt/api/attemptApi";
import { elapsed, percent, points } from "./format";
import styles from "./results.module.css";

type Breakdown = AttemptResultView["breakdown"];

/** The breakdown tables, in the order the page shows them. */
const dimensions: { key: keyof Breakdown; title: string; column: string }[] = [
  { key: "client_needs_parent", title: "By Client Needs category", column: "Category" },
  { key: "client_needs", title: "By Client Needs subcategory", column: "Subcategory" },
  { key: "body_system", title: "By body system", column: "Body system" },
  { key: "item_type", title: "By question type", column: "Question type" },
  { key: "cjmm_skill", title: "By clinical judgment step", column: "Step" },
  { key: "difficulty", title: "By difficulty", column: "Difficulty" },
];

/**
 * /attempts/[id]/results (interfaces.md 5.2): the score, the readiness estimate with its
 * disclaimer, time per question, the weakest areas and the breakdown tables.
 */
export function ResultsPage({ attemptId }: { attemptId: string }) {
  const result = useQuery({ queryKey: resultQueryKey(attemptId), queryFn: () => fetchResult(attemptId) });

  // The browser only jumps to #breakdown if it exists on arrival; here it appears after loading.
  useEffect(() => {
    if (result.isSuccess && window.location.hash === "#breakdown") {
      document.getElementById("breakdown")?.scrollIntoView();
    }
  }, [result.isSuccess]);

  if (result.isPending) return <p className={ui.muted}>Loading your results…</p>;
  if (result.isError) return <ResultsError attemptId={attemptId} error={result.error} />;

  const { data } = result;
  const score = data.score;
  return (
    <div className={styles.page}>
      <header>
        <h1 className={styles.title}>Results: {data.title}</h1>
        <p className={ui.muted}>
          Submitted {new Date(data.submittedAt).toLocaleString()}
          {data.submitReason === "time_expired" && " when the time ran out"}
        </p>
      </header>

      <section aria-labelledby="score-heading" className={`${ui.card} ${styles.summary}`}>
        <h2 id="score-heading" className={ui.cardTitle}>
          Your score
        </h2>
        <p className={styles.score}>{percent(score.pct)}</p>
        <ul className={styles.facts}>
          <li>
            {points(score.pointsEarned)} of {points(score.maxPoints)} points
          </li>
          <li>
            {toNumber(score.itemsCorrect)} of {toNumber(score.itemsTotal)} questions fully correct
          </li>
          <li>{toNumber(score.itemsAnswered)} answered</li>
          <li>
            {elapsed(data.time.totalMs)} in total, {elapsed(data.time.avgPerItemMs)} per question on average
          </li>
        </ul>
        <Readiness readiness={data.readiness} />
      </section>

      {data.weakest.length > 0 && (
        <section aria-labelledby="weakest-heading" className={ui.card}>
          <h2 id="weakest-heading" className={ui.cardTitle}>
            Focus on these next
          </h2>
          <ol className={styles.weakest}>
            {data.weakest.map((area) => (
              <li key={area.key}>
                {area.name}: {percent(area.pct)}
              </li>
            ))}
          </ol>
        </section>
      )}

      <nav aria-label="Review your answers" className={styles.links}>
        <Link href={`/attempts/${attemptId}/review`} className={ui.button}>
          Review all answers
        </Link>
        <Link href={`/attempts/${attemptId}/review?filter=incorrect`} className={ui.buttonSecondary}>
          Review incorrect answers
        </Link>
        <Link href="/dashboard" className={ui.buttonGhost}>
          Back to the dashboard
        </Link>
      </nav>

      {/* Result notifications link here (interfaces.md 5.5: results page anchored at #breakdown). */}
      <div id="breakdown" className={styles.breakdown}>
        {dimensions.map(({ key, title, column }) =>
          data.breakdown[key].length > 0 ? (
            <BreakdownTable key={key} title={title} column={column} rows={data.breakdown[key]} />
          ) : null,
        )}
      </div>
    </div>
  );
}

/** The estimate is always shown with its caveat (architecture.md 5.5). */
function Readiness({ readiness }: { readiness: AttemptResultView["readiness"] }) {
  if (!readiness) return null;
  return (
    <div className={styles.readiness}>
      <p>
        <strong>{readiness.label === "likely_ready" ? "Likely ready" : "Needs more practice"}</strong>{" "}
        <span className={ui.muted}>(passing threshold {percent(readiness.passThreshold)})</span>
      </p>
      <p className={ui.muted}>{readiness.disclaimer}</p>
    </div>
  );
}

function BreakdownTable({ title, column, rows }: { title: string; column: string; rows: CategoryResult[] }) {
  return (
    <section className={ui.card}>
      <div className={styles.tableScroll}>
        <table className={styles.table}>
          <caption>{title}</caption>
          <thead>
            <tr>
              <th scope="col">{column}</th>
              <th scope="col">Questions</th>
              <th scope="col">Fully correct</th>
              <th scope="col">Points</th>
              <th scope="col">Score</th>
              <th scope="col">Avg time</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.key}>
                <th scope="row">{row.name}</th>
                <td>{toNumber(row.items)}</td>
                <td>{toNumber(row.correct)}</td>
                <td>
                  {points(row.pointsEarned)} / {points(row.maxPoints)}
                </td>
                <td>{percent(row.pct)}</td>
                <td>{elapsed(row.avgTimeMs)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

function ResultsError({ attemptId, error }: { attemptId: string; error: unknown }) {
  const notSubmitted = isApiError(error, "ATTEMPT_NOT_ACTIVE");
  return (
    <section className={ui.card}>
      <p role="alert">
        {notSubmitted
          ? "This test hasn't been submitted yet, so there are no results."
          : isApiError(error, "NOT_FOUND")
            ? "These results were not found."
            : "Your results could not be loaded. Please try again."}
      </p>
      <Link href={notSubmitted ? `/attempts/${attemptId}` : "/dashboard"} className={ui.buttonSecondary}>
        {notSubmitted ? "Back to the test" : "Back to the dashboard"}
      </Link>
    </section>
  );
}
