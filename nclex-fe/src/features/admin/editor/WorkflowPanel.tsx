"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useId, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import { isApiError } from "@/lib/api/errors";
import {
  type AdminQuestionView,
  type AdminVersionView,
  adminKeys,
  createVersion,
  retireQuestion,
  runWorkflowStep,
  type WorkflowStep,
} from "../api/adminApi";
import { canAuthor, canReview } from "../roles";
import { statusLabel } from "../statusLabels";
import styles from "./editor.module.css";

interface WorkflowPanelProps {
  question: AdminQuestionView;
  version: AdminVersionView;
  /** Called with the question after an action, e.g. to select its new version. */
  onChanged(question: AdminQuestionView): void;
}

/**
 * The review workflow for the version on screen (architecture.md section 7): submit, approve or
 * reject with notes, publish, start a new version, retire. Only the steps the signed-in person may
 * take now are offered; the API enforces the same rules (api.md section 8).
 */
export function WorkflowPanel({ question, version, onChanged }: WorkflowPanelProps) {
  const notesId = useId();
  const { data: me } = useMe();
  const queryClient = useQueryClient();
  const [notes, setNotes] = useState("");
  const [confirmRetire, setConfirmRetire] = useState(false);
  const mutation = useMutation({
    mutationFn: (action: () => Promise<AdminQuestionView>) => action(),
    onSuccess(updated) {
      queryClient.setQueryData(adminKeys.question(updated.id), updated);
      void queryClient.invalidateQueries({ queryKey: ["admin", "questions"] });
      setNotes("");
      setConfirmRetire(false);
      onChanged(updated);
    },
  });

  const role = me?.role;
  const retired = question.status === "retired";
  const latest = question.versions[0];
  const ownVersion = me?.id === version.authorId;
  const anyOpen = question.versions.some((v) => v.status === "draft" || v.status === "in_review" || v.status === "approved");
  const step = (s: WorkflowStep) => mutation.mutate(() => runWorkflowStep(version.id, s, notes.trim() || undefined));

  return (
    <section className={styles.workflow} aria-labelledby="workflow-heading">
      <h2 id="workflow-heading" className={styles.panelTitle}>
        Version {String(version.version)}: {statusLabel(version.status)}
        {retired && " · question retired"}
      </h2>

      {!retired && version.status === "draft" && canAuthor(role) && (
        <p className={ui.muted}>Save your changes first, then submit this draft so a reviewer can check it.</p>
      )}

      {!retired && version.status === "in_review" && canReview(role) && ownVersion && (
        <p className={ui.muted}>You wrote this version, so another reviewer must review it.</p>
      )}

      {!retired && version.status === "in_review" && canReview(role) && !ownVersion && (
        <div>
          <label htmlFor={notesId}>Review notes (required to reject)</label>
          <textarea id={notesId} rows={3} value={notes} onChange={(event) => setNotes(event.target.value)} />
        </div>
      )}

      <div className={styles.actions}>
        {!retired && version.status === "draft" && canAuthor(role) && (
          <button type="button" className={ui.button} disabled={mutation.isPending} onClick={() => step("submit-for-review")}>
            Submit for review
          </button>
        )}
        {!retired && version.status === "in_review" && canReview(role) && !ownVersion && (
          <>
            <button type="button" className={ui.button} disabled={mutation.isPending} onClick={() => step("approve")}>
              Approve
            </button>
            <button type="button" className={ui.buttonSecondary} disabled={mutation.isPending || notes.trim() === ""} onClick={() => step("reject")}>
              Reject
            </button>
          </>
        )}
        {!retired && version.status === "approved" && canReview(role) && (
          <button type="button" className={ui.button} disabled={mutation.isPending} onClick={() => step("publish")}>
            Publish
          </button>
        )}
        {!retired && !anyOpen && canAuthor(role) && version.id === latest.id && (
          <button type="button" className={ui.buttonSecondary} disabled={mutation.isPending} onClick={() => mutation.mutate(() => createVersion(question.id))}>
            Start a new version
          </button>
        )}
        {!retired && canReview(role) &&
          (confirmRetire ? (
            <>
              <span>Retire this question from all future sets?</span>
              <button type="button" className={ui.buttonSecondary} disabled={mutation.isPending} onClick={() => mutation.mutate(() => retireQuestion(question.id))}>
                Yes, retire it
              </button>
              <button type="button" className={ui.buttonGhost} onClick={() => setConfirmRetire(false)}>
                Keep it
              </button>
            </>
          ) : (
            <button type="button" className={ui.buttonGhost} onClick={() => setConfirmRetire(true)}>
              Retire question
            </button>
          ))}
      </div>

      {mutation.isError && (
        <div role="alert" className={styles.error}>
          <p>{isApiError(mutation.error) ? mutation.error.message : "Something went wrong. Please try again."}</p>
          {isApiError(mutation.error) && mutation.error.fieldErrors.length > 0 && (
            <ul>
              {mutation.error.fieldErrors.map((e) => (
                <li key={`${e.field}:${e.code}`}>
                  <code>{e.field}</code>: {e.message ?? e.code}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </section>
  );
}
