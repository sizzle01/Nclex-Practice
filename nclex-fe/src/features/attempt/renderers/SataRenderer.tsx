"use client";

import { useId } from "react";
import styles from "./renderers.module.css";
import type { ItemRendererProps, Option } from "./types";

interface SataContent {
  options: Option[];
}

interface SataResponse {
  selected: string[];
}

/**
 * Select all that apply (interfaces.md 2.2): a group of checkboxes, Tab between them and Space to
 * toggle (interfaces.md section 4).
 */
export function SataRenderer({
  content,
  response,
  onChange,
  disabled,
  struckOptions,
  labelledBy,
}: ItemRendererProps<SataContent, SataResponse>) {
  const instructionsId = useId();
  const selected = response?.selected ?? [];

  function toggle(optionId: string) {
    const next = selected.includes(optionId) ? selected.filter((id) => id !== optionId) : [...selected, optionId];
    // Keep the content's order, so the same choice always saves the same answer.
    const ordered = content.options.map((o) => o.id).filter((id) => next.includes(id));
    onChange(ordered.length > 0 ? { selected: ordered } : null);
  }

  return (
    <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId}>
      <p id={instructionsId} className={styles.instructions}>
        Select all that apply.
      </p>
      <div className={styles.options}>
        {content.options.map((option) => (
          <label key={option.id} className={styles.option}>
            <input
              type="checkbox"
              value={option.id}
              checked={selected.includes(option.id)}
              disabled={disabled}
              onChange={() => toggle(option.id)}
            />
            <span className={struckOptions.includes(option.id) ? styles.struck : undefined}>{option.text}</span>
          </label>
        ))}
      </div>
    </div>
  );
}
