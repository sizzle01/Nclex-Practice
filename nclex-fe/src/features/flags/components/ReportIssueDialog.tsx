"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useId, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { useReportIssue } from "../api/flagQueries";
import { reasonOptions } from "../flagCopy";
import { maxTextLength, type ReportIssueValues, reportIssueSchema } from "../schemas";
import styles from "./flags.module.css";
import { useDialogFocus } from "./useDialogFocus";

interface ReportIssueDialogProps {
  questionVersionId: string;
  /** The attempt the question was seen in, so moderators can see the context. */
  attemptId?: string;
  onClose: () => void;
}

/**
 * "Report issue" (F15): the candidate picks what is wrong with the question and may add details.
 * After sending, the dialog says so and points to "My reports". A second report with the same
 * reason is refused by the API (409 FLAG_DUPLICATE) and explained here.
 */
export function ReportIssueDialog({ questionVersionId, attemptId, onClose }: ReportIssueDialogProps) {
  const titleId = useId();
  const commentId = useId();
  const dialogRef = useRef<HTMLDivElement>(null);
  const onKeyDown = useDialogFocus(dialogRef, onClose);
  const report = useReportIssue();
  const [formError, setFormError] = useState<string | null>(null);
  const { register, handleSubmit, formState } = useForm<ReportIssueValues>({
    resolver: zodResolver(reportIssueSchema),
    defaultValues: { comment: "" },
  });

  const onSubmit = handleSubmit(async ({ reason, comment }) => {
    setFormError(null);
    try {
      await report.mutateAsync({ questionVersionId, reason, comment: comment || undefined, attemptId });
    } catch (error) {
      setFormError(
        isApiError(error, "FLAG_DUPLICATE")
          ? "You already reported this question for that reason. You can follow it in My reports."
          : "We couldn't send your report. Please try again.",
      );
    }
  });

  return (
    <div className={styles.backdrop}>
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={`${ui.card} ${styles.dialog}`}
        onKeyDown={onKeyDown}
      >
        <h2 id={titleId}>Report an issue</h2>
        {report.isSuccess ? (
          <>
            <p role="status">Thanks — your report was sent. We&apos;ll let you know when we&apos;ve reviewed it.</p>
            <div className={styles.actions}>
              <Link href="/settings/reports" className={ui.buttonGhost}>
                My reports
              </Link>
              <button type="button" className={ui.button} onClick={onClose}>
                Close
              </button>
            </div>
          </>
        ) : (
          <form className={formStyles.form} onSubmit={onSubmit} noValidate>
            <fieldset className={formStyles.fieldset} aria-describedby={formState.errors.reason ? `${titleId}-reason-error` : undefined}>
              <legend>What is wrong with this question?</legend>
              {reasonOptions.map((option) => (
                <label key={option.value} className={formStyles.radio}>
                  <input type="radio" value={option.value} {...register("reason")} />
                  {option.label}
                </label>
              ))}
              {formState.errors.reason && (
                <p id={`${titleId}-reason-error`} className={formStyles.error}>
                  {formState.errors.reason.message}
                </p>
              )}
            </fieldset>
            <div className={formStyles.field}>
              <label htmlFor={commentId}>Details (optional)</label>
              <textarea
                id={commentId}
                className={styles.textarea}
                maxLength={maxTextLength}
                aria-invalid={formState.errors.comment ? true : undefined}
                aria-describedby={formState.errors.comment ? `${commentId}-error` : undefined}
                {...register("comment")}
              />
              {formState.errors.comment && (
                <p id={`${commentId}-error`} className={formStyles.error}>
                  {formState.errors.comment.message}
                </p>
              )}
            </div>
            {formError && (
              <p role="alert" className={formStyles.formError}>
                {formError}
              </p>
            )}
            <div className={styles.actions}>
              <button type="button" className={ui.buttonSecondary} onClick={onClose}>
                Cancel
              </button>
              <button type="submit" className={ui.button} disabled={report.isPending}>
                {report.isPending ? "Sending…" : "Send report"}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
