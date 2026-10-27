"use client";

import { useLayoutEffect, useSyncExternalStore } from "react";
import {
  type ThemePreference,
  applyThemePreference,
  readThemePreference,
  saveThemePreference,
  themeStorageKey,
} from "./themePreference";

const listeners = new Set<() => void>();

function notify() {
  for (const listener of listeners) listener();
}

// Another tab changed the theme: apply it here too.
function onStorage(event: StorageEvent) {
  if (event.key !== themeStorageKey && event.key !== null) return;
  applyThemePreference(readThemePreference());
  notify();
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  if (listeners.size === 1) window.addEventListener("storage", onStorage);
  return () => {
    listeners.delete(listener);
    if (listeners.size === 0) window.removeEventListener("storage", onStorage);
  };
}

/** Saves the choice, applies it to the page and updates every component using useTheme. */
export function setThemePreference(preference: ThemePreference) {
  saveThemePreference(preference);
  applyThemePreference(preference);
  notify();
}

// The server cannot see localStorage, so it renders "system"; React swaps in the saved value after hydration.
const serverSnapshot = (): ThemePreference => "system";
const clientSnapshot = (): ThemePreference => readThemePreference();

/** The current theme choice and a setter. localStorage is the single source of truth. */
export function useTheme() {
  const preference = useSyncExternalStore(subscribe, clientSnapshot, serverSnapshot);

  // In development, Strict Mode's remount resets <html> attributes and drops the one the inline
  // head script set. Re-apply the saved choice before paint; in production this is a no-op.
  useLayoutEffect(() => {
    applyThemePreference(readThemePreference());
  }, []);

  return { preference, setPreference: setThemePreference };
}
