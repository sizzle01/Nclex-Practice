/** The user's theme choice. "system" follows the operating system's light/dark setting. */
export type ThemePreference = "system" | "light" | "dark";

export const themePreferences: readonly ThemePreference[] = ["system", "light", "dark"];

/** Key in localStorage. A theme name is not sensitive, so the browser is a fine place for it. */
export const themeStorageKey = "nclex-theme";

function isThemePreference(value: unknown): value is ThemePreference {
  return themePreferences.includes(value as ThemePreference);
}

// Reading localStorage itself can throw (blocked storage, some private modes), so every access is guarded.
function browserStorage(): Storage | null {
  try {
    return window.localStorage;
  } catch {
    return null;
  }
}

/** The saved choice, or "system" when nothing (or something unrecognised) is saved. */
export function readThemePreference(storage: Storage | null = browserStorage()): ThemePreference {
  try {
    const saved = storage?.getItem(themeStorageKey);
    return isThemePreference(saved) ? saved : "system";
  } catch {
    return "system";
  }
}

/** Saves the choice. "system" is the default, so it is stored as no value at all. */
export function saveThemePreference(preference: ThemePreference, storage: Storage | null = browserStorage()) {
  try {
    if (preference === "system") storage?.removeItem(themeStorageKey);
    else storage?.setItem(themeStorageKey, preference);
  } catch {
    // Storage unavailable: the theme still applies for this page view, it just is not remembered.
  }
}

/**
 * Sets data-theme on <html>, which globals.css uses to force light or dark. "system" removes it so
 * the page follows the operating system's setting.
 */
export function applyThemePreference(preference: ThemePreference, root: HTMLElement = document.documentElement) {
  if (preference === "system") root.removeAttribute("data-theme");
  else root.setAttribute("data-theme", preference);
}
