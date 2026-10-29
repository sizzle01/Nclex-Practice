"use client";

import type { ItemFormProps } from "./payloads";
import { OptionListEditor } from "./OptionListEditor";
import styles from "./itemForms.module.css";

/** Multiple choice (interfaces.md 2.1): options and the one correct option. */
export function MultipleChoiceForm({ value, onChange, disabled }: ItemFormProps<"multiple_choice">) {
  const { content, answerKey } = value;
  return (
    <OptionListEditor
      legend="Options"
      options={content.options}
      disabled={disabled}
      onChange={(options) => onChange({ content: { options }, answerKey })}
      renderMark={(option) => (
        <label className={styles.mark}>
          <input
            type="radio"
            name="mc-correct"
            checked={answerKey.correct === option.id}
            onChange={() => onChange({ content, answerKey: { correct: option.id } })}
          />
          Correct
        </label>
      )}
    />
  );
}

/** Select all that apply (2.2): options, any number of them correct. */
export function SataForm({ value, onChange, disabled }: ItemFormProps<"sata">) {
  const { content, answerKey } = value;
  return (
    <OptionListEditor
      legend="Options"
      options={content.options}
      disabled={disabled}
      onChange={(options) => onChange({ content: { options }, answerKey: { correct: keep(answerKey.correct, options) } })}
      renderMark={(option) => (
        <CorrectBox checked={answerKey.correct.includes(option.id)} onToggle={() => onChange({ content, answerKey: { correct: toggle(answerKey.correct, option.id) } })} />
      )}
    />
  );
}

/** Select N (2.3): options, how many to select, and exactly that many correct ones. */
export function SelectNForm({ value, onChange, disabled }: ItemFormProps<"select_n">) {
  const { content, answerKey } = value;
  return (
    <>
      <div className={styles.inline}>
        <label>
          How many to select
          <input
            type="number"
            min={1}
            disabled={disabled}
            value={content.selectCount}
            onChange={(event) => onChange({ content: { ...content, selectCount: Number(event.target.value) }, answerKey })}
          />
        </label>
        <label className={styles.mark}>
          <input
            type="checkbox"
            disabled={disabled}
            checked={answerKey.scoringOverride === "all_or_nothing"}
            onChange={(event) =>
              onChange({ content, answerKey: { ...answerKey, scoringOverride: event.target.checked ? "all_or_nothing" : null } })
            }
          />
          Score all or nothing (default: a point per correct choice)
        </label>
      </div>
      <OptionListEditor
        legend="Options"
        options={content.options}
        disabled={disabled}
        onChange={(options) => onChange({ content: { ...content, options }, answerKey: { ...answerKey, correct: keep(answerKey.correct, options) } })}
        renderMark={(option) => (
          <CorrectBox
            checked={answerKey.correct.includes(option.id)}
            onToggle={() => onChange({ content, answerKey: { ...answerKey, correct: toggle(answerKey.correct, option.id) } })}
          />
        )}
      />
    </>
  );
}

/**
 * Ordered response (2.9): the items in their correct order. Candidates get them shuffled, so the
 * order written here is the key.
 */
export function OrderedResponseForm({ value, onChange, disabled }: ItemFormProps<"ordered_response">) {
  const { content, answerKey } = value;
  return (
    <>
      <p className={styles.hint}>List the steps in the correct order; candidates see them shuffled.</p>
      <div className={styles.inline}>
        <label>
          Scoring
          <select
            disabled={disabled}
            value={answerKey.scoring}
            onChange={(event) => onChange({ content, answerKey: { ...answerKey, scoring: event.target.value as "all_or_nothing" | "per_position" } })}
          >
            <option value="all_or_nothing">One point for the whole sequence</option>
            <option value="per_position">One point per item in its place</option>
          </select>
        </label>
      </div>
      <OptionListEditor
        legend="Items in the correct order"
        options={content.items}
        disabled={disabled}
        reorderable
        onChange={(items) => onChange({ content: { items }, answerKey: { ...answerKey, order: items.map((i) => i.id) } })}
      />
    </>
  );
}

function CorrectBox({ checked, onToggle }: { checked: boolean; onToggle(): void }) {
  return (
    <label className={styles.mark}>
      <input type="checkbox" checked={checked} onChange={onToggle} />
      Correct
    </label>
  );
}

/** Adds or removes one id. */
export function toggle(ids: string[], id: string): string[] {
  return ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id];
}

// Drops key entries for options that were removed.
function keep(ids: string[], options: Array<{ id: string }>): string[] {
  return ids.filter((id) => options.some((o) => o.id === id));
}
