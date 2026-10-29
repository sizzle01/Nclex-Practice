import { expect, test } from "@playwright/test";
import { signUpVerified } from "./support/accounts";
import { answers, startSet, tutorSet } from "./support/practice";

test("in a tutor set the candidate sees the answer and rationale after each item", async ({ page }) => {
  await signUpVerified(page);
  await startSet(page, tutorSet);

  await answers(page, "radio").first().check();
  await page.getByRole("button", { name: "Submit answer" }).click();

  await expect(page.getByRole("region", { name: "Rationale" })).toBeVisible();
  await page.getByRole("button", { name: "Next ▶" }).click();
  await expect(page.getByText(/^Question 2 of \d+$/)).toBeVisible();
});
