"use client";

import Link from "next/link";
import ui from "@/components/ui/ui.module.css";
import type { AttemptSummary } from "@/features/attempt/api/attemptApi";
import { percent } from "@/features/results/format";
import { useHistory } from "../api/historyApi";
import styles from "./history.module.css";

const modeLabels: Record<AttemptSummary["mode"], string> = { practice: "Practice", challenge: "Challenge", cat: "Adaptive" };

/** Shown only when the set has a pass threshold; the "estimate, not a prediction" caveat is on the results page. */
export function outcomeLabel(passed: boolean | null) {
  if (passed === null) return "—";
  return passed ? "Likely ready" : "Needs more practice";
}

/** /history (interfaces.md 5.2): every finished test, newest first, with links to its results and review. */
export function HistoryPage() {
  const history = useHistory();

  if (history.isPending) return <p className={ui.muted}>Loading your history…</p>;
  if (history.isError)
    return (
      <p role="alert" className={styles.error}>
        Your history could not be loaded. Please try again.
      </p>
    );

  const attempts = history.data.pages.flatMap((page) => page.items);
  return (
    <div className={styles.page}>
      <header>
        <h1 className={styles.title}>History</h1>
        <p className={ui.muted}>Every test you have finished, newest first.</p>
      </header>

      {attempts.length === 0 ? (
        <section className={ui.card}>
          <h2 className={ui.cardTitle}>No finished tests yet</h2>
          <p>
            <Link href="/practice" className={ui.button}>
              Browse practice sets
            </Link>
          </p>
        </section>
      ) : (
        <section className={ui.card}>
          <div className={styles.tableScroll}>
            <table className={styles.table}>
              <caption>Finished tests</caption>
              <thead>
                <tr>
                  <th scope="col">Test</th>
                  <th scope="col">Mode</th>
                  <th scope="col">Finished</th>
                  <th scope="col" className={styles.number}>
                    Score
                  </th>
                  <th scope="col">Readiness estimate</th>
                  <th scope="col">
                    <span className={styles.visuallyHidden}>Links</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {attempts.map((attempt) => (
                  <HistoryRow key={attempt.id} attempt={attempt} />
                ))}
              </tbody>
            </table>
          </div>
          {history.hasNextPage && (
            <div className={styles.more}>
              <button
                type="button"
                className={ui.buttonSecondary}
                disabled={history.isFetchingNextPage}
                onClick={() => history.fetchNextPage()}
              >
                {history.isFetchingNextPage ? "Loading…" : "Show more"}
              </button>
            </div>
          )}
          {history.isFetchNextPageError && (
            <p role="alert" className={styles.error}>
              More tests could not be loaded. Please try again.
            </p>
          )}
        </section>
      )}
    </div>
  );
}

function HistoryRow({ attempt }: { attempt: AttemptSummary }) {
  const titleId = `history-${attempt.id}`;
  return (
    <tr>
      <th scope="row" id={titleId}>
        {attempt.testSet.title}
      </th>
      <td>{modeLabels[attempt.mode]}</td>
      <td>{attempt.submittedAt ? new Date(attempt.submittedAt).toLocaleString() : "—"}</td>
      <td className={styles.number}>{attempt.scorePct === null ? "—" : percent(attempt.scorePct)}</td>
      <td>{outcomeLabel(attempt.passed)}</td>
      <td>
        <span className={styles.links}>
          <Link href={`/attempts/${attempt.id}/results`} aria-describedby={titleId}>
            Results
          </Link>
          <Link href={`/attempts/${attempt.id}/review`} aria-describedby={titleId}>
            Review
          </Link>
        </span>
      </td>
    </tr>
  );
}
