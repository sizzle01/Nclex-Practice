import forms from "@/components/forms/forms.module.css";
import styles from "./builder.module.css";

interface CheckboxGroupProps<T extends string | number> {
  legend: string;
  hint?: string;
  options: { value: T; label: string }[];
  value: T[];
  onChange: (value: T[]) => void;
}

/** A fieldset of checkboxes for a multi-select filter. Nothing chosen means "any". */
export function CheckboxGroup<T extends string | number>({ legend, hint, options, value, onChange }: CheckboxGroupProps<T>) {
  function toggle(option: T, on: boolean) {
    // Kept in the options' order, so the same choices always give the same filters (and cache key).
    onChange(options.map((o) => o.value).filter((v) => (v === option ? on : value.includes(v))));
  }

  return (
    <fieldset className={forms.fieldset}>
      <legend>{legend}</legend>
      {hint && <p className={forms.hint}>{hint}</p>}
      <div className={styles.checkGrid}>
        {options.map((option) => (
          <label key={option.value} className={styles.check}>
            <input
              type="checkbox"
              checked={value.includes(option.value)}
              onChange={(event) => toggle(option.value, event.target.checked)}
            />
            {option.label}
          </label>
        ))}
      </div>
    </fieldset>
  );
}
