"use client";

import type { Option } from "@/features/player/renderers/types";
import { toggle } from "./ChoiceForms";
import { OptionListEditor } from "./OptionListEditor";
import type { ItemFormProps } from "./payloads";
import styles from "./itemForms.module.css";

/** Each wing of the bowtie has two slots, so exactly two actions and two parameters are correct. */
const SLOTS_PER_WING = 2;

/**
 * Bowtie (interfaces.md 2.x): three option lists, one correct condition, and two correct actions
 * and parameters. Ids carry their list's prefix (c1, a1, p1), so they stay unique across the item,
 * as the rationale needs.
 */
export function BowtieForm({ value, onChange, disabled }: ItemFormProps<"bowtie">) {
  const { content, answerKey } = value;

  function wingMark(list: "actions" | "parameters", option: Option) {
    const correct = answerKey[list];
    const checked = correct.includes(option.id);
    return (
      <label className={styles.mark}>
        <input
          type="checkbox"
          checked={checked}
          // Once two are ticked, untick one before choosing another.
          disabled={!checked && correct.length >= SLOTS_PER_WING}
          onChange={() => onChange({ content, answerKey: { ...answerKey, [list]: toggle(correct, option.id) } })}
        />
        Correct
      </label>
    );
  }

  return (
    <>
      <p className={styles.hint}>
        Candidates place 1 condition, 2 actions to take and 2 parameters to monitor. Give each list 4 to 8 options (the
        exam uses 4 conditions and 5 of each of the others); candidates see every list shuffled.
      </p>
      <OptionListEditor
        legend="Potential conditions"
        options={content.condition}
        idPrefix="c"
        disabled={disabled}
        onChange={(condition) =>
          onChange({
            content: { ...content, condition },
            answerKey: { ...answerKey, condition: condition.some((o) => o.id === answerKey.condition) ? answerKey.condition : "" },
          })
        }
        renderMark={(option) => (
          <label className={styles.mark}>
            <input
              type="radio"
              name="bowtie-condition"
              checked={answerKey.condition === option.id}
              onChange={() => onChange({ content, answerKey: { ...answerKey, condition: option.id } })}
            />
            Correct
          </label>
        )}
      />
      <OptionListEditor
        legend="Actions to take"
        options={content.actions}
        idPrefix="a"
        disabled={disabled}
        onChange={(actions) => onChange({ content: { ...content, actions }, answerKey: { ...answerKey, actions: keep(answerKey.actions, actions) } })}
        renderMark={(option) => wingMark("actions", option)}
      />
      <OptionListEditor
        legend="Parameters to monitor"
        options={content.parameters}
        idPrefix="p"
        disabled={disabled}
        onChange={(parameters) =>
          onChange({ content: { ...content, parameters }, answerKey: { ...answerKey, parameters: keep(answerKey.parameters, parameters) } })
        }
        renderMark={(option) => wingMark("parameters", option)}
      />
    </>
  );
}

// Drops key entries for options that were removed.
function keep(ids: string[], options: Option[]): string[] {
  return ids.filter((id) => options.some((o) => o.id === id));
}
