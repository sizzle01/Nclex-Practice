"use client";

import { useId } from "react";
import { highlightMarkLabels } from "./highlightMarks";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import { optionMarks } from "./revealMarks";
import { toggleIn } from "./selection";
import type { ItemRendererProps } from "./types";

interface HighlightTextContent {
  /** The passage, already split by the authoring tool; only selectable segments can be highlighted. */
  segments: Array<{ id: string; text: string; selectable: boolean }>;
}

interface HighlightResponse {
  selected: string[];
}

/**
 * Highlight text (interfaces.md 2.7): the passage with each selectable segment as a toggle button
 * (aria-pressed), so Tab reaches every segment and Space or Enter highlights it (interfaces.md 4).
 */
export function HighlightTextRenderer({
  content,
  response,
  onChange,
  disabled,
  labelledBy,
  reveal,
}: ItemRendererProps<HighlightTextContent, HighlightResponse>) {
  const instructionsId = useId();
  const selected = response?.selected ?? [];
  const marks = optionMarks(reveal);
  const order = content.segments.filter((s) => s.selectable).map((s) => s.id);

  function toggle(segmentId: string) {
    const next = toggleIn(selected, segmentId, order);
    onChange(next.length > 0 ? { selected: next } : null);
  }

  return (
    <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId}>
      <p id={instructionsId} className={styles.instructions}>
        Select each part of the text you want to highlight. Select it again to remove the highlight.
      </p>
      <p className={styles.passage}>
        {content.segments.map((segment) =>
          segment.selectable ? (
            <span key={segment.id}>
              <button
                type="button"
                className={styles.segment}
                aria-pressed={selected.includes(segment.id)}
                disabled={disabled}
                onClick={() => toggle(segment.id)}
              >
                {segment.text}
              </button>
              <OptionMarkText
                mark={marks[segment.id]}
                label={marks[segment.id] && highlightMarkLabels[marks[segment.id]]}
              />
            </span>
          ) : (
            <span key={segment.id}>{segment.text}</span>
          ),
        )}
      </p>
    </div>
  );
}
