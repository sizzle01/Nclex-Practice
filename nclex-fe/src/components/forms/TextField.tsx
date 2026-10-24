import type { ComponentProps } from "react";
import styles from "./forms.module.css";

interface TextFieldProps extends ComponentProps<"input"> {
  name: string;
  label: string;
  error?: string;
  hint?: string;
}

/**
 * A labelled text input with an optional hint and error message. The hint and error are linked
 * with aria-describedby, so screen readers announce them with the field (interfaces.md section 4).
 */
export function TextField({ name, label, error, hint, ...inputProps }: TextFieldProps) {
  const id = `field-${name}`;
  const hintId = hint ? `${id}-hint` : undefined;
  const errorId = error ? `${id}-error` : undefined;
  const describedBy = [hintId, errorId].filter(Boolean).join(" ") || undefined;

  return (
    <div className={styles.field}>
      <label htmlFor={id}>{label}</label>
      {hint && (
        <p id={hintId} className={styles.hint}>
          {hint}
        </p>
      )}
      <input id={id} name={name} aria-invalid={error ? true : undefined} aria-describedby={describedBy} {...inputProps} />
      {error && (
        <p id={errorId} className={styles.error}>
          {error}
        </p>
      )}
    </div>
  );
}
