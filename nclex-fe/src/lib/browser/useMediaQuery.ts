import { useSyncExternalStore } from "react";

/** Below this width the matrix stacks into one card per row (interfaces.md section 4). */
export const narrowScreenQuery = "(max-width: 640px)";

/** Below this width the exhibit panel becomes a drawer instead of a split view (interfaces.md 5.3). */
export const belowSplitViewQuery = "(max-width: 1023px)";

/**
 * Whether a CSS media query matches, kept up to date as the window resizes. On the server, and in
 * browsers without matchMedia, it reports false (the wide layout).
 */
export function useMediaQuery(query: string): boolean {
  return useSyncExternalStore(
    (onChange) => {
      if (typeof window.matchMedia !== "function") return () => undefined;
      const list = window.matchMedia(query);
      list.addEventListener("change", onChange);
      return () => list.removeEventListener("change", onChange);
    },
    () => typeof window.matchMedia === "function" && window.matchMedia(query).matches,
    () => false,
  );
}
