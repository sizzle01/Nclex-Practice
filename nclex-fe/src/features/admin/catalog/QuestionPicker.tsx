"use client";

import { useQuery } from "@tanstack/react-query";
import { useId, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import type { Track } from "@/features/custom-builder/api/builderApi";
import { useDebouncedValue } from "@/features/custom-builder/useDebouncedValue";
import { adminKeys, type MemberQuestion, searchQuestions } from "../api/adminApi";
import styles from "../components/admin.module.css";
import { typeLabel } from "../item-forms/payloads";

interface QuestionPickerProps {
  track: Track;
  /** Questions already chosen; they are not offered again. */
  chosenIds: string[];
  onAdd(question: MemberQuestion): void;
}

/**
 * Finds questions a set or bank may hold: published, active, on the same track (the API refuses
 * anything else). Results are buttons, so they are reachable with Tab and Enter.
 */
export function QuestionPicker({ track, chosenIds, onAdd }: QuestionPickerProps) {
  const headingId = useId();
  const [text, setText] = useState("");
  const q = useDebouncedValue(text.trim(), 300);
  const search = { track, status: "published" as const, questionStatus: "active" as const, q: q || undefined };
  const { data, isPending } = useQuery({ queryKey: adminKeys.questions(search), queryFn: () => searchQuestions(search) });
  const results = (data?.items ?? []).filter((item) => !chosenIds.includes(item.id));

  return (
    <section aria-labelledby={headingId}>
      <h2 id={headingId}>Add questions</h2>
      <label>
        Search published {track} questions
        <input type="search" value={text} onChange={(event) => setText(event.target.value)} />
      </label>
      {isPending ? (
        <p role="status">Searching…</p>
      ) : results.length === 0 ? (
        <p role="status">No more published questions match.</p>
      ) : (
        <ul className={styles.memberList}>
          {results.map((item) => (
            <li key={item.id}>
              <span>
                {item.stem} <span className={styles.status}>{typeLabel(item.itemType)}</span>
              </span>
              <button
                type="button"
                className={ui.buttonSecondary}
                aria-label={`Add: ${item.stem}`}
                onClick={() =>
                  onAdd({ questionId: item.id, stem: item.stem, itemType: item.itemType, status: item.status, publishedVersion: item.publishedVersion })
                }
              >
                Add
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
