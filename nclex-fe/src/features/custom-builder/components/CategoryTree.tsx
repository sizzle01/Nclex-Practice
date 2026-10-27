"use client";

import { useEffect, useRef } from "react";
import forms from "@/components/forms/forms.module.css";
import type { ClientNeedsNode } from "../api/builderApi";
import styles from "./builder.module.css";

interface CategoryTreeProps {
  categories: ClientNeedsNode[];
  /** Selected leaf ids; parents are never stored, they stand for their leaves. */
  value: number[];
  onChange: (leafIds: number[]) => void;
}

/** A category's leaves: its subcategories, or the category itself when it has none (e.g. Psychosocial Integrity). */
function leavesOf(category: ClientNeedsNode): ClientNeedsNode[] {
  return category.children && category.children.length > 0 ? category.children : [category];
}

function toId(id: number | string): number {
  return Number(id);
}

/**
 * The Client Needs test plan as nested checkboxes. A category's box selects or clears all of its
 * subcategories and shows a mixed state when only some are chosen. Native checkboxes keep it
 * keyboard-operable with Tab and Space (interfaces.md section 4).
 */
export function CategoryTree({ categories, value, onChange }: CategoryTreeProps) {
  const selected = new Set(value);

  function toggle(ids: number[], on: boolean) {
    const next = new Set(selected);
    for (const id of ids) {
      if (on) next.add(id);
      else next.delete(id);
    }
    onChange([...next].sort((a, b) => a - b));
  }

  return (
    <fieldset className={forms.fieldset}>
      <legend>Client Needs categories</legend>
      <p className={forms.hint}>
        Choose none for every category. Several categories are mixed by the NCLEX test plan weights.
      </p>
      <ul className={styles.tree}>
        {categories.map((category) => {
          const leaves = leavesOf(category);
          const leafIds = leaves.map((leaf) => toId(leaf.id));
          const chosen = leafIds.filter((id) => selected.has(id)).length;
          const hasChildren = leaves[0] !== category;
          return (
            <li key={category.id}>
              <ParentCheckbox
                label={category.name}
                checked={chosen === leafIds.length}
                mixed={chosen > 0 && chosen < leafIds.length}
                onChange={(on) => toggle(leafIds, on)}
              />
              {hasChildren && (
                <ul className={styles.treeChildren}>
                  {leaves.map((leaf) => (
                    <li key={leaf.id}>
                      <label className={styles.check}>
                        <input
                          type="checkbox"
                          checked={selected.has(toId(leaf.id))}
                          onChange={(event) => toggle([toId(leaf.id)], event.target.checked)}
                        />
                        {leaf.name}
                      </label>
                    </li>
                  ))}
                </ul>
              )}
            </li>
          );
        })}
      </ul>
    </fieldset>
  );
}

interface ParentCheckboxProps {
  label: string;
  checked: boolean;
  mixed: boolean;
  onChange: (on: boolean) => void;
}

// "indeterminate" exists only as a DOM property, so it is set after render; screen readers then
// announce the box as "mixed".
function ParentCheckbox({ label, checked, mixed, onChange }: ParentCheckboxProps) {
  const ref = useRef<HTMLInputElement>(null);
  useEffect(() => {
    if (ref.current) ref.current.indeterminate = mixed;
  }, [mixed]);

  return (
    <label className={`${styles.check} ${styles.parent}`}>
      <input ref={ref} type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} />
      {label}
    </label>
  );
}
