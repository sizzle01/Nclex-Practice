"use client";

import { type ReactNode, useId } from "react";
import type { ThemePreference } from "../themePreference";
import { useTheme } from "../useTheme";
import styles from "./ThemeSwitcher.module.css";

const options: { value: ThemePreference; label: string; icon: ReactNode }[] = [
  {
    value: "system",
    label: "System",
    icon: (
      <path d="M4 5h16v11H4zM9 20h6M12 16v4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" fill="none" />
    ),
  },
  {
    value: "light",
    label: "Light",
    icon: (
      <>
        <circle cx="12" cy="12" r="4" fill="currentColor" />
        <path
          d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
        />
      </>
    ),
  },
  {
    value: "dark",
    label: "Dark",
    icon: <path d="M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5z" fill="currentColor" />,
  },
];

/**
 * Three-way theme control (System / Light / Dark). It is a native radio group, so Tab enters it,
 * the arrow keys move the choice and screen readers announce "Theme" (interfaces.md section 4).
 * Each option shows an icon and a text label; on small screens the label is visually hidden.
 */
export function ThemeSwitcher() {
  const { preference, setPreference } = useTheme();
  const name = useId();

  return (
    <fieldset className={styles.switcher}>
      <legend className={styles.visuallyHidden}>Theme</legend>
      {options.map((option) => (
        <label key={option.value} className={styles.option}>
          <input
            type="radio"
            name={name}
            value={option.value}
            checked={preference === option.value}
            onChange={() => setPreference(option.value)}
            className={styles.visuallyHidden}
          />
          <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
            {option.icon}
          </svg>
          <span className={styles.label}>{option.label}</span>
        </label>
      ))}
    </fieldset>
  );
}
