import { expect, test } from "@playwright/test";
import { signUpVerified } from "./support/accounts";
import { answers, expectSaved, startSet, timedExamSet } from "./support/practice";

test("a timed exam can be paused, resumed and submitted, and the result arrives as a notification", async ({ page }) => {
  await signUpVerified(page);
  await startSet(page, timedExamSet);
  await answers(page, "checkbox").first().check();
  await expectSaved(page);

  await page.getByRole("button", { name: "Pause" }).click();
  const paused = page.getByRole("dialog", { name: "Test paused" });
  await expect(paused).toBeVisible();
  await paused.getByRole("button", { name: "Resume" }).click();
  await expect(paused).toBeHidden();

  await page.getByRole("button", { name: "Submit test" }).click();
  await page.getByRole("dialog", { name: "Submit this test?" }).getByRole("button", { name: "Submit test" }).click();
  await expect(page).toHaveURL(/\/attempts\/[0-9a-f-]+\/results$/);

  // One answered item out of 40 cannot pass, so the result notification is the "didn't reach" one.
  // It reaches the bell through the outbox, Redis and the SSE stream (architecture.md 5.5, 5.6).
  const bell = page.getByRole("button", { name: /^Notifications, \d+ unread$/ });
  await expect(bell).toBeVisible({ timeout: 15_000 });
  await bell.click();
  await expect(page.getByRole("button", { name: /You didn't reach the passing threshold/ })).toBeVisible();
});
