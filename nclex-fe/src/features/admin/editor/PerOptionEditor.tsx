"use client";

import type { RationaleTarget } from "../item-forms/rationaleTargets";
import styles from "../item-forms/itemForms.module.css";

interface PerOptionEditorProps {
  targets: RationaleTarget[];
  value: Record<string, string>;
  onChange(next: Record<string, string>): void;
  disabled: boolean;
}

/**
 * A "why" for every option, cell, blank choice or selectable segment (PRD 5.1). The boxes follow
 * the item's content, so adding an option adds its box; the server refuses a draft with any empty.
 */
export function PerOptionEditor({ targets, value, onChange, disabled }: PerOptionEditorProps) {
  if (targets.length === 0) {
    return <p className={styles.hint}>This item type has no options, so it needs only the overall rationale.</p>;
  }

  return (
    <fieldset className={styles.list} disabled={disabled}>
      <legend>Why each choice is right or wrong</legend>
      {targets.map((target) => (
        <label key={target.id} className={styles.group}>
          <span>
            <span className={styles.id}>{target.id}</span> {target.label}
          </span>
          <textarea rows={2} value={value[target.id] ?? ""} onChange={(event) => onChange({ ...value, [target.id]: event.target.value })} />
        </label>
      ))}
    </fieldset>
  );
}
