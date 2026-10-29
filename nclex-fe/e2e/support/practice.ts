import { expect, type Page } from "@playwright/test";

// The sample content (nclex-be/content/sample-questions.rn.json) that `seed` loads.
export const tutorSet = "Comprehensive Practice 1";
export const timedExamSet = "Comprehensive Challenge 1";

/** Starts an attempt of `title` from /practice and waits for its first question. */
export async function startSet(page: Page, title: string): Promise<void> {
  await page.goto("/practice");
  const card = page.getByRole("article").filter({ has: page.getByRole("heading", { name: title, exact: true }) });
  await card.getByRole("button", { name: "View details" }).click();
  await page.getByRole("button", { name: "Start" }).click();
  await expect(page).toHaveURL(/\/attempts\/[0-9a-f-]+$/);
  await expect(page.getByText(/^Question 1 of \d+$/)).toBeVisible();
}

/** The status line under the question once the autosave has reached the server. */
export async function expectSaved(page: Page): Promise<void> {
  await expect(page.getByRole("status").filter({ hasText: /Saved/ })).toBeVisible();
}

/**
 * The question's own answer controls. Scoped to <main> because the header's theme switcher is a
 * radio group too.
 */
export function answers(page: Page, role: "radio" | "checkbox") {
  return page.getByRole("main").getByRole(role);
}
