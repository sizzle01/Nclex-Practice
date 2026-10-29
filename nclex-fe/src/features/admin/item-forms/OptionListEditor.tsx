"use client";

import type { ReactNode } from "react";
import ui from "@/components/ui/ui.module.css";
import type { Option } from "@/features/player/renderers/types";
import { nextId } from "./payloads";
import styles from "./itemForms.module.css";

interface OptionListEditorProps {
  /** Names the list for screen readers, e.g. "Options" or "Blank b1 choices". */
  legend: string;
  options: Option[];
  onChange(next: Option[]): void;
  disabled: boolean;
  /** Prefix for new ids; letters a, b, c... when empty. */
  idPrefix?: string;
  /** A control beside each option, e.g. the "correct" checkbox. */
  renderMark?: (option: Option, index: number) => ReactNode;
  /** Show move up / move down buttons, for lists whose order matters. */
  reorderable?: boolean;
}

/**
 * Edits a list of options: their text, adding and removing, and optionally their order. Every
 * control is a native button or input with its own label, so the list works from the keyboard
 * (interfaces.md section 4). Ids are kept stable so the key and rationale keep pointing at them.
 */
export function OptionListEditor({
  legend,
  options,
  onChange,
  disabled,
  idPrefix = "",
  renderMark,
  reorderable = false,
}: OptionListEditorProps) {
  function update(index: number, text: string) {
    onChange(options.map((o, i) => (i === index ? { ...o, text } : o)));
  }

  function move(index: number, by: -1 | 1) {
    const next = [...options];
    [next[index], next[index + by]] = [next[index + by], next[index]];
    onChange(next);
  }

  return (
    <fieldset className={styles.list} disabled={disabled}>
      <legend>{legend}</legend>
      <ol className={styles.rows}>
        {options.map((option, index) => (
          <li key={option.id} className={styles.row}>
            <span className={styles.id} aria-hidden="true">
              {option.id}
            </span>
            {renderMark?.(option, index)}
            <input
              className={styles.grow}
              aria-label={`${legend}: ${option.id} text`}
              value={option.text}
              onChange={(event) => update(index, event.target.value)}
            />
            {reorderable && (
              <>
                <button type="button" className={ui.buttonGhost} aria-label={`Move ${option.id} up`} disabled={index === 0} onClick={() => move(index, -1)}>
                  ↑
                </button>
                <button
                  type="button"
                  className={ui.buttonGhost}
                  aria-label={`Move ${option.id} down`}
                  disabled={index === options.length - 1}
                  onClick={() => move(index, 1)}
                >
                  ↓
                </button>
              </>
            )}
            <button type="button" className={ui.buttonGhost} aria-label={`Remove ${option.id}`} onClick={() => onChange(options.filter((_, i) => i !== index))}>
              Remove
            </button>
          </li>
        ))}
      </ol>
      <button
        type="button"
        className={ui.buttonSecondary}
        onClick={() => onChange([...options, { id: nextId(options.map((o) => o.id), idPrefix), text: "" }])}
      >
        Add {idPrefix ? "choice" : "option"}
      </button>
    </fieldset>
  );
}
