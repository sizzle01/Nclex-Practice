import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";

afterEach(() => cleanup());

// jsdom has no ResizeObserver, which Recharts' ResponsiveContainer needs. Charts render with no
// size in tests; their numbers are checked through the data tables beside them.
globalThis.ResizeObserver ??= class {
  observe() {}
  unobserve() {}
  disconnect() {}
};
