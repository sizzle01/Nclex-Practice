"use client";

import type { ItemFormProps } from "./payloads";
import styles from "./itemForms.module.css";

/**
 * Fill in the blank, calculation (interfaces.md 2.8): the unit and decimals candidates see, the
 * correct value, and how close an answer must be. A tolerance, when set, replaces rounding.
 */
export function CalcForm({ value, onChange, disabled }: ItemFormProps<"fill_blank_calc">) {
  const { content, answerKey } = value;
  const tolerance = answerKey.tolerance ?? null;

  return (
    <fieldset className={styles.list} disabled={disabled}>
      <legend>Answer box and key</legend>
      <div className={styles.inline}>
        <label>
          Unit
          <input value={content.unit} onChange={(event) => onChange({ content: { ...content, unit: event.target.value }, answerKey })} />
        </label>
        <label>
          Decimal places
          <input
            type="number"
            min={0}
            max={6}
            value={content.decimals}
            onChange={(event) => onChange({ content: { ...content, decimals: Number(event.target.value) }, answerKey })}
          />
        </label>
        <label>
          Input label (for screen readers)
          <input
            value={content.inputLabel ?? ""}
            onChange={(event) => onChange({ content: { ...content, inputLabel: event.target.value || null }, answerKey })}
          />
        </label>
      </div>
      <div className={styles.inline}>
        <label>
          Correct value
          <input
            type="number"
            step="any"
            value={answerKey.value}
            onChange={(event) => onChange({ content, answerKey: { ...answerKey, value: Number(event.target.value) } })}
          />
        </label>
        <label>
          Rounding
          <select
            value={answerKey.rounding}
            disabled={tolerance !== null}
            onChange={(event) => onChange({ content, answerKey: { ...answerKey, rounding: event.target.value as "exact" | "round" } })}
          >
            <option value="exact">Exact</option>
            <option value="round">Round both to the decimal places</option>
          </select>
        </label>
        <label>
          Tolerance (optional)
          <input
            type="number"
            step="any"
            min={0}
            value={tolerance ?? ""}
            onChange={(event) =>
              onChange({ content, answerKey: { ...answerKey, tolerance: event.target.value === "" ? null : Number(event.target.value) } })
            }
          />
        </label>
      </div>
    </fieldset>
  );
}
