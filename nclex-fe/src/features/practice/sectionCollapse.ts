import type { SetSection } from "./setCategories";

/**
 * Which catalog sections the candidate has opened or closed (F28), keyed by sectionKey. Only the
 * sections they toggled are stored; every other section uses its default.
 */
export type SectionOpenState = Record<string, boolean>;

/** Key in localStorage. Open or closed headings are not sensitive, so the browser is a fine place for them. */
export const sectionStorageKey = "nclex-practice-sections";

/**
 * A stable key for a section. Topic sections get index-based ids ("topic-0"), which shift when a
 * new book section gets tests, so they are keyed by their book section name instead.
 */
export function sectionKey(section: Pick<SetSection, "id" | "title">): string {
  return section.id.startsWith("topic-") ? `topic:${section.title}` : section.id;
}

/** Whether a section is open: the candidate's saved choice, or else the tab's default. */
export function isSectionOpen(state: SectionOpenState, key: string, openByDefault: boolean): boolean {
  return state[key] ?? openByDefault;
}

// Reading localStorage itself can throw (blocked storage, some private modes), so every access is guarded.
function browserStorage(): Storage | null {
  try {
    return window.localStorage;
  } catch {
    return null;
  }
}

/** The saved state as the raw JSON text, so useSyncExternalStore can compare snapshots cheaply. */
export function readSectionStateText(storage: Storage | null = browserStorage()): string {
  try {
    return storage?.getItem(sectionStorageKey) ?? "{}";
  } catch {
    return "{}";
  }
}

/** Parses the saved state, ignoring anything that is not a map of booleans. */
export function parseSectionState(text: string): SectionOpenState {
  try {
    const value: unknown = JSON.parse(text);
    if (typeof value !== "object" || value === null || Array.isArray(value)) return {};
    return Object.fromEntries(
      Object.entries(value).filter((entry): entry is [string, boolean] => typeof entry[1] === "boolean"),
    );
  } catch {
    return {};
  }
}

/** Saves the state. An empty state is stored as no value at all. */
export function saveSectionState(state: SectionOpenState, storage: Storage | null = browserStorage()) {
  try {
    if (Object.keys(state).length === 0) storage?.removeItem(sectionStorageKey);
    else storage?.setItem(sectionStorageKey, JSON.stringify(state));
  } catch {
    // Storage unavailable: the toggle still works for this page view, it just is not remembered.
  }
}
