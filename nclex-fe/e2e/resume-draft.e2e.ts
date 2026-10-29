import { expect, test } from "@playwright/test";
import { logIn, signUpVerified } from "./support/accounts";
import { answers, expectSaved, startSet, timedExamSet } from "./support/practice";

test("a draft started in one browser resumes with its answers in another", async ({ browser }) => {
  const laptop = await (await browser.newContext()).newPage();
  const email = await signUpVerified(laptop);
  await startSet(laptop, timedExamSet);
  await answers(laptop, "checkbox").first().check();
  await expectSaved(laptop);

  // A second browser context shares nothing with the first: no cookies, no local storage.
  const phone = await (await browser.newContext()).newPage();
  await logIn(phone, email);
  await phone.getByRole("link", { name: "Resume" }).first().click();
  await expect(phone.getByText(/^Question 1 of \d+$/)).toBeVisible();
  await expect(answers(phone, "checkbox").first()).toBeChecked();

  // Opening it on the phone took over the device lease (architecture.md 5.2), so the laptop is told.
  await laptop.getByRole("button", { name: "Next ▶" }).click();
  await expect(laptop.getByRole("alertdialog", { name: "This attempt is open on another device" })).toBeVisible();
});
