"use client";

import { useMutation } from "@tanstack/react-query";
import { useId, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import type { ItemReveal } from "@/features/attempt/api/attemptApi";
import { RationalePanel } from "@/features/attempt/reveal/RationalePanel";
import { ItemAnswer } from "@/features/player/renderers/ItemAnswer";
import { isApiError } from "@/lib/api/errors";
import { type AdminVersionView, type ItemGrade, previewGrade } from "../api/adminApi";
import styles from "./editor.module.css";

/**
 * "Test the key" (api.md section 8, preview-grade): answer the saved version as a candidate would,
 * and the real grader scores it. The marks and rationale then show exactly what a candidate's
 * reveal would, so an author or reviewer can check partial credit. Nothing is stored.
 */
export function TestKeyPanel({ version }: { version: AdminVersionView }) {
  const stemId = useId();
  const [response, setResponse] = useState<unknown>(null);
  const [grade, setGrade] = useState<ItemGrade | null>(null);
  const mutation = useMutation({
    mutationFn: () => previewGrade(version.id, response),
    onSuccess: setGrade,
  });

  const reveal = grade ? revealFor(version, grade) : null;

  function tryAgain() {
    setGrade(null);
    setResponse(null);
    mutation.reset();
  }

  return (
    <section className={styles.preview} aria-labelledby={`${stemId}-heading`}>
      <h2 id={`${stemId}-heading`} className={styles.panelTitle}>
        Test the key (version {String(version.version)}, as saved)
      </h2>
      <p id={stemId} className={styles.stem}>
        {version.stem}
      </p>
      <ItemAnswer
        itemType={version.itemType}
        content={version.content}
        response={response}
        onChange={setResponse}
        disabled={reveal !== null}
        struckOptions={[]}
        onToggleStrike={() => undefined}
        labelledBy={stemId}
        reveal={reveal}
      />
      <div className={styles.actions}>
        {reveal ? (
          <button type="button" className={ui.buttonSecondary} onClick={tryAgain}>
            Try another answer
          </button>
        ) : (
          <button type="button" className={ui.button} disabled={mutation.isPending} onClick={() => mutation.mutate()}>
            {mutation.isPending ? "Grading…" : "Grade this answer"}
          </button>
        )}
      </div>
      {mutation.isError && (
        <p role="alert" className={styles.error}>
          {problemText(mutation.error)}
        </p>
      )}
      {reveal && <RationalePanel reveal={reveal} itemType={version.itemType} content={version.content} />}
    </section>
  );
}

// The reveal a candidate would get for this grade, built from the version the staff member already has.
function revealFor(version: AdminVersionView, grade: ItemGrade): ItemReveal {
  const rationale = version.rationale as ItemReveal["rationale"];
  return {
    answerKey: version.answerKey,
    grade,
    rationale: { ...rationale, perOption: rationale.perOption ?? {} },
    educationalObjective: version.educationalObjective,
    tags: { clientNeeds: "", bodySystem: "", cjmmSkill: version.cjmmSkill, difficulty: version.difficulty },
  };
}

function problemText(error: unknown): string {
  if (isApiError(error, "VALIDATION_FAILED")) {
    return `The grader can't read this answer: ${error.fieldErrors.map((e) => e.message ?? e.code).join(" ")}`;
  }
  return isApiError(error) ? error.message : "Something went wrong. Please try again.";
}
