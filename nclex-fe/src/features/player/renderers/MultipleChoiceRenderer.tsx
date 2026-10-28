"use client";

import { ChoiceOption } from "./ChoiceOption";
import styles from "./renderers.module.css";
import { optionMarks } from "./revealMarks";
import type { ItemRendererProps, Option } from "./types";

interface MultipleChoiceContent {
  options: Option[];
}

interface MultipleChoiceResponse {
  selected: string | null;
}

/**
 * One answer from a list (interfaces.md 2.1). Native radio buttons give the arrow-key model and
 * radiogroup semantics that interfaces.md section 4 asks for.
 */
export function MultipleChoiceRenderer({
  content,
  response,
  onChange,
  disabled,
  struckOptions,
  onToggleStrike,
  labelledBy,
  reveal,
}: ItemRendererProps<MultipleChoiceContent, MultipleChoiceResponse>) {
  const name = `${labelledBy}-choice`;
  const marks = optionMarks(reveal);
  return (
    <div role="radiogroup" aria-labelledby={labelledBy} className={styles.options}>
      {content.options.map((option) => (
        <ChoiceOption
          key={option.id}
          type="radio"
          name={name}
          option={option}
          checked={response?.selected === option.id}
          disabled={disabled}
          onChange={() => onChange({ selected: option.id })}
          struck={struckOptions.includes(option.id)}
          showStrike={!disabled && !reveal}
          onToggleStrike={onToggleStrike}
          mark={marks[option.id]}
        />
      ))}
    </div>
  );
}
