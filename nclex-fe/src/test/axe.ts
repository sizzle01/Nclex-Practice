import axe from "axe-core";
import { expect } from "vitest";

/**
 * Runs the axe accessibility checks (WCAG 2.1 A and AA) on a rendered component and fails with the
 * rule ids and offending markup. jsdom has no layout or canvas, so colour contrast is checked by
 * eye in the dev gallery instead.
 */
export async function expectNoAxeViolations(container: Element) {
  const results = await axe.run(container, {
    runOnly: { type: "tag", values: ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "best-practice"] },
    rules: { "color-contrast": { enabled: false }, region: { enabled: false } },
  });
  const problems = results.violations.map(
    (violation) => `${violation.id}: ${violation.nodes.map((node) => node.html).join(" | ")}`,
  );
  expect(problems).toEqual([]);
}
