"use client";

import { type Blank, BlankSelect, withBlank } from "./BlankSelect";
import styles from "./renderers.module.css";
import type { ItemRendererProps } from "./types";

interface DropdownClozeContent {
  /** Text with placeholders such as {{b1}}, each replaced by the blank with that id. */
  template: string;
  blanks: Blank[];
}

interface DropdownResponse {
  byBlank: Record<string, string | null>;
}

const placeholder = /\{\{([^}]+)\}\}/g;

/**
 * Drop-down cloze (interfaces.md 2.6): the template with a select in place of each placeholder.
 * Each select is named after its sentence, e.g. "Blank 1 of 2: The client is experiencing blank,
 * as shown by …", so a screen reader user knows what they are completing.
 */
export function DropdownClozeRenderer({
  content,
  response,
  onChange,
  disabled,
  labelledBy,
  reveal,
}: ItemRendererProps<DropdownClozeContent, DropdownResponse>) {
  const byBlank = response?.byBlank ?? {};
  const blanks = new Map(content.blanks.map((blank) => [blank.id, blank]));
  const labels = blankLabels(content.template);
  // split() with a capture group alternates text and blank ids: even indexes are text, odd are ids.
  const parts = content.template.split(placeholder);

  return (
    <div role="group" aria-labelledby={labelledBy}>
      <p className={styles.instructions}>Choose the option that best completes each blank.</p>
      <p className={styles.cloze}>
        {parts.map((part, index) => {
          if (index % 2 === 0) return <span key={index}>{part}</span>;
          const blank = blanks.get(part);
          if (!blank) return null;
          return (
            <BlankSelect
              key={part}
              blank={blank}
              value={byBlank[part] ?? null}
              label={labels.get(part) ?? `Blank ${part}`}
              disabled={disabled}
              onChange={(optionId) => onChange(withBlank(byBlank, part, optionId))}
              reveal={reveal}
            />
          );
        })}
      </p>
    </div>
  );
}

/** "Blank n of N: <its sentence>", with this blank read as "blank" and any others as "…". */
export function blankLabels(template: string): Map<string, string> {
  const ids = [...template.matchAll(placeholder)].map((match) => match[1]);
  const labels = new Map<string, string>();
  for (const sentence of template.split(/(?<=[.!?])\s+/)) {
    for (const [, id] of sentence.matchAll(placeholder)) {
      const context = sentence.replace(placeholder, (_, other: string) => (other === id ? "blank" : "…")).trim();
      labels.set(id, `Blank ${ids.indexOf(id) + 1} of ${ids.length}: ${context}`);
    }
  }
  return labels;
}
