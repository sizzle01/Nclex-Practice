import { defineConfig, devices } from "@playwright/test";

// End-to-end tests (F16) run against the whole app in Docker, as staging runs it:
//   docker compose --profile app up -d --build --wait     (from the repo root)
//   npm run e2e
// E2E_BASE_URL and MAILPIT_URL point elsewhere when the stack uses other ports.
export default defineConfig({
  testDir: "./e2e",
  // *.e2e.ts, so Vitest (which picks up *.test.ts and *.spec.ts) never runs these.
  testMatch: "**/*.e2e.ts",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  // One retry in CI only, so a flaky step shows up in the report instead of failing the build silently.
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [["list"], ["html", { open: "never" }]] : "list",
  use: {
    baseURL: process.env.E2E_BASE_URL ?? "http://localhost:3000",
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
  projects: [{ name: "chromium", use: { ...devices["Desktop Chrome"] } }],
});
