import { beforeEach, describe, expect, it } from "vitest";
import {
  applyThemePreference,
  readThemePreference,
  saveThemePreference,
  themeStorageKey,
} from "./themePreference";

// A storage whose every call throws, like blocked site data or some private-browsing modes.
const brokenStorage = {
  getItem() {
    throw new Error("blocked");
  },
  setItem() {
    throw new Error("blocked");
  },
  removeItem() {
    throw new Error("blocked");
  },
} as unknown as Storage;

describe("themePreference", () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute("data-theme");
  });

  it("defaults to system when nothing is saved", () => {
    expect(readThemePreference()).toBe("system");
  });

  it("reads a saved light or dark choice", () => {
    localStorage.setItem(themeStorageKey, "dark");
    expect(readThemePreference()).toBe("dark");
  });

  it("ignores an unrecognised saved value", () => {
    localStorage.setItem(themeStorageKey, "purple");
    expect(readThemePreference()).toBe("system");
  });

  it("saves light and dark, and clears the key for system", () => {
    saveThemePreference("light");
    expect(localStorage.getItem(themeStorageKey)).toBe("light");

    saveThemePreference("system");
    expect(localStorage.getItem(themeStorageKey)).toBeNull();
  });

  it("falls back to system and does not throw when storage is unavailable", () => {
    expect(readThemePreference(brokenStorage)).toBe("system");
    expect(() => saveThemePreference("dark", brokenStorage)).not.toThrow();
    expect(readThemePreference(null)).toBe("system");
  });

  it("sets data-theme for light and dark, and removes it for system", () => {
    applyThemePreference("dark");
    expect(document.documentElement).toHaveAttribute("data-theme", "dark");

    applyThemePreference("system");
    expect(document.documentElement).not.toHaveAttribute("data-theme");
  });
});
