"use client";

import ui from "@/components/ui/ui.module.css";
import { toggle } from "./ChoiceForms";
import { OptionListEditor } from "./OptionListEditor";
import { type ItemFormProps, nextId } from "./payloads";
import styles from "./itemForms.module.css";

/**
 * Multiple response grouping (interfaces.md 2.5): labelled groups of options, scored per group.
 * Option ids carry their group's id ("g1a"), so they stay unique across the whole item.
 */
export function GroupingForm({ value, onChange, disabled }: ItemFormProps<"multiple_response_grouping">) {
  const { content, answerKey } = value;
  const groups = content.groups;

  function setGroups(next: typeof groups) {
    const byGroup = Object.fromEntries(
      next.map((g) => [g.id, (answerKey.byGroup[g.id] ?? []).filter((id) => g.options.some((o) => o.id === id))]),
    );
    onChange({ content: { groups: next }, answerKey: { byGroup } });
  }

  return (
    <>
      {groups.map((group, index) => (
        <div key={group.id} className={styles.group}>
          <div className={styles.row}>
            <label className={styles.grow}>
              Group {group.id} label
              <input
                disabled={disabled}
                value={group.label}
                onChange={(event) => setGroups(groups.map((g, i) => (i === index ? { ...g, label: event.target.value } : g)))}
              />
            </label>
            <button type="button" className={ui.buttonGhost} disabled={disabled} onClick={() => setGroups(groups.filter((_, i) => i !== index))}>
              Remove group {group.id}
            </button>
          </div>
          <OptionListEditor
            legend={`Group ${group.id} options`}
            options={group.options}
            idPrefix={group.id}
            disabled={disabled}
            onChange={(options) => setGroups(groups.map((g, i) => (i === index ? { ...g, options } : g)))}
            renderMark={(option) => (
              <label className={styles.mark}>
                <input
                  type="checkbox"
                  checked={(answerKey.byGroup[group.id] ?? []).includes(option.id)}
                  onChange={() =>
                    onChange({
                      content,
                      answerKey: { byGroup: { ...answerKey.byGroup, [group.id]: toggle(answerKey.byGroup[group.id] ?? [], option.id) } },
                    })
                  }
                />
                Correct
              </label>
            )}
          />
        </div>
      ))}
      <button
        type="button"
        className={ui.buttonSecondary}
        disabled={disabled}
        onClick={() => setGroups([...groups, { id: nextId(groups.map((g) => g.id), "g"), label: "", options: [] }])}
      >
        Add group
      </button>
    </>
  );
}
