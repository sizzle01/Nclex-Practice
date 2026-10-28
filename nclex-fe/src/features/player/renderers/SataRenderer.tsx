"use client";

import { useId } from "react";
import { ChoiceOption } from "./ChoiceOption";
import styles from "./renderers.module.css";
import { optionMarks } from "./revealMarks";
import { toggleIn } from "./selection";
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
  onToggleStrike,
  labelledBy,
  reveal,
}: ItemRendererProps<SataContent, SataResponse>) {
  const instructionsId = useId();
  const selected = response?.selected ?? [];
  const marks = optionMarks(reveal);
  const order = content.options.map((o) => o.id);

  function toggle(optionId: string) {
    const next = toggleIn(selected, optionId, order);
    onChange(next.length > 0 ? { selected: next } : null);
  }

  return (
    <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId}>
      <p id={instructionsId} className={styles.instructions}>
        Select all that apply.
      </p>
      <div className={styles.options}>
        {content.options.map((option) => (
          <ChoiceOption
            key={option.id}
            type="checkbox"
            option={option}
            checked={selected.includes(option.id)}
            disabled={disabled}
            onChange={() => toggle(option.id)}
            struck={struckOptions.includes(option.id)}
            showStrike={!disabled && !reveal}
            onToggleStrike={onToggleStrike}
            mark={marks[option.id]}
          />
        ))}
      </div>
    </div>
  );
}
