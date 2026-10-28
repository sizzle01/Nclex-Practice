"use client";

import { useId } from "react";
import { ChoiceOption } from "./ChoiceOption";
import styles from "./renderers.module.css";
import { optionMarks } from "./revealMarks";
import { compactGroups, toggleIn } from "./selection";
import type { ItemRendererProps, Option } from "./types";

interface GroupingContent {
  groups: Array<{ id: string; label: string; options: Option[] }>;
}

interface GroupedResponse {
  byGroup: Record<string, string[]>;
}

/**
 * Multiple response grouping (interfaces.md 2.5): a fieldset of checkboxes per group, each scored
 * on its own. The legend names the group, so a screen reader hears "Circulation, group" before its
 * options.
 */
export function GroupingRenderer({
  content,
  response,
  onChange,
  disabled,
  struckOptions,
  onToggleStrike,
  labelledBy,
  reveal,
}: ItemRendererProps<GroupingContent, GroupedResponse>) {
  const instructionsId = useId();
  const byGroup = response?.byGroup ?? {};
  const marks = optionMarks(reveal);

  function toggle(groupId: string, optionId: string, order: string[]) {
    const next = { ...byGroup, [groupId]: toggleIn(byGroup[groupId] ?? [], optionId, order) };
    const answered = compactGroups(next, (ids) => ids.length === 0);
    onChange(answered ? { byGroup: answered } : null);
  }

  return (
    <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId} className={styles.groups}>
      <p id={instructionsId} className={styles.instructions}>
        Select all that apply in each group.
      </p>
      {content.groups.map((group) => {
        const order = group.options.map((o) => o.id);
        return (
          <fieldset key={group.id} className={styles.fieldset}>
            <legend className={styles.legend}>{group.label}</legend>
            <div className={styles.options}>
              {group.options.map((option) => (
                <ChoiceOption
                  key={option.id}
                  type="checkbox"
                  option={option}
                  checked={(byGroup[group.id] ?? []).includes(option.id)}
                  disabled={disabled}
                  onChange={() => toggle(group.id, option.id, order)}
                  struck={struckOptions.includes(option.id)}
                  showStrike={!disabled && !reveal}
                  onToggleStrike={onToggleStrike}
                  mark={marks[option.id]}
                />
              ))}
            </div>
          </fieldset>
        );
      })}
    </div>
  );
}
