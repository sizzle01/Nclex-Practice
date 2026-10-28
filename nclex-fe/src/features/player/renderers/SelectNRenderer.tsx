"use client";

import { useId } from "react";
import { ChoiceOption } from "./ChoiceOption";
import styles from "./renderers.module.css";
import { optionMarks } from "./revealMarks";
import { toggleIn } from "./selection";
import type { ItemRendererProps, Option } from "./types";

interface SelectNContent {
  options: Option[];
  selectCount: number;
}

interface SelectNResponse {
  selected: string[];
}

/**
 * Select N (interfaces.md 2.3): checkboxes, like SATA, but the response may hold at most N ids.
 * Once N are ticked the rest are disabled, and the instructions say how many are left, so the
 * limit is explained rather than silent.
 */
export function SelectNRenderer({
  content,
  response,
  onChange,
  disabled,
  struckOptions,
  onToggleStrike,
  labelledBy,
  reveal,
}: ItemRendererProps<SelectNContent, SelectNResponse>) {
  const instructionsId = useId();
  const selected = response?.selected ?? [];
  const marks = optionMarks(reveal);
  const order = content.options.map((o) => o.id);
  const limit = Number(content.selectCount);
  const full = selected.length >= limit;

  function toggle(optionId: string) {
    const next = toggleIn(selected, optionId, order);
    onChange(next.length > 0 ? { selected: next } : null);
  }

  return (
    <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId}>
      <p id={instructionsId} className={styles.instructions}>
        Select {limit}.{" "}
        {full
          ? `You have selected ${limit}; untick one to change your answer.`
          : `${selected.length} of ${limit} selected.`}
      </p>
      <div className={styles.options}>
        {content.options.map((option) => {
          const checked = selected.includes(option.id);
          return (
            <ChoiceOption
              key={option.id}
              type="checkbox"
              option={option}
              checked={checked}
              disabled={disabled || (full && !checked)}
              onChange={() => toggle(option.id)}
              struck={struckOptions.includes(option.id)}
              showStrike={!disabled && !reveal}
              onToggleStrike={onToggleStrike}
              mark={marks[option.id]}
            />
          );
        })}
      </div>
    </div>
  );
}
