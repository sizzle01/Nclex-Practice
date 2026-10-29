import { expect, test } from "@playwright/test";
import { signUp, verifyEmail } from "./support/accounts";

test("a new candidate signs up and verifies their email from the link they receive", async ({ page }) => {
  const email = await signUp(page);

  // The worker sends the verification email through the outbox to Mailpit.
  await verifyEmail(page, email);

  await page.getByRole("link", { name: "Go to your dashboard" }).click();
  await expect(page).toHaveURL(/\/dashboard/);
});
