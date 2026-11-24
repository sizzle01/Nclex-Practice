"use client";

import { useCallback, useMemo, useSyncExternalStore } from "react";
import {
  type SectionOpenState,
  isSectionOpen,
  parseSectionState,
  readSectionStateText,
  saveSectionState,
  sectionStorageKey,
} from "./sectionCollapse";

const listeners = new Set<() => void>();

function notify() {
  for (const listener of listeners) listener();
}

// Another tab opened or closed a section: follow it here too.
function onStorage(event: StorageEvent) {
  if (event.key === sectionStorageKey || event.key === null) notify();
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  if (listeners.size === 1) window.addEventListener("storage", onStorage);
  return () => {
    listeners.delete(listener);
    if (listeners.size === 0) window.removeEventListener("storage", onStorage);
  };
}

// The server cannot see localStorage, so it renders the defaults; the saved state follows after hydration.
const serverSnapshot = () => "{}";

function update(change: (state: SectionOpenState) => SectionOpenState) {
  saveSectionState(change(parseSectionState(readSectionStateText())));
  notify();
}

/**
 * Open and closed catalog sections (F28), remembered per browser. `openByDefault` applies to every
 * section the candidate has not toggled yet.
 */
export function useSectionCollapse(openByDefault: boolean) {
  const text = useSyncExternalStore(subscribe, readSectionStateText, serverSnapshot);
  const state = useMemo(() => parseSectionState(text), [text]);

  const isOpen = useCallback((key: string) => isSectionOpen(state, key, openByDefault), [state, openByDefault]);

  const toggle = useCallback(
    (key: string) => update((saved) => ({ ...saved, [key]: !isSectionOpen(saved, key, openByDefault) })),
    [openByDefault],
  );

  const setAll = useCallback(
    (keys: string[], open: boolean) =>
      update((saved) => ({ ...saved, ...Object.fromEntries(keys.map((key) => [key, open])) })),
    [],
  );

  return { isOpen, toggle, setAll };
}
