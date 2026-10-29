"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import { useMe } from "@/features/auth/AuthProvider";
import { adminKeys, type AdminQuestionView, fetchQuestion, updateDraft } from "../api/adminApi";
import { isAuthorable, typeLabel } from "../item-forms/payloads";
import { canAuthor } from "../roles";
import { DraftForm } from "./DraftForm";
import { formValuesFrom } from "./draftValues";
import styles from "./editor.module.css";
import { TestKeyPanel } from "./TestKeyPanel";
import { VersionHistory } from "./VersionHistory";
import { WorkflowPanel } from "./WorkflowPanel";

/**
 * /admin/questions/[id] (interfaces.md 5.2): version history, the workflow actions, the editor
 * (editable only for a draft and an author) with its live preview, and "Test the key".
 * `?version=` opens a given version, e.g. from a report in the moderation queue (F15).
 */
export function QuestionEditor({ questionId, initialVersionId }: { questionId: string; initialVersionId?: string }) {
  const queryClient = useQueryClient();
  const { data: me } = useMe();
  const { data: question, isPending, isError } = useQuery({
    queryKey: adminKeys.question(questionId),
    queryFn: () => fetchQuestion(questionId),
  });
  const [selectedId, setSelectedId] = useState<string | null>(initialVersionId ?? null);

  if (isPending) return <p role="status">Loading the question…</p>;
  if (isError || !question) return <p role="alert">This question could not be loaded.</p>;

  const version = question.versions.find((v) => v.id === selectedId) ?? question.versions[0];
  const editable = version.status === "draft" && canAuthor(me?.role) && question.status === "active";

  function showLatest(updated: AdminQuestionView) {
    setSelectedId(updated.versions[0].id);
  }

  async function save(draft: Parameters<typeof updateDraft>[1]) {
    const updated = await updateDraft(version.id, draft);
    queryClient.setQueryData(adminKeys.question(updated.id), updated);
  }

  return (
    <div className={styles.fields}>
      <div className={styles.header}>
        <h1>{typeLabel(version.itemType)} question</h1>
        <p>
          {question.track} · {question.accessTier} · {question.status} ·{" "}
          {question.currentVersionId ? "published" : "not published yet"} · <Link href="/admin/questions">All questions</Link>
        </p>
      </div>
      <VersionHistory versions={question.versions} selectedId={version.id} onSelect={setSelectedId} />
      <WorkflowPanel question={question} version={version} onChanged={showLatest} />
      {isAuthorable(version.itemType) ? (
        <DraftForm
          // A fresh form per version, so switching versions shows that version's own values.
          key={version.id}
          track={question.track}
          initialValues={formValuesFrom(version)}
          readOnly={!editable}
          saveLabel="Save draft"
          onSave={save}
        />
      ) : (
        <p>This item type can&apos;t be edited in the console yet.</p>
      )}
      <TestKeyPanel key={`key-${version.id}`} version={version} />
    </div>
  );
}
