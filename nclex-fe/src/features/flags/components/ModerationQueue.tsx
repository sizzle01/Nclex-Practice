"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useId, useState } from "react";
import { useForm } from "react-hook-form";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import { isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import { useFlagQueue, useUpdateFlag } from "../api/flagQueries";
import type { FlagQueueItem, FlagStatus, UpdateFlagRequest } from "../api/flagsApi";
import { editorPath, queueStatuses, queueStatusLabels, reasonLabel } from "../flagCopy";
import { type CloseFlagValues, closeFlagSchema, maxTextLength } from "../schemas";
import styles from "./flags.module.css";

const timeFormat = new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" });

/**
 * /admin/flags (interfaces.md 5.2): candidates' reports by status, oldest first. A moderator can
 * take a report, triage it, resolve it (the reporter is notified with the note) or reject it, and
 * open the question version in the editor.
 */
export function ModerationQueue() {
  const [status, setStatus] = useState<FlagStatus>("open");
  const queue = useFlagQueue(status);
  const items = queue.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className={styles.page}>
      <div className={styles.pageHeader}>
        <h1>Reported questions</h1>
      </div>
      <div role="group" aria-label="Show reports that are" className={styles.tabs}>
        {queueStatuses.map((option) => (
          <button
            key={option}
            type="button"
            className={status === option ? ui.button : ui.buttonSecondary}
            aria-pressed={status === option}
            onClick={() => setStatus(option)}
          >
            {queueStatusLabels[option]}
          </button>
        ))}
      </div>
      {queue.isPending ? (
        <p role="status">Loading reports…</p>
      ) : queue.isError ? (
        <p role="alert">We couldn&apos;t load the reports. Please refresh the page.</p>
      ) : items.length === 0 ? (
        <p className={ui.card}>No {queueStatusLabels[status].toLowerCase()} reports.</p>
      ) : (
        <>
          <ul className={styles.list}>
            {items.map((flag) => (
              <li key={flag.id}>
                <QueueCard flag={flag} />
              </li>
            ))}
          </ul>
          {queue.hasNextPage && (
            <button
              type="button"
              className={ui.buttonSecondary}
              disabled={queue.isFetchingNextPage}
              onClick={() => void queue.fetchNextPage()}
            >
              {queue.isFetchingNextPage ? "Loading…" : "Show more"}
            </button>
          )}
        </>
      )}
    </div>
  );
}

type Closing = "resolved" | "rejected";

