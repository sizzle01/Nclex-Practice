"use client";

import Link from "next/link";
import ui from "@/components/ui/ui.module.css";
import { useMyFlags } from "../api/flagQueries";
import type { MyFlag } from "../api/flagsApi";
import { reasonLabel, statusLabels } from "../flagCopy";
import styles from "./flags.module.css";

const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: "medium" });

/**
 * /settings/reports ("My reports", F15): every question the user reported, what they said, and
 * where it stands. Once resolved, the moderator's note is shown; the flag_resolved notification
 * links here.
 */
export function MyReports() {
  const reports = useMyFlags();
  const items = reports.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className={styles.page}>
      <div className={styles.pageHeader}>
        <h1>My reports</h1>
        <Link href="/settings/notifications" className={ui.buttonGhost}>
          Notification settings
        </Link>
      </div>
      {reports.isPending ? (
        <p role="status">Loading your reports…</p>
      ) : reports.isError ? (
        <p role="alert">We couldn&apos;t load your reports. Please refresh the page.</p>
      ) : items.length === 0 ? (
        <p className={ui.card}>
          You haven&apos;t reported any questions. Use &ldquo;Report issue&rdquo; on a question if something looks wrong.
        </p>
      ) : (
        <>
          <ul className={styles.list}>
            {items.map((report) => (
              <li key={report.id}>
                <ReportCard report={report} />
              </li>
            ))}
          </ul>
          {reports.hasNextPage && (
            <button
              type="button"
              className={ui.buttonSecondary}
              disabled={reports.isFetchingNextPage}
              onClick={() => void reports.fetchNextPage()}
            >
              {reports.isFetchingNextPage ? "Loading…" : "Show more"}
            </button>
          )}
        </>
      )}
    </div>
  );
}

function ReportCard({ report }: { report: MyFlag }) {
  return (
    <article className={`${ui.card} ${styles.report}`}>
      <div className={styles.reportHeader}>
        <h2>{reasonLabel(report.reason)}</h2>
        <span className={ui.badge}>{statusLabels[report.status]}</span>
      </div>
      <p className={styles.stem}>{report.stemPreview}</p>
      {report.comment && <p className={ui.muted}>You wrote: {report.comment}</p>}
      <dl className={styles.meta}>
        <div>
          <dt>Reported</dt>
          <dd>
            <time dateTime={report.createdAt}>{dateFormat.format(new Date(report.createdAt))}</time>
          </dd>
        </div>
        {report.resolvedAt && (
          <div>
            <dt>Reviewed</dt>
            <dd>
              <time dateTime={report.resolvedAt}>{dateFormat.format(new Date(report.resolvedAt))}</time>
            </dd>
          </div>
        )}
      </dl>
      {report.resolutionNote && <p className={styles.note}>{report.resolutionNote}</p>}
    </article>
  );
}
