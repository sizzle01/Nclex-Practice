"use client";

import Link from "next/link";
import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { type AttemptSummary, useDiscardAttempt, useDrafts } from "@/features/attempt/api/attemptApi";
import { toNumber } from "@/lib/format/toNumber";
import styles from "./DraftsList.module.css";

/** Answered share of a draft, rounded down, for the "% complete" line. */
export function percentComplete(draft: Pick<AttemptSummary, "answeredItems" | "totalItems">) {
  const total = toNumber(draft.totalItems);
  return total === 0 ? 0 : Math.floor((toNumber(draft.answeredItems) / total) * 100);
}

/**
 * Tests the candidate can pick up again (interfaces.md 5.2, dashboard). Drafts untouched for over
 * 30 days ask "resume or discard?" (architecture.md 5.3); discarding asks for confirmation first.
 */
export function DraftsList() {
  const { data, error, isPending } = useDrafts();

  if (isPending) return <p className={ui.muted}>Loading your tests…</p>;
  if (error)
    return (
      <p role="alert" className={styles.error}>
        Your tests in progress could not be loaded.
      </p>
    );
  if (data.items.length === 0) return <p className={ui.muted}>You have no tests in progress.</p>;

  return (
    <ul className={styles.list} aria-label="Tests in progress">
      {data.items.map((draft) => (
        <li key={draft.id}>
          <DraftRow draft={draft} />
        </li>
      ))}
    </ul>
  );
}

function DraftRow({ draft }: { draft: AttemptSummary }) {
  const discard = useDiscardAttempt();
  const [confirming, setConfirming] = useState(false);
  const percent = percentComplete(draft);
  const titleId = `draft-${draft.id}`;

  return (
    <article aria-labelledby={titleId} className={styles.draft}>
      <div className={styles.draftHeader}>
        <h3 id={titleId} className={styles.draftTitle}>
          {draft.testSet.title}
        </h3>
        <span>{percent}% complete</span>
      </div>
      <progress className={styles.progress} max={100} value={percent} aria-labelledby={titleId} />
      {draft.staleDraft && (
        <p className={styles.stale}>You haven&apos;t worked on this test for over 30 days. Resume it or discard it?</p>
      )}

      {confirming ? (
        <div role="group" aria-label="Confirm discard" className={styles.actions}>
          <span>Discard this test? Your answers will be lost.</span>
          <button type="button" className={ui.button} disabled={discard.isPending} onClick={() => discard.mutate(draft.id)}>
            {discard.isPending ? "Discarding…" : "Yes, discard"}
          </button>
          <button type="button" className={ui.buttonGhost} onClick={() => setConfirming(false)}>
            Keep it
          </button>
        </div>
      ) : (
        <div className={styles.actions}>
          <Link href={`/attempts/${draft.id}`} className={ui.buttonSecondary} aria-describedby={titleId}>
            Resume
          </Link>
          {draft.staleDraft && (
            <button type="button" className={ui.buttonGhost} aria-describedby={titleId} onClick={() => setConfirming(true)}>
              Discard
            </button>
          )}
        </div>
      )}
      {discard.error && (
        <p role="alert" className={styles.error}>
          The test could not be discarded. Please try again.
        </p>
      )}
    </article>
  );
}