function QueueCard({ flag }: { flag: FlagQueueItem }) {
  const titleId = useId();
  const me = useMe();
  const update = useUpdateFlag();
  const [closing, setClosing] = useState<Closing | null>(null);
  const [error, setError] = useState<string | null>(null);
  const closed = flag.status === "resolved" || flag.status === "rejected";
  const assignedToMe = flag.assignee?.id === me.data?.id;

  async function send(body: UpdateFlagRequest) {
    setError(null);
    try {
      await update.mutateAsync({ id: flag.id, body });
      setClosing(null);
    } catch (caught) {
      setError(
        isApiError(caught, "FLAG_CLOSED")
          ? "Someone else has already closed this report."
          : "That change couldn't be saved. Please try again.",
      );
    }
  }

  return (
    <article className={`${ui.card} ${styles.report}`} aria-labelledby={titleId}>
      <div className={styles.reportHeader}>
        <h2 id={titleId}>{reasonLabel(flag.reason)}</h2>
        <span className={ui.badge}>{queueStatusLabels[flag.status]}</span>
      </div>
      <p className={styles.stem}>{flag.question.stemPreview}</p>
      {flag.comment && <p className={styles.note}>{flag.comment}</p>}
      <dl className={styles.meta}>
        <div>
          <dt>Reported by</dt>
          <dd>{flag.reporter.displayName ?? "A candidate"}</dd>
        </div>
        <div>
          <dt>On</dt>
          <dd>
            <time dateTime={flag.createdAt}>{timeFormat.format(new Date(flag.createdAt))}</time>
          </dd>
        </div>
        <div>
          <dt>Version</dt>
          <dd>{toNumber(flag.question.version)}</dd>
        </div>
        <div>
          <dt>Assigned to</dt>
          <dd>{flag.assignee ? (assignedToMe ? "You" : (flag.assignee.displayName ?? "A moderator")) : "Nobody"}</dd>
        </div>
      </dl>
      {flag.resolutionNote && (
        <p className={ui.muted}>
          Note from {flag.resolvedBy?.displayName ?? "a moderator"}: {flag.resolutionNote}
        </p>
      )}

      {error && (
        <p role="alert" className={styles.error}>
          {error}
        </p>
      )}

      {closing ? (
        <CloseForm
          outcome={closing}
          busy={update.isPending}
          onCancel={() => setClosing(null)}
          onSubmit={(note) => send({ status: closing, resolutionNote: note })}
        />
      ) : (
        <div className={styles.actions}>
          <Link href={editorPath(flag.question.questionId, flag.question.versionId)} className={ui.buttonGhost}>
            Open in editor
          </Link>
          {!closed && (
            <>
              {!assignedToMe && me.data && (
                <button
                  type="button"
                  className={ui.buttonSecondary}
                  disabled={update.isPending}
                  onClick={() => void send({ assigneeId: me.data.id })}
                >
                  Assign to me
                </button>
              )}
              {flag.status === "open" && (
                <button
                  type="button"
                  className={ui.buttonSecondary}
                  disabled={update.isPending}
                  onClick={() => void send({ status: "triaged" })}
                >
                  Triage
                </button>
              )}
              <button type="button" className={ui.buttonSecondary} onClick={() => setClosing("rejected")}>
                Reject…
              </button>
              <button type="button" className={ui.button} onClick={() => setClosing("resolved")}>
                Resolve…
              </button>
            </>
          )}
        </div>
      )}
    </article>
  );
}

interface CloseFormProps {
  outcome: Closing;
  busy: boolean;
  onCancel: () => void;
  onSubmit: (note: string) => Promise<void>;
}

/** The note a moderator must write to close a report. On resolve the reporter reads it. */
function CloseForm({ outcome, busy, onCancel, onSubmit }: CloseFormProps) {
  const noteId = useId();
  const { register, handleSubmit, formState } = useForm<CloseFlagValues>({
    resolver: zodResolver(closeFlagSchema),
    defaultValues: { note: "" },
  });
  const resolving = outcome === "resolved";

  return (
    <form className={formStyles.form} onSubmit={handleSubmit(({ note }) => onSubmit(note))} noValidate>
      <div className={formStyles.field}>
        <label htmlFor={noteId}>{resolving ? "Note to the reporter" : "Why no change is needed"}</label>
        <p id={`${noteId}-hint`} className={formStyles.hint}>
          {resolving
            ? "The reporter gets this in a notification."
            : "Kept for moderators; the reporter only sees that we reviewed it."}
        </p>
        <textarea
          id={noteId}
          className={styles.textarea}
          maxLength={maxTextLength}
          autoFocus
          aria-invalid={formState.errors.note ? true : undefined}
          aria-describedby={[`${noteId}-hint`, formState.errors.note ? `${noteId}-error` : null].filter(Boolean).join(" ")}
          {...register("note")}
        />
        {formState.errors.note && (
          <p id={`${noteId}-error`} className={formStyles.error}>
            {formState.errors.note.message}
          </p>
        )}
      </div>
      <div className={styles.actions}>
        <button type="button" className={ui.buttonSecondary} disabled={busy} onClick={onCancel}>
          Cancel
        </button>
        <button type="submit" className={ui.button} disabled={busy}>
          {busy ? "Saving…" : resolving ? "Resolve and notify" : "Reject report"}
        </button>
      </div>
    </form>
  );
}
