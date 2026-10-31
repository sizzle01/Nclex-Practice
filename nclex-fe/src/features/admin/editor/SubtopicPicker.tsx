"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { type AdminQuestionView, adminKeys, listSubtopics, setQuestionSubtopics } from "../api/adminApi";
import styles from "./editor.module.css";

const maxTags = 3;

/**
 * The question's subtopic tags. They belong to the question rather than a version, so they save on
 * their own, even on a published question, and need no new version (docs/features/F17).
 */
export function SubtopicPicker({ question, editable }: { question: AdminQuestionView; editable: boolean }) {
  const queryClient = useQueryClient();
  const clientNeedsId = question.versions[0].clientNeedsId;
  const [selected, setSelected] = useState<string[]>(question.subtopics.map((s) => s.id));
  const [saved, setSaved] = useState(false);
  const { data, isPending, isError } = useQuery({
    queryKey: adminKeys.subtopics({ clientNeedsId: Number(clientNeedsId) }),
    queryFn: () => listSubtopics({ clientNeedsId: Number(clientNeedsId) }),
    enabled: editable,
  });
  const mutation = useMutation({
    mutationFn: (ids: string[]) => setQuestionSubtopics(question.id, ids),
    onSuccess(updated) {
      queryClient.setQueryData(adminKeys.question(updated.id), updated);
      setSaved(true);
    },
  });

  if (!editable) {
    return (
      <section className={styles.workflow} aria-labelledby="subtopics-heading">
        <h2 id="subtopics-heading" className={styles.panelTitle}>
          Subtopics
        </h2>
        {question.subtopics.length === 0 ? <p>None yet.</p> : <p>{question.subtopics.map((s) => s.name).join(", ")}</p>}
      </section>
    );
  }

  const options = (data?.items ?? []).filter((s) => s.isActive);
  const chosen = selected.filter((id) => options.some((s) => s.id === id));
  // Tags left behind when the draft moved category or a subtopic was deactivated.
  const outdated = question.subtopics.filter((s) => !isPending && !options.some((o) => o.id === s.id));

  function toggle(id: string, checked: boolean) {
    setSaved(false);
    setSelected(checked ? [...chosen, id] : chosen.filter((other) => other !== id));
  }

  return (
    <section className={styles.workflow} aria-labelledby="subtopics-heading">
      <h2 id="subtopics-heading" className={styles.panelTitle}>
        Subtopics
      </h2>
      {isPending ? (
        <p role="status">Loading subtopics…</p>
      ) : isError ? (
        <p role="alert">The subtopics could not be loaded.</p>
      ) : options.length === 0 ? (
        <p>This Client Needs subcategory has no subtopics yet. A reviewer can add them under Subtopics.</p>
      ) : (
        <fieldset className={styles.fields}>
          <legend>Choose 1 to {maxTags} topics this question tests. Candidates see these names after a test.</legend>
          {options.map((subtopic) => (
            <label key={subtopic.id}>
              <input
                type="checkbox"
                checked={chosen.includes(subtopic.id)}
                disabled={!chosen.includes(subtopic.id) && chosen.length >= maxTags}
                onChange={(event) => toggle(subtopic.id, event.target.checked)}
              />{" "}
              {subtopic.name}
            </label>
          ))}
        </fieldset>
      )}
      {outdated.length > 0 && <p role="status">No longer usable here: {outdated.map((s) => s.name).join(", ")}. Choose replacements and save.</p>}
      <div className={styles.actions}>
        <button type="button" className={ui.buttonSecondary} disabled={chosen.length === 0 || mutation.isPending} onClick={() => mutation.mutate(chosen)}>
          Save subtopics
        </button>
        {saved && <span role="status">Saved.</span>}
      </div>
      {mutation.isError && (
        <p role="alert" className={styles.error}>
          {isApiError(mutation.error) ? mutation.error.message : "Something went wrong. Please try again."}
        </p>
      )}
    </section>
  );
}
