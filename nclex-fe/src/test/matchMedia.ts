import { vi } from "vitest";

/**
 * jsdom has no matchMedia, so components render their wide layout. This makes every media query
 * report `matches`, to test the narrow (phone) layouts. Undo with vi.unstubAllGlobals().
 */
export function stubMatchMedia(matches: boolean) {
  vi.stubGlobal(
    "matchMedia",
    vi.fn((query: string) => ({
      matches,
      media: query,
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      addListener: vi.fn(),
      removeListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  );
}
