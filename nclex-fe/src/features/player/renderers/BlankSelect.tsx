"use client";

import type { ItemReveal } from "@/features/attempt/api/attemptApi";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import { detailFor, detailValue } from "./revealMarks";
import type { Option } from "./types";

/** A drop-down blank and its options (interfaces.md 2.6). */
export interface Blank {
  id: string;
  options: Option[];
}

interface BlankSelectProps {
  blank: Blank;
  value: string | null;
  /** The blank's accessible name, which gives its sentence or table context (interfaces.md section 4). */
  label: string;
  disabled: boolean;
  onChange(optionId: string | null): void;
  reveal: ItemReveal | null;
}

/**
 * One blank as a native select, which brings the keyboard model and, on phones, the native picker.
 * After the reveal it says whether the choice was right and, when it wasn't, what the answer is.
 */
export function BlankSelect({ blank, value, label, disabled, onChange, reveal }: BlankSelectProps) {
  return (
    <span className={styles.blank}>
      <select
        className={styles.select}
        aria-label={label}
        value={value ?? ""}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value || null)}
      >
        <option value="">Select…</option>
        {blank.options.map((option) => (
          <option key={option.id} value={option.id}>
            {option.text}
          </option>
        ))}
      </select>
      <BlankMark blank={blank} reveal={reveal} />
    </span>
  );
}

function BlankMark({ blank, reveal }: { blank: Blank; reveal: ItemReveal | null }) {
  const detail = detailFor(reveal, blank.id);
  if (!detail) return null;
  if (detailValue(detail.selected) === detailValue(detail.expected)) return <OptionMarkText mark="correct" />;
  const expected = blank.options.find((option) => option.id === detailValue(detail.expected));
  return (
    <>
      <OptionMarkText mark="incorrect" label={detail.selected == null ? "✗ Not answered" : "✗ Incorrect"} />
      {expected && <span className={styles.expected}> Correct answer: {expected.text}</span>}
    </>
  );
}

/** Sets or clears one blank; null when no blank is answered, which clears the whole answer. */
export function withBlank(byBlank: Record<string, string | null>, blankId: string, optionId: string | null) {
  const next = Object.fromEntries(
    Object.entries({ ...byBlank, [blankId]: optionId }).filter(([, value]) => value !== null && value !== undefined),
  );
  return Object.keys(next).length > 0 ? { byBlank: next } : null;
}
