"use client";

import styles from "./renderers.module.css";
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
  labelledBy,
}: ItemRendererProps<MultipleChoiceContent, MultipleChoiceResponse>) {
  const name = `${labelledBy}-choice`;
  return (
    <div role="radiogroup" aria-labelledby={labelledBy} className={styles.options}>
      {content.options.map((option) => (
        <label key={option.id} className={styles.option}>
          <input
            type="radio"
            name={name}
            value={option.id}
            checked={response?.selected === option.id}
            disabled={disabled}
            onChange={() => onChange({ selected: option.id })}
          />
          <span className={struckOptions.includes(option.id) ? styles.struck : undefined}>{option.text}</span>
        </label>
      ))}
    </div>
  );
}
