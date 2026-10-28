"use client";

import { useId } from "react";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import { detailFor, detailValue } from "./revealMarks";
import type { ItemRendererProps } from "./types";

interface CalcContent {
  unit: string;
  decimals: number;
  inputLabel?: string | null;
}

interface CalcResponse {
  value: string;
}

/** The grade detail target of the single answer box (FillBlankCalcModule.AnswerTarget). */
const answerTarget = "value";

/**
 * Fill in the blank, calculation (interfaces.md 2.8): a text box with a decimal keypad on phones
 * and the unit as a suffix. The raw text is saved and the server parses it, so every client is
 * scored the same way; the description gives the unit and the decimals (interfaces.md section 4).
 */
export function CalcRenderer({
  content,
  response,
  onChange,
  disabled,
  labelledBy,
  reveal,
}: ItemRendererProps<CalcContent, CalcResponse>) {
  const inputId = useId();
  const hintId = useId();
  const decimals = Number(content.decimals);

  return (
    <div role="group" aria-labelledby={labelledBy}>
      <label htmlFor={inputId} className={styles.calcLabel}>
        {content.inputLabel || "Your answer"}
      </label>
      <div className={styles.calcInput}>
        <input
          id={inputId}
          type="text"
          inputMode="decimal"
          autoComplete="off"
          aria-describedby={hintId}
          value={response?.value ?? ""}
          disabled={disabled}
          onChange={(event) => onChange(event.target.value.trim() === "" ? null : { value: event.target.value })}
        />
        <span className={styles.unit}>{content.unit}</span>
      </div>
      <p id={hintId} className={styles.instructions}>
        Answer in {content.unit},{" "}
        {decimals === 0 ? "as a whole number" : `to ${decimals} decimal ${decimals === 1 ? "place" : "places"}`}.
      </p>
      <CalcMark reveal={reveal} unit={content.unit} />
    </div>
  );
}

function CalcMark({ reveal, unit }: { reveal: ItemRendererProps["reveal"]; unit: string }) {
  const detail = detailFor(reveal, answerTarget);
  if (!detail) return null;
  if (!detail.missed) return <OptionMarkText mark="correct" />;
  return (
    <p>
      <OptionMarkText mark="incorrect" label={detail.selected == null ? "✗ Not answered" : "✗ Incorrect"} />
      <span className={styles.expected}>
        {" "}
        Correct answer: {detailValue(detail.expected)} {unit}
      </span>
    </p>
  );
}
