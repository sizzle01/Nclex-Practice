import ui from "@/components/ui/ui.module.css";
import { useClinicalJudgmentTrend } from "../api/performanceApi";
import styles from "./performance.module.css";
import { ScorePercent } from "./ScorePercent";

const shortDate = new Intl.DateTimeFormat(undefined, { day: "numeric", month: "short" });

/**
 * Each clinical judgment step across the last five timed tests (F25), laid out like the candidate's NCLEX
 * performance report: steps down the side, tests across, oldest first. It shows whether the later steps
 * (generate solutions, take action, evaluate outcomes) are recovering under exam conditions.
 */
export function ClinicalJudgmentTrendTable() {
  const trend = useClinicalJudgmentTrend();

  return (
    <section aria-labelledby="judgment-trend-heading" className={ui.card}>
      <h2 id="judgment-trend-heading" className={ui.cardTitle}>
        Clinical judgment in timed tests
      </h2>
      {trend.isPending && <p className={ui.muted}>Loading…</p>}
      {trend.isError && (
        <p role="alert" className={styles.error}>
          Your clinical judgment trend could not be loaded.
        </p>
      )}
      {trend.isSuccess && trend.data.attempts.length === 0 && (
        <p className={ui.muted}>
          Finish a timed test, such as a timed case-study block, to see each step of your clinical judgment here.
        </p>
      )}
      {trend.isSuccess && trend.data.attempts.length > 0 && (
        <div className={styles.tableScroll}>
          <table className={styles.table}>
            <caption>Score on each step in your last {trend.data.attempts.length} timed tests, oldest first</caption>
            <thead>
              <tr>
                <th scope="col">Step</th>
                {trend.data.attempts.map((attempt) => (
                  <th scope="col" key={attempt.attemptId}>
                    {attempt.title}
                    <br />
                    <span className={ui.muted}>{shortDate.format(new Date(attempt.submittedAt))}</span>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {trend.data.steps.map((step) => (
                <tr key={step.key}>
                  <th scope="row">{step.name}</th>
                  {step.scores.map((score, i) => (
                    <td key={trend.data.attempts[i].attemptId}>
                      {score === null ? (
                        <>
                          <span aria-hidden="true">–</span>
                          <span className={styles.visuallyHidden}>No questions</span>
                        </>
                      ) : (
                        <ScorePercent value={score} />
                      )}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
